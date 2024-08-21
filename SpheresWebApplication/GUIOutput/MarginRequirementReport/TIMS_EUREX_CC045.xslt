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
  <!-- when true(): generate a EUREX layout report  -->
  <!-- when false(): generate a Spheres layout report  -->
  <!--<xsl:variable name ="vIsCustomReport" select ="true()"/>-->
  <xsl:variable name ="vIsCustomReport" select ="false()"/>

  <!-- xslt includes -->
  <xsl:include href="EUREX_Common.xslt" />
  
  <!-- Margin Calculation Method Name -->
  <xsl:variable name ="vDescribedMethod" select ="'TIMS EUREX'"/>

  <!-- set the name of page sequence -->
  <!-- Landscape page for EUREX layout report  -->
  <!-- A4LandscapePage page for Spheres layout report  -->
  <xsl:variable name ="vPageSequenceMasterReference">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>A4LandscapePage</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>A4LandscapeFirstPageDifferent</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Variables for header titles ====================================================== -->
  <xsl:variable name ="vLabelReportName">
    <xsl:text>CC045   Additional Margin</xsl:text>
  </xsl:variable>

  <!--! =================================================================================== -->
  <!--!                      TEMPLATES                                                      -->
  <!--! =================================================================================== -->

  <!-- *************************************************** -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">

      <xsl:call-template name ="setPagesCaracteristics"/>

      <fo:page-sequence master-reference="{$vPageSequenceMasterReference}" initial-page-number="1">

        <xsl:call-template name ="handleStaticContentHeader">
          <xsl:with-param name ="pEurexMethod" select ="$vDescribedMethod"/>
        </xsl:call-template>

        <fo:static-content flow-name="A4LandscapeFooter">
          <xsl:call-template name="displayFooter"/>
        </fo:static-content>

        <fo:flow flow-name="A4LandscapeBody">

          <xsl:choose>
            <xsl:when test="$vIsGross='false'">
              <xsl:call-template name="displayNetMarginBody"/>
              <xsl:choose>
                <!-- PM 20151116 [21561] RBM with Prisma -->
                <!--<xsl:when test ="$vMarginCalculationMethodName = $vDescribedMethod">-->
                <xsl:when test ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name = $vDescribedMethod]">
                  <xsl:call-template name="displayNetMarginBody"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name ="displayErrorMessage"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="displayGrossMarginBody"/>
            </xsl:otherwise>
          </xsl:choose>

          <fo:block id="EndOfDoc" />
        </fo:flow>

      </fo:page-sequence>

    </fo:root>

  </xsl:template>

  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->

  <!-- *************************************************** -->
  <xsl:template name ="displayAdditionalMarginHeader">

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create9Columns">
          <xsl:with-param name ="pColumnWidth1" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth2" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth3" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth4" select ="2"/>
          <xsl:with-param name ="pColumnWidth5" select ="1"/>
          <xsl:with-param name ="pColumnWidth6" select ="4"/>
          <xsl:with-param name ="pColumnWidth7" select ="4"/>
          <xsl:with-param name ="pColumnWidth8" select ="2"/>
          <xsl:with-param name ="pColumnWidth9" select ="4"/>
        </xsl:call-template>

        <fo:table-body>
          <!-- titles -->
          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelMgnGrp"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelOfsFac"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelMgnCl"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExpiry"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelCur"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelWorstCaseScenario"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPureAdditionalMgn"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelMgnFct"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelAdditionalMgn"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="displayAdditionalMarginRow">
    <xsl:param name ="pClassNode" />
    <xsl:param name ="pGroupName" />

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create9Columns">
          <xsl:with-param name ="pColumnWidth1" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth2" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth3" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth4" select ="2"/>
          <xsl:with-param name ="pColumnWidth5" select ="1"/>
          <xsl:with-param name ="pColumnWidth6" select ="4"/>
          <xsl:with-param name ="pColumnWidth7" select ="4"/>
          <xsl:with-param name ="pColumnWidth8" select ="2"/>
          <xsl:with-param name ="pColumnWidth9" select ="4"/>
        </xsl:call-template>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">

            <!-- variables -->
            <xsl:variable name ="vOffsetFactor">
              <xsl:call-template name ="getOffsetFactor">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vClassName">
              <xsl:call-template name ="getClassName">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vExpiry">
              <xsl:call-template name ="getExpiry">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vMarginAmountCurrency">
              <xsl:call-template name ="getMarginCurrency">
                <xsl:with-param name ="pNode" select ="$pClassNode/efs:additional/efs:marginAmount"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vWorstCaseScenario">
              <xsl:call-template name ="getWorstCaseScenario">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vPureAdditionalMargin">
              <xsl:call-template name ="getPureAdditionalMargin">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vMarginFactor">
              <xsl:call-template name ="getMarginFactor">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vAdditionalMargin">
              <xsl:call-template name ="getAdditionalMargin">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- cell -->
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$pGroupName"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vOffsetFactor"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vClassName"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vExpiry"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vMarginAmountCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vWorstCaseScenario"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vPureAdditionalMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vMarginFactor"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vAdditionalMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <!--<xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>-->
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="displayAdditionalMarginScenarioHeader">
    <xsl:param name ="pClassNode" />
    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%" >

        <xsl:call-template name ="create8Columns">
          <xsl:with-param name ="pColumnWidth1" select ="1.9"/>
          <xsl:with-param name ="pColumnWidth2" select ="1"/>
          <xsl:with-param name ="pColumnWidth3" select ="4"/>
          <xsl:with-param name ="pColumnWidth4" select ="4"/>
          <xsl:with-param name ="pColumnWidth5" select ="4"/>
          <xsl:with-param name ="pColumnWidth6" select ="4"/>
          <xsl:with-param name ="pColumnWidth7" select ="4"/>
          <xsl:with-param name ="pColumnWidth8" select ="4"/>
        </xsl:call-template>

        <fo:table-body>

          <!-- titles -->
          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelScenario"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelCur"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPriceUpVolaUp"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPriceUpVolaNeut"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPriceUpVolaDown"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPriceDownVolaUp"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPriceDownVolaNeut"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPriceDownVolaDown"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="handleAdditionalMarginScenarioRow">
    <xsl:param name ="pClassNode" />
    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create8Columns">
          <xsl:with-param name ="pColumnWidth1" select ="1.9"/>
          <xsl:with-param name ="pColumnWidth2" select ="1"/>
          <xsl:with-param name ="pColumnWidth3" select ="4"/>
          <xsl:with-param name ="pColumnWidth4" select ="4"/>
          <xsl:with-param name ="pColumnWidth5" select ="4"/>
          <xsl:with-param name ="pColumnWidth6" select ="4"/>
          <xsl:with-param name ="pColumnWidth7" select ="4"/>
          <xsl:with-param name ="pColumnWidth8" select ="4"/>
        </xsl:call-template>

        <fo:table-body>

          <xsl:if test ="$pClassNode/efs:additional/efs:factors/efs:factor[@ID='Risk']">
            <xsl:call-template name ="displayAdditionalMarginScenarioRow">
              <xsl:with-param name ="pFactorNode" select ="$pClassNode/efs:additional/efs:factors/efs:factor[@ID='Risk']"/>
              <xsl:with-param name ="pScenario" select ="$vLabelRisk"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:if test ="$pClassNode/efs:additional/efs:factors/efs:factor[@ID='Risk']">
            <xsl:call-template name ="displayAdditionalMarginScenarioRow">
              <xsl:with-param name ="pFactorNode" select ="$pClassNode/efs:additional/efs:factors/efs:factor[@ID='RiskWithNeutralPricesAndOffset']"/>
              <xsl:with-param name ="pScenario" select ="$vLabelOffset"/>
            </xsl:call-template>
          </xsl:if>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name= "displayAdditionalMarginScenarioRow">
    <xsl:param name ="pFactorNode"/>
    <xsl:param name ="pScenario"/>
    <xsl:param name ="pTableCellFontWeight" select="'normal'"/>

    <fo:table-row height="{$vRowHeight}cm">

      <!-- variables  -->
      <xsl:variable name ="vSidePointCurrency">
        <xsl:call-template name ="getMarginCurrency">
          <xsl:with-param name ="pNode" select ="$pFactorNode/efs:riskarray/efs:sidepoint[1]/efs:marginAmount"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name ="vPUVU">
        <xsl:call-template name ="getSidepointMarginAmount">
          <xsl:with-param name="pFactorNode" select="$pFactorNode"/>
          <xsl:with-param name="pSidepointId" select ="'PUVU'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name ="vPUVN">
        <xsl:call-template name ="getSidepointMarginAmount">
          <xsl:with-param name="pFactorNode" select="$pFactorNode"/>
          <xsl:with-param name="pSidepointId" select ="'PUVN'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name ="vPUVD">
        <xsl:call-template name ="getSidepointMarginAmount">
          <xsl:with-param name="pFactorNode" select="$pFactorNode"/>
          <xsl:with-param name="pSidepointId" select ="'PUVD'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name ="vPDVU">
        <xsl:call-template name ="getSidepointMarginAmount">
          <xsl:with-param name="pFactorNode" select="$pFactorNode"/>
          <xsl:with-param name="pSidepointId" select ="'PDVU'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name ="vPDVN">
        <xsl:call-template name ="getSidepointMarginAmount">
          <xsl:with-param name="pFactorNode" select="$pFactorNode"/>
          <xsl:with-param name="pSidepointId" select ="'PDVN'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name ="vPDVD">
        <xsl:call-template name ="getSidepointMarginAmount">
          <xsl:with-param name="pFactorNode" select="$pFactorNode"/>
          <xsl:with-param name="pSidepointId" select ="'PDVD'"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- cell -->
      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$pScenario"/>
        <xsl:with-param name ="pTextAlign" select ="'left'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
        <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColorLight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vSidePointCurrency"/>
        <xsl:with-param name ="pTextAlign" select ="'center'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vPUVU"/>
        <xsl:with-param name ="pTextAlign" select ="'right'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vPUVN"/>
        <xsl:with-param name ="pTextAlign" select ="'right'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vPUVD"/>
        <xsl:with-param name ="pTextAlign" select ="'right'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vPDVU"/>
        <xsl:with-param name ="pTextAlign" select ="'right'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vPDVN"/>
        <xsl:with-param name ="pTextAlign" select ="'right'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTableCellBody">
        <xsl:with-param name ="pValue" select ="$vPDVD"/>
        <xsl:with-param name ="pTextAlign" select ="'right'"/>
        <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
        <xsl:with-param name ="pFontWeight" select ="$pTableCellFontWeight"/>
      </xsl:call-template>

    </fo:table-row>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="displayAdditionalMarginScenarioTotalRow">
    <xsl:param name ="pGroupNode"/>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%" >

        <xsl:call-template name ="create8Columns">
          <xsl:with-param name ="pColumnWidth1" select ="1.9"/>
          <xsl:with-param name ="pColumnWidth2" select ="1"/>
          <xsl:with-param name ="pColumnWidth3" select ="4"/>
          <xsl:with-param name ="pColumnWidth4" select ="4"/>
          <xsl:with-param name ="pColumnWidth5" select ="4"/>
          <xsl:with-param name ="pColumnWidth6" select ="4"/>
          <xsl:with-param name ="pColumnWidth7" select ="4"/>
          <xsl:with-param name ="pColumnWidth8" select ="4"/>
        </xsl:call-template>

        <fo:table-body>

          <xsl:call-template name ="displayAdditionalMarginScenarioRow">
            <xsl:with-param name ="pFactorNode" select ="$pGroupNode/efs:additionals/efs:additional/efs:factors/efs:factor[@ID='RiskWithNeutralPricesAndOffset']"/>
            <xsl:with-param name ="pScenario" select ="$vLabelGroupTot"/>
            <xsl:with-param name ="pTableCellFontWeight" select="$vTableCellFontWeight"/>
          </xsl:call-template>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayNetMarginBody">

    <xsl:for-each select ="efs:EfsML/efs:marginRequirementOffice/efs:marginAmounts/efs:Money">

      <xsl:variable name ="vMarginRequirementCurrency">
        <xsl:call-template name ="getMarginCurrency">
          <xsl:with-param name ="pNode" select ="current()"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:call-template name ="displayBodyAccountRow">
        <xsl:with-param name ="pCurrency" select ="$vMarginRequirementCurrency"/>
      </xsl:call-template>

      <xsl:for-each select ="//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:marginAmount[@curr=$vMarginRequirementCurrency]]">

        <xsl:variable name ="vGroupName">
          <xsl:call-template name ="getGroupName">
            <xsl:with-param name ="pGroupNode" select ="current()"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:for-each select="efs:classes/efs:class[efs:marginAmount[@curr=$vMarginRequirementCurrency]]">

          <fo:block linefeed-treatment="preserve"  keep-together="always" >

            <xsl:call-template name ="displayAdditionalMarginHeader"/>
            <xsl:call-template name ="displayAdditionalMarginRow">
              <xsl:with-param name ="pClassNode" select ="current()"/>
              <xsl:with-param name ="pGroupName" select ="$vGroupName"/>
            </xsl:call-template>

            <xsl:call-template name ="displayAdditionalMarginScenarioHeader"/>
            <xsl:call-template name ="handleAdditionalMarginScenarioRow">
              <xsl:with-param name ="pClassNode" select ="current()"/>
            </xsl:call-template>

            <xsl:if test ="position()= last()">
              <xsl:call-template name ="displayAdditionalMarginScenarioTotalRow">
                <xsl:with-param name ="pGroupNode" select="ancestor::node()"/>
              </xsl:call-template>
            </xsl:if>

            <xsl:call-template name ="displayBreakline"/>

          </fo:block>

        </xsl:for-each>

      </xsl:for-each>

      <xsl:call-template name ="displayBreakline"/>
      <xsl:call-template name ="displayBreakline"/>
      <xsl:call-template name ="displayBreakline"/>

    </xsl:for-each>

    <xsl:if test ="$vIsCustomReport=false()">
      <xsl:call-template name ="displayBreakline"/>
      <xsl:call-template name ="displayBreakline"/>
      <xsl:call-template name ="displayEndOfReport"/>
    </xsl:if>

  </xsl:template>

</xsl:stylesheet>