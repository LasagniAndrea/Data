<?xml version="1.0" encoding="utf-8" ?>

<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:efs  ="http://www.efs.org/2007/EFSmL-3-0"
                xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
                xmlns:fpml ="http://www.fpml.org/2007/FpML-4-4">

  <xsl:output method="xml" encoding="UTF-8" />

  <!-- report culture (in use by dates, money, etc..)-->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />

  <xsl:variable name ="vIsA4VerticalReport" select ="true()"/>

  <!-- this variable defines the type of report -->
  <xsl:variable name ="vIsCustomReport" select ="false()"/>

  <!-- xslt includes -->
  <xsl:include href="SPAN_Eurosys_Common.xslt" />

  <!-- set the name of page sequence -->
  <xsl:variable name ="vPageSequenceMasterReference">
    <xsl:text>A4VerticalPage</xsl:text>
  </xsl:variable>

  <!-- Variables for header titles ====================================================== -->
  <xsl:variable name ="vLabelReportName">
    <xsl:value-of select ="$vLabelPerformanceBondReport"/>
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

        <fo:static-content flow-name="A4VerticalFooter">
          <xsl:call-template name="displayFooter"/>
        </fo:static-content>

        <fo:flow flow-name="A4VerticalBody">

          <xsl:choose>
            <!-- PM 20160404 [22116] Gestion du cas de plusieurs méthodes dans le log -->
            <!--<xsl:when test ="$vMarginCalculationMethodName = $vSpanCME or $vMarginCalculationMethodName = $vLondonSpan or $vMarginCalculationMethodName = $vSpanC21">-->
            <xsl:when test ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vSpanCME] or //efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vLondonSpan] or //efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vSpanC21]">
              <fo:block linefeed-treatment="preserve" white-space-collapse="false"  white-space-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black">

                <xsl:for-each select ="//efs:span/efs:exchangecomplex/efs:groups/efs:group/efs:combinedcommodities/efs:combinedcommodity">
                  <xsl:sort select="@name" />

                  <!-- Saut de page après le bloc mais pas en dernière page  -->
                  <xsl:choose>
                    <xsl:when test = "position() = last()">
                      <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
                        <xsl:call-template name="displayBody">
                          <xsl:with-param name ="pNode" select ="current()"/>
                        </xsl:call-template>
                      </fo:block>
                    </xsl:when>

                    <xsl:otherwise>
                      <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" break-after="page">
                        <xsl:call-template name="displayBody">
                          <xsl:with-param name ="pNode" select ="current()"/>
                        </xsl:call-template>
                      </fo:block>
                    </xsl:otherwise>
                  </xsl:choose>

                </xsl:for-each>
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
          <xsl:with-param name="pColumnWidth1" select ="5"/>
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

  </xsl:template>
  
  
  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->
  <xsl:template name="displayBody">
    <xsl:param name ="pNode" />

    <xsl:call-template name ="displayReportOverview">
      <xsl:with-param name ="pNode" select ="$pNode"/>
    </xsl:call-template>

    <xsl:call-template name ="displayBreakline"/>
    <xsl:call-template name ="displayBreakline"/>

    <xsl:call-template name ="displayMarketBlock">
      <xsl:with-param name ="pNode" select ="$pNode"/>
    </xsl:call-template>

    <xsl:call-template name ="displayCombinedCommodityBlock1">
      <xsl:with-param name ="pNode" select ="$pNode"/>
    </xsl:call-template>

    <xsl:call-template name ="displayBreakline"/>

    <xsl:call-template name ="displayCombinedCommodityBlock2">
      <xsl:with-param name ="pNode" select ="$pNode"/>
    </xsl:call-template>
    <xsl:call-template name ="displayBreakline"/>

    <xsl:call-template name ="displayCombinedCommodityBlock3">
      <xsl:with-param name ="pNode" select ="$pNode"/>
    </xsl:call-template>

    <xsl:call-template name ="displayBreakline"/>

    <xsl:call-template name ="displayReportRemarks"/>

  </xsl:template>
  

  <xsl:template name ="displayReportOverview">
    <xsl:param name ="pNode" />

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create2Columns">
          <xsl:with-param name="pColumnWidth1" select ="3"/>
          <xsl:with-param name="pColumnWidth2" select ="4"/>
        </xsl:call-template>
        <fo:table-body padding="{$vTablePadding}">
          <fo:table-row height="{$vRowHeight}cm"/>
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
  
  
  <xsl:template name ="displayMarketBlock">
    <xsl:param name ="pNode" />
    <xsl:variable name ="vExchangeComplexAcronym">
      <xsl:call-template name ="getExchange">
        <xsl:with-param name ="pNode" select="ancestor::efs:exchangecomplex"/>
      </xsl:call-template>
    </xsl:variable>
    <fo:table table-layout="fixed" width="100%">
      <xsl:call-template name ="create2Columns">
        <xsl:with-param name="pColumnWidth1" select ="2"/>
        <xsl:with-param name="pColumnWidth2" select ="2"/>
      </xsl:call-template>
      <fo:table-body>
        <fo:table-row height="{$vRowHeight}cm">
          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelMarche"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>
          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vExchangeComplexAcronym"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>
      
        
  <xsl:template name ="displayCombinedCommodityBlock1">
    <xsl:param name ="pNode" />

    <fo:table table-layout="fixed" width="100%">
      <xsl:call-template name ="create11Columns">
        <xsl:with-param name="pColumnWidth1" select ="2"/>
        <xsl:with-param name="pColumnWidth2" select ="2"/>
        <xsl:with-param name="pColumnWidth3" select ="0.1"/>
        <xsl:with-param name="pColumnWidth4" select ="2"/>
        <xsl:with-param name="pColumnWidth5" select ="2"/>
        <xsl:with-param name="pColumnWidth6" select ="2"/>
        <xsl:with-param name="pColumnWidth7" select ="2"/>
        <xsl:with-param name="pColumnWidth8" select ="2"/>
        <xsl:with-param name="pColumnWidth9" select ="2"/>
        <xsl:with-param name="pColumnWidth10" select ="2"/>
        <xsl:with-param name="pColumnWidth11" select ="1.9"/>
      </xsl:call-template>
      <fo:table-body>

        <xsl:variable name ="vProduitValue">
          <xsl:call-template name ="getCommodityCode">
            <xsl:with-param name ="pNode" select="$pNode"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc1">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'1'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc2">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'2'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc3">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'3'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc4">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'4'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc5">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'5'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc6">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'6'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc7">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'7'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc8">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'8'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc9">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'9'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc10">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'10'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc11">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'11'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc12">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'12'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc13">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'13'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc14">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'14'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc15">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'15'"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name ="vSc16">
          <xsl:call-template name ="getScanScenario">
            <xsl:with-param name ="pNode" select="$pNode"/>
            <xsl:with-param name ="pScenario" select="'16'"/>
          </xsl:call-template>
        </xsl:variable>

        <fo:table-row height="{$vRowHeight}cm">

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelProduit"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vProduitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>
          
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorder" select ="'0pt solid #000'"/>
            <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
            <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 1')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 2')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 3')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 4')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 5')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 6')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 7')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 8')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelGroupeSpan"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vProduitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <!-- EG 20160404 Migration vs2013 -->
          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorder" select ="'0pt solid #000'"/>
            <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
            <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 9')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 10')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 11')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 12')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 13')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 14')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 15')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="concat($vLabelScenario,' 16')"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vLabelCodeSpan"/>
            <xsl:with-param name ="pTextAlign" select ="'left'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            <xsl:with-param name ="pFontWeight" select ="'bold'"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vProduitValue"/>
            <xsl:with-param name ="pTextAlign" select ="'center'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <!-- EG 20160404 Migration vs2013 -->
          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorder" select ="'0pt solid #000'"/>
            <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
            <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc1"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc2"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc3"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc4"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc5"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc6"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc7"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc8"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

        </fo:table-row>

        <fo:table-row height="{$vRowHeight}cm">

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
            <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pBorderTopStyle" select ="$vTableCellBorderTopStyle"/>
            <xsl:with-param name ="pBorderTopWidth" select ="$vTableCellBorderTopWidth"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody"/>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc9"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc10"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc11"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc12"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc13"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc14"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc15"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

          <xsl:call-template name ="displayTableCellBody">
            <xsl:with-param name ="pValue" select ="$vSc16"/>
            <xsl:with-param name ="pTextAlign" select ="'right'"/>
            <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
          </xsl:call-template>

        </fo:table-row>

      </fo:table-body>
    </fo:table>

  </xsl:template>
    
      
  <xsl:template name ="displayCombinedCommodityBlock2">
    <xsl:param name ="pNode" />

    <fo:block border="{$vTableCellBorder}" >

      <fo:table table-layout="fixed" width="100%" >
        <xsl:call-template name ="create11Columns">
          <xsl:with-param name="pColumnWidth1" select ="0.8"/>
          <xsl:with-param name="pColumnWidth2" select ="3"/>
          <xsl:with-param name="pColumnWidth3" select ="0.8"/>
          <xsl:with-param name="pColumnWidth4" select ="3"/>
          <xsl:with-param name="pColumnWidth5" select ="0.8"/>
          <xsl:with-param name="pColumnWidth6" select ="3"/>
          <xsl:with-param name="pColumnWidth7" select ="0.8"/>
          <xsl:with-param name="pColumnWidth8" select ="3"/>
          <xsl:with-param name="pColumnWidth9" select ="0.8"/>
          <xsl:with-param name="pColumnWidth10" select ="3"/>
          <xsl:with-param name="pColumnWidth11" select ="0.8"/>
        </xsl:call-template>
        <fo:table-body >

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

          <xsl:variable name ="vCommodityRiskValue">
            <xsl:call-template name ="getCommodityRisk">
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

          <fo:table-row height="{$vRowHeight}cm" />

          <fo:table-row height="{$vRowHeight}cm" >

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelCommodityRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'='"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelScanRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'+'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelIntermonth"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'+'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelDeliveryMonth"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>
            <xsl:call-template name ="displayTableCellBody"/>
            <xsl:call-template name ="displayTableCellBody"/>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vCommodityRiskValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vScanRiskValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vIntermonthValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vDeliveryMonthValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>
            <xsl:call-template name ="displayTableCellBody"/>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm"/>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelTimeRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelVolatilyRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelWeightedRisk"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelIntercomCdt"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelDeltaRemain"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTimeriskValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vVolatilyRiskValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vWeightedRiskValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vIntercomCdtValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vDeltaRemainValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

          </fo:table-row>


        </fo:table-body>
      </fo:table>

      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create9Columns">
          <xsl:with-param name="pColumnWidth1" select ="0.8"/>
          <xsl:with-param name="pColumnWidth2" select ="4"/>
          <xsl:with-param name="pColumnWidth3" select ="0.8"/>
          <xsl:with-param name="pColumnWidth4" select ="3.9"/>
          <xsl:with-param name="pColumnWidth5" select ="0.8"/>
          <xsl:with-param name="pColumnWidth6" select ="3.9"/>
          <xsl:with-param name="pColumnWidth7" select ="0.9"/>
          <xsl:with-param name="pColumnWidth8" select ="3.9"/>
          <xsl:with-param name="pColumnWidth9" select ="0.8"/>
        </xsl:call-template>
        <fo:table-body >

          <xsl:variable name ="vNetOptValueValue">
            <xsl:call-template name ="getNov">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vShortOptMinValue">
            <xsl:call-template name ="getSom">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vRiskMaintValue">
            <xsl:call-template name ="getMaintenancemargin">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vRiskInitialValue">
            <xsl:call-template name ="getInitialmargin">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <fo:table-row height="{$vRowHeight}cm"/>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelNetOptValue"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelShortOptMin"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelRiskMaint"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelRiskInitial"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vNetOptValueValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vShortOptMinValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vRiskMaintValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vRiskInitialValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm"/>

        </fo:table-body>
      </fo:table>

    </fo:block>

  </xsl:template>
  
  
  <xsl:template name ="displayCombinedCommodityBlock3">
    <xsl:param name ="pNode" />

    <fo:block border="{$vTableCellBorder}" >

      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create9Columns">
          <xsl:with-param name="pColumnWidth1" select ="0.8"/>
          <xsl:with-param name="pColumnWidth2" select ="4"/>
          <xsl:with-param name="pColumnWidth3" select ="0.5"/>
          <xsl:with-param name="pColumnWidth4" select ="3.9"/>
          <xsl:with-param name="pColumnWidth5" select ="0.5"/>
          <xsl:with-param name="pColumnWidth6" select ="3.9"/>
          <xsl:with-param name="pColumnWidth7" select ="1"/>
          <xsl:with-param name="pColumnWidth8" select ="3.9"/>
          <xsl:with-param name="pColumnWidth9" select ="0.8"/>
        </xsl:call-template>
        <fo:table-body >

          <xsl:variable name ="vRiskInitialValue">
            <xsl:call-template name ="getInitialmargin">
              <xsl:with-param name ="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vNetOptValueValue">
            <xsl:call-template name ="getNov">
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

          <fo:table-row height="{$vRowHeight}cm"/>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelRiskInitial"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'-'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelNetOptValue"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'='"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelDepositInit"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExcLgOptValue"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>


          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vRiskInitialValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vNetOptValueValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vDepositInitValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vExcLgOptValueInitValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm"/>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelRiskMaint"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'-'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelNetOptValue"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="'='"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelDepositMaint"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExcLgOptValue"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody"/>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vRiskMaintValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vNetOptValueValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vDepositMaintValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vExcLgOptValueMaintValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm"/>

        </fo:table-body>
      </fo:table>

    </fo:block>

  </xsl:template>
  
    
  <xsl:template name ="displayReportRemarks">

    <fo:block border="{$vTableCellBorder}" >

      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create2Columns">
          <xsl:with-param name="pColumnWidth1" select ="0.5"/>
          <xsl:with-param name="pColumnWidth2" select ="15"/>
        </xsl:call-template>
        <fo:table-body >

          <fo:table-row height="{$vRowHeight}cm"/>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRemarques"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisqueDeScenario"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisqueDeTemps"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisqueVolatilite"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisquePondere"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisqueProduit"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisqueMaintenance"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pBorderLeftStyle" select ="$vTableCellBorderLeftStyle"/>
              <xsl:with-param name ="pBorderLeftWidth" select ="$vTableCellBorderLeftWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vTextRisqueInitial"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm"/>

        </fo:table-body>
      </fo:table>

    </fo:block>

  </xsl:template>

</xsl:stylesheet>