<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!--  
================================================================================================================
Summary : Spheres report - Account Statement report 
          XSL for generation PDF document of Account Statement report 
          
File    : AccountStatementReport.xsl
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking
================================================================================================================
Version : v3.7.0.1 (Spheres 3.7.0.0)
Date    : 20131016
Author  : RD
Comment : [18585] Bug sur l'extrait de compte
================================================================================================================
Version : v3.7.0.0 (Spheres 3.7.0.0)
Date    : 20131015
Author  : RD
Comment : [19067] Daily FxRate  
================================================================================================================
Version : v3.0.0.0 (Spheres 3.0.0.x)
Date    : 20121115
Author  : RD
Comment : fix the bug on stream date
================================================================================================================
Version : v2.0.0.2 (Spheres 2.6.7.x)
Date    : 20121002
Author  : FI
Comment : minor correction
================================================================================================================
Version : v2.0.0.1 (Spheres 2.6.7.x)
Date    : 20121001
Author  : RD
Comment : fix the bug on "sort" element on which we could not put a value more than 9
================================================================================================================
Version : v2.0.0.0 (Spheres 2.6.7.x)
Date    : 20120919
Author  : FI
Comment : new version
================================================================================================================
Version : v1.0.0.0 (Spheres 2.6.4.x)
Date    : 20120830
Author  : MF
Comment : Account statement, that is a financial reports (including derivative contract details) collection, 
          starting from a given clearing date till an ending clearing date. The report amounts will be displayed 
          by currency groups and ordered by clearing date.
================================================================================================================
  -->

  <!--Special sections-->
  <xsl:variable name="gIsReportWithDCDetails" select="true()"/>
  <xsl:variable name="gIsReportWithCSSDetails" select="true()"/>

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
  <xsl:variable name="gDataDocumentTrades" select="//dataDocument"/>
  <xsl:variable name="gReportType" select="$gcReportTypeACCSTAT"/>

  <!--Legend-->
  <xsl:variable name="gIsReportWithSpecificLegend" select="false()"/>
  <xsl:variable name="gIsReportWithOrderTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithTradeTypeLegend" select="false()"/>
  <xsl:variable name="gIsReportWithPriceTypeLegend" select="false()"/>

  <!-- ================================================== -->
  <!--        Global Variables : Display                  -->
  <!-- ================================================== -->

  <!-- Report model, containing the columns/rows definition -->
  <xsl:variable name="gGroupDisplaySettings">
    <group sort="1" name="CBStream" isDataGroup="{true()}" padding-top="{$gGroupPaddingTop}" withNewPage="{true()}">

      <!-- Title of the account stamente for a specific currency : Currency + FXRate -->
      <title empty-row="2mm" withColumnHdr="{false()}" withSubColumn="{false()}" font-weight="bold">
        <column number="01" name="TS-Currency" colspan="11">
          <data font-size="10pt"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>
        <!--<column number="02" name="TS-FxRate" colspan="5">
          <data text-align="right" display-align="center" font-size="7pt"
                border-bottom-style="none"
                border-top-style="none"
                border-left-style="none"
                border-right-style="none"/>
        </column>-->
      </title>

      <!-- Columns - the structure of the table displaying the amounts for a specific currency -->

      <!-- 
      Designation title / 
      Previous Cash Balance (first day account statement) / 
      Current clearing day / 
      Current Cash Balance (last day account statement) -->
      <column number="01" name="TS-Designation1" width="16mm">
        <header colspan="4" ressource="CSHBAL-Designation"
                font-size="8pt" font-weight="bold" text-align="left" border-right-style="none"/>
        <data display-align="center"/>
      </column>
      <!-- Premium / Variation Margin / Cash settlement / Fees VAT  -->
      <column number="02" name="TS-Designation2" width="29mm" >
        <header colspan="-1"/>
        <data display-align="center"/>
      </column>
      <!-- Market name / Total label -->
      <column number="03" name="TS-Designation3" width="8mm" >
        <header colspan="-1"/>
        <data display-align="center"/>
      </column>
      <!-- Derivative Contract  -->
      <column number="04" name="TS-Contract" width="51mm" >
        <header colspan="-1"/>
      </column>
      <!-- Date -->
      <column number="05" name="TS-ProvisionDate" width="18mm" >
        <header ressource="TS-ValueDate"
                text-align="center" font-size="8pt" font-weight="bold" border-right-style="solid"/>
        <data text-align="center"/>
      </column>
      <!-- Amount currency -->
      <column number="06" name="TS-Currency" width="8mm">
        <header colspan="-1"/>
        <data text-align="right" border-right-style="none" />
      </column>
      <!-- Amount  -->
      <column number="07" name="TS-Amount" width="26mm">
        <header ressource="%%RES:CSHBAL-AmountTitle%%"
                text-align="center" colspan="3" font-size="8pt" font-weight="bold" border-left-style="solid"/>
        <data text-align="right" border-left-style="none" border-right-style="none" decPattern="%%PAT:currency.PARENT%%"/>
      </column>
      <!-- Cr/Dr indicator -->
      <column number="08" name="TS-CrDr" width="3.5mm" font-size="{$gDetTrdDetFontSize}">
        <header colspan="-1"/>
        <data border-left-style="none" display-align="center"/>
      </column>
      <!-- FX Currency  -->
      <column number="09" name="TS-ExCurrency" width="8mm">
        <header colspan="1" ressource=" "/>
        <data text-align="right" border-right-style="none"/>
      </column>
      <!-- FX Amount  -->
      <column number="10" name="TS-ExAmount" width="26mm">
        <header ressource="%%RES:CSHBAL-CounterValueTitle%% {$gReportingCurrency}"
                colspan="2" font-size="8pt" font-weight="bold" border-left-style="none" text-align="right"/>
        <data text-align="right" border-left-style="none" border-right-style="none" decPattern="%%PAT:RepCurrency%%"/>
      </column>
      <!-- FX Cr/Dr indicator  -->
      <column number="11" name="TS-ExCrDr" width="3.5mm" font-size="{$gDetTrdDetFontSize}">
        <header colspan="-1"/>
        <data border-left-style="none" display-align="center"/>
      </column>

      <!-- Conditions - the condition statements are used to set the hide/show state of the "Total" lable for the related amount.
      
      No specification is provided for this functionality (part of the Spheres XSL API), some informations are provided below:
      1. the condition is active only for the "row" node having "name" attribute equals to the "condition" attribute of the "condition" node
      2. the condition is true just when the "condition" attribute on the "row" node is equals to the "name" attribute of the "condition" node
      3. the condition attribute value of the row node is evaluated by the business level (check out the template matching the dataDocument node)
      
      I will demand as soon as possible a specific documentation for this function as well as a refactoring of the names, 
      to avoid misunderstandings
      -->

      <condition name="show" condition="optionPremium"/>
      <condition name="show" condition="variationMargin"/>
      <condition name="show" condition="cashSettlement"/>
      <condition name="show" condition="feesVAT"/>

      <!-- Rows - each row defines a specific amount -->

      <row sort="1" name="cashDayBefore" font-size="7pt">
        <column name="TS-Designation1">
          <data colspan="4" value="%%RES:CSHBAL-cashDayBefore%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
        <column name="TS-ProvisionDate">
          <data value="%%DATA:cbDate%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold" font-size="6pt"/>
        </column>
        <column name="TS-Currency">
          <data value="%%DATA:currency.PARENT%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
        <column name="TS-Amount">
          <data value="%%DATA:amount%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
        <column name="TS-CrDr">
          <data value="%%DATA:crDr%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
        <column name="TS-ExCurrency">
          <data value="{$gReportingCurrency}" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
        <column name="TS-ExAmount">
          <data value="%%DATA:exAmount%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
        <column name="TS-ExCrDr">
          <data value="%%DATA:crDr%%" border-bottom-style="solid" border-bottom-width="medium" font-weight="bold"/>
        </column>
      </row>

      <row sort="2" name="clearingDay" dataLink="%%DATA:keyForSum%%" font-size="6pt">
        <column name="TS-Designation1">
          <!--<data rowspan="%%DATA:rowspan%%"  value="%%RES:TS-DayOf%%" colspan="1"
                border-bottom-style="solid" border-bottom-width="medium">
            <inline>
              <xsl:value-of select="'%%DATA:cbDate%%'"/>
            </inline>
          </data>-->
          <data rowspan="%%DATA:rowspan%%"  value="%%DATA:cbDate%%" colspan="1"
                border-bottom-style="solid" border-bottom-width="medium" text-align="center">
          </data>
        </column>
        <row sort="3" name="cashMouvements" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <!-- Rustine DEMO RATP-->
            <data colspan="3" value="%%RES:CSHBAL-cashMouvements%%"/>
            <!--<data colspan="3" value="%%DATA:crDrResource.RES%%"/>-->     
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency">
          </column>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-weight="bold" />
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
          <column name="TS-ExCurrency">
          </column>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold" />
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
        </row>
        <row sort="4" name="optionPremium-detail" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data rowspan="%%DATA:rowspanAmount%%" value="%%RES:CSHBAL-optionPremium%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data rowspan="%%DATA:rowspanMarketCss%%" value="%%DATA:nameMarket%%" hide="%%DATA:hideMarketName%%" font-size="6pt"/>
          </column>
          <column name="TS-Contract">
            <data value="%%DATA:nameDc%%" font-size="6pt"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-size="6pt"/>
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-size="{$gDetTrdDetFontSize}"/>
          </column>
          <column name="TS-ExCurrency" font-size="6pt"/>
          <column name="TS-ExAmount">
            <!--<data value="%%DATA:exAmount%%" font-size="6pt"/>-->
          </column>
          <column name="TS-ExCrDr"/>
        </row>
        <row sort="5" name="optionPremium" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-optionPremium%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data colspan="2" value="%%RES:CSHBAL-Total%%" font-weight="bold" condition="%%DATA:showTotal%%"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-weight="bold" />
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"  />
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
        </row>

        <row sort="6" name="variationMargin-detail" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data rowspan="%%DATA:rowspanAmount%%" value="%%RES:CSHBAL-variationMargin%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data rowspan="%%DATA:rowspanMarketCss%%" value="%%DATA:nameMarket%%" hide="%%DATA:hideMarketName%%" font-size="6pt"/>
          </column>
          <column name="TS-Contract">
            <data value="%%DATA:nameDc%%" font-size="6pt"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-size="6pt"/>
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-size="{$gDetTrdDetFontSize}"/>
          </column>
          <column name="TS-ExCurrency" font-size="6pt"/>
          <column name="TS-ExAmount">
            <!--<data value="%%DATA:exAmount%%" font-size="6pt"/>-->
          </column>
          <column name="TS-ExCrDr"/>
        </row>
        <row sort="7" name="variationMargin" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-variationMargin%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data colspan="2" value="%%RES:CSHBAL-Total%%" font-weight="bold" condition="%%DATA:showTotal%%"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-weight="bold" />
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"  />
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
        </row>
        <row sort="8" name="cashSettlement-detail" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data rowspan="%%DATA:rowspanAmount%%" value="%%RES:CSHBAL-cashSettlement%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data rowspan="%%DATA:rowspanMarketCss%%" value="%%DATA:nameMarket%%" hide="%%DATA:hideMarketName%%" font-size="6pt"/>
          </column>
          <column name="TS-Contract">
            <data value="%%DATA:nameDc%%" font-size="6pt"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-size="6pt"/>
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-size="{$gDetTrdDetFontSize}"/>
          </column>
          <column name="TS-ExCurrency" font-size="6pt"/>
          <column name="TS-ExAmount">
            <!--<data value="%%DATA:exAmount%%" font-size="6pt"/>-->
          </column>
          <column name="TS-ExCrDr"/>
        </row>
        <row sort="9" name="cashSettlement" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-cashSettlement%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data colspan="2" value="%%RES:CSHBAL-Total%%" font-weight="bold" condition="%%DATA:showTotal%%"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-weight="bold" />
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"  />
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
        </row>
        <row sort="10" name="feesVAT-detail" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data rowspan="%%DATA:rowspanAmount%%" value="%%RES:CSHBAL-feesVAT%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data rowspan="%%DATA:rowspanMarketCss%%" value="%%DATA:nameMarket%%" hide="%%DATA:hideMarketName%%" font-size="6pt"/>
          </column>
          <column name="TS-Contract">
            <data value="%%DATA:nameDc%%" font-size="6pt"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-size="6pt"/>
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-size="{$gDetTrdDetFontSize}"/>
          </column>
          <column name="TS-ExCurrency" font-size="6pt"/>
          <column name="TS-ExAmount">
            <!--<data value="%%DATA:exAmount%%" font-size="6pt"/>-->
          </column>
          <column name="TS-ExCrDr"/>
        </row>
        <row sort="11" name="feesVAT" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data value="%%RES:CSHBAL-feesVAT%%" hide="%%DATA:hideAmountName%%"/>
          </column>
          <column name="TS-Designation3">
            <data colspan="2" value="%%RES:CSHBAL-Total%%" font-weight="bold" condition="%%DATA:showTotal%%"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" font-size="6pt"/>
          </column>
          <column name="TS-Currency"/>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-weight="bold" />
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
          <column name="TS-ExCurrency"/>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold"  />
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" />
          </column>
        </row>
        <row sort="12" name="returnCallDeposit" dataLink="%%DATA:keyForSum%%" font-size="7pt" >
          <column name="TS-Designation2">
            <data colspan="3" value="%%RES:CSHBAL-returnCallDeposit%%" border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
          <column name="TS-ProvisionDate">
            <data value="%%DATA:cbDate%%" border-bottom-style="solid" border-bottom-width="medium" font-size="6pt"/>
          </column>
          <column name="TS-Currency">
            <data border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" font-weight="bold" border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
          <column name="TS-ExCurrency">
            <data border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" font-weight="bold" border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" font-weight="bold" border-bottom-style="solid" border-bottom-width="medium"/>
          </column>
        </row>
        <row sort="13" name="todayCashBalance" dataLink="%%DATA:keyForSum%%"
           font-size="7pt" font-weight="bold" background-color="{$gDetTrdTotalBackgroundColor}">
          <column name="TS-Designation1">
            <data colspan="3"
                  value="%%RES:CSHBAL-cashBalanceAt%% "
                  border-right-style="none" border-bottom-style="solid" border-top-style="solid" border-top-width="medium">
              <inline>
                <xsl:value-of select="'%%DATA:cbDate%%'"/>
              </inline>
            </data>
          </column>
          <column name="TS-Contract">
            <data  colspan="2"
                   value="%%DATA:fxRate%%"
                   text-align="left" display-align="center"
                   border-left-style="none" border-bottom-style="solid" border-top-style="solid" border-top-width="medium">
              <inline font-size="6pt" font-weight="italic">
                <xsl:value-of select="'%%DATA:fxRateLastRes%%'"/>
              </inline>
            </data>
          </column>
          <column name="TS-Currency">
            <data value="%%DATA:currency.PARENT%%" border-bottom-style="solid" border-top-style="solid" border-top-width="medium"/>
          </column>
          <column name="TS-Amount">
            <data value="%%DATA:amount%%" border-bottom-style="solid" border-top-style="solid" border-top-width="medium"/>
          </column>
          <column name="TS-CrDr">
            <data value="%%DATA:crDr%%" border-bottom-style="solid" border-top-style="solid" border-top-width="medium"/>
          </column>
          <column name="TS-ExCurrency">
            <data value="{$gReportingCurrency}" border-bottom-style="solid" border-top-style="solid" border-top-width="medium"/>
          </column>
          <column name="TS-ExAmount">
            <data value="%%DATA:exAmount%%" border-bottom-style="solid" border-top-style="solid" border-top-width="medium"/>
          </column>
          <column name="TS-ExCrDr">
            <data value="%%DATA:crDr%%" border-bottom-style="solid" border-top-style="solid" border-top-width="medium"/>
          </column>
        </row>
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

  <!--  -->
  <xsl:key name="keyStreamCurrency" match="trade/cashBalance/cashBalanceStream/currency" use="text()"/>

  <!-- Template where we build the account statement by currency, any  account statement element will be named as "trade" (in order to be
  compliant with the current XSL API, and it will contain all the financial reports amount for the given datetime period -->
  <xsl:template match="dataDocument" mode="business">

    <!-- adding derivative contracts datas -->
    <xsl:variable name="vDerivativeContractsRepository">
      <xsl:copy-of select="$gDataDocumentRepository/derivativeContract"/>
    </xsl:variable>

    <!-- the market repository has been added (20120803) by FDA into the cash balance XML. 
         This is a DONT: we did not want the market repository to be added into the XML because of volumetry.
         An alternative is to pass directly the market name in the DC details.
     -->
    <xsl:variable name="vMarketRepository">
      <xsl:copy-of select="$gDataDocumentRepository/market"/>
    </xsl:variable>

    <!-- adding actor party datas -->
    <xsl:variable name="vPartyRepository">
      <xsl:copy-of select="party"/>
    </xsl:variable>

    <!-- taking the book identifier reference for the current account statement, the report is mono-book and it does not exist a 
    consolodated version, then we may take out the book reference of the first trade as global book reference -->
    <xsl:variable name="vBook" select="string(trade[1]/tradeHeader/partyTradeIdentifier/bookId[@bookIdScheme='http://www.euro-finance-systems.fr/otcml/bookid' and string-length(text())>0]/text())"/>
    <xsl:variable name="vIDB" select="number($gDataDocumentRepository/book[identifier=$vBook]/@OTCmlId)"/>

    <!-- reporting currency infos -->
    <xsl:variable name="vPrefixedReportingCurrency" select="concat($gPrefixCurrencyId, $gReportingCurrency)"/>
    <xsl:variable name="vRoundDirReportingCurrency" select="$gDataDocumentRepository/currency[@id=$vPrefixedReportingCurrency]/rounddir"/>
    <xsl:variable name="vRoundPrecReportingCurrency" select="$gDataDocumentRepository/currency[@id=$vPrefixedReportingCurrency]/roundprec"/>

    <!-- taking the entity identifier reference for the current account statement, the report is mono-book and it does not exist a 
    consolodated version, then we may take out the entity reference of the first trade as global entity reference -->
    <xsl:variable name="vEntityHref" select="string(trade[1]/cashBalance/entityPartyReference/@href)"/>

    <!-- select all the trades cash balance -->
    <xsl:variable name="vTrades" select="trade"/>

    <!-- taking all the currencies inside of the cash balances streams -->
    <xsl:variable name="vAllCurrencies"
                  select="$vTrades/cashBalance/cashBalanceStream/currency[generate-id()=generate-id(key('keyStreamCurrency',text()))]"/>

    <!-- Building the trade identifier reference, a trade element identifies the whole account statement -->
    <xsl:variable name="vTradeId" select="$gcReportTypeACCSTAT"/>

    <!-- building the "trade" element, each trade element includes the amounts 
      in regards to the wole account statement period -->
    <trade href="{$vTradeId}"
           repCuRoundDir="{$vRoundDirReportingCurrency}" repCuRoundPrec="{$vRoundPrecReportingCurrency}">

      <!--Book-->
      <BOOK data="{$vBook}"/>

      <!-- for each currency we build a cbStream element. A "cbStream" element identifies a specific currency account statement  -->
      <xsl:for-each select="$vAllCurrencies">
        <xsl:sort select="text()"/>

        <!-- current currency infos -->
        <xsl:variable name="vCurrency" select="text()"/>
        <xsl:variable name="vPrefixedCurrencyId" select="concat($gPrefixCurrencyId, $vCurrency)"/>
        <xsl:variable name="vCurrencyDescription" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]/description"/>
        <xsl:variable name="vRoundDir" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]/rounddir"/>
        <xsl:variable name="vRoundPrec" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]/roundprec"/>
        <!--<xsl:variable name="vCurrencyFxRate">
          <xsl:call-template name="FxRate">
            <xsl:with-param name="pFlowCurrency" select="$vCurrency" />
            <xsl:with-param name="pExCurrency" select="$gReportingCurrency" />
          </xsl:call-template>
        </xsl:variable>-->

        <!-- cash balance currency stream of the whole period of the statement -->
        <cbStream currency="{$vCurrency}" cuDesc="{$vCurrencyDescription}"
                  cuRoundDir="{$vRoundDir}" cuRoundPrec="{$vRoundPrec}">
          <!--fxRate="{$vCurrencyFxRate}">-->

          <xsl:variable name="vCurrencyTrades" select="$vTrades[cashBalance/cashBalanceStream/currency[text()=$vCurrency]]"/>

          <xsl:for-each select="$vCurrencyTrades">
            <xsl:sort select="tradeHeader/tradeDate/text()" data-type="text" order="ascending"/>

            <xsl:variable name="vStreamPosition" select="position()"/>
            <xsl:variable name="vCurrencyStream" select="cashBalance/cashBalanceStream[currency[text()=$vCurrency]]"/>

            <xsl:variable name="vCurrencyDailyFxRate">
              <xsl:call-template name="FxRate">
                <xsl:with-param name="pFlowCurrency" select="$vCurrency" />
                <xsl:with-param name="pExCurrency" select="$gReportingCurrency" />
                <xsl:with-param name="pFixingDate" select="tradeHeader/tradeDate/text()" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCurrencyLastFxRate">
              <xsl:if test="$vCurrencyDailyFxRate = number(-1)">
                <xsl:call-template name="FxRate">
                  <xsl:with-param name="pFlowCurrency" select="$vCurrency" />
                  <xsl:with-param name="pExCurrency" select="$gReportingCurrency" />
                  <xsl:with-param name="pFixingDate" select="tradeHeader/tradeDate/text()" />
                  <xsl:with-param name="pIsLastFxRate" select="true()" />
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>

            <xsl:variable name="vCurrencyFxRate">
              <xsl:choose>
                <xsl:when test="$vCurrencyDailyFxRate != number(-1)">
                  <xsl:value-of select="$vCurrencyDailyFxRate"/>
                </xsl:when>
                <xsl:when test="$vCurrencyLastFxRate != number(-1)">
                  <xsl:value-of select="$vCurrencyLastFxRate"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="number(-1)"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:if test="number($vStreamPosition) = 1">

              <!-- 1. Amount Before - Solde Espèces Veille -->
              <xsl:variable name="vAmountBeforeCashBalance" select="number($vCurrencyStream/cashAvailable/constituent/previousCashBalance/amount/amount/text())"/>
              <xsl:variable name="vCrDrAmountBeforeCashBalance">
                <xsl:call-template name="AmountSufix">
                  <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                  <xsl:with-param name="pAmount" select="$vAmountBeforeCashBalance" />
                  <xsl:with-param name="pPayerHref" select="$vCurrencyStream/cashAvailable/constituent/previousCashBalance/payerPartyReference/@href" />
                  <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/cashAvailable/constituent/previousCashBalance/receiverPartyReference/@href" />
                </xsl:call-template>
              </xsl:variable>

              <xsl:variable name="vExAmountBeforeCashBalance">
                <xsl:call-template name="ExchangeAmount">
                  <xsl:with-param name="pAmount" select="$vAmountBeforeCashBalance" />
                  <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
                </xsl:call-template>
              </xsl:variable>

              <amount name="cashDayBefore"
                      amount="{$vAmountBeforeCashBalance}"
                      crDr="{$vCrDrAmountBeforeCashBalance}"
                      exAmount="{$vExAmountBeforeCashBalance}"
                      cbDate="" />
            </xsl:if>

            <!-- the key needed to link the amounts of a given clearing date, this value will be used into the dataLink attribute of the 
            following amounts-->
            <xsl:variable name="vKey" select="string(tradeHeader/tradeDate)"/>

            <!-- retrieving the stream date form the right trade reference (we use the stream physical position vStreamPosition, 
            because we have previously got the cash balance trades sorted by date)  -->
            <xsl:variable name="vDate"
                          select="string(tradeHeader/tradeDate)" />

            <xsl:variable name="vAbbreviatedDate">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($vDate , 1, 4)" />
                <xsl:with-param name="month" select="substring($vDate , 6, 2)" />
                <xsl:with-param name="day" select="substring($vDate , 9, 2)" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExtentDate">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$vDate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vTimeStamp" select="substring(tradeHeader/tradeDate/@timeStamp,1,5)"/>

            <!-- Compute the row span for all the amounts of the current clearing date -->
            <xsl:variable name="vNodeSetFees">
              <xsl:copy-of select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent//fee//detail"/>
            </xsl:variable>

            <!--RD 20141119 [18585] Use new Template GetRowSpanCashFlowConstituent-->
            <!--<xsl:variable name="vRowSpanCashFlowConstituent">
              <xsl:value-of select="
                 count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/detail) +  
                 count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) +
                 count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) +
                 count(msxsl:node-set($vNodeSetFees)//detail[count(. | key('amounts-by-marketdc', concat(@Exch, @Sym))[1]) = 1])
                 + 7"/>
            </xsl:variable>-->
            <xsl:variable name="vRowSpanCashFlowConstituent">
              <xsl:call-template name="GetRowSpanCashFlowConstituent">
                <xsl:with-param name="pStream" select="$vCurrencyStream"/>
                <xsl:with-param name="pNodeSetFees" select="$vNodeSetFees"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- Fake empty amount - It will fill the Clearing day column       -->

            <xsl:if test="number($vRowSpanCashFlowConstituent) > 0">
            <amount name="clearingDay" crDr="" amount="" exAmount="" cbDate="{$vAbbreviatedDate}"
                    rowspan="{$vRowSpanCashFlowConstituent}" keyForSum="{$vKey}"/>
            </xsl:if>
            <xsl:for-each select="$vCurrencyStream/cashAvailable/constituent/cashBalancePayment">

              <xsl:variable name="vCurrentCashMouvement" select="current()"/>

            <!-- 2. Cash Mouvements - Mouvement Espèces       -->
            <xsl:variable name="vAmountCashMouvements" select="number($vCurrentCashMouvement/amount/amount/text())"/>

            <!-- Le sens (Débit/Crédit) est inversé sur l'état, car:
            - Sur le trade CashBalance: 
            * Le Versement  est toujours Payé par le Client à l’Entité et Reçu par le Clearer  de la part de l’Entité.
            * Le  Retrait est  toujours Reçu par le Client de la part l’Entité et Payé par le Clearer à l’Entité.
            - Sur le report: 
            * Pour un Client: le Versement est au Crédit et le Retrait est au Débit.
            * Pour un Clearer: le Versement est au Crédit et le Retrait est au Débit.-->
            <xsl:variable name="vCrDrAmountCashMouvements">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountCashMouvements" />
                <xsl:with-param name="pPayerHref" select="$vCurrentCashMouvement/receiverPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrentCashMouvement/payerPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountCashMouvements">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountCashMouvements" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

              <xsl:if test="number($vAmountCashMouvements) > 0">

                <!-- Rustine DEMO RATP-->
                <!--<xsl:variable name="vCrDrResource">
                <xsl:choose>
                  <xsl:when test="$vCrDrAmountCashMouvements=$gcDebit">
                    <xsl:value-of select="'Décaissement'"/>
                  </xsl:when>
                  <xsl:when test="$vCrDrAmountCashMouvements=$gcCredit">
                    <xsl:value-of select="'Encaissement'"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>-->

                <!--<amount name="cashMouvements"
                    amount="{$vAmountCashMouvements}"
                    crDr="{$vCrDrAmountCashMouvements}" crDrResource="{$vCrDrResource}"
                    exAmount="{$vExAmountCashMouvements}"
                    cbDate="{$vAbbreviatedDate}"
                    keyForSum="{$vKey}"/>-->
                
            <amount name="cashMouvements"
                    amount="{$vAmountCashMouvements}"
                    crDr="{$vCrDrAmountCashMouvements}"
                    exAmount="{$vExAmountCashMouvements}"
                    cbDate=""
                    keyForSum="{$vKey}"/>
              </xsl:if>
            </xsl:for-each>

            <!-- Compute the row span for all the amounts providing details:
            premium + variation + cash settlement + fees  -->

            <xsl:variable name="vSecondRowSpanCashFlowConstituent">
              <xsl:value-of select="
                 count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/detail) +  
                 count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) +
                 count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) +
                 count(msxsl:node-set($vNodeSetFees)//detail[count(. | key('amounts-by-marketdc', concat(@Exch, @Sym))[1]) = 1])
                 + 4"/>
            </xsl:variable>

            <!-- 3. Premium - Prime -->

            <xsl:variable name="vFullPaymentDateOptionPremium" select="string($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/paymentDate/adjustedDate/text())" />
            <xsl:variable name="vPaymentDateOptionPremium">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($vFullPaymentDateOptionPremium , 1, 4)" />
                <xsl:with-param name="month" select="substring($vFullPaymentDateOptionPremium , 6, 2)" />
                <xsl:with-param name="day" select="substring($vFullPaymentDateOptionPremium , 9, 2)" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="$vSecondRowSpanCashFlowConstituent"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'optionPremium'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="0"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="false()"/>
              <xsl:with-param name="pKeyForSum" select="$vKey"/>
              <xsl:with-param name="cbDate" select="$vPaymentDateOptionPremium"/>
            </xsl:call-template>

            <xsl:variable name="vHideEmptyCellAndAmountNamePremium">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/detail) > 0">
                  <xsl:value-of select="'true'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'false'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vShowTotalPremium">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/detail) = 0">
                  <xsl:value-of select="'hide'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'show'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vAmountOptionPremium" select="number($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/paymentAmount/amount/text())"/>
            <xsl:variable name="vCrDrAmountOptionPremium">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountOptionPremium" />
                <xsl:with-param name="pPayerHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/premium/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExvAmountOptionPremium">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountOptionPremium" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:if test="number($vAmountOptionPremium) > 0">
            <amount name="optionPremium"
                    amount="{$vAmountOptionPremium}"
                    crDr="{$vCrDrAmountOptionPremium}"
                    exAmount="{$vExvAmountOptionPremium}"
                    rowspan="{$vSecondRowSpanCashFlowConstituent}"
                    hideEmptyCell="{$vHideEmptyCellAndAmountNamePremium}"
                    hideAmountName="{$vHideEmptyCellAndAmountNamePremium}"
                    showTotal="{$vShowTotalPremium}"
                    cbDate=""
                    keyForSum="{$vKey}"/>
            </xsl:if>

            <!-- 4. Variation margin - Appels de Marge      -->

            <xsl:variable name="vFullPaymentDateVariationMargin" select="string($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/paymentDate/adjustedDate/text())" />
            <xsl:variable name="vPaymentDateVariationMargin">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($vFullPaymentDateVariationMargin , 1, 4)" />
                <xsl:with-param name="month" select="substring($vFullPaymentDateVariationMargin , 6, 2)" />
                <xsl:with-param name="day" select="substring($vFullPaymentDateVariationMargin , 9, 2)" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="1"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'variationMargin'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="0"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="true()"/>
              <xsl:with-param name="pKeyForSum" select="$vKey"/>
              <xsl:with-param name="cbDate" select="$vPaymentDateVariationMargin"/>
            </xsl:call-template>

            <xsl:variable name="vHideAmountNameVariationMargin">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) > 0">
                  <xsl:value-of select="'true'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'false'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vShowTotalVariationMargin">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) = 0">
                  <xsl:value-of select="'hide'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'show'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vAmountVariationMargin"
                          select="number($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/paymentAmount/amount/text())"/>

            <xsl:variable name="vCrDrAmountVariationMargin">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountVariationMargin" />
                <xsl:with-param name="pPayerHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExvAmountVariationMargin">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountVariationMargin" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:if test="number($vAmountVariationMargin) > 0">
            <amount name="variationMargin"
                    amount="{$vAmountVariationMargin}"
                    crDr="{$vCrDrAmountVariationMargin}"
                    exAmount="{$vExvAmountVariationMargin}"
                    hideEmptyCell="'true'"
                    hideAmountName="{$vHideAmountNameVariationMargin}"
                    showTotal="{$vShowTotalVariationMargin}"
                    cbDate=""
                    keyForSum="{$vKey}"/>
            </xsl:if>

            <!-- 5. Cash Settlement     -->

            <xsl:variable name="vFullPaymentDateCashSettlement" select="string($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/paymentDate/adjustedDate/text())" />
            <xsl:variable name="vPaymentDateCashSettlement">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($vFullPaymentDateCashSettlement , 1, 4)" />
                <xsl:with-param name="month" select="substring($vFullPaymentDateCashSettlement , 6, 2)" />
                <xsl:with-param name="day" select="substring($vFullPaymentDateCashSettlement , 9, 2)" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="1"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'cashSettlement'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="0"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="true()"/>
              <xsl:with-param name="pKeyForSum" select="$vKey"/>
              <xsl:with-param name="cbDate" select="$vPaymentDateCashSettlement"/>
            </xsl:call-template>

            <xsl:variable name="vHideAmountNameCashSettlement">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) > 0">
                  <xsl:value-of select="'true'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'false'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vShowTotalCashSettlement">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) = 0">
                  <xsl:value-of select="'hide'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'show'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vAmountCashSettlement" select="number($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/paymentAmount/amount/text())"/>
            <xsl:variable name="vCrDrAmountCashSettlement">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountCashSettlement" />
                <xsl:with-param name="pPayerHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExvAmountCashSettlement">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountCashSettlement" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:if test="number($vAmountCashSettlement) > 0">
            <amount name="cashSettlement"
                    amount="{$vAmountCashSettlement}"
                    crDr="{$vCrDrAmountCashSettlement}"
                    exAmount="{$vExvAmountCashSettlement}"
                    hideEmptyCell="'true'"
                    hideAmountName="{$vHideAmountNameCashSettlement}"
                    showTotal="{$vShowTotalCashSettlement}"
                    cbDate=""
                    keyForSum="{$vKey}"/>
            </xsl:if>

            <!-- 6. Fees with Taxes - Frais TTC    -->

            <xsl:variable name="vFullPaymentDateFeesVat" select="string($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/adjustedPaymentDate/text())" />
            <xsl:variable name="vPaymentDateFeesVat">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($vFullPaymentDateFeesVat , 1, 4)" />
                <xsl:with-param name="month" select="substring($vFullPaymentDateFeesVat , 6, 2)" />
                <xsl:with-param name="day" select="substring($vFullPaymentDateFeesVat , 9, 2)" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="1"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'feesVAT'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="0"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="true()"/>
              <xsl:with-param name="pKeyForSum" select="$vKey"/>
              <xsl:with-param name="cbDate" select="$vPaymentDateFeesVat"/>
            </xsl:call-template>

            <xsl:variable name="vHideAmountNameFee">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail) > 0">
                  <xsl:value-of select="'true'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'false'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vShowTotalFeesVat">
              <xsl:choose>
                <xsl:when test="count($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail) = 0">
                  <xsl:value-of select="'hide'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'show'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vHTCFees">
              <xsl:choose>
                <xsl:when test="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail">
                  <xsl:value-of select="
                      sum($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail[@AmtSide = $gcCredit]/@Amt)
                      -
                      sum($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail[@AmtSide = $gcDebit]/@Amt)"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="0"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:variable name="vTaxAmount">
              <xsl:choose>
                <xsl:when test="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail/tax">
                  <xsl:value-of select="
                      sum($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail/tax[@AmtSide = $gcCredit]/@Amt) 
                      - 
                      sum($vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/detail/tax[@AmtSide = $gcDebit]/@Amt)"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="0"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vTTCAmount" select="number($vHTCFees + $vTaxAmount)"/>
            <xsl:variable name="vCrDr">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vTTCAmount" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAmountFeesVat">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vTTCAmount" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vCrDrAmountFeesVat">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountFeesVat" />
                <xsl:with-param name="pPayerHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/cashAvailable/constituent/cashFlows/constituent/fee/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vExvAmountFeesVat">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountFeesVat" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:if test="number($vAmountFeesVat) > 0">
            <amount name="feesVAT"
                    amount="{$vAmountFeesVat}"
                    crDr="{$vCrDrAmountFeesVat}"
                    exAmount="{$vExvAmountFeesVat}"
                    hideEmptyCell="'true'"
                    hideAmountName="{$vHideAmountNameFee}"
                    showTotal="{$vShowTotalFeesVat}"
                    cbDate=""
                    keyForSum="{$vKey}"/>
            </xsl:if>

            <!-- 7. Return Call Deposit - Appel/Restitution de Déposit -->

            <xsl:variable name="vAmountReturnCallDeposit" select="number($vCurrencyStream/marginCall/paymentAmount/amount/text())"/>
            <xsl:variable name="vCrDrAmountReturnCallDeposit">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountReturnCallDeposit" />
                <xsl:with-param name="pPayerHref" select="$vCurrencyStream/marginCall/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/marginCall/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountReturnCallDeposit">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountReturnCallDeposit" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vFullPaymentDateReturnCallDeposit" select="string($vCurrencyStream/marginCall/paymentDate/adjustedDate/text())" />
            <xsl:variable name="vPaymentDateReturnCallDeposit">
              <xsl:if test="$vFullPaymentDateReturnCallDeposit and number(0) != number($vAmountReturnCallDeposit)">
                <xsl:call-template name="format-shortdate_ddMMMyy">
                  <xsl:with-param name="year" select="substring($vFullPaymentDateReturnCallDeposit , 1, 4)" />
                  <xsl:with-param name="month" select="substring($vFullPaymentDateReturnCallDeposit , 6, 2)" />
                  <xsl:with-param name="day" select="substring($vFullPaymentDateReturnCallDeposit , 9, 2)" />
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>

            <xsl:if test="number($vAmountReturnCallDeposit) > 0">
            <amount name="returnCallDeposit"
                    amount="{$vAmountReturnCallDeposit}"
                    crDr="{$vCrDrAmountReturnCallDeposit}"
                    exAmount="{$vExAmountReturnCallDeposit}"
                    cbDate="{$vPaymentDateReturnCallDeposit}"
                    keyForSum="{$vKey}"/>
            </xsl:if>

            <!-- Today Cash Balance - Solde Espèces Jour  -->
            <xsl:variable name="vAmountTodayCashBalance" select="number($vCurrencyStream/cashBalance/amount/amount/text())"/>
            <xsl:variable name="vExAmountTodayCashBalance">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountTodayCashBalance" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vCrDrTodayCashBalance">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountTodayCashBalance" />
                <xsl:with-param name="pPayerHref" select="$vCurrencyStream/cashBalance/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="$vCurrencyStream/cashBalance/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vEndPeriodDate">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$gEndPeriodDate" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="todayCashBalance"
                    amount="{$vAmountTodayCashBalance}"
                    crDr="{$vCrDrTodayCashBalance}"
                    exAmount="{$vExAmountTodayCashBalance}"
                    cbDate="{$vExtentDate}"
                    keyForSum="{$vKey}">

              <xsl:choose>
                <xsl:when test="$vCurrencyDailyFxRate != number(-1)">
                  <xsl:attribute name="fxRate">
                    <xsl:value-of select="$vCurrencyFxRate"/>
                  </xsl:attribute>
                </xsl:when>
                <xsl:when test="$vCurrencyLastFxRate != number(-1)">
                  <xsl:attribute name="lastFxRate">
                    <xsl:value-of select="$vCurrencyFxRate"/>
                  </xsl:attribute>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="fxRate">
                    <xsl:value-of select="$vCurrencyFxRate"/>
                  </xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>

            </amount>

          </xsl:for-each>

        </cbStream>

      </xsl:for-each>

    </trade>

  </xsl:template>

  <!-- ===== GetRowSpanCashFlowConstituent ===== -->
  <xsl:template name="GetRowSpanCashFlowConstituent">
    <xsl:param name="pStream"/>
    <xsl:param name="pNodeSetFees"/>

    <!--<xsl:value-of select="
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/premium/detail) +  
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) +
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) +
                  count(msxsl:node-set($pNodeSetFees)//detail[count(. | key('amounts-by-marketdc', concat(@Exch, @Sym))[1]) = 1])
                  + 7"/>-->

    <xsl:variable name="vRowSpan" select="
                  count($pStream/cashAvailable/constituent/cashBalancePayment/amount/amount[number(text()) >0]) +  
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/premium/paymentAmount/amount[number(text()) >0][1]) +  
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/paymentAmount/amount[number(text()) >0][1]) +  
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/paymentAmount/amount[number(text()) >0][1]) +  
                  count($pStream/marginCall/paymentDate/adjustedDate[number(text()) >0][1]) +  
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/premium/detail) +  
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) +
                  count($pStream/cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) +
                  count(msxsl:node-set($pNodeSetFees)//detail[count(. | key('amounts-by-marketdc', concat(@Exch, @Sym))[1]) = 1])"/>

    <xsl:choose>
      <xsl:when test="msxsl:node-set($pNodeSetFees)//detail">
        <xsl:value-of select="$vRowSpan+2"/>
      </xsl:when>
      <xsl:when test="number($vRowSpan) >0">
        <xsl:value-of select="$vRowSpan+1"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$vRowSpan"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

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
          <xsl:choose>
            <xsl:when test="$pRowData/@fxRate and number($pRowData/@fxRate) != number(-1)">
              <xsl:value-of select="$pRowData/@fxRate"/>
            </xsl:when>
            <xsl:when test="$pRowData/@lastFxRate and number($pRowData/@lastFxRate) != number(-1)">
              <xsl:value-of select="$pRowData/@lastFxRate"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(-1)"/>
            </xsl:otherwise>
          </xsl:choose>
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
            <xsl:if test="number($vFxRate) != number(-1) and number($vFxRate) = number($pRowData/@lastFxRate)">
              <fo:inline font-size="6pt" font-weight="italic">
                <xsl:text> (</xsl:text>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-FxRateLast'" />
                </xsl:call-template>
                <xsl:text>)</xsl:text>
              </fo:inline>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!--GetColumnIsAltData-->
  <xsl:template name="GetColumnIsAltData"/>
  <!-- GetSpecificDataFontWeight -->
  <xsl:template name="GetSpecificDataFontWeight"/>

</xsl:stylesheet>
