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
 
Version 1.0.0.1 (Spheres 2.6.4.0)
 Date: 03/08/2012
 Author: MF
 Description: added gIsReportWithDCDetails flag
================================================================================================================
  -->

  <!--Special section-->
  <xsl:variable name="gIsReportWithDCDetails" select="false()"/>
  <xsl:variable name="gIsReportWithCSSDetails" select="false()"/>

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Shared/Shared_Report_PDFCB.xslt" />

  <!-- ================================================== -->
  <!--        Parameters                                  -->
  <!-- ================================================== -->
  <xsl:param name="pCurrentCulture" select="'fr-FR'" />

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <xsl:variable name="gDataDocumentTrades" select="//dataDocument/trade"/>
  <xsl:variable name="gReportType" select="$gcReportTypeCSHBAL"/>

  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!-- ================================================== -->
  <!--        Global Variables : Display                  -->
  <!-- ================================================== -->

  <xsl:variable name="gGroupDisplaySettings">
    <group sort="1" name="CBStream" isDataGroup="{true()}" padding-top="{$gGroupPaddingTop}"  table-omit-header-at-break="true" withNewPage="{true()}">
      <!--Titles-->
      <title empty-row="2mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-weight="bold">
        <column number="01" name="TS-Currency" colspan="7">
          <data font-size="10pt"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>
        <column number="02" name="TS-FxRate" colspan="4">
          <data text-align="right" display-align="center" font-size="7pt"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>
      </title>

      <!--Columns-->
      <column number="01" name="TS-Designation1" width="3mm">
        <header colspan="4"
                ressource="CSHBAL-Designation"
                font-size="8pt" font-weight="bold" text-align="left" border-right-style="none"/>
        <data display-align="center"/>
      </column>
      <column number="02" name="TS-Designation2" width="26mm" >
        <header colspan="-1"/>
        <data display-align="center"/>
      </column>
      <column number="03" name="TS-Designation3" width="29mm" >
        <header colspan="-1"/>
        <data display-align="center"/>
      </column>
      <column number="04" name="TS-Designation4" width="39mm" >
        <header colspan="-1"/>
        <data display-align="center"/>
      </column>
      <column number="05" name="TS-Currency" width="8mm">
        <header colspan="-1"/>
        <data display-align="center" text-align="right" border-right-style="none" />
      </column>
      <column number="06" name="TS-Amount" width="26mm">
        <header ressource="%%RES:CSHBAL-AmountTitle%%"
                text-align="center" colspan="3" font-size="8pt" font-weight="bold" border-left-style="none"/>
        <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:currency.PARENT%%"/>
      </column>
      <column number="07" name="TS-CrDr" width="3.5mm" font-size="{$gDetTrdDetFontSize}">
        <header colspan="-1"/>
        <data border-left-style="none" display-align="center"/>
      </column>
      <column number="08" name="TS-Desc" width="25mm" font-size="6pt">
        <header ressource=" " colspan="2" border-right-style="none"/>
        <data font-style="italic" display-align="center"/>
      </column>
      <column number="09" name="TS-ExCurrency" width="8mm">
        <header colspan="-1"/>
        <data text-align="right" display-align="center" border-right-style="none"/>
      </column>
      <column number="10" name="TS-ExAmount" width="26mm">
        <header text-align="right" ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}" colspan="2" font-size="8pt" font-weight="bold" border-left-style="none"/>
        <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
      </column>
      <column number="11" name="TS-ExCrDr" width="3.5mm" font-size="{$gDetTrdDetFontSize}">
        <header colspan="-1"/>
        <data border-left-style="none" display-align="center"/>
      </column>

      <condition name="IsFeesDetailsExist" condition="feeGrossAmount"/>

      <!--rows-->
      <row sort="1" name="cashDayBefore" font-size="8pt" font-weight="bold">
        <column name="TS-Designation1">
          <data colspan="4" value="%%RES:CSHBAL-cashDayBefore%%" />
        </column>
        <column name="TS-Currency">
          <data value="%%DATA:currency.PARENT%%"/>
        </column>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr">
          <data value="%%DATA:crDr%%"/>
        </column>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency">
          <data value="{$gReportingCurrency}" />
        </column>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%" />
        </column>
        <column name="TS-ExCrDr">
          <data value="%%DATA:crDr%%"/>
        </column>
      </row>
      <row sort="2" name="cashMouvements" font-size="8pt" >
        <column name="TS-Designation1">
          <data colspan="4" value="%%RES:CSHBAL-cashMouvements%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" font-weight="bold" />
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
      <row sort="3" name="optionPremium" font-size="8pt" >
        <column name="TS-Designation1">
          <data rowspan="4"/>
        </column>
        <column name="TS-Designation2">
          <data colspan="3" value="%%RES:CSHBAL-optionPremium%%"/>
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
      <row sort="4" name="variationMargin" font-size="8pt" >
        <column name="TS-Designation2">
          <data colspan="3" value="%%RES:CSHBAL-variationMargin%%"/>
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
      <row sort="5" name="cashSettlement" font-size="8pt" >
        <column name="TS-Designation2">
          <data colspan="3" value="%%RES:CSHBAL-cashSettlement%%"/>
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
      <row sort="6" name="feesVAT" font-size="8pt" >
        <column name="TS-Designation2">
          <data colspan="3" value="%%RES:CSHBAL-feesVAT%%"/>
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
      <row sort="7" name="returnCallDeposit" font-size="8pt" >
        <column name="TS-Designation1">
          <data colspan="4" value="%%RES:CSHBAL-returnCallDeposit%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" font-weight="bold"/>
        </column>
        <column name="TS-CrDr">
          <data value="%%DATA:crDr%%" font-weight="bold"/>
        </column>
        <column name="TS-Desc">
          <data value="%%DESC:DetCashBalance%%" font-size="6pt"/>
        </column>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%" font-weight="bold"/>
        </column>
        <column name="TS-ExCrDr">
          <data value="%%DATA:crDr%%" font-weight="bold"/>
        </column>
      </row>
      <row sort="8" name="todayCashBalance" font-size="8pt" font-weight="bold" background-color="{$gDetTrdTotalBackgroundColor}">
        <column name="TS-Designation1">
          <data colspan="4"
                value="%%RES:CSHBAL-todayCashBalance%%"
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
      <row sort="9" name="%%EMPTY%%" height="2mm" condition="IsFeesDetailsExist"/>
      <row sort="10" name="%%FREE%%" condition="IsFeesDetailsExist"
           font-size="7pt" font-weight="bold"
           background-color="{$gDetTrdHdrCellBackgroundColor}">
        <column number="01" name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data colspan="3" value="%%RES:CSHBAL-feesConstituents%%"/>
        </column>
        <column number="03" name="%%EMPTY%%">
          <data colspan="7" border-left-style="solid"/>
        </column>
      </row>
      <row sort="11" name="feeGrossAmount" dataLink="%%DATA:keyForSum%%" font-size="7pt">
        <column name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data colspan="2" rowspan="%%DATA:rowspan%%" value="%%DATA:keyForSum.RES%%"/>
        </column>
        <column name="TS-Designation3">
          <data colspan="1" value="%%RES:CSHBAL-feeGrossAmount%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" />
        </column>
        <column name="TS-CrDr">
          <data value="%%DATA:crDr%%"/>
        </column>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%" />
        </column>
        <column name="TS-ExCrDr">
          <data value="%%DATA:crDr%%"/>
        </column>
        <row sort="12" name="feeTax" dataLink="%%DATA:keyForSum%%" font-size="7pt">
          <column name="%%EMPTY%%">
            <data border-right-style="solid"/>
          </column>
          <column name="TS-Designation3">
            <data colspan="2" value="%%DATA:taxDetailName.RES%% (">
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
      </row>
      <row sort="13" name="totalFeesVAT" font-size="7pt" font-weight="bold" background-color="{$gDetTrdHdrCellBackgroundColor}">
        <column name="%%EMPTY%%" background-color="white"/>
        <column name="TS-Designation2">
          <data colspan="3"
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
          <data value="%%DATA:crDr%%" border-bottom-style="solid"/>
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
          <data value="%%DATA:crDr%%" border-bottom-style="solid"/>
        </column>
      </row>
      <row sort="14" name="%%EMPTY%%" height="2mm" />
      <row sort="15" name="%%FREE%%"
          font-size="{$gDetTrdHdrFontSize}"
          font-weight="bold"
          background-color="{$gDetTrdHdrCellBackgroundColor}" >
        <column number="01" name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data colspan="3"
                value="%%RES:CSHBAL-returnCallDepositDet%%"
                border-right-style="solid"/>
        </column>
        <column number="03" name="%%EMPTY%%">
          <data colspan="7" border-left-style="solid"/>
        </column>
      </row>
      <row sort="16" name="previousMarginRequirement" font-size="7pt" >
        <column name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data rowspan="%%DATA:rowspan%%" value="%%RES:CSHBAL-previousOrders%%"/>
        </column>
        <column name="TS-Designation3">
          <data colspan="2" value="%%RES:CSHBAL-previousMarginRequirement%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="17" name="previousCollateralAvailable" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation3">
          <data rowspan="2" value="%%RES:CSHBAL-previousCollateralAvailable%%"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Available%%" font-style="italic">
            <inline font-size="6pt">
              <xsl:value-of select="' (%%RES:CSHBAL-ExceptHaircut%%)'"/>
            </inline>
          </data>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" font-style="italic" text-align="left"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount"/>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="18" name="previousCollateralUsed" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Used%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="19" name="previousCashAvailable" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation3">
          <data rowspan="2" value="%%RES:CSHBAL-previousCashAvailable%%"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Available%%" font-style="italic" text-align="left"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" font-style="italic" text-align="left"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount"/>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="20" name="previousCashUsed" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Used%%">
            <inline font-size="60%" baseline-shift="super">
              <xsl:value-of select="' (A1)'"/>
            </inline>
          </data>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="21" name="previousUncoveredMarginRequirement" font-size="7pt" font-weight="bold" >
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation3">
          <data colspan="2" value="%%RES:CSHBAL-previousUncoveredMarginRequirement%%">
            <inline font-size="60%" baseline-shift="super">
              <xsl:choose>
                <xsl:when test="$gUseAvailableCash = 'true'">
                  <xsl:value-of select="' (A2)'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="' (A)'"/>
                </xsl:otherwise>
              </xsl:choose>
            </inline>
          </data>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>

      <row sort="22" name="marginRequirement" font-size="7pt">
        <column name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data rowspan="%%DATA:rowspan%%" value="%%RES:CSHBAL-todayOrders%%"/>
        </column>
        <column name="TS-Designation3">
          <data colspan="2" value="%%RES:CSHBAL-marginRequirement%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="23" name="collateralAvailable" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation3">
          <data rowspan="2" value="%%RES:CSHBAL-collateralAvailable%%"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Available%%" font-style="italic">
            <inline font-size="6pt">
              <xsl:value-of select="' (%%RES:CSHBAL-ExceptHaircut%%)'"/>
            </inline>
          </data>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" font-style="italic" text-align="left"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount"/>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="24" name="collateralUsed" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Used%%"/>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="25" name="cashAvailable" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation3">
          <data rowspan="2" value="%%RES:CSHBAL-cashAvailable%%"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Available%%" font-style="italic" />
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" font-style="italic" text-align="left"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount"/>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="26" name="cashUsed" font-size="7pt">
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation4">
          <data value="%%RES:CSHBAL-Used%%">
            <inline font-size="60%" baseline-shift="super">
              <xsl:value-of select="' (B1)'"/>
            </inline>
          </data>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="27" name="uncoveredMarginRequirement" font-size="7pt" font-weight="bold" >
        <column name="%%EMPTY%%">
          <data border-right-style="solid"/>
        </column>
        <column name="TS-Designation3">
          <data colspan="2" value="%%RES:CSHBAL-uncoveredMarginRequirement%%">
            <inline font-size="60%" baseline-shift="super">
              <xsl:choose>
                <xsl:when test="$gUseAvailableCash = 'true'">
                  <xsl:value-of select="' (B2)'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="' (B)'"/>
                </xsl:otherwise>
              </xsl:choose>
            </inline>
          </data>
        </column>
        <column name="TS-Currency"/>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%"/>
        </column>
        <column name="TS-CrDr"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr"/>
      </row>
      <row sort="28" name="returnCallDepositAB" font-size="7pt" font-weight="bold" background-color="{$gDetTrdHdrCellBackgroundColor}">
        <column name="%%EMPTY%%" background-color="white"/>
        <column name="TS-Designation2">
          <data colspan="3"
                value="%%DATA:crDrResource.RES%%"
                background-color="{$gDetTrdHdrCellBackgroundColor}"
                border-bottom-style="solid">
            <inline font-size="60%" baseline-shift="super">
              <xsl:choose>
                <xsl:when test="$gUseAvailableCash = 'true'">
                  <xsl:value-of select="' (A1 + A2) - (B1 + B2)'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="' (A - B)'"/>
                </xsl:otherwise>
              </xsl:choose>
            </inline>
          </data>
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
          <data value="%%DATA:crDr%%" border-bottom-style="solid"/>
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
          <data value="%%DATA:crDr%%" border-bottom-style="solid"/>
        </column>
      </row>
    </group>
    <group sort="2" name="CBStreamEx" isDataGroup="{true()}" padding-top="{$gGroupPaddingTop}" table-omit-header-at-break="true">
      <!--Titles-->
      <title empty-row="2mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-weight="bold" border=" ">
        <column number="01" name="TS-Currency" colspan="8">
          <data font-size="10pt" value="%%RES:CSHBAL-CBStreamEx%% {$gReportingCurrency}"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>
      </title>

      <!--Columns-->
      <column number="01" name="TS-Designation1" width="3mm">
        <header colspan="4"
                ressource="CSHBAL-Designation"
                font-size="8pt" font-weight="bold" text-align="left"/>
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
      <column number="04" name="TS-Filler" width="37.5mm">
        <header colspan="-1"/>
      </column>
      <column number="05" name="TS-Desc" width="25mm" font-size="6pt">
        <header ressource=" " colspan="2" border-right-style="none"/>
        <data font-style="italic" display-align="center"/>
      </column>
      <column number="06" name="TS-ExCurrency" width="8mm">
        <header colspan="-1"/>
        <data text-align="right" display-align="center" border-right-style="none"/>
      </column>
      <column number="07" name="TS-ExAmount" width="26mm">
        <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                text-align="right" colspan="2" font-size="8pt" font-weight="bold" border-left-style="none"/>
        <data text-align="right" display-align="center" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
      </column>
      <column number="08" name="TS-ExCrDr" width="3.5mm" font-size="{$gDetTrdDetFontSize}">
        <header colspan="-1"/>
        <data border-left-style="none" display-align="center"/>
      </column>

      <condition name="IsFeesDetailsExist" condition="feeGrossAmount"/>

      <!--row-->
      <row sort="1" name="cashDayBefore" font-size="8pt" font-weight="bold">
        <column name="TS-Designation1">
          <data colspan="3" value="%%RES:CSHBAL-cashDayBefore%%" />
        </column>
        <column name="TS-Filler"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency">
          <data value="{$gReportingCurrency}" />
        </column>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%" />
        </column>
        <column name="TS-ExCrDr">
          <data value="%%DATA:crDr%%"/>
        </column>
      </row>
      <row sort="2" name="cashMouvements" font-size="8pt" >
        <column name="TS-Designation1">
          <data colspan="3" value="%%RES:CSHBAL-cashMouvements%%"/>
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
      <row sort="3" name="optionPremium" font-size="8pt" >
        <column name="TS-Designation1">
          <data rowspan="4"/>
        </column>
        <column name="TS-Designation2">
          <data colspan="2" value="%%RES:CSHBAL-optionPremium%%"/>
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
      <row sort="4" name="variationMargin" font-size="8pt" >
        <column name="TS-Designation2">
          <data colspan="2" value="%%RES:CSHBAL-variationMargin%%"/>
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
      <row sort="5" name="cashSettlement" font-size="8pt" >
        <column name="TS-Designation2">
          <data colspan="2" value="%%RES:CSHBAL-cashSettlement%%"/>
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
      <row sort="6" name="feesVAT" font-size="8pt" >
        <column name="TS-Designation2">
          <data colspan="2" value="%%RES:CSHBAL-feesVAT%%"/>
        </column>
        <column name="TS-Filler"/>
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
      <row sort="7" name="returnCallDeposit" font-size="8pt" >
        <column name="TS-Designation1">
          <data colspan="3" value="%%RES:CSHBAL-returnCallDeposit%%"/>
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
      <row sort="8" name="todayCashBalance" font-size="8pt" font-weight="bold" background-color="{$gDetTrdTotalBackgroundColor}">
        <column name="TS-Designation1">
          <data colspan="3"
                value="%%RES:CSHBAL-todayCashBalance%%"
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
      <row sort="9" name="%%EMPTY%%" height="2mm" condition="IsFeesDetailsExist"/>
      <row sort="10" name="%%FREE%%" condition="IsFeesDetailsExist"
           font-size="7pt" font-weight="bold"
           background-color="{$gDetTrdHdrCellBackgroundColor}">
        <column number="01" name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data colspan="2" value="%%RES:CSHBAL-feesConstituents%%"/>
        </column>
        <column number="03" name="%%EMPTY%%">
          <data colspan="5" border-left-style="solid"/>
        </column>
      </row>
      <row sort="11" name="feeGrossAmount" dataLink="%%DATA:keyForSum%%" font-size="7pt">
        <column name="%%EMPTY%%"/>
        <column name="TS-Designation2">
          <data rowspan="%%DATA:rowspan%%" value="%%DATA:keyForSum.RES%%" border-right-style="none" />
        </column>
        <column name="TS-Designation3">
          <data value="%%RES:CSHBAL-feeGrossAmount%%" text-align="right" border-left-style="none" />
        </column>
        <column name="TS-Filler"/>
        <column name="TS-Desc"/>
        <column name="TS-ExCurrency"/>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%"/>
        </column>
        <column name="TS-ExCrDr">
          <data value="%%DATA:crDr%%"/>
        </column>
        <row sort="12" name="feeTax" dataLink="%%DATA:keyForSum%%" font-size="7pt">
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
          <column name="TS-Filler"/>
          <column name="TS-Desc"/>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%"/>
          </column>
        </row>
      </row>
      <row sort="13" name="totalFeesVAT" font-size="7pt" font-weight="bold" background-color="{$gDetTrdHdrCellBackgroundColor}">
        <column name="%%EMPTY%%" background-color="white"/>
        <column name="TS-Designation2">
          <data colspan="2"
                value="%%RES:CSHBAL-totalFeesVAT%%"
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
  <!-- ===== CreateSpecificGroups ===== -->
  <xsl:template name="CreateSpecificGroups"/>

  <!-- ================================================== -->
  <!--        Display Template                            -->
  <!-- ================================================== -->
  <!-- DisplaySummaryPageBody template -->
  <xsl:template name="DisplaySummaryPageBody"/>

  <!-- ===== Mapping between columns to display and Data to display for each column ===== -->
  <!--GetColumnDataToDisplay-->
  <xsl:template name="GetColumnDataToDisplay">
    <xsl:param name="pRowData" />

    <xsl:variable name="vColumnName" select="@name"/>
    <xsl:variable name="vCurrency" select="$pRowData/@currency" />

    <xsl:choose>
      <!-- ===== Currency ===== -->
      <xsl:when test="$vColumnName='TS-Currency'">
        <xsl:variable name="vCurrencyDescription" select="$pRowData/@cuDesc" />

        <xsl:choose>
          <xsl:when test="$vCurrencyDescription != ''">
            <xsl:value-of select="$vCurrencyDescription" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$vCurrency" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FxRate ===== -->
      <xsl:when test="$vColumnName='TS-FxRate'">
        <xsl:variable name="vFxRate">
          <xsl:if test="$pRowData/@fxRate">
            <xsl:value-of select="$pRowData/@fxRate"/>
          </xsl:if>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="string-length($vFxRate) = 0 or $vCurrency = $gReportingCurrency">
            <xsl:value-of select="''"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-FxRateTitle'" />
            </xsl:call-template>
            <xsl:value-of select="concat($gcEspace,$gReportingCurrency,'/',$vCurrency,':',$gcEspace)" />
            <xsl:choose>
              <xsl:when test="number($vFxRate) = number(-1)">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="number($vFxRate)" />
                  <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--GetColumnIsAltData-->
  <xsl:template name="GetColumnIsAltData"/>
  <!-- GetSpecificDataFontWeight -->
  <xsl:template name="GetSpecificDataFontWeight"/>

  <!--GetSpecificColumnDynamicData-->
  <!--<xsl:template name="GetSpecificColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:value-of select="$pValue"/>
  </xsl:template>-->

</xsl:stylesheet>
