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
  <!-- LandscapeCustomPage page for Spheres layout report  -->
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
    <xsl:text>CC050   Daily Margin</xsl:text>
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

  <!--! =================================================================================== -->
  <!--!                      layout templates                                               -->
  <!--! =================================================================================== -->


  <!-- =========================================================  -->
  <!-- Body section                                             -->
  <!-- =========================================================  -->

  <!-- *************************************************** -->
  <xsl:template name ="displayDailyMarginClassHeader">

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create7Columns">
          <xsl:with-param name ="pColumnWidth1" select="1.5"/>
          <xsl:with-param name ="pColumnWidth2" select="1.5"/>
          <xsl:with-param name ="pColumnWidth3" select="4"/>
          <xsl:with-param name ="pColumnWidth4" select="4"/>
          <xsl:with-param name ="pColumnWidth5" select="4"/>
          <xsl:with-param name ="pColumnWidth6" select="4"/>
          <xsl:with-param name ="pColumnWidth7" select="4"/>
        </xsl:call-template>

        <fo:table-body>
          <!-- header -->
          <fo:table-row height="{$vRowHeight}cm" keep-with-next="always">

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
              <xsl:with-param name ="pValue" select ="$vLabelMgnCl"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPremMgn"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelCurtLiq"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelFutSprdMgn"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelAddMgn"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelUnadMgnReqr"/>
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
  <xsl:template name ="displayDailyMarginClassRow">
    <xsl:param name ="pClassNode" />
    <xsl:param name ="pGroupName" />

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create7Columns">
          <xsl:with-param name ="pColumnWidth1" select="1.5"/>
          <xsl:with-param name ="pColumnWidth2" select="1.5"/>
          <xsl:with-param name ="pColumnWidth3" select="4"/>
          <xsl:with-param name ="pColumnWidth4" select="4"/>
          <xsl:with-param name ="pColumnWidth5" select="4"/>
          <xsl:with-param name ="pColumnWidth6" select="4"/>
          <xsl:with-param name ="pColumnWidth7" select="4"/>
        </xsl:call-template>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">
            <!-- variables -->
            <xsl:variable name ="vClassName">
              <xsl:call-template name ="getClassName">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vPremiumMargin">
              <xsl:call-template name ="getPremium">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vCurtLiqDeliveryMargin">
              <xsl:call-template name ="getCurtLiqDelivery">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vSpreadMargin">
              <xsl:call-template name ="getSpread">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vAdditionalMargin">
              <xsl:call-template name ="getAdditional">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vUnadjustedMarginReqr">
              <xsl:call-template name ="getUnadjustedMarginReqr">
                <xsl:with-param name ="pNode" select ="$pClassNode"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- cell-->
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$pGroupName"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vClassName"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vPremiumMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vCurtLiqDeliveryMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vSpreadMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vAdditionalMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vUnadjustedMarginReqr"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <xsl:template name ="displayDailyMarginTotalRow">
    <xsl:param name ="pCurrency"/>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create7Columns">
          <xsl:with-param name ="pColumnWidth1" select="1.5"/>
          <xsl:with-param name ="pColumnWidth2" select="1.5"/>
          <xsl:with-param name ="pColumnWidth3" select="4"/>
          <xsl:with-param name ="pColumnWidth4" select="4"/>
          <xsl:with-param name ="pColumnWidth5" select="4"/>
          <xsl:with-param name ="pColumnWidth6" select="4"/>
          <xsl:with-param name ="pColumnWidth7" select="4"/>
        </xsl:call-template>

        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">

            <!-- variables -->

            <xsl:variable name ="vTotalPremiumForCurrency">
              <xsl:call-template name ="getTotalPremiumForCurrency">
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vTotalCurtLiqDeliveryForCurrency">
              <xsl:call-template name ="getTotalCurtLiqDeliveryForCurrency">
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vTotalSpreadForCurrency">
              <xsl:call-template name ="getTotalSpreadForCurrency">
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vTotalAdditionalForCurrency">
              <xsl:call-template name ="getTotalAdditionalForCurrency">
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vTotalUnadjustedMarginReqrForCurrency">
              <xsl:call-template name ="getTotalUnadjustedMarginReqrForCurrency">
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- cell-->
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelAccount"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelTotal"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTotalPremiumForCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTotalCurtLiqDeliveryForCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTotalSpreadForCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTotalAdditionalForCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTotalUnadjustedMarginReqrForCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

          </fo:table-row>

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

      <xsl:call-template name ="displayDailyMarginClassHeader"/>

      <xsl:for-each select ="//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:marginAmount[@curr=$vMarginRequirementCurrency]]">

        <fo:block linefeed-treatment="preserve">

          <xsl:variable name ="vGroupName">
            <xsl:call-template name ="getGroupName">
              <xsl:with-param name ="pGroupNode" select ="current()"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:for-each select="efs:classes/efs:class[efs:marginAmount[@curr=$vMarginRequirementCurrency]]">
            <xsl:call-template name ="displayDailyMarginClassRow">
              <xsl:with-param name ="pClassNode" select ="current()"/>
              <xsl:with-param name ="pGroupName" select ="$vGroupName"/>
            </xsl:call-template>
          </xsl:for-each>

        </fo:block>

      </xsl:for-each>

      <xsl:call-template name ="displayDailyMarginTotalRow">
        <xsl:with-param name ="pCurrency" select="$vMarginRequirementCurrency"/>
      </xsl:call-template>
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