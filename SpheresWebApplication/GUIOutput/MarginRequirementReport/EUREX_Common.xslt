<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
                xmlns:xsl  ="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt   ="http://xsltsl.org/date-time"
                xmlns:fo   ="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:efs  ="http://www.efs.org/2007/EFSmL-3-0"
                xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
                xmlns:fpml ="http://www.fpml.org/2007/FpML-4-4">

  <!-- xslt includes -->
  <xsl:include href="..\..\Library\Resource.xslt" />
  <xsl:include href="..\..\Library\DtFunc.xslt" />
  <xsl:include href="..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\Library\xsltsl/date-time.xsl" />
  <xsl:include href="..\..\Library\StrFunc.xslt" />
  <!-- *************************************************** -->

  <!-- true is gross margin / false is net margin-->
  <xsl:variable name ="vIsGross">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/@isGross"/>
  </xsl:variable>

  <!--! ================================================================================ -->
  <!--!                      VARIABLES A4 VERTICAL                                       -->
  <!--! ================================================================================ -->
  <!-- A4 page caracteristics Vertical ================================================== -->
  <xsl:variable name="vPageA4VerticalPageHeight">29.7</xsl:variable>
  <xsl:variable name="vPageA4VerticalPageWidth">21</xsl:variable>
  <xsl:variable name="vPageA4VerticalMargin">0.5</xsl:variable>
  <!-- Variables for Header Vertical ====================================================== -->
  <xsl:variable name ="vPageA4VerticalHeaderExtent">
    <xsl:value-of select ="$vPageA4VerticalMargin + $vLogoHeight + ($vRowHeight * 6) "/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderExtentFirst">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 9)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderExtentRest">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderLeftMargin">1.5</xsl:variable>
  <!-- Variables for Footer Vertical====================================================== -->
  <xsl:variable name ="vPageA4VerticalFooterExtent">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vPageA4VerticalFooterFontSize">7pt</xsl:variable>
  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vPageA4VerticalBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyTopMargin">
    <xsl:value-of select ="$vPageA4VerticalHeaderExtent"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyTopMarginFirst">
    <xsl:value-of select ="$vPageA4VerticalHeaderExtentFirst"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyTopMarginRest">
    <xsl:value-of select ="$vPageA4VerticalHeaderExtentRest"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyBottomMargin">
    <xsl:value-of select ="$vPageA4VerticalFooterExtent+$vPageA4VerticalMargin"/>
  </xsl:variable>

  <!--! =================================================================================== -->
  <!--!                      VARIABLES A4 LANDSCAPE                                         -->
  <!--! =================================================================================== -->
  <!-- A4 page caracteristics Lanscape ================================================== -->
  <xsl:variable name="vPageA4LandscapePageHeight">21</xsl:variable>
  <xsl:variable name="vPageA4LandscapePageWidth">29.7</xsl:variable>
  <xsl:variable name="vPageA4LandscapeMargin">0.5</xsl:variable>
  <!-- Variables for Header Landscape ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeHeaderExtent">
    <xsl:value-of select ="$vPageA4LandscapeMargin + $vLogoHeight + ($vRowHeight * 5) "/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderExtentFirst">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 9)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderExtentRest">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderLeftMargin">1.5</xsl:variable>
  <!-- Variables for Footer Landscape====================================================== -->
  <xsl:variable name ="vPageA4LandscapeFooterExtent">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeFooterFontSize">7pt</xsl:variable>
  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyTopMargin">
    <xsl:value-of select ="$vPageA4LandscapeHeaderExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyTopMarginFirst">
    <xsl:value-of select ="$vPageA4LandscapeHeaderExtentFirst+$vPageA4LandscapeMargin"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyTopMarginRest">
    <xsl:value-of select ="$vPageA4LandscapeHeaderExtentRest+$vPageA4LandscapeMargin"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyBottomMargin">
    <xsl:value-of select ="$vPageA4LandscapeFooterExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>
  <!--! =================================================================================== -->
  <!--!                      COMMON VARIABLES                                               -->
  <!--! =================================================================================== -->
  <xsl:variable name="vIsDebug">0</xsl:variable>
  <xsl:variable name="vBorder">
    <xsl:choose>
      <xsl:when test="$vIsDebug=1">0.5pt solid black</xsl:when>
      <xsl:otherwise>0pt solid black</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name ="vEntity">
    <xsl:text>ENTITY</xsl:text>
  </xsl:variable>

  <xsl:variable name="varImgLogo">
    <xsl:value-of select="concat('sql(select top(1) IDENTIFIER, LOLOGO from dbo.ACTOR a inner join dbo.ACTORROLE ar on ar.IDA = a.IDA where IDROLEACTOR =', $vApos, $vEntity, $vApos, 'and a.IDENTIFIER !=', $vApos, 'EFSLIC', $vApos,  ')')" />
  </xsl:variable>

  <xsl:variable name ="vLogoHeight">1.35</xsl:variable>

  <xsl:variable name ="vApos">'</xsl:variable>

  <xsl:variable name="vSpaceCharacter">&#160;</xsl:variable>

  <xsl:variable name ="vFillCharacter">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=false()">
        <xsl:text>*</xsl:text>
      </xsl:when>
      <xsl:otherwise>-</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- *************************************************** -->
  <xsl:variable name ="vPageWidth">
    <xsl:choose>
      <xsl:when test ="$vIsA4VerticalReport=true()">
        <xsl:value-of select ="$vPageA4VerticalPageWidth"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vPageA4LandscapePageWidth"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- *************************************************** -->
  <xsl:variable name ="vPageMargin">
    <xsl:choose>
      <xsl:when test ="$vIsA4VerticalReport=true()">
        <xsl:value-of select ="$vPageA4VerticalMargin"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vPageA4LandscapeMargin"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- font ================================================================================= -->
  <!-- sans-serif/serif/Courier/Helvetica/Times/ -->
  <!-- EG 20160404 Migration vs2013 ok -->
  <xsl:variable name ="vFontFamily">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">sans-serif</xsl:when>
      <xsl:otherwise>Courier</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- EG 20160404 Migration vs2013 ok -->
  <xsl:variable name ="vFontSize">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">8pt</xsl:when>
      <xsl:otherwise>9pt</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name ="vFooterFontFamily">sans-serif</xsl:variable>
  <xsl:variable name ="vFooterFontSize">7pt</xsl:variable>
  <!-- Variables for Block ====================================================== -->
  <xsl:variable name ="vBlockLeftMargin">0.1</xsl:variable>
  <xsl:variable name ="vBlockRightMargin">1</xsl:variable>
  <xsl:variable name ="vBlockTopMargin">0.2</xsl:variable>
  <!-- Variables for table-cell ====================================================== -->
  <xsl:variable name ="vTableCellBackgroundColor">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <!--DarkGrey-->
        <xsl:text>LightGrey</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>White</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name ="vTableCellBackgroundColorLight">
    <xsl:choose>
      <!--LightGrey/Silver-->
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>LightGrey</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>White</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name ="vTableCellFontWeight">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>bold</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>normal</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="vTableCellBorder">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>0.2pt solid black</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>0pt solid black</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="vTableCellBorderTopStyle">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>none</xsl:text>
      </xsl:when>
      <!-- hidden dotted dashed solid double groove ridge inset  outset   -->
      <xsl:otherwise>
        <xsl:text>dashed</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="vTableCellBorderTopWidth">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>0</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <!--thin thick medium-->
        <xsl:text>medium</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="vTableCellBorderBottomStyle">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>none</xsl:text>
      </xsl:when>
      <!-- hidden dotted dashed solid double groove ridge inset  outset   -->
      <xsl:otherwise>
        <xsl:text>dashed</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="vTableCellBorderBottomWidth">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>0</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <!--thin thick medium-->
        <xsl:text>medium</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="vBorderCollapse">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>0</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>separate</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="vBorderSeparation">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>0pt</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>3pt</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>


  <xsl:variable name ="vTableCellPadding">
    <xsl:choose>
      <xsl:when test ="$vIsCustomReport=true()">
        <xsl:text>0.05</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>0.15</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="vTableCellDisplayAlign">center</xsl:variable>
  <xsl:variable name ="vRowHeight">0.5</xsl:variable>
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>
  <!-- Variables for header titles ====================================================== -->
  <xsl:variable name ="vLabelMarket">
    <xsl:text>EUREX</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelConfidential">
    <xsl:text>Confidential</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelConfidential_Upper">
    <xsl:text>CONFIDENTIAL</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAsOfDate">
    <xsl:text>As of date :</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelRunDate">
    <xsl:text>Run date   :</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAsOfDate_Upper">
    <xsl:text>AS OF DATE :</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelRunDate_Upper">
    <xsl:text>RUN DATE    :</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelBeginningOfReport">
    <xsl:text>BEGINNING OF REPORT</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelEndOfReport">
    <xsl:text>*** END OF REPORT ***</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelClearingHouse">
    <xsl:text>Clearing House</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelClearingMember">
    <xsl:text>Clearing Member</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelExchangeMember">
    <xsl:text>Exchange Member</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelClearingCurrency">
    <xsl:text>Clearing Currency</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAccount">
    <xsl:text>Account</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMgnGrp">
    <xsl:text>MgnGrp</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOfsFac">
    <xsl:text>OfsFac</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMgnCl">
    <xsl:text>MgnCl</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelExpiry">
    <xsl:text>Expiry</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCur">
    <xsl:text>Cur</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCurr">
    <xsl:text>Curr</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelWorstCaseScenario">
    <xsl:text>Worst case scenario</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPureAdditionalMgn">
    <xsl:text>Pure Additional Mgn</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMgnFct">
    <xsl:text>MgnFct %</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAdditionalMgn">
    <xsl:text>Additional Mgn</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelScenario">
    <xsl:text>Scenario</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPriceUpVolaUp">
    <xsl:text>PriceUpVolaUp</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPriceUpVolaNeut">
    <xsl:text>PriceUpVolaNeut</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPriceUpVolaDown">
    <xsl:text>PriceUpVolaDown</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPriceDownVolaUp">
    <xsl:text>PriceDownVolaUp</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPriceDownVolaNeut">
    <xsl:text>PriceDownVolaNeut</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPriceDownVolaDown">
    <xsl:text>PriceDownVolaDown</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelRisk">
    <xsl:text>Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOffset">
    <xsl:text>Offset</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelGroupTot">
    <xsl:text>GroupTot</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPremMgn">
    <xsl:text>Prem Mgn</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCurtLiq">
    <xsl:text>Curt Liq / Dlv Mgn</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelFutSprdMgn">
    <xsl:text>Fut Sprd Mgn</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAddMgn">
    <xsl:text>Add Mgn</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelUnadMgnReqr">
    <xsl:text>Unad Mgn Reqr</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotal">
    <xsl:text>Total</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelWeightingFactor">
    <xsl:text>Weighting Factor</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMarginAmount">
    <xsl:text>Margin Amount</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAcct">
    <xsl:text>Acct/Risk Netting Group</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelLiquidationGroup">
    <xsl:text>Liquidation Group</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMgType">
    <xsl:text>MgType</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vValueInitial">
    <xsl:text>Initial</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelExpGrp">
    <xsl:text>Liq. Grp. Split</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSubPortIdentifier">
    <xsl:text>Sub-Port. Identifier</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMarketRisk">
    <xsl:text>Market Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelLiquidityRisk">
    <xsl:text>Liquidity Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelLongOptionCredit">
    <xsl:text>Long Option Credit</xsl:text>
  </xsl:variable>
  <!-- PM 20180903 [24015] Prisma v8.0 : add Time to Expiry Adjustment -->
  <xsl:variable name ="vLabelTimeToExpiryAdjustment">
    <xsl:text>Time to Expiry Adj</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelInitialMargin">
    <xsl:text>Initial Margin</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPremiumMargin">
    <xsl:text>Premium Margin</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotalPremiumMargin">
    <xsl:text>Total Premium Margin</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotalInitialMargin">
    <xsl:text>Total Initial Margin</xsl:text>
  </xsl:variable>

  <!--! =================================================================================== -->
  <!--!   BUSINESS VARIABLES (PREFIX V) AND TEMPLATES(PREFIX GET)                 -->
  <!--! =================================================================================== -->

  <!-- this variable returns the Margin Calculation Method Name available into XML -->
  <xsl:variable name ="vMarginCalculationMethodName">
    <xsl:value-of select ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@name"/>
  </xsl:variable>

  <!-- return trade date -->
  <xsl:variable name ="vISODate">
    <xsl:value-of select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate"/>
  </xsl:variable>

  <!-- when parameter pCurrentCulture= 'en-GB' it returns: 6 JUL 2001-->
  <!-- when parameter pCurrentCulture= 'it-IT' it returns: 6 Luglio 2001-->
  <xsl:template name="getTradeDate">
    <xsl:param name="pDate"/>

    <xsl:variable name="vDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$pDate"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select="normalize-space($vDate)"/>

  </xsl:template>

  <!-- this variable returns value from getTradeDate template -->
  <xsl:variable name ="vTradeDate">
    <xsl:call-template name ="getTradeDate">
      <xsl:with-param name="pDate" select="$vISODate"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- input format date = YYYY-MM-DD -->
  <!-- output format date = DD-MM-YYYY -->
  <xsl:template name="formatShortDate2">
    <xsl:param name="xsd-date-time"/>

    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="'%d-%m-%Y'"/>
    </xsl:call-template>

  </xsl:template>

  <xsl:variable name="vPrismaTradeDate">
    <xsl:call-template name="formatShortDate2">
      <xsl:with-param name="xsd-date-time" select="$vISODate"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- return today date time (eg: 6 JUL 2001 + 14:45)-->
  <xsl:template name ="getDateTime">
    <xsl:variable name ="vFormatDate">
      <xsl:call-template name ="getTradeDate"/>
    </xsl:variable>
    <xsl:variable name ="vFormatTime">
      <xsl:value-of select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate/@timeStamp"/>
    </xsl:variable>
    <xsl:value-of select="concat (normalize-space($vFormatDate), ' ', $vFormatTime)"/>
  </xsl:template>

  <!-- this variable returns value from getDateTime template -->
  <xsl:variable name ="vDateTime">
    <xsl:call-template name ="getDateTime"/>
  </xsl:variable>

  <!-- returns formatted number with 2 decimal pattern using current culture-->
  <xsl:template name ="formatMoney">
    <xsl:param name ="pAmount"/>

    <xsl:if test ="$pAmount != ''">
      <xsl:call-template name="format-number">
        <xsl:with-param name="pAmount" select="$pAmount" />
        <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <!-- return amount value from marginAmount node (amount is a attribute of the node)-->
  <xsl:template name ="getMarginAmount">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:choose>
        <xsl:when test ="$pNode/efs:amount/@amount">
          <xsl:value-of select ="$pNode/efs:amount/@amount"/>
        </xsl:when>
        <xsl:when test ="$pNode/efs:marginAmount/@amount">
          <xsl:value-of select ="$pNode/efs:marginAmount/@amount"/>
        </xsl:when>
        <xsl:when test ="$pNode/fpml:amount">
          <xsl:value-of select ="$pNode/fpml:amount"/>
        </xsl:when>
        <xsl:when test ="$pNode/efs:margin/@amount">
          <xsl:value-of select ="$pNode/efs:margin/@amount"/>
        </xsl:when>
        <xsl:when test ="$pNode/efs:risk/@amount">
          <xsl:value-of select ="$pNode/efs:risk/@amount"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- returns Margin Requirement Office party name-->
  <xsl:template name ="getPartyName">
    <xsl:value-of select ="//efs:EfsML/efs:party/efs:Party[@id=//efs:EfsML/efs:marginRequirementOffice/@href]/fpml:partyName"/>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getMarginCurrency">
    <xsl:param name ="pNode"/>

    <xsl:choose>
      <xsl:when test ="$pNode/fpml:currency">
        <xsl:value-of select ="$pNode/fpml:currency"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$pNode/@curr"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- return Weighting Factor -->
  <xsl:template name="getWeightingFactor">
    <xsl:value-of select="//efs:marginRequirementOffice/@wratio"/>
  </xsl:template>

  <!-- return Exchange member -->
  <xsl:template name ="getExchangeMember">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:payerPartyReference/@href"/>
  </xsl:template>

  <!-- this variable returns value from getExchangeMember template -->
  <xsl:variable name ="vExchangeMember">
    <xsl:call-template name ="getExchangeMember"/>
  </xsl:variable>

  <!-- returns Clearing member -->
  <xsl:template name ="getClearingMember">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:receiverPartyReference/@href"/>
  </xsl:template>

  <!-- this variable returns value from getClearingMember template -->
  <xsl:variable name ="vClearingMember">
    <xsl:call-template name ="getClearingMember"/>
  </xsl:variable>

  <!-- returns Book of Exchange member (marginRequirementOffice) -->
  <xsl:template name ="getBook">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:bookId/@bookName"/>
  </xsl:template>

  <!-- this variable returns value from getBook template -->
  <xsl:variable name ="vBook">
    <xsl:call-template name ="getBook"/>
  </xsl:variable>

  <!-- *************************************************** -->
  <xsl:template name ="getGroupName">
    <xsl:param name ="pGroupNode"/>
    <xsl:value-of select ="$pGroupNode/@name"/>
  </xsl:template>

  <!-- returns sidepoint displayname from sidepoint ID-->
  <xsl:template name ="getSidepointDisplayname">
    <xsl:param name ="pId"/>
    <xsl:choose>
      <xsl:when test ="$pId='PUVU'">
        <xsl:text>PriceUpVolaUp</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PUVN'">
        <xsl:text>PriceUpVolaNeutr</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PUVD'">
        <xsl:text>PriceUpVolaDown</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PDVU'">
        <xsl:text>PriceDownVolaUp</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PDVN'">
        <xsl:text>PriceDownVolaNeutr</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PDVD'">
        <xsl:text>PriceDownVolaDown</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PNVU'">
        <xsl:text>PriceNeutVolaUp</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PNVN'">
        <xsl:text>PriceNeutVolaNeutr</xsl:text>
      </xsl:when>
      <xsl:when test ="$pId='PNVD'">
        <xsl:text>PriceNeutVolaDown</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$pId"/>
        <xsl:text> [Not transcoded]</xsl:text>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getOffsetFactor">
    <xsl:param name ="pNode"/>
    <xsl:choose>
      <xsl:when test ="$pNode/@offset">
        <xsl:value-of select ="$pNode/@offset"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- EUREX report display null if offset factor is missing -->
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getClassName">
    <xsl:param name ="pNode"/>
    <xsl:value-of select ="$pNode/@name"/>
  </xsl:template>

  <!-- returns the Worst Factor Id-->
  <xsl:template name ="getWorstFactorId">
    <xsl:param name ="pNode"/>
    <xsl:value-of select ="$pNode/efs:additional/efs:factors/efs:factor[@ID and @ID != 'Risk' and @ID != 'RiskWithNeutralPrices' and @ID != 'RiskWithNeutralPricesAndOffset']/@ID"/>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getWorstFactorMaturity">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pWorstFactorId"/>
    <xsl:value-of select ="$pNode/efs:additional/efs:factors/efs:factor[@ID=$pWorstFactorId]/@maturity"/>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getExpiry">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vWorstFactorId">
      <xsl:call-template name="getWorstFactorId">
        <xsl:with-param name ="pNode" select="$pNode"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vWorstFactorMaturity">
      <xsl:call-template name ="getWorstFactorMaturity">
        <xsl:with-param name ="pNode" select ="$pNode"/>
        <xsl:with-param name ="pWorstFactorId" select ="$vWorstFactorId"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vWorstFactorMaturity"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getWorstCaseScenario">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vWorstFactorId">
      <xsl:call-template name="getWorstFactorId">
        <xsl:with-param name ="pNode" select="$pNode"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vWorstCaseScenario">
      <xsl:call-template name ="getSidepointDisplayname">
        <xsl:with-param name ="pId" select ="$vWorstFactorId"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vWorstCaseScenario"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getPureAdditionalMargin">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vWorstFactorId">
      <xsl:call-template name="getWorstFactorId">
        <xsl:with-param name ="pNode" select="$pNode"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode/efs:additional/efs:factors/efs:factor[@ID=$vWorstFactorId]"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getMarginFactor">
    <xsl:param name ="pNode"/>
    <xsl:choose>
      <xsl:when test ="$pNode/@mgnfct">
        <xsl:choose>
          <!--GS/FL 20121212 Display 100 when margin factor = 0 -->
          <xsl:when test="$pNode/@mgnfct='0'">
            <xsl:text>100</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="$pNode/@mgnfct"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <!-- EUREX report display null if offset factor is missing -->
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getAdditionalMargin">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode/efs:additional"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getSidepointMarginAmount">
    <xsl:param name="pFactorNode"/>
    <xsl:param name="pSidepointId"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pFactorNode/efs:riskarray/efs:sidepoint[@ID=$pSidepointId]"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getPremium">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode/efs:premium"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- *************************************************** -->
  <!-- to do: not available value into XMl flux-->
  <xsl:template name ="getCurtLiqDelivery">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode/efs:delivery"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getSpread">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode/efs:spread"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getAdditional">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode/efs:additional"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getUnadjustedMarginReqr">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="$pNode"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>

  </xsl:template>

  <!-- *************************************************** -->
  <!-- Total premium from Liquidation group (split sum)    -->
  <!-- In use into CP046 Prisma Report                     -->
  <!-- *************************************************** -->
  <!--
  <xsl:template name ="getTotalLiquidationGroupPremium">
    <xsl:param name ="pNode"/>

    <xsl:variable name ="vAmount">
      <xsl:value-of select ="sum($pNode/efs:liquidationGroupSplit/efs:split/efs:premiumMargin/efs:amount/@amount)"/>
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  -->


  <xsl:template name ="getTotalPremiumForCurrency">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vAmount">
      <xsl:value-of select ="sum(//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:marginAmount/@curr=$pCurrency]/efs:classes/efs:class/efs:premium/efs:amount[@curr=$pCurrency]/@amount)"/>
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getTotalCurtLiqDeliveryForCurrency">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vAmount">
      <xsl:value-of select="0"/>
      <!--<xsl:value-of select ="sum(//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:MarginMoney/@curr=$pCurrency]/efs:classes/efs:class/efs:premium/efs:marginAmount[@curr=$pCurrency]/@amount)"/>-->
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>

        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getTotalSpreadForCurrency">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vAmount">
      <xsl:value-of select ="sum(//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:marginAmount/@curr=$pCurrency]/efs:classes/efs:class/efs:spread/efs:amount[@curr=$pCurrency]/@amount)"/>
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>

        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getTotalAdditionalForCurrency">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vAmount">
      <xsl:value-of select ="sum(//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:marginAmount/@curr=$pCurrency]/efs:classes/efs:class/efs:additional/efs:amount[@curr=$pCurrency]/@amount)"/>
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>

        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getTotalUnadjustedMarginReqrForCurrency">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vAmount">
      <xsl:value-of select ="sum(//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:groups/efs:group[efs:marginAmounts/efs:marginAmount/@curr=$pCurrency]/efs:classes/efs:class/efs:marginAmount[@curr=$pCurrency]/@amount)"/>
    </xsl:variable>

    <xsl:variable name ="vFormatMoney">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring($vFormatMoney,1,1) = '-'">
        <xsl:variable name ="vAbsoluteValue">
          <xsl:value-of select="substring-after($vFormatMoney,'-')"/>
        </xsl:variable>
        <xsl:variable name ="vSign">
          <xsl:value-of select ="substring($vFormatMoney,1,1)"/>
        </xsl:variable>
        <xsl:value-of select ="concat($vAbsoluteValue,$vSign)"/>
      </xsl:when>
      <xsl:otherwise>

        <xsl:value-of select ="concat($vFormatMoney,$vSpaceCharacter)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="getAccountMarginRequirementAmount">
    <xsl:param name ="pCurrency"/>

    <xsl:variable name ="vAmount">
      <xsl:call-template name ="getMarginAmount">
        <xsl:with-param name ="pNode" select="//efs:marginRequirementOffice/efs:marginAmounts/efs:Money[fpml:currency=$pCurrency]"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- PRISMA SPECIFIC BUSINESS TEMPLATES                         -->
  <!-- =========================================================  -->
  <xsl:template name ="getSplitName">
    <xsl:param name ="pNode"/>
    <!--PM 20140623 [19911] Multiple LGS
    <xsl:value-of select="$pNode/efs:split/@name"/>-->
    <xsl:value-of select="$pNode/@name"/>
  </xsl:template>





  <!-- =========================================================  -->
  <!-- LAYOUT TEMPLATES                                           -->
  <!-- =========================================================  -->

  <xsl:template name ="setPagesCaracteristics">

    <fo:layout-master-set>

      <!-- A4 VERTICAL -->
      <!-- Spheres layout report-->
      <!-- EG 20160404 Migration vs2013 ok -->
      <!--<fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin="{$vPageA4VerticalMargin}cm">-->
      <fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin-left="{$vPageA4VerticalMargin}cm" margin-top="{$vPageA4VerticalMargin}cm" margin-right="{$vPageA4VerticalMargin}cm" margin-bottom="{$vPageA4VerticalMargin}cm">
        <fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMargin}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />
        <fo:region-before region-name="A4VerticalHeader" background-color="white" extent="{$vPageA4VerticalHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4VerticalFooter" background-color="white" extent="{$vPageA4VerticalFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
      <!-- EUREX layout report first page -->
      <!-- EG 20160404 Migration vs2013 ok -->
      <!--<fo:simple-page-master master-name="A4VerticalFirstPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin="{$vPageA4VerticalMargin}cm">-->
      <fo:simple-page-master master-name="A4VerticalFirstPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin-left="{$vPageA4VerticalMargin}cm" margin-top="{$vPageA4VerticalMargin}cm" margin-right="{$vPageA4VerticalMargin}cm" margin-bottom="{$vPageA4VerticalMargin}cm">
        <fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMarginFirst}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />
        <fo:region-before region-name="A4VerticalHeaderFirstPage" background-color="white" extent="{$vPageA4VerticalHeaderExtentFirst}cm"  precedence="true" />
        <fo:region-after region-name="A4VerticalFooter" background-color="white" extent="{$vPageA4VerticalFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
      <!-- EUREX layout report rest pages -->
      <!-- EG 20160404 Migration vs2013 ok -->
      <!--<fo:simple-page-master master-name="A4VerticalRestPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin="{$vPageA4VerticalMargin}cm">-->
      <fo:simple-page-master master-name="A4VerticalRestPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin-left="{$vPageA4VerticalMargin}cm" margin-top="{$vPageA4VerticalMargin}cm" margin-right="{$vPageA4VerticalMargin}cm" margin-bottom="{$vPageA4VerticalMargin}cm">
        <fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMarginRest}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />
        <fo:region-before region-name="A4VerticalHeaderRestPage" background-color="white" extent="{$vPageA4VerticalHeaderExtentRest}cm"  precedence="true" />
        <fo:region-after region-name="A4VerticalFooter" background-color="white" extent="{$vPageA4VerticalFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>

      <!-- A4 LANDSCAPE -->
      <!-- Spheres layout report-->
      <!-- EG 20160404 Migration vs2013 ok -->
      <!--<fo:simple-page-master master-name="A4LandscapePage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">-->
      <fo:simple-page-master master-name="A4LandscapePage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin-left="{$vPageA4LandscapeMargin}cm" margin-top="{$vPageA4LandscapeMargin}cm" margin-right="{$vPageA4LandscapeMargin}cm" margin-bottom="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vPageA4LandscapeBodyLeftMargin}cm" margin-top="{$vPageA4LandscapeBodyTopMargin}cm" margin-bottom="{$vPageA4LandscapeBodyBottomMargin}cm" />
        <fo:region-before region-name="A4LandscapeHeader" background-color="white" extent="{$vPageA4LandscapeHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4LandscapeFooter" background-color="white" extent="{$vPageA4LandscapeFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
      <!-- EUREX layout report first page -->
      <!-- EG 20160404 Migration vs2013 ok -->
      <!--<fo:simple-page-master master-name="A4LandscapeFirstPage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">-->
      <fo:simple-page-master master-name="A4LandscapeFirstPage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin-left="{$vPageA4LandscapeMargin}cm" margin-top="{$vPageA4LandscapeMargin}cm" margin-right="{$vPageA4LandscapeMargin}cm" margin-bottom="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vPageA4LandscapeBodyLeftMargin}cm" margin-top="{$vPageA4LandscapeBodyTopMarginFirst}cm" margin-bottom="{$vPageA4LandscapeBodyBottomMargin}cm" />
        <fo:region-before region-name="A4LandscapeHeaderFirstPage" background-color="white" extent="{$vPageA4LandscapeHeaderExtentFirst}cm"  precedence="true" />
        <fo:region-after region-name="A4LandscapeFooter" background-color="white" extent="{$vPageA4LandscapeFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
      <!-- EUREX layout report rest pages -->
      <!-- EG 20160404 Migration vs2013 ok -->
      <!--<fo:simple-page-master master-name="A4LandscapeRestPage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">-->
      <fo:simple-page-master master-name="A4LandscapeRestPage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin-left="{$vPageA4LandscapeMargin}cm" margin-top="{$vPageA4LandscapeMargin}cm" margin-right="{$vPageA4LandscapeMargin}cm" margin-bottom="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vPageA4LandscapeBodyLeftMargin}cm" margin-top="{$vPageA4LandscapeBodyTopMarginRest}cm" margin-bottom="{$vPageA4LandscapeBodyBottomMargin}cm" />
        <fo:region-before region-name="A4LandscapeHeaderRestPage" background-color="white" extent="{$vPageA4LandscapeHeaderExtentRest}cm"  precedence="true" />
        <fo:region-after region-name="A4LandscapeFooter" background-color="white" extent="{$vPageA4LandscapeFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>

      <xsl:choose>
        <xsl:when test ="$vIsA4VerticalReport=true()">
          <xsl:if test ="$vIsCustomReport=false()">
            <fo:page-sequence-master master-name="A4VerticalFirstPageDifferent">
              <fo:repeatable-page-master-alternatives>
                <fo:conditional-page-master-reference page-position="first" master-reference="A4VerticalFirstPage"/>
                <fo:conditional-page-master-reference page-position="rest" master-reference="A4VerticalRestPage"/>
              </fo:repeatable-page-master-alternatives>
            </fo:page-sequence-master>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test ="$vIsCustomReport=false()">
            <fo:page-sequence-master master-name="A4LandscapeFirstPageDifferent">
              <fo:repeatable-page-master-alternatives>
                <fo:conditional-page-master-reference page-position="first" master-reference="A4LandscapeFirstPage"/>
                <fo:conditional-page-master-reference page-position="rest" master-reference="A4LandscapeRestPage"/>
              </fo:repeatable-page-master-alternatives>
            </fo:page-sequence-master>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </fo:layout-master-set>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="handleStaticContentHeader">
    <xsl:param name ="pEurexMethod" />
    <xsl:choose>
      <!-- vertical report -->
      <xsl:when test ="$vIsA4VerticalReport=true()">
        <xsl:choose>
          <xsl:when test ="$vIsCustomReport=true()">
            <fo:static-content flow-name="A4VerticalHeader">
              <xsl:call-template name="displayHeader"/>
            </fo:static-content>
          </xsl:when>
          <xsl:otherwise>
            <fo:static-content flow-name="A4VerticalHeaderFirstPage">
              <xsl:call-template name="displayHeaderFirstPage">
                <xsl:with-param name ="pEurexMethod" select ="$pEurexMethod"/>
              </xsl:call-template>
            </fo:static-content>
            <fo:static-content flow-name="A4VerticalHeaderRestPage">
              <xsl:call-template name="displayHeaderRestPage"/>
            </fo:static-content>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- landscape report -->
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test ="$vIsCustomReport=true()">
            <fo:static-content flow-name="A4LandscapeHeader">
              <xsl:call-template name="displayHeader"/>
            </fo:static-content>
          </xsl:when>
          <xsl:otherwise>
            <fo:static-content flow-name="A4LandscapeHeaderFirstPage">
              <xsl:call-template name="displayHeaderFirstPage">
                <xsl:with-param name ="pEurexMethod" select ="$pEurexMethod"/>
              </xsl:call-template>
            </fo:static-content>
            <fo:static-content flow-name="A4LandscapeHeaderRestPage">
              <xsl:call-template name="displayHeaderRestPage"/>
            </fo:static-content>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayHeaderFirstPage">
    <xsl:param name ="pEurexMethod" />

    <xsl:call-template name ="displayHeaderFillingRow"/>

    <xsl:choose>
      <xsl:when test="$pEurexMethod='EUREX PRISMA'">
        <xsl:call-template name ="displayPrismaHeaderContentRows"/>
      </xsl:when>
      <xsl:when test="$pEurexMethod='TIMS EUREX'">
        <xsl:call-template name ="displayHeaderContentRows"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name ="displayHeaderContentRows"/>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:call-template name ="displayHeaderFillingRow"/>

  </xsl:template>


  <!-- *************************************************** -->
  <!-- draw header content rows (titles of report)-->
  <!-- it is used in standard PRISMA EUREX report -->
  <xsl:template name ="displayPrismaHeaderContentRows">
    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create5Columns">
          <xsl:with-param name="pColumnWidth1" select ="10"/>
          <xsl:with-param name="pColumnWidth2" select ="1"/>
        </xsl:call-template>

        <xsl:variable name ="vStartIndentMin">0.5</xsl:variable>
        <xsl:variable name ="vStartIndentMax">1</xsl:variable>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelMarket"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelConfidential_Upper"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelReportName"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMin"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vExchangeMember"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMax"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat ($vLabelAsOfDate_Upper, '   ', $vPrismaTradeDate)"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMax"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat ($vLabelRunDate_Upper, '   ', $vPrismaTradeDate) "/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMax"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelBeginningOfReport"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>




  <!-- *************************************************** -->
  <!-- draw header content rows (titles of report)-->
  <!-- it is used in standard TIMS EUREX report -->
  <xsl:template name ="displayHeaderContentRows">
    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create5Columns">
          <xsl:with-param name="pColumnWidth1" select ="10"/>
          <xsl:with-param name="pColumnWidth2" select ="1"/>
        </xsl:call-template>

        <xsl:variable name ="vStartIndentMin">0.5</xsl:variable>
        <xsl:variable name ="vStartIndentMax">1</xsl:variable>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelMarket"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelConfidential"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelReportName"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMin"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vExchangeMember"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMax"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat ($vLabelAsOfDate, '   ', $vTradeDate)"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMax"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="concat ($vLabelRunDate, '   ', $vTradeDate) "/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pStartIndent" select ="$vStartIndentMax"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelBeginningOfReport"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vFillCharacter"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
            </xsl:call-template>
          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayHeaderRestPage">
    <fo:block linefeed-treatment="preserve">
      <!-- This field is intentionally left blank -->
    </fo:block>
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 1 columns (for Landscape and Vertical page) -->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name="create1Column">
    <!--<fo:table-column column-width="100%" column-number="01"/>-->
    <fo:table-column column-width="proportional-column-width(100)" column-number="01"/>
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 2 columns (for Landscape and Vertical page) -->
  <!-- mandatory parameters: pColumnWidth1 -->
  <!-- optional parameters: pColumnWidth2 -->
  <!-- ============================================================================ -->
  <!-- case 1 (column2 width is dynamically calcultated) -->
  <!-- column2 width is dynamically calcultated when pColumnWidth2 param is missing -->
  <!--
  <xsl:call-template name="createLandscape2Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
  </xsl:call-template>
  -->
  <!-- ============================================================================ -->
  <!-- case 2 (draws two columns with static width) -->
  <!--
  <xsl:call-template name="createLandscape2Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth1" select="19.7"/>
  </xsl:call-template>
  -->
  <!-- ============================================================================ -->
  <xsl:template name="create2Columns">
    <xsl:param name ="pColumnWidth1"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">

      <xsl:choose>
        <xsl:when test ="$pColumnWidth2=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth2"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 3 columns (for Landscape and Vertical page)-->
  <!-- mandatory parameters: two parameters [pColumnWidth1 and pColumnWidth2 (or pColumnWidth3)] -->
  <!-- optional parameters: pColumnWidth2 or pColumnWidth3 -->
  <!-- Not less than two declared parameters-->
  <!-- ============================================================================ -->
  <!-- case 1 (column2 width is dynamically calcultated): middle dynamic field -->
  <!-- column2 width is dynamically calcultated when pColumnWidth2 param is missing -->
  <!--
  <xsl:call-template name="createLandscape3Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth3" select="10"/>
  </xsl:call-template>
  -->
  <!-- ============================================================================ -->
  <!-- case 2 (column3 width is dynamically calcultated): last dynamic field -->
  <!-- column3 width is dynamically calcultated when pColumnWidth3 param is missing -->
  <!--
  <xsl:call-template name="createLandscape3Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth2" select="8"/>
  </xsl:call-template>
  -->
  <!-- ============================================================================ -->
  <!-- case 3 (draws three columns with static width) -->
  <!--
  <xsl:call-template name="createLandscape3Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth2" select="10"/>
    <xsl:with-param name="pColumnWidth3" select="9.7"/>
  </xsl:call-template>
  -->
  <!-- ============================================================================ -->
  <xsl:template name="create3Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth2=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - $pColumnWidth3 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth2"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth3=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - $pColumnWidth2 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth3"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 4 columns (for Landscape and Vertical page)-->
  <!-- mandatory parameters: three parameters [pColumnWidth1 - pColumnWidth2 - pColumnWidth3] -->
  <!-- column4 width is dynamically calcultated): last dynamic field -->
  <!-- column4 width is dynamically calcultated when pColumnWidth4 param is missing -->
  <!--
  <xsl:call-template name="create4Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth2" select="8"/>
    <xsl:with-param name="pColumnWidth3" select="5"/>
  </xsl:call-template>
  -->
  <!-- else all columns are drawed with static width) -->
  <xsl:template name="createBody4Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth4">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth4=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth4"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 5 columns (for Landscape and Vertical page)-->
  <!-- ============================================================================ -->
  <!-- mandatory parameters: two parameter [pColumnWidth1 and pColumnWidth2] -->
  <!-- 
  middle dynamic field 
  pColumnWidth5 = pColumnWidth1 (or specific)
  pColumnWidth4 = pColumnWidth2 (or specific)
  column3 width is dynamically calcultated 
  -->
  <!-- ============================================================================ -->
  <!-- 
  last dynamic field 
  pColumnWidth1 = valorized
  pColumnWidth2 = valorized
  pColumnWidth3 = valorized
  pColumnWidth4 = valorized
  column5 width is dynamically calcultated 
  -->
  <!-- ============================================================================ -->
  <xsl:template name="create5Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:choose>
        <!-- middle dynamic field -->
        <xsl:when test ="$pColumnWidth3=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <xsl:value-of select =" $vPageWidth - ($vColumnWidth1 * 2) -  ($vColumnWidth2 * 2) - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth3"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth4">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth3=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <!-- middle dynamic field (ColumnWidth4 = ColumnWidth2) -->
          <xsl:value-of select="$vColumnWidth2"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth4"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth5">
      <xsl:choose>
        <!-- middle dynamic field (ColumnWidth5 = ColumnWidth1) -->
        <xsl:when test ="$pColumnWidth3=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <xsl:value-of select="$pColumnWidth1"/>
        </xsl:when>
        <!--last dynamic field-->
        <!-- all others parameters must be valorized-->
        <xsl:when test ="$pColumnWidth3!=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pColumnWidth5"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 7 columns (for Landscape and Vertical page)-->
  <!-- mandatory parameters: first 6 parameters  -->
  <!-- column7 width is dynamically calcultated): last dynamic field -->
  <!-- column7 width is dynamically calcultated when pColumnWidth7 param is missing -->
  <!--
  <xsl:call-template name="create7Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth2" select="8"/>
    <xsl:with-param name="pColumnWidth3" select="5"/>
    .......
    .......
  </xsl:call-template>
  -->
  <!-- else all columns are drawed with static width) -->
  <xsl:template name="create7Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth7=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth7"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />

  </xsl:template>

  <!-- *************************************************** -->
  <!-- 8 columns (for Landscape and Vertical page)-->
  <!-- mandatory parameters: first seven parameters  -->
  <!-- column8 width is dynamically calcultated): last dynamic field -->
  <!-- column8 width is dynamically calcultated when pColumnWidth8 param is missing -->
  <!--
  <xsl:call-template name="create8Columns">
    <xsl:with-param name="pColumnWidth1" select="10"/>
    <xsl:with-param name="pColumnWidth2" select="8"/>
    <xsl:with-param name="pColumnWidth3" select="5"/>
    .......
    .......
  </xsl:call-template>
  -->
  <!-- else all columns are drawed with static width) -->
  <xsl:template name="create8Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>


    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth8">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth8=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth8"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
  </xsl:template>

  <!-- 9 columns -->
  <xsl:template name="create9Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>
    <xsl:param name ="pColumnWidth9" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth8">
      <xsl:value-of select ="$pColumnWidth8"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth9">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth9=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth9"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
    <fo:table-column column-width="{$vColumnWidth9}cm" column-number="09" />
  </xsl:template>

  <!-- ***************** -->
  <!-- create 11 columns -->
  <!-- ***************** -->
  <xsl:template name="create11Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>
    <xsl:param name ="pColumnWidth9" select ="0"/>
    <xsl:param name ="pColumnWidth10" select ="0"/>
    <xsl:param name ="pColumnWidth11" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth8">
      <xsl:value-of select ="$pColumnWidth8"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth9">
      <xsl:value-of select ="$pColumnWidth9"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth10">
      <xsl:value-of select ="$pColumnWidth10"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth11">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth11=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - $vColumnWidth9 - $vColumnWidth10 -($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth11"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
    <fo:table-column column-width="{$vColumnWidth9}cm" column-number="09" />
    <fo:table-column column-width="{$vColumnWidth10}cm" column-number="10" />
    <fo:table-column column-width="{$vColumnWidth11}cm" column-number="11" />
  </xsl:template>

  <!-- ***************** -->
  <!-- create 13 columns -->
  <!-- ***************** -->
  <!-- PM 20180903 [24015] Prisma v8.0 : add create13Columns -->
  <xsl:template name="create13Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>
    <xsl:param name ="pColumnWidth9" select ="0"/>
    <xsl:param name ="pColumnWidth10" select ="0"/>
    <xsl:param name ="pColumnWidth11" select ="0"/>
    <xsl:param name ="pColumnWidth12" select ="0"/>
    <xsl:param name ="pColumnWidth13" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth8">
      <xsl:value-of select ="$pColumnWidth8"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth9">
      <xsl:value-of select ="$pColumnWidth9"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth10">
      <xsl:value-of select ="$pColumnWidth10"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth11">
      <xsl:value-of select ="$pColumnWidth11"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth12">
      <xsl:value-of select ="$pColumnWidth12"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth13">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth13=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - $vColumnWidth9 - $vColumnWidth10 - $vColumnWidth11 - $vColumnWidth12 -($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth13"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
    <fo:table-column column-width="{$vColumnWidth9}cm" column-number="09" />
    <fo:table-column column-width="{$vColumnWidth10}cm" column-number="10" />
    <fo:table-column column-width="{$vColumnWidth11}cm" column-number="11" />
    <fo:table-column column-width="{$vColumnWidth12}cm" column-number="12" />
    <fo:table-column column-width="{$vColumnWidth13}cm" column-number="13" />
  </xsl:template>


  <!--! =================================================================================== -->
  <!--!                      layout templates                                               -->
  <!--! =================================================================================== -->

  <!-- *************************************************** -->
  <!-- it returns a string consisting of a given number of copies of a string concatenated together.-->
  <!-- pString = string to be concatenated -->
  <!-- pLoops =  number of copies -->
  <!-- this template replaces leader with custom content pattern syntax unhandled in Apache fop installed in Spheres -->
  <!--
  <fo:leader leader-length="100%" rule-style="solid" rule-thickness="1px" color="black" leader-pattern="use-content">
   <xsl:value-of select="$vFillCharacter"/>
  fo:leader>
  -->
  <xsl:template name="repeaterString">
    <xsl:param name="pString" select="''"/>
    <xsl:param name="pLoops" select="0"/>
    <xsl:if test="$pLoops > 0">
      <xsl:value-of select="$pString"/>
      <xsl:call-template name="repeaterString">
        <xsl:with-param name="pString" select="$pString"/>
        <xsl:with-param name="pLoops" select="$pLoops - 1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayBreakline">
    <fo:block linefeed-treatment="preserve" >
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeightBreakline}cm">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}"/>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- draw horizontal line -->
  <xsl:template name ="drawHorizontalLine">
    <fo:table table-layout="fixed" width="100%">
      <xsl:call-template name ="create1Column"/>
      <fo:table-body>
        <fo:table-row height="{$vRowHeight}cm">
          <fo:table-cell border="{$vBorder}" display-align="center">
            <fo:block border="{$vBorder}" text-align="center">
              <fo:leader leader-length="100%" leader-pattern="rule" rule-style="solid" rule-thickness="1px" color="grey" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- draw table cell for header -->
  <xsl:template name ="displayTableCellHeader">
    <xsl:param name ="pValue"/>
    <xsl:param name ="pBackgroundColor" select ="'White'"/>
    <xsl:param name ="pDisplayAlign" select ="$vTableCellDisplayAlign"/>
    <xsl:param name ="pFontSize" select ="$vFontSize"/>
    <xsl:param name ="pFontWeight" select ="'normal'"/>
    <xsl:param name ="pColor" select ="'black'"/>
    <xsl:param name ="pTextAlign"/>
    <xsl:param name ="pStartIndent" select ="0"/>

    <fo:table-cell border="{$vBorder}" display-align="{$pDisplayAlign}" background-color="{$pBackgroundColor}" >
      <fo:block border="{$vBorder}" font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}" >
        <xsl:value-of select ="$pValue"/>
      </fo:block>
    </fo:table-cell>

  </xsl:template>

  <!-- draw table cell for body-->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name ="displayTableCellBody">
    <xsl:param name ="pValue"/>
    <xsl:param name ="pBorder" select ="$vBorder"/>
    <xsl:param name ="pPadding" select ="$vTableCellPadding"/>
    <xsl:param name ="pDisplayAlign" select ="$vTableCellDisplayAlign"/>
    <xsl:param name ="pBackgroundColor" select ="'#fff'"/>
    <xsl:param name ="pBorderTopStyle" />
    <xsl:param name ="pBorderTopWidth" />
    <xsl:param name ="pBorderBottomStyle" />
    <xsl:param name ="pBorderBottomWidth" />
    <xsl:param name ="pFontSize" select ="$vFontSize"/>
    <xsl:param name ="pFontWeight" select ="'normal'"/>
    <xsl:param name ="pColor" select ="'#000'"/>
    <xsl:param name ="pTextAlign" select ="'left'"/>

    <fo:table-cell border="{$pBorder}" padding="{$pPadding}cm" display-align="{$pDisplayAlign}" background-color="{$pBackgroundColor}" >
      <fo:block font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}">
        <xsl:call-template name="DisplayBorderAttribute">
          <xsl:with-param name="pStyleName" select="'border-top-style'"/>
          <xsl:with-param name="pStyleValue" select="$pBorderTopStyle"/>
          <xsl:with-param name="pWidthName" select="'border-top-width'"/>
          <xsl:with-param name="pWidthValue" select="$pBorderTopWidth"/>
        </xsl:call-template>
        <xsl:call-template name="DisplayBorderAttribute">
          <xsl:with-param name="pStyleName" select="'border-bottom-style'"/>
          <xsl:with-param name="pStyleValue" select="$pBorderBottomStyle"/>
          <xsl:with-param name="pWidthName" select="'border-bottom-width'"/>
          <xsl:with-param name="pWidthValue" select="$pBorderBottomWidth"/>
        </xsl:call-template>
        <xsl:value-of select ="$pValue"/>
      </fo:block>
    </fo:table-cell>

  </xsl:template>

  <!-- *************************************************** -->
  <!-- draw a header filling row using filling character (*) -->
  <xsl:template name ="displayHeaderFillingRow">
    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create3Columns">
          <xsl:with-param name="pColumnWidth1" select ="10"/>
          <xsl:with-param name="pColumnWidth3" select ="10"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">

            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
              </fo:block>
            </fo:table-cell>

            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="center">
                <xsl:call-template name="repeaterString">
                  <xsl:with-param name="pString" select="$vFillCharacter"/>
                  <xsl:with-param name="pLoops" select="46"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>

            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- *************************************************** -->
  <!-- it is used in custom report -->
  <xsl:template name="displayHeader">

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create3Columns">
          <xsl:with-param name="pColumnWidth1" select ="7"/>
          <xsl:with-param name="pColumnWidth3" select ="7"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vLogoHeight}cm">

            <fo:table-cell border="{$vBorder}" text-align="left">
              <fo:block border="{$vBorder}">
                <fo:external-graphic src="{$varImgLogo}" height="{$vLogoHeight}cm" />
              </fo:block>
            </fo:table-cell>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelReportName"/>
              <xsl:with-param name ="pFontSize" select ="'14pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pDisplayAlign" select ="'after'"/>
            </xsl:call-template>

            <xsl:variable name="vTimeStamp">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'TS-Published'"/>
              </xsl:call-template>
              <xsl:text> </xsl:text>
              <!-- to do: put a real time stamp-->
              <xsl:value-of select="$vDateTime"/>
            </xsl:variable>

            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select="$vTimeStamp" />
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pDisplayAlign" select ="'before'"/>
            </xsl:call-template>

          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelMarket"/>
              <xsl:with-param name ="pFontSize" select ="'12pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pColor" select ="'grey'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pDisplayAlign" select ="'before'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelConfidential"/>
              <xsl:with-param name ="pFontSize" select ="'10pt'"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pColor" select ="'grey'"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pDisplayAlign" select ="'before'"/>
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create2Columns">
          <xsl:with-param name ="pColumnWidth1" select ="2"/>
        </xsl:call-template>

        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelAsOfDate"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vTradeDate"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vLabelRunDate"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
            <xsl:call-template name ="displayTableCellHeader">
              <xsl:with-param name ="pValue" select ="$vTradeDate"/>
              <xsl:with-param name ="pFontWeight" select ="'bold'"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <xsl:call-template name ="drawHorizontalLine"/>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="displayBodyAccountRow">
    <xsl:param name ="pCurrency"/>

    <fo:block linefeed-treatment="preserve" white-space-collapse="false"  white-space-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black">

      <!--<fo:table table-layout="fixed" width="100%" border-collapse="{$vBorderCollapse}" border-separation="{$vBorderSeparation}">-->
      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="createBody4Columns">
          <xsl:with-param name ="pColumnWidth1" select="8"/>
          <xsl:with-param name ="pColumnWidth2" select="8"/>
          <xsl:with-param name ="pColumnWidth3" select="3.8"/>
          <xsl:with-param name ="pColumnWidth4" select="7"/>
        </xsl:call-template>

        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm" keep-with-next="always">
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelClearingMember"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelExchangeMember"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelClearingCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelAccount"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm" keep-with-next="always">
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vClearingMember"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vExchangeMember"/>
              <xsl:with-param name ="pTextAlign" select ="'left'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$pCurrency"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vBook"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>
          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name ="displayTotalMarginForAccountRow">
    <xsl:param name="pCurrency"/>

    <fo:block linefeed-treatment="preserve" white-space-collapse="false"  white-space-treatment="preserve" font-family="{$vFontFamily}" font-size="{$vFontSize}" color="black">

      <fo:table table-layout="fixed" width="100%">

        <xsl:call-template name ="create2Columns">
          <xsl:with-param name ="pColumnWidth1" select="4"/>
          <xsl:with-param name ="pColumnWidth2" select="4"/>
        </xsl:call-template>
        <fo:table-body>

          <fo:table-row height="{$vRowHeight}cm" keep-with-next="always">
            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelWeightingFactor"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vLabelMarginAmount"/>
              <xsl:with-param name ="pTextAlign" select ="'center'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontWeight" select ="$vTableCellFontWeight"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vTableCellBackgroundColor"/>
              <xsl:with-param name ="pBorderBottomStyle" select ="$vTableCellBorderBottomStyle"/>
              <xsl:with-param name ="pBorderBottomWidth" select ="$vTableCellBorderBottomWidth"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm" keep-with-next="always">

            <xsl:variable name="vWeightingFactor">
              <xsl:call-template name="getWeightingFactor"/>
            </xsl:variable>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vWeightingFactor"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>

            <xsl:variable name="vAccountMarginAmount">
              <xsl:call-template name="getAccountMarginRequirementAmount">
                <xsl:with-param name="pCurrency" select="$pCurrency"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:call-template name ="displayTableCellBody">
              <xsl:with-param name ="pValue" select ="$vAccountMarginAmount"/>
              <xsl:with-param name ="pTextAlign" select ="'right'"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
            </xsl:call-template>
          </fo:table-row>

        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayGrossMarginBody">

    <xsl:for-each select ="efs:EfsML/efs:marginRequirementOffice/efs:marginAmounts/efs:Money">

      <xsl:variable name ="vMarginRequirementCurrency">
        <xsl:call-template name ="getMarginCurrency">
          <xsl:with-param name ="pNode" select ="current()"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:call-template name ="displayBodyAccountRow">
        <xsl:with-param name ="pCurrency" select ="$vMarginRequirementCurrency"/>
      </xsl:call-template>

      <xsl:call-template name ="displayTotalMarginForAccountRow">
        <xsl:with-param name ="pCurrency" select ="$vMarginRequirementCurrency"/>
      </xsl:call-template>

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


  <!-- End of report message  -->
  <xsl:template name="displayEndOfReport">
    <fo:block border="{$vBorder}" linefeed-treatment="preserve">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="center" font-family="{$vFontFamily}" font-size="{$vFontSize}">
                <xsl:value-of select ="$vLabelEndOfReport"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- Error message -->
  <xsl:template name="displayErrorMessage">
    <fo:block border="{$vBorder}" linefeed-treatment="preserve">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="center" font-family="{$vFontFamily}" font-size="{$vFontSize}" font-weight="bold">
                <!-- to do add message error: this report is not in EUREX format-->
                <xsl:text>ERROR</xsl:text>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- Display footer  -->
  <xsl:template name="displayFooter">

    <xsl:if test ="$vIsCustomReport=true()">
      <xsl:call-template name ="drawHorizontalLine"/>
    </xsl:if>

    <fo:block border="{$vBorder}" linefeed-treatment="preserve">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="right" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="bold" color="DarkGrey">
                <xsl:choose>
                  <xsl:when test ="$vIsCustomReport=true()">
                    <fo:page-number/>/<fo:page-number-citation ref-id="EndOfDoc"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- EG 20160404 Migration vs2013 -->
                    <!--<xsl:text>Powered by Spheres©</xsl:text>-->
                    <xsl:text>Powered by Spheres - © 2024 EFS</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

</xsl:stylesheet>
