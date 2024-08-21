<?xml version="1.0" encoding="utf-8" ?>

<!-- PM 20180903 [24015] Prisma v8.0 : add Time to Expiry Adjustment -->

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
  <xsl:variable name ="vDescribedMethod" select ="'EUREX PRISMA'"/>

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
    <xsl:text>CP046 Aggregate Prisma Risk</xsl:text>
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
                  <xsl:call-template name="displayNetMarginHeader"/>
                  <xsl:call-template name ="displayBreakline"/>
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


  <!-- ************************************************** -->
  <!-- Clearing House - Clearing member - Exchange Member -->
  <!-- ************************************************** -->
  <xsl:template name="displayNetMarginHeader">

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

        <xsl:call-template name ="create5Columns">
          <xsl:with-param name ="pColumnWidth1" select ="8"/>
          <xsl:with-param name ="pColumnWidth2" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth3" select ="8"/>
          <xsl:with-param name ="pColumnWidth4" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth5" select ="8"/>
        </xsl:call-template>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelClearingHouse"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelClearingMember"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExchangeMember"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'Eurex Clearing AG'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'N/A'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'N/A'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>


  <!--************************************-->
  <!-- Contains:                          -->
  <!-- Account/Liquidation Group Block    -->
  <!-- Premium Block Block                -->
  <!-- Initial Margin Block               -->
  <!--************************************-->
  <xsl:template name="displayNetMarginBody">

    <!--PM 20140623 [19911] Multiple LG
    <xsl:for-each select ="//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:liquidationGroup">-->
    <xsl:for-each select ="//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:liquidationGroup/efs:group">
      <fo:block linefeed-treatment="preserve"  keep-together="always" >

        <!-- Account/Liquidation Group Block    -->
        <xsl:call-template name ="displayLiquidationGroupBlock">
          <xsl:with-param name="pNode" select="current()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayBreakline"/>

        <!-- Initial Margin Block includes:  -->
        <!-- Message type and currency block -->
        <!-- Initial Margin details block    -->
        <!-- Initial Margin total block    -->
        <xsl:call-template name ="displayMgTypeBlock">
          <xsl:with-param name="pNode" select="current()"/>
          <xsl:with-param name="pMessageType" select="'Initial'"/>
        </xsl:call-template>
        <xsl:call-template name ="displayBreakline"/>
        <xsl:call-template name ="displayInitialMarginDetailBlock">
          <xsl:with-param name="pNode" select="current()"/>
        </xsl:call-template>
        <!--<xsl:call-template name ="displayBreakline"/>-->
        <xsl:call-template name ="displayTotalBlock">
          <xsl:with-param name="pNode" select="current()"/>
          <xsl:with-param name="pType" select="'Initial'"/>
        </xsl:call-template>

        <xsl:call-template name ="displayBreakline"/>

        <!-- Premium Block includes:  -->
        <!-- Message type and currency block -->
        <!-- Premium details block    -->
        <!-- Premium total block    -->
        <xsl:call-template name ="displayMgTypeBlock">
          <xsl:with-param name="pNode" select="current()"/>
          <xsl:with-param name="pMessageType" select="'Premium'"/>
        </xsl:call-template>
        <xsl:call-template name ="displayBreakline"/>
        <xsl:call-template name ="displayPremiumDetailBlock">
          <xsl:with-param name="pNode" select="current()"/>
        </xsl:call-template>
        <!--<xsl:call-template name ="displayBreakline"/>-->
        <xsl:call-template name ="displayTotalBlock">
          <xsl:with-param name="pNode" select="current()"/>
          <xsl:with-param name="pType" select="'Premium'"/>
        </xsl:call-template>

      </fo:block>
    </xsl:for-each>

    <xsl:call-template name ="displayBreakline"/>
    <xsl:call-template name ="displayBreakline"/>
    <xsl:call-template name ="displayBreakline"/>

    <xsl:if test ="$vIsCustomReport=false()">
      <xsl:call-template name ="displayBreakline"/>
      <xsl:call-template name ="displayBreakline"/>
      <xsl:call-template name ="displayEndOfReport"/>
    </xsl:if>

  </xsl:template>

  <!-- Account/Liquidation Group Block    -->
  <xsl:template name ="displayLiquidationGroupBlock">
    <xsl:param name ="pNode" />
    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

        <xsl:call-template name ="create3Columns">
          <xsl:with-param name ="pColumnWidth1" select ="6"/>
          <xsl:with-param name ="pColumnWidth2" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth3" select ="6"/>
        </xsl:call-template>

        <fo:table-body>

          <xsl:variable name ="vPartyName">
            <xsl:call-template name ="getPartyName"/>
          </xsl:variable>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelAcct"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelLiquidationGroup"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vBook"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <!--PM 20140623 [19911] Multiple LG
              <xsl:with-param name ="pValue" select ="$pNode/efs:group/@name"/>-->
              <xsl:with-param name ="pValue" select ="$pNode/@name"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>


  </xsl:template>


  <!-- *************************************************************************** -->
  <!-- Display the message type block (share to premium and initial margin section)-->
  <!-- *************************************************************************** -->
  <xsl:template name ="displayMgTypeBlock">
    <xsl:param name ="pNode" />
    <xsl:param name ="pMessageType" />

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

        <xsl:call-template name ="create3Columns">
          <xsl:with-param name ="pColumnWidth1" select ="1.5"/>
          <xsl:with-param name ="pColumnWidth2" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth3" select ="1"/>
        </xsl:call-template>

        <fo:table-body>

          <xsl:variable name ="vCurrency">

            <xsl:choose>
              <xsl:when test ="$pMessageType='Premium'">
                <xsl:call-template name ="getMarginCurrency">
                  <xsl:with-param name ="pNode" select="$pNode/efs:premiumMargin/efs:margin"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name ="getMarginCurrency">
                  <xsl:with-param name ="pNode" select="$pNode/efs:initialMargin/efs:margin"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelMgType"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelCurr"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$pMessageType"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>


  <!-- ********************************************* -->
  <!-- Display Premium details for each split -->
  <!-- ********************************************* -->
  <xsl:template name ="displayPremiumDetailBlock">
    <xsl:param name ="pNode" />

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

        <xsl:call-template name ="create5Columns">
          <xsl:with-param name ="pColumnWidth1" select ="4"/>
          <xsl:with-param name ="pColumnWidth2" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth3" select ="4"/>
          <xsl:with-param name ="pColumnWidth4" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth5" select ="3.5"/>
        </xsl:call-template>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExpGrp"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelSubPortIdentifier"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelPremiumMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <xsl:for-each select="$pNode/efs:liquidationGroupSplit/efs:split">

            <xsl:variable name ="vSplitName">
              <xsl:call-template name ="getSplitName">
                <xsl:with-param name ="pNode" select="current()"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vPremiumMargin">
              <xsl:call-template name ="getMarginAmount">
                <xsl:with-param name ="pNode" select="current()/efs:premiumMargin"/>
              </xsl:call-template>
            </xsl:variable>

            <fo:table-row height="{$vRowHeight}cm">

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vSplitName"/>
                <xsl:with-param name ="pTextAlign" select ="'left'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pTextAlign" select ="'left'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vPremiumMargin"/>
                <xsl:with-param name ="pTextAlign" select ="'right'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

            </fo:table-row>

          </xsl:for-each>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>


  <!-- ********************************************* -->
  <!-- Display Initial margin details for each split -->
  <!-- ********************************************* -->
  <xsl:template name ="displayInitialMarginDetailBlock">
    <xsl:param name ="pNode" />

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

        <xsl:call-template name ="create13Columns">
          <xsl:with-param name ="pColumnWidth1" select ="4"/>
          <xsl:with-param name ="pColumnWidth2" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth3" select ="4"/>
          <xsl:with-param name ="pColumnWidth4" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth5" select ="3.5"/>
          <xsl:with-param name ="pColumnWidth6" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth7" select ="3.5"/>
          <xsl:with-param name ="pColumnWidth8" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth9" select ="3.5"/>
          <xsl:with-param name ="pColumnWidth10" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth11" select ="3.5"/>
          <!-- PM 20180903 [24015] Prisma v8.0 : add Time to Expiry Adjustment -->
          <xsl:with-param name ="pColumnWidth12" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth13" select ="3.5"/>
        </xsl:call-template>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExpGrp"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelSubPortIdentifier"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelMarketRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelLiquidityRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelLongOptionCredit"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>
            
            <!-- PM 20180903 [24015] Prisma v8.0 : add Time to Expiry Adjustment -->
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelTimeToExpiryAdjustment"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>
            
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelInitialMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <!--PM 20140623 [19911] Multiple LG & LGS
          <xsl:for-each select="$pNode/efs:group/efs:liquidationGroupSplit">-->
          <xsl:for-each select="$pNode/efs:liquidationGroupSplit/efs:split">

            <xsl:variable name ="vSplitName">
              <xsl:call-template name ="getSplitName">
                <xsl:with-param name ="pNode" select="current()"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vMarketRisk">
              <xsl:call-template name ="getMarginAmount">
                <!--PM 20140623 [19911] Multiple LGS
                <xsl:with-param name ="pNode" select="current()/efs:split/efs:marketRisk"/>-->
                <xsl:with-param name ="pNode" select="current()/efs:marketRisk"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- PM 20180903 [24015] Prisma v8.0 : add Time to Expiry Adjustment -->
            <xsl:variable name ="vTimeToExpiryAdjustment">
              <xsl:call-template name ="getMarginAmount">
                <xsl:with-param name ="pNode" select="current()/efs:timeToExpiryAdjustment"/>
              </xsl:call-template>
            </xsl:variable>
            
            <xsl:variable name ="vLiquidityRisk">
              <xsl:call-template name ="getMarginAmount">
                <!--PM 20140623 [19911] Multiple LGS
                <xsl:with-param name ="pNode" select="current()/efs:split/efs:liquidityRisk"/>-->
                <xsl:with-param name ="pNode" select="current()/efs:liquidityRisk"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vLongOptionCredit">
              <xsl:call-template name ="getMarginAmount">
                <xsl:with-param name ="pNode" select="current()/efs:longOptionCredit"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name ="vInitialMargin">
              <xsl:call-template name ="getMarginAmount">
                <!--PM 20140623 [19911] Multiple LGS
                <xsl:with-param name ="pNode" select="current()/efs:split/efs:totalInitialMargin"/>-->
                <xsl:with-param name ="pNode" select="current()/efs:totalInitialMargin"/>
              </xsl:call-template>
            </xsl:variable>

            <fo:table-row height="{$vRowHeight}cm">

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vSplitName"/>
                <xsl:with-param name ="pTextAlign" select ="'left'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <!--<xsl:with-param name ="pValue" select ="$vBook"/>-->
                <xsl:with-param name ="pTextAlign" select ="'left'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vMarketRisk"/>
                <xsl:with-param name ="pTextAlign" select ="'right'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vLiquidityRisk"/>
                <xsl:with-param name ="pTextAlign" select ="'right'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vLongOptionCredit"/>
                <xsl:with-param name ="pTextAlign" select ="'right'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <!-- PM 20180903 [24015] Prisma v8.0 : add Time to Expiry Adjustment -->
              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vTimeToExpiryAdjustment"/>
                <xsl:with-param name ="pTextAlign" select ="'right'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pPadding" select ="'0'"/>
              </xsl:call-template>

              <xsl:call-template name ="displayTableCellBody">
                <xsl:with-param name ="pValue" select ="$vInitialMargin"/>
                <xsl:with-param name ="pTextAlign" select ="'right'"/>
                <xsl:with-param name ="pPadding" select ="'0'"/>
                <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              </xsl:call-template>

            </fo:table-row>

          </xsl:for-each>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>


  <!-- ********************************************************** -->
  <!-- Total block (share to premium and initial margin section)  -->
  <!-- ********************************************************** -->

  <xsl:template name ="displayTotalBlock">
    <xsl:param name ="pNode" />
    <xsl:param name ="pType"/>

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

        <xsl:variable name ="vColumnWidth1">
          <xsl:choose>
            <xsl:when test ="$pType='Premium'">
              <xsl:value-of select ="'8.3'"/>
            </xsl:when>
            <xsl:when test ="$pType='Initial'">
              <!-- PM 20180903 [24015] Prisma v8.0-->
              <!--<xsl:value-of select ="'19.7'"/>-->
              <xsl:value-of select ="'23.5'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="'4'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:call-template name ="create3Columns">
          <xsl:with-param name ="pColumnWidth1" select ="$vColumnWidth1"/>
          <xsl:with-param name ="pColumnWidth2" select ="0.3"/>
          <xsl:with-param name ="pColumnWidth3" select ="3.5"/>
        </xsl:call-template>

        <fo:table-body>

          <!-- Returns label -->
          <xsl:variable name ="vLabel">
            <xsl:choose>
              <xsl:when test ="$pType='Premium'">
                <xsl:value-of select ="$vLabelTotalPremiumMargin"/>
              </xsl:when>
              <xsl:when test ="$pType='Initial'">
                <xsl:value-of select ="$vLabelTotalInitialMargin"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="$vLabelTotalInitialMargin"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name ="vValue">
            <xsl:choose>
              <xsl:when test ="$pType='Premium'">
                <xsl:call-template name ="getMarginAmount">
                  <xsl:with-param name ="pNode" select="$pNode/efs:premiumMargin"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test ="$pType='Initial'">
                <xsl:call-template name ="getMarginAmount">
                  <xsl:with-param name ="pNode" select="$pNode/efs:initialMargin"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name ="getMarginAmount">
                  <xsl:with-param name ="pNode" select="$pNode/efs:initialMargin"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 1° row -->
          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <!-- 2° row -->
          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabel"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pPadding" select ="'0'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pPadding" select ="'0'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>




  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->

  <!-- *************************************************** -->
  <xsl:template name ="displayAdditionalMarginHeader">

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

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
          <!--titles-->
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

  <!--***************************************************-->
  <xsl:template name ="displayAdditionalMarginRow">
    <xsl:param name ="pClassNode" />
    <xsl:param name ="pGroupName" />

    <fo:block border="{$vBorder}" linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" >
      <fo:table border="{$vBorder}" table-layout="fixed" width="100%">

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


            <!--variables-->

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


            <!--cell-->

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$pGroupName"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vOffsetFactor"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vClassName"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vExpiry"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vMarginAmountCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vWorstCaseScenario"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vPureAdditionalMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vMarginFactor"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vAdditionalMargin"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
              <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!--***************************************************-->
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


          <!--titles-->

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





</xsl:stylesheet>