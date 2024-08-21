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
  <xsl:include href="SPAN2_Common.xslt" />

  <!-- set the name of page sequence -->
  <xsl:variable name ="vPageSequenceMasterReference">
    <xsl:text>A4LandscapePage</xsl:text>
  </xsl:variable>

  <!-- Variables for header titles ====================================================== -->
  <xsl:variable name ="vLabelReportName">
    <xsl:value-of select ="$vLabelPBRequirements"/>
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

          <xsl:for-each select ="efs:EfsML/efs:marginRequirementOffice/efs:marginAmounts/efs:Money">

            <xsl:variable name ="vMarginRequirementCurrency">
              <xsl:call-template name ="getMarginCurrency">
                <xsl:with-param name ="pNode" select ="current()"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vSpan2Core] or //efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vSpan2Software]">
                <fo:block linefeed-treatment="preserve" white-space-collapse="false"  white-space-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black" break-before="page">
                  <xsl:call-template name="displayOverviewMarginBody">
                    <xsl:with-param name ="pCurrency" select ="$vMarginRequirementCurrency"/>
                  </xsl:call-template>
                  <xsl:call-template name ="displayBreakline"/>
                  <xsl:call-template name="displayDetailMarginBody">
                    <xsl:with-param name ="pCurrency" select ="$vMarginRequirementCurrency"/>
                  </xsl:call-template>
                </fo:block>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name ="displayErrorMessage"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>

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
          <xsl:with-param name="pColumnWidth1" select ="2"/>
          <xsl:with-param name="pColumnWidth2" select ="8"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelPBRequirements"/>
              <xsl:with-param name ="pFontSize" select ="'12pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat($vLabelBusinessDate,$vColon)"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vBusinessDate"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <xsl:call-template name ="displayBreakline"/>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create8Columns">
          <xsl:with-param name="pColumnWidth1" select ="1.2"/>
          <xsl:with-param name="pColumnWidth2" select ="6"/>
          <xsl:with-param name="pColumnWidth3" select ="1.2"/>
          <xsl:with-param name="pColumnWidth4" select ="8"/>
          <xsl:with-param name="pColumnWidth5" select ="1.2"/>
          <xsl:with-param name="pColumnWidth6" select ="4"/>
          <xsl:with-param name="pColumnWidth7" select ="1"/>
          <xsl:with-param name="pColumnWidth8" select ="4"/>
        </xsl:call-template>
        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">
            <!-- Firm label -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat($vLabelFirm,$vColon)"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Firm value -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFirm"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Acct label -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat($vLabelAcct,$vColon)"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Acct value -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vAcct"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Type label -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat($vLabelType,$vColon)"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Type value -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vAcctType"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Seg label -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat($vLabelSeg,$vColon)"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <!-- Seg value -->
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vMissingValue"/>
              <xsl:with-param name ="pFontSize" select ="'11pt'"/>
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
  <xsl:template name="displayOverviewMarginBody">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vLedgerBalanceOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getLedgerBalanceOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vOpenTradeEquityOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getOpenTradeEquityOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vSecuritiesOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getSecuritiesOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vTotalEquityOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getTotalEquityOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vMaintSpanReqOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getMaintSpanReqOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vInitSpanReqOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getInitSpanReqOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vAvailableNetOptionValueOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getAvailableNetOptionValueOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vMaintTotalReqOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getMaintTotalReqOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vInitTotalReqOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getInitTotalReqOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vNetLiqValueOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getNetLiqValueOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vLabelMaintExcessDeficitOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getMaintExcessDeficitOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vLabelInitExcessDeficitOverview">
      <xsl:variable name ="vAmount">
        <xsl:call-template name ="getInitExcessDeficitOverview">
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name ="formatOverviewAmount">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:table table-layout="fixed" width="100%">
      <xsl:call-template name ="create6Columns">
        <xsl:with-param name="pColumnWidth1" select ="2.5"/>
        <xsl:with-param name="pColumnWidth2" select ="2.5"/>
        <xsl:with-param name="pColumnWidth3" select ="1"/>
        <xsl:with-param name="pColumnWidth4" select ="2"/>
        <xsl:with-param name="pColumnWidth5" select ="4"/>
        <xsl:with-param name="pColumnWidth6" select ="4"/>
      </xsl:call-template>
      <fo:table-body>
        <fo:table-row height="{$vRowHeight}cm">
          <!-- Currency label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelCurrency,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- Currency value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$pCurrency"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- Core Maint label -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vLabelCoreMaint"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- Core Maint label -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vLabelCoreInit"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">
          <!-- LedgerBalance label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelLedgerBalance,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- LedgerBalance value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vLedgerBalanceOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- SpanReq label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelSpanReq,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- Maint SpanReq Value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vMaintSpanReqOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- Init SpanReq Value-->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vInitSpanReqOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">
          <!-- OpenTradeEquity label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelOpenTradeEquity,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- OpenTradeEquity value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vOpenTradeEquityOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- ANOV label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat('-',$vLabelAnov,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- Maint ANOV Value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vAvailableNetOptionValueOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- Init ANOV Value-->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vAvailableNetOptionValueOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">
          <!-- Securities label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelSecurities,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- Securities value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vSecuritiesOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- TotalReq label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelTotalReq,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- Maint TotalReq Value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vMaintTotalReqOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- Init TotalReq Value-->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vInitTotalReqOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">
          <!-- TotalEquity label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelTotalEquity,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- TotalEquity value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vTotalEquityOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- NetLiqValue label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelNetLiqValue,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- NetLiqValue Maint Value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vNetLiqValueOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- NetLiqValue Init Value-->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="$vNetLiqValueOverview"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- empty cell -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- empty cell  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- ExcessDeficit label  -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat($vLabelExcessDeficit,$vColon)"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
          </xsl:call-template>
          <!-- ExcessDeficit Maint Value -->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat('(',$vLabelMaintExcessDeficitOverview,')')"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
          <!-- ExcessDeficit Init Value-->
          <xsl:call-template name ="displayTableCellHeader">
            <xsl:with-param name ="pValue" select ="concat('(',$vLabelInitExcessDeficitOverview,')')"/>
            <xsl:with-param name ="pFontSize" select ="$vFontSize"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
          </xsl:call-template>
        </fo:table-row>

      </fo:table-body>
    </fo:table>
  </xsl:template>

  <xsl:template name="displayDetailMarginBody">
    <xsl:param name ="pCurrency"/>

    <fo:table table-layout="fixed" width="100%">

      <xsl:call-template name ="create16Columns">
        <xsl:with-param name ="pColumnWidth1" select="1"/>
        <xsl:with-param name ="pColumnWidth2" select="1"/>
        <xsl:with-param name ="pColumnWidth3" select="1"/>
        <xsl:with-param name ="pColumnWidth4" select="1"/>
        <xsl:with-param name ="pColumnWidth5" select="2"/>
        <xsl:with-param name ="pColumnWidth6" select="2"/>
        <xsl:with-param name ="pColumnWidth7" select="1"/>
        <xsl:with-param name ="pColumnWidth8" select="1"/>
        <xsl:with-param name ="pColumnWidth9" select="2"/>
        <xsl:with-param name ="pColumnWidth10" select="2"/>
        <xsl:with-param name ="pColumnWidth11" select="2"/>
        <xsl:with-param name ="pColumnWidth12" select="2"/>
        <xsl:with-param name ="pColumnWidth13" select="2"/>
        <xsl:with-param name ="pColumnWidth14" select="2"/>
        <xsl:with-param name ="pColumnWidth15" select="2"/>
        <xsl:with-param name ="pColumnWidth16" select="2"/>
      </xsl:call-template>

      <xsl:call-template name ="displayDetailMarginRowTitles"/>

      <xsl:for-each select ="//efs:ccps/efs:ccp/efs:groups/efs:group[efs:riskInitialByCurrency/efs:riskInitial[@curr=$pCurrency]]">

        <xsl:sort select="@name" />

        <!-- block ccp-->
        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelPort"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplayLOV" select ="true()"/>
          <xsl:with-param name ="pDisplaySOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelCurVal"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplayLOV" select ="true()"/>
          <xsl:with-param name ="pDisplaySOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelOReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelMaint"/>
          <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelCurReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelMaint"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelOReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelInit"/>
          <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelCurReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelInit"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <!-- block ccp-->
        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelEcPort"/>
          <xsl:with-param name ="pDisplayExchangeComplexAcronym" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplayLOV" select ="true()"/>
          <xsl:with-param name ="pDisplaySOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelCurVal"/>
          <xsl:with-param name ="pDisplayExchangeComplexAcronym" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pDisplayLOV" select ="true()"/>
          <xsl:with-param name ="pDisplaySOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelOReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelMaint"/>
          <xsl:with-param name ="pDisplayExchangeComplexAcronym" select ="true()"/>
          <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelCurReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelMaint"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelOReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelInit"/>
          <xsl:with-param name ="pDisplayExchangeComplexAcronym" select ="true()"/>
          <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <xsl:call-template name ="displayDetailMarginRowValues">
          <xsl:with-param name ="pNode" select ="current()"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pLevel" select ="$vLabelCurReq"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$vLabelInit"/>
          <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
          <xsl:with-param name ="pIsGroupNode" select ="true()"/>
          <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
          <xsl:with-param name ="pDisplayANOV" select ="true()"/>
        </xsl:call-template>

        <!--block pod (ex combinedcommodity)-->
        <xsl:for-each select ="efs:pods/efs:pod[efs:riskInitial[@curr=$pCurrency]]">
          <xsl:sort select="@name" />

          <xsl:call-template name ="displayDetailMarginRowValues">
            <xsl:with-param name ="pNode" select ="current()"/>
            <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
            <xsl:with-param name ="pLevel" select ="$vLabelCcPort"/>
            <xsl:with-param name ="pDisplayExchangeComplexAcronym" select ="true()"/>
            <xsl:with-param name ="pDisplayCommodityCode" select ="true()"/>
            <xsl:with-param name ="pDisplayCurrency" select ="true()"/>
            <xsl:with-param name ="pDisplayLOV" select ="true()"/>
            <xsl:with-param name ="pDisplaySOV" select ="true()"/>
          </xsl:call-template>

          <xsl:choose>
            <xsl:when test ="$vGlobalRequirementType=$vLabelMaint or $vGlobalRequirementType=''">
              <xsl:call-template name ="displayDetailMarginRowValues">
                <xsl:with-param name ="pNode" select ="current()"/>
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
                <xsl:with-param name ="pLevel" select ="$vLabelNReq"/>
                <xsl:with-param name ="pDetailRequirementType" select ="$vLabelMaint"/>
                <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
                <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
                <xsl:with-param name ="pDisplayANOV" select ="true()"/>
                <xsl:with-param name ="pDisplayCommodityDetails" select ="true()"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="displayDetailMarginRowValues">
                <xsl:with-param name ="pNode" select ="current()"/>
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
                <xsl:with-param name ="pLevel" select ="$vLabelNReq"/>
                <xsl:with-param name ="pDetailRequirementType" select ="$vLabelMaint"/>
                <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
                <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
                <xsl:with-param name ="pDisplayANOV" select ="true()"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

          <xsl:choose>
            <xsl:when test ="$vGlobalRequirementType=$vLabelInit">
              <xsl:call-template name ="displayDetailMarginRowValues">
                <xsl:with-param name ="pNode" select ="current()"/>
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
                <xsl:with-param name ="pLevel" select ="$vLabelDReq"/>
                <xsl:with-param name ="pDetailRequirementType" select ="$vLabelInit"/>
                <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
                <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
                <xsl:with-param name ="pDisplayANOV" select ="true()"/>
                <xsl:with-param name ="pDisplayCommodityDetails" select ="true()"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="displayDetailMarginRowValues">
                <xsl:with-param name ="pNode" select ="current()"/>
                <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
                <xsl:with-param name ="pLevel" select ="$vLabelDReq"/>
                <xsl:with-param name ="pDetailRequirementType" select ="$vLabelInit"/>
                <xsl:with-param name ="pDisplayRequirementType" select ="true()"/>
                <xsl:with-param name ="pDisplaySpanReq" select ="true()"/>
                <xsl:with-param name ="pDisplayANOV" select ="true()"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>

        <xsl:call-template name ="displayEmptyRow"/>

      </xsl:for-each>

    </fo:table>

  </xsl:template>

  <xsl:template name="displayDetailMarginRowTitles">

    <fo:table-body>
      <fo:table-row height="{$vRowHeight}cm">
        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelLevel"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelEC"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelCC"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelCurr"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelLOV"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelSOV"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelMI"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelPBC"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelSpanReq"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelAnov"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelScanRisk"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelIntra"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelSpot"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelInter"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelIntex"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLabelSOM"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>
      </fo:table-row>
    </fo:table-body>
  </xsl:template>

  <xsl:template name="displayDetailMarginRowValues">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pCurrency"/>
    <xsl:param name ="pLevel"/>
    <xsl:param name ="pDetailRequirementType" select="''"/>
    <xsl:param name ="pDisplayExchangeComplexAcronym" select="false()"/>
    <xsl:param name ="pDisplayCommodityCode" select="false()"/>
    <xsl:param name ="pDisplayCurrency" select="false()"/>
    <xsl:param name ="pDisplayLOV" select="false()"/>
    <xsl:param name ="pDisplaySOV" select="false()"/>
    <xsl:param name ="pDisplayRequirementType" select="false()"/>
    <xsl:param name ="pIsGroupNode" select="false()"/>
    <xsl:param name ="pDisplaySpanReq" select="false()"/>
    <xsl:param name ="pDisplayANOV" select="false()"/>
    <xsl:param name ="pDisplayCommodityDetails" select="false()"/>


    <xsl:variable name ="vExchangeComplexAcronym">
      <xsl:if test ="$pDisplayExchangeComplexAcronym=true()">
        <xsl:call-template name ="getExchange">
          <xsl:with-param name ="pNode" select="ancestor::efs:ccp"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vCommodityCode">
      <xsl:if test ="$pDisplayCommodityCode=true()">
        <xsl:call-template name ="getCommodityCode">
          <xsl:with-param name ="pNode" select="$pNode"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vCurr">
      <xsl:if test ="$pDisplayCurrency=true()">
        <xsl:value-of select ="$pCurrency"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vLOV">
      <xsl:if test ="$pDisplayLOV=true()">
        <xsl:call-template name ="getLongOptionValue">
          <xsl:with-param name ="pNode" select ="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pIsGroupNode" select ="$pIsGroupNode"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vSOV">
      <xsl:if test ="$pDisplaySOV=true()">
        <xsl:call-template name ="getShortOptionValue">
          <xsl:with-param name ="pNode" select ="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pIsGroupNode" select ="$pIsGroupNode"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vGlobalRequirementType">
      <xsl:if test ="$pDisplayRequirementType=true()">
        <xsl:value-of select ="$pDetailRequirementType"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vPBClassCode">
      <xsl:if test ="$pDisplayRequirementType=true()">
        <xsl:value-of select ="'CORE'"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vSpanReq">
      <xsl:if test ="$pDisplaySpanReq=true()">
        <xsl:call-template name ="getSpanReq">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
          <xsl:with-param name ="pIsGroupNode" select ="$pIsGroupNode"/>
          <xsl:with-param name ="pDetailRequirementType" select ="$pDetailRequirementType"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vANOV">
      <xsl:if test ="$pDisplayANOV=true()">
        <xsl:choose>
          <xsl:when test ="$pIsGroupNode=true()">
            <xsl:call-template name ="getNetOptionValue">
              <xsl:with-param name ="pNode" select="$pNode/efs:novByCurrency"/>
              <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name ="getNetOptionValue">
              <xsl:with-param name ="pNode" select="$pNode"/>
              <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vScanRisk">
      <xsl:if test ="$pDisplayCommodityDetails=true()">
        <xsl:call-template name ="getScanRisk">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vIntra">
      <xsl:if test ="$pDisplayCommodityDetails=true()">
        <xsl:call-template name ="getIntraCommodityCharge">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vSpot">
      <xsl:if test ="$pDisplayCommodityDetails=true()">
        <xsl:call-template name ="getDeliveryCharge">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vInter">
      <xsl:if test ="$pDisplayCommodityDetails=true()">
        <xsl:call-template name ="getInterCommodityCredit">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vIntex">
      <xsl:if test ="$pDisplayCommodityDetails=true()">
        <!-- PM 20130620 Ajout appel à getInterExchangeCredit -->
        <xsl:call-template name ="getInterExchangeCredit">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
        <!--xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="'0'"/>
        </xsl:call-template-->
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vSOM">
      <xsl:if test ="$pDisplayCommodityDetails=true()">
        <xsl:call-template name ="getShortOptionMinimum">
          <xsl:with-param name ="pNode" select="$pNode"/>
          <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <fo:table-body>
      <fo:table-row height="{$vRowHeight}cm">
        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$pLevel"/>
          <xsl:with-param name ="pTextAlign" select ="'left'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vExchangeComplexAcronym"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vCommodityCode"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vCurr"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vLOV"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vSOV"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vGlobalRequirementType"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vPBClassCode"/>
          <xsl:with-param name ="pTextAlign" select ="'center'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vSpanReq"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vANOV"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vScanRisk"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vIntra"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vSpot"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vInter"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vIntex"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

        <xsl:call-template name ="displayTableCellBody">
          <xsl:with-param name ="pValue" select ="$vSOM"/>
          <xsl:with-param name ="pTextAlign" select ="'right'"/>
          <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
          <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
        </xsl:call-template>

      </fo:table-row>

    </fo:table-body>

  </xsl:template>

</xsl:stylesheet>