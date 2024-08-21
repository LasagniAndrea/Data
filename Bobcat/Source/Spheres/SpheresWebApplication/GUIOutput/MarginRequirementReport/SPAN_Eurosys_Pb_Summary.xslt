<?xml version="1.0" encoding="utf-8" ?>

<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:efs  ="http://www.efs.org/2007/EFSmL-3-0"
                xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
                xmlns:fpml ="http://www.fpml.org/2007/FpML-4-4"
                >

  <xsl:output method="xml" encoding="UTF-8" />

  <!-- report culture (in use by dates, money, etc..)-->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />

  <xsl:variable name ="vIsA4VerticalReport" select ="false()"/>

  <!-- this variable defines the type of report -->
  <xsl:variable name ="vIsCustomReport" select ="false()"/>

  <!-- xslt includes -->
  <xsl:include href="SPAN_Eurosys_Common.xslt" />

  <!-- set the name of page sequence -->
  <xsl:variable name ="vPageSequenceMasterReference">
    <xsl:text>A4LandscapePage</xsl:text>
  </xsl:variable>

  <!-- Variables for header titles ====================================================== -->
  <xsl:variable name ="vLabelReportName">
    <xsl:value-of select ="$vLabelPerformanceBondSummaryReport"/>
  </xsl:variable>

  <!--! =================================================================================== -->
  <!--!                      TEMPLATES                                                      -->
  <!--! =================================================================================== -->

  <!-- *************************************************** -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">

      <xsl:call-template name ="setPagesCaracteristics"/>

      <fo:page-sequence master-reference="{$vPageSequenceMasterReference}" initial-page-number="1">

        <xsl:call-template name ="handleStaticContentHeader"/>

        <fo:static-content flow-name="A4LandscapeFooter">
          <xsl:call-template name="displayFooter"/>
        </fo:static-content>

        <fo:flow flow-name="A4LandscapeBody">

          <xsl:choose>
            <!-- PM 20160404 [22116] Gestion du cas de plusieurs méthodes dans le log -->
            <!--<xsl:when test ="$vMarginCalculationMethodName = $vSpanCME or $vMarginCalculationMethodName = $vLondonSpan or $vMarginCalculationMethodName = $vSpanC21">-->
            <xsl:when test ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vSpanCME] or //efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vLondonSpan] or //efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vSpanC21]">
              <fo:block linefeed-treatment="preserve" white-space-collapse="false"  white-space-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" break-before="page">
                <xsl:call-template name="displaySummaryBody"/>
              </fo:block>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="displayErrorMessage"/>
            </xsl:otherwise>
          </xsl:choose>

          <fo:block id="EndOfDoc" />

        </fo:flow>

      </fo:page-sequence>

    </fo:root>

  </xsl:template>

  <!-- =========================================================  -->
  <!-- Header section                                             -->
  <!-- =========================================================  -->
  <xsl:template name="displayHeader">

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create2Columns">
          <xsl:with-param name="pColumnWidth1" select ="8"/>
          <xsl:with-param name="pColumnWidth2" select ="12"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelPerformanceBondSummaryReport"/>
              <xsl:with-param name ="pFontSize" select ="'12pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <xsl:call-template name ="displayBreakline"/>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create2Columns">
          <xsl:with-param name="pColumnWidth1" select ="3"/>
          <xsl:with-param name="pColumnWidth2" select ="4"/>
        </xsl:call-template>
        <fo:table-body padding="{$vTablePadding}">
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelCompte"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vCompte"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>
          </fo:table-row>
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelTypeCompte"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vTypeCompte"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
        <fo:table-body padding="{$vTablePadding}">
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelTimeStamp"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vBusinessDateTime"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>
  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->
  <xsl:template name="displaySummaryBody">

    <xsl:for-each select ="//efs:span/efs:exchangecomplex/efs:groups/efs:group">
      <xsl:sort select="@name" />

      <xsl:call-template name ="displayBreakline"/>

      <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">

        <xsl:call-template name ="displayMarketBlock">
          <xsl:with-param name ="pNode" select ="current()"/>
        </xsl:call-template>

        <xsl:for-each select ="efs:combinedcommodities/efs:combinedcommodity">
          <xsl:sort select="@name" />

          <xsl:call-template name ="displayCombinedCommodityBlock">
            <xsl:with-param name ="pNode" select ="current()"/>
          </xsl:call-template>
          <xsl:call-template name ="displayBreakline"/>
        </xsl:for-each>

      </fo:block>

    </xsl:for-each>

  </xsl:template>


  <xsl:template name ="displayMarketBlock">
    <xsl:param name ="pNode" />
    <xsl:variable name ="vExchangeComplexAcronym">
      <xsl:call-template name ="getExchange">
        <xsl:with-param name ="pNode" select="ancestor::efs:exchangecomplex"/>
      </xsl:call-template>
    </xsl:variable>
    <fo:table table-layout="fixed" width="100%">
      <xsl:call-template name ="create1Column">
        <xsl:with-param name="pColumnWidth1" select ="3.8"/>
      </xsl:call-template>
      <fo:table-body>
        <fo:table-row height="{$vRowHeight}cm">
          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vExchangeComplexAcronym"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
            <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
          </xsl:call-template>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <xsl:template name ="displayCombinedCommodityBlock">
    <xsl:param name ="pNode" />

    <fo:table table-layout="fixed" width="100%" >

      <xsl:call-template name ="create14Columns">
        <xsl:with-param name ="pColumnWidth1" select="1.9"/>
        <xsl:with-param name ="pColumnWidth2" select="1.9"/>
        <xsl:with-param name ="pColumnWidth3" select="1.9"/>
        <xsl:with-param name ="pColumnWidth4" select="1.9"/>
        <xsl:with-param name ="pColumnWidth5" select="1.9"/>
        <xsl:with-param name ="pColumnWidth6" select="1.9"/>
        <xsl:with-param name ="pColumnWidth7" select="1.9"/>
        <xsl:with-param name ="pColumnWidth8" select="1.9"/>
        <xsl:with-param name ="pColumnWidth9" select="1.9"/>
        <xsl:with-param name ="pColumnWidth10" select="1.9"/>
        <xsl:with-param name ="pColumnWidth11" select="1.9"/>
        <xsl:with-param name ="pColumnWidth12" select="1.9"/>
        <xsl:with-param name ="pColumnWidth13" select="1.9"/>
        <xsl:with-param name ="pColumnWidth14" select="2.2"/>
      </xsl:call-template>
      <fo:table-body padding="{$vTablePadding}">

        <fo:table-row height="{$vRowHeight}cm">

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelProduit"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelGroupeSpan"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelScanRisk"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelIntermonth"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelDeliveryMonth"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelTimeRisk"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelVolatilyRisk"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelWeightedRisk"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelIntercomCdt"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelDeltaRemain"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelNetOptValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelRiskMaint"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelDepositMaint"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelExcLgOptValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">

          <xsl:variable name ="vProduitValue">
            <xsl:call-template name ="getCommodityCode">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vScanRiskValue">
            <xsl:call-template name ="getScanRisk">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vIntermonthValue">
            <xsl:call-template name ="getIntracommoditycharge">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vDeliveryMonthValue">
            <xsl:call-template name ="getDeliverycharge">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vTimeriskValue">
            <xsl:call-template name ="getTimerisk">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vVolatilyRiskValue">
            <xsl:call-template name ="getVolatilyrisk">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vWeightedRiskValue">
            <xsl:call-template name ="getWeightedrisk">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vIntercomCdtValue">
            <xsl:call-template name ="getIntercommoditycredit">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vDeltaRemainValue">
            <xsl:call-template name ="getDeltanetremaining">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vNetOptValueValue">
            <xsl:call-template name ="getNov">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vRiskMaintValue">
            <xsl:call-template name ="getMaintenancemargin">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vDepositMaintValue">
            <xsl:call-template name ="getDepositMaint">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vExcLgOptValueMaintValue">
            <xsl:call-template name ="getExcLgOptValueMaint">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vProduitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vProduitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vScanRiskValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vIntermonthValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vDeliveryMonthValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vTimeriskValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vVolatilyRiskValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vWeightedRiskValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vIntercomCdtValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vDeltaRemainValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vNetOptValueValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vRiskMaintValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vDepositMaintValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vExcLgOptValueMaintValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

        </fo:table-row>

      </fo:table-body>

      <fo:table-body padding="{$vTablePadding}">

        <fo:table-row height="{$vRowHeight}cm" >

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelCodeSpan"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
            <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelShortOptMin"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelRiskInitial"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelDepositInit"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelExcLgOptValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm" >

          <xsl:variable name ="vCodeSpanValue">
            <xsl:call-template name ="getCommodityCode">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vShortOptMinValue">
            <xsl:call-template name ="getSom">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vRiskInitialValue">
            <xsl:call-template name ="getInitialmargin">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vDepositInitValue">
            <xsl:call-template name ="getDepositInit">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vExcLgOptValueInitValue">
            <xsl:call-template name ="getExcLgOptValueInit">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vCodeSpanValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
            <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vShortOptMinValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vRiskInitialValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vDepositInitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vExcLgOptValueInitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

        </fo:table-row>

      </fo:table-body>

    </fo:table>

  </xsl:template>

</xsl:stylesheet>