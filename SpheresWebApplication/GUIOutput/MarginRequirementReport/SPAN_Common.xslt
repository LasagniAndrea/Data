<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
                xmlns:xsl  ="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt   ="http://xsltsl.org/date-time"
                xmlns:fo   ="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:efs  ="http://www.efs.org/2007/EFSmL-3-0"
                xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
                xmlns:fpml ="http://www.fpml.org/2007/FpML-4-4">

  <xsl:include href="..\..\Library\Resource.xslt" />
  <xsl:include href="..\..\Library\DtFunc.xslt" />
  <xsl:include href="..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\Library\xsltsl/date-time.xsl" />
  <xsl:include href="..\..\Library\StrFunc.xslt" />
  
  <!-- *************************************************** -->

  <xsl:variable name ="vSpanCME" select ="'SPAN CME'"/>
  <xsl:variable name ="vLondonSpan" select ="'London SPAN'"/>
  <xsl:variable name ="vSpanC21" select ="'SPAN C21'"/>
  
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
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 14)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderLeftMargin">1.5</xsl:variable>
  <!-- Variables for Footer Vertical====================================================== -->
  <xsl:variable name ="vPageA4VerticalFooterExtent">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 1)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vPageA4VerticalFooterFontSize">7pt</xsl:variable>
  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vPageA4VerticalBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyTopMargin">
    <xsl:value-of select ="$vPageA4VerticalHeaderExtent"/>
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
  <xsl:variable name="vPageA4LandscapeMargin">0.8</xsl:variable>
  <!-- Variables for Header Landscape ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeHeaderExtent">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 3)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderLeftMargin">1.5</xsl:variable>
  <!-- Variables for Footer Landscape====================================================== -->
  <xsl:variable name ="vPageA4LandscapeFooterExtent">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 0.5)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeFooterFontSize">7pt</xsl:variable>
  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyTopMargin">
    <xsl:value-of select ="$vPageA4LandscapeHeaderExtent+$vPageA4LandscapeMargin"/>
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
  <xsl:variable name ="vFillCharacter">-</xsl:variable>

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
  <!-- =========================================================================== -->
  <!-- Font ====================================================================== -->
  <!-- sans-serif/serif/Courier/Helvetica/Times/ -->
  <xsl:variable name ="vFontFamily">sans-serif</xsl:variable>
  <xsl:variable name ="vFontSize">7pt</xsl:variable>
  <xsl:variable name ="vFooterFontFamily">sans-serif</xsl:variable>
  <xsl:variable name ="vFooterFontSize">6pt</xsl:variable>
  <!-- ========================================================================== -->
  <!-- Block ==================================================================== -->
  <xsl:variable name ="vBlockLeftMargin">0.1</xsl:variable>
  <xsl:variable name ="vBlockRightMargin">1</xsl:variable>
  <xsl:variable name ="vBlockTopMargin">0.2</xsl:variable>
  <!-- =========================================================================== -->
  <!-- TableCell ================================================================= -->
  <xsl:variable name ="vTableCellBackgroundColor">White</xsl:variable>
  <xsl:variable name ="vTableCellBackgroundColorLight">LightGrey</xsl:variable>
  <xsl:variable name ="vTableCellFontWeight">normal</xsl:variable>
  <xsl:variable name="vTableCellBorder">0.2pt solid black</xsl:variable>
  <!-- hidden dotted dashed solid double groove ridge inset  outset   -->
  <xsl:variable name="vTableCellBorderTopStyle">none</xsl:variable>
  <!--thin thick medium-->
  <xsl:variable name="vTableCellBorderTopWidth">0</xsl:variable>
  <!-- hidden dotted dashed solid double groove ridge inset  outset   -->
  <xsl:variable name="vTableCellBorderBottomStyle">none</xsl:variable>
  <!--thin thick medium-->
  <xsl:variable name="vTableCellBorderBottomWidth">0</xsl:variable>
  <!-- separate -->
  <xsl:variable name="vBorderCollapse">0</xsl:variable>
  <xsl:variable name="vBorderSeparation">0pt</xsl:variable>
  <xsl:variable name ="vTableCellPadding">0.05</xsl:variable>
  <xsl:variable name="vTableCellDisplayAlign">center</xsl:variable>
  <xsl:variable name ="vRowHeight">0.4</xsl:variable>
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>
  <!-- =========================================================================== -->
  <!-- Labels ====================================================== -->
  <!-- =========================================================================== -->
  <xsl:variable name ="vColon">
    <xsl:text>:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vDash">
    <xsl:text>-</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPBRequirements">
    <xsl:text>PB Requirements</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelBusinessDate">
    <xsl:text>Business Date</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelFirm">
    <xsl:text>Firm</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAcct">
    <xsl:text>Acct</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelType">
    <xsl:text>Type</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSeg">
    <xsl:text>Seg</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCurrency">
    <xsl:text>Currency</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelLedgerBalance">
    <xsl:text>Ledger Balance</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOpenTradeEquity">
    <xsl:text>Open Trade Equity</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSecurities">
    <xsl:text>Securities</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotalEquity">
    <xsl:text>Total Equity</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSpanReq">
    <xsl:text>Span Req</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAnov">
    <xsl:text>ANOV</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotalReq">
    <xsl:text>Total Req</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelNetLiqValue">
    <xsl:text>Net Liq Value</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelExcessDeficit">
    <xsl:text>Excess/Deficit</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCoreMaint">
    <xsl:text>Core Maint</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCoreInit">
    <xsl:text>Core Init</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMaint">
    <xsl:text>MNT</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelInit">
    <xsl:text>INIT</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelLevel">
    <xsl:text>Level</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelEC">
    <xsl:text>EC</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCC">
    <xsl:text>CC</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCurr">
    <xsl:text>Curr</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelLOV">
    <xsl:text>LOV</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSOV">
    <xsl:text>SOV</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMI">
    <xsl:text>M/I</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPBC">
    <xsl:text>PBC</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelScanRisk">
    <xsl:text>Scan Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelIntra">
    <xsl:text>Intra</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSpot">
    <xsl:text>Spot</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelInter">
    <xsl:text>Inter</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelIntex">
    <xsl:text>Intex</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelSOM">
    <xsl:text>SOM</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPort">
    <xsl:text>port</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCurVal">
    <xsl:text>curVal</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOReq">
    <xsl:text>oReq</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCurReq">
    <xsl:text>curReq</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelEcPort">
    <xsl:text>ecPort</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCcPort">
    <xsl:text>ccPort</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelNReq">
    <xsl:text>nReq</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelDReq">
    <xsl:text>dReq</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vMissingValue">
    <xsl:text>N/A</xsl:text>
  </xsl:variable>
  <!--! =================================================================================== -->
  <!--!   BUSINESS VARIABLES AND TEMPLATES                -->
  <!--! =================================================================================== -->
  <!-- returns currency symbol from currency code eg: EUR/€ -->
  <xsl:template name ="getCurrencySymbol">
    <xsl:param name ="pCurrency"/>
    <xsl:choose>
      <xsl:when test ="$pCurrency='CAD'">
        <xsl:text>C$ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='EUR'">
        <xsl:text>€ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='GBP'">
        <xsl:text>£ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='USD'">
        <xsl:text>$ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='AUD'">
        <xsl:text>A$ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='JPY'">
        <xsl:text>¥ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='HKD'">
        <xsl:text>HK$ </xsl:text>
      </xsl:when>
      <xsl:when test ="$pCurrency='NZD'">
        <xsl:text>NZ$ </xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat($pCurrency, ' ')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- returns formatted amount for overview section ($10.00 for USD - €5.00 for EUR amount)-->
  <xsl:template name ="formatOverviewAmount">
    <xsl:param name ="pCurrency"/>
    <xsl:param name ="pAmount"/>
    <xsl:variable name ="vAmount">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pAmount"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vCurr">
      <xsl:call-template name ="getCurrencySymbol">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select ="concat($vCurr,$vAmount)"/>
  </xsl:template>

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

  <!-- this variable returns the Margin Calculation Method Name available into XML -->
  <xsl:variable name ="vMarginCalculationMethodName">
    <xsl:value-of select ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@name"/>
  </xsl:variable>

  <!-- return margin amount currency -->
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

  <!-- returns amount requiremnt type for risk calculation (MAINT or INIT)-->
  <xsl:variable name ="vGlobalRequirementType">
    <xsl:value-of select ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@maint_init"/>
  </xsl:variable>

  <!-- return BusinessDate -->
  <xsl:variable name ="vBusinessDate">
    <xsl:variable name ="vDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="normalize-space($vDate)"/>
  </xsl:variable>

  <!-- return Business DateTime -->
  <xsl:variable name ="vBusinessDateTime">
    <xsl:variable name ="vFormatTime">
      <xsl:call-template name="format-time2">
        <xsl:with-param name="xsd-date-time" select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate/@timeStamp"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="concat (normalize-space($vBusinessDate), ' ', $vFormatTime)"/>
  </xsl:variable>

  <!--  return Firm -->
  <!--
  <xsl:variable name ="vFirm">
    <xsl:value-of select ="//efs:EfsML/efs:trade/efs:Trade/efs:marginRequirement/efs:entityPartyReference/@href"/>
  </xsl:variable>-->
  
  <!--  return Firm -->
  <!-- Firm: The firm or entity as entered into Span -->
  <!-- <efs:marginRequirementOfficePartyReference href="CLIENT8-Sub-account-2"/> -->
  
  <xsl:variable name ="vFirm">
    
    <xsl:variable name ="vPartyRef">
      <xsl:value-of select ="//efs:EfsML/efs:trade/efs:Trade/efs:marginRequirement/efs:marginRequirementOfficePartyReference/@href"/>
    </xsl:variable>
    
    <xsl:value-of select="//efs:EfsML/efs:party/efs:Party[@id=$vPartyRef]/fpml:partyName"/>
  
  </xsl:variable>

  <!--  return Acct -->
  <xsl:variable name ="vAcct">
    <xsl:value-of select ="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:partyTradeIdentifier/efs:bookId/@bookName"/>
  </xsl:variable>

  <!-- return AcctType -->
  <xsl:variable name ="vAcctType">
    <xsl:choose>
      <xsl:when test ="//efs:EfsML/efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@accounttype">
        <xsl:value-of select ="//efs:EfsML/efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@accounttype"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vMissingValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- NOT AVAILABLE (0 static)-->
  <xsl:template name ="getLedgerBalanceOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vAmount">
      <xsl:value-of select ="'0'"/>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- NOT AVAILABLE (0 static)-->
  <xsl:template name ="getOpenTradeEquityOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vAmount">
      <xsl:value-of select ="'0'"/>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- NOT AVAILABLE (0 static)-->
  <xsl:template name ="getSecuritiesOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vAmount">
      <xsl:value-of select ="'0'"/>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- Calculated (LedgerBalance + OpenTradeEquity + Securities) -->
  <xsl:template name ="getTotalEquityOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vLedgerBalance">
      <xsl:call-template name ="getLedgerBalanceOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vOpenTradeEquity">
      <xsl:call-template name ="getOpenTradeEquityOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSecurities">
      <xsl:call-template name ="getSecuritiesOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select ="$vLedgerBalance + $vOpenTradeEquity + $vSecurities"/>
  </xsl:template>

  <!-- The node maintenancemarginbycurrency is available for Maintenance requirement (MNT) and Initial requirement (INIT) -->
  <!-- Use initialmarginbycurrency when requirement is not specified -->
  <xsl:template name ="getMaintSpanReqOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vAmount">
      <xsl:choose>
        <xsl:when test ="$vGlobalRequirementType = ''">
          <xsl:value-of select ="sum(//efs:groups/efs:group/efs:initialmarginbycurrency/efs:initialmargin[@curr=$pCurrency]/@amount)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="sum(//efs:groups/efs:group/efs:maintenancemarginbycurrency/efs:maintenancemargin[@curr=$pCurrency]/@amount)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- The node initialmarginbycurrency is always available-->
  <xsl:template name ="getInitSpanReqOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vAmount">
      <xsl:value-of select ="sum(//efs:groups/efs:group/efs:initialmarginbycurrency/efs:initialmargin[@curr=$pCurrency]/@amount)"/>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <xsl:template name ="getAvailableNetOptionValueOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vAmount">
      <xsl:value-of select="sum(//efs:groups/efs:group/efs:novbycurrency/efs:nov[@curr=$pCurrency]/@amount)"/>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- MaintTotalReq Calculated (MaintSpanReq - AvailableNetOptionValue) -->
  <xsl:template name ="getMaintTotalReqOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vMaintSpanReq">
      <xsl:call-template name ="getMaintSpanReqOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vAvailableNetOptionValue">
      <xsl:call-template name ="getAvailableNetOptionValueOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <!--Un montant de Déposit ne peut pas être négatif-->
    <xsl:variable name="vAmount">
      <xsl:value-of select ="$vMaintSpanReq - $vAvailableNetOptionValue"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="number($vAmount)>0">
        <xsl:value-of select="$vAmount"/>
      </xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- InitTotalReq Calculated (InitSpanReq - AvailableNetOptionValue) -->
  <xsl:template name ="getInitTotalReqOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vInitSpanReq">
      <xsl:call-template name ="getInitSpanReqOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vAvailableNetOptionValue">
      <xsl:call-template name ="getAvailableNetOptionValueOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <!--Un montant de Déposit ne peut pas être négatif-->
    <xsl:variable name="vAmount">
      <xsl:value-of select ="$vInitSpanReq - $vAvailableNetOptionValue"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="number($vAmount)>0">
        <xsl:value-of select="$vAmount"/>
      </xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- NetLiqValue calculated (TotalEquity + AvailableNetOptionValue)-->
  <xsl:template name ="getNetLiqValueOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vTotalEquity">
      <xsl:call-template name ="getTotalEquityOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vAvailableNetOptionValue">
      <xsl:call-template name ="getAvailableNetOptionValueOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select ="$vAvailableNetOptionValue - $vTotalEquity"/>
  </xsl:template>

  <!-- MaintExcessDeficit calculated (MaintSpanReq - NetLiqValue) -->
  <xsl:template name ="getMaintExcessDeficitOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vMaintSpanReq">
      <xsl:call-template name ="getMaintSpanReqOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vNetLiqValue">
      <xsl:call-template name ="getNetLiqValueOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select ="$vMaintSpanReq - $vNetLiqValue"/>
  </xsl:template>

  <!-- InitExcessDeficit calculated (InitSpanReq - NetLiqValue) -->
  <xsl:template name ="getInitExcessDeficitOverview">
    <xsl:param name ="pCurrency"/>
    <xsl:variable name ="vInitSpanReq">
      <xsl:call-template name ="getInitSpanReqOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vNetLiqValue">
      <xsl:call-template name ="getNetLiqValueOverview">
        <xsl:with-param name ="pCurrency" select ="$pCurrency"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select ="$vInitSpanReq - $vNetLiqValue"/>
  </xsl:template>

  <!-- Exchange -->
  <xsl:template name ="getExchange">
    <xsl:param name="pNode"/>
    <xsl:value-of select ="$pNode/@name"/>
  </xsl:template>

  <!-- Commodity Code -->
  <xsl:template name ="getCommodityCode">
    <xsl:param name="pNode"/>
    <xsl:value-of select ="$pNode/@name"/>
  </xsl:template>

  <!--  return Long Option Value -->
  <xsl:template name ="getLongOptionValue">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pIsGroupNode"/>

    <xsl:variable name ="vAmount">
      <xsl:choose>
        <xsl:when test ="$pIsGroupNode=true()">
          <xsl:choose>
            <xsl:when test ="$pNode/efs:lovbycurrency/efs:lov[@curr=$pCurrency]">
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="$pNode/efs:lovbycurrency/efs:lov[@curr=$pCurrency]/@amount"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="sum($pNode/efs:combinedcommodities/efs:combinedcommodity/efs:lov[@curr=$pCurrency]/@amount)"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test ="$pNode/efs:lov[@curr=$pCurrency]">
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="$pNode/efs:lov[@curr=$pCurrency]/@amount"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="$vMissingValue"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!--  return Short Option Value template -->
  <xsl:template name ="getShortOptionValue">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pIsGroupNode"/>

    <xsl:variable name ="vAmount">
      <xsl:choose>
        <xsl:when test ="$pIsGroupNode=true()">
          <xsl:choose>
            <xsl:when test ="$pNode/efs:sovbycurrency/efs:sov[@curr=$pCurrency]">
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="$pNode/efs:sovbycurrency/efs:sov[@curr=$pCurrency]/@amount"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="sum($pNode/efs:combinedcommodities/efs:combinedcommodity/efs:sov[@curr=$pCurrency]/@amount)"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test ="$pNode/efs:sov[@curr=$pCurrency]">
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="$pNode/efs:sov[@curr=$pCurrency]/@amount"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="$vMissingValue"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>

  </xsl:template>

  <!--  return Net Option Value template -->
  <xsl:template name ="getNetOptionValue">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:nov[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:nov[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>


  <xsl:template name ="getSpanReq">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pIsGroupNode"/>
    <xsl:param name="pDetailRequirementType"/>

    <xsl:variable name ="vAmount">
      <xsl:choose>
        <xsl:when test ="$vGlobalRequirementType = '' or $vGlobalRequirementType = $vLabelInit">
          <xsl:choose>
            <xsl:when test ="$pIsGroupNode=true()">
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="$pNode/efs:initialmarginbycurrency/efs:initialmargin[@curr=$pCurrency]/@amount"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="formatMoney">
                <xsl:with-param name ="pAmount" select ="$pNode/efs:initialmargin[@curr=$pCurrency]/@amount"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test ="$pDetailRequirementType = $vLabelMaint">
              <xsl:choose>
                <xsl:when test ="$pIsGroupNode=true()">
                  <xsl:call-template name ="formatMoney">
                    <xsl:with-param name ="pAmount" select ="$pNode/efs:maintenancemarginbycurrency/efs:maintenancemargin[@curr=$pCurrency]/@amount"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name ="formatMoney">
                    <xsl:with-param name ="pAmount" select ="$pNode/efs:maintenancemargin[@curr=$pCurrency]/@amount"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test ="$pIsGroupNode=true()">
                  <xsl:call-template name ="formatMoney">
                    <xsl:with-param name ="pAmount" select ="$pNode/efs:initialmarginbycurrency/efs:initialmargin[@curr=$pCurrency]/@amount"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name ="formatMoney">
                    <xsl:with-param name ="pAmount" select ="$pNode/efs:initialmargin[@curr=$pCurrency]/@amount"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select ="$vAmount"/>

  </xsl:template>


  <xsl:template name ="getScanRisk">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:scanrisk[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:scanrisk[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getIntraCommodityCharge">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:intracommoditycharge[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:intracommoditycharge[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getDeliveryCharge">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:deliverycharge[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:deliverycharge[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getInterCommodityCredit">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:intercommoditycredit[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:intercommoditycredit[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- PM 20130620 Ajout getInterExchangeCredit -->
  <xsl:template name ="getInterExchangeCredit">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:interexchangecredit[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:interexchangecredit[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getShortOptionMinimum">
    <xsl:param name="pNode"/>
    <xsl:param name="pCurrency"/>
    <xsl:if test ="$pNode/efs:som[@curr=$pCurrency]">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:som[@curr=$pCurrency]/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>


  <!-- =========================================================  -->
  <!-- LAYOUT TEMPLATES                                           -->
  <!-- =========================================================  -->

  <xsl:template name ="setPagesCaracteristics">

    <fo:layout-master-set>

      <!-- A4 VERTICAL -->
      <!-- EG 20160404 Migration vs2013 -->
      <!--<fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin="{$vPageA4VerticalMargin}cm">-->
      <fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin-left="{$vPageA4VerticalMargin}cm" margin-top="{$vPageA4VerticalMargin}cm" margin-right="{$vPageA4VerticalMargin}cm" margin-bottom="{$vPageA4VerticalMargin}cm">
        <fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMargin}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />
        <fo:region-before region-name="A4VerticalHeader" background-color="white" extent="{$vPageA4VerticalHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4VerticalFooter" background-color="white" extent="{$vPageA4VerticalFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>

      <!-- A4 LANDSCAPE -->
      <!-- EG 20160404 Migration vs2013 -->
      <!--<fo:simple-page-master master-name="A4LandscapePage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">-->
      <fo:simple-page-master master-name="A4LandscapePage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin-left="{$vPageA4LandscapeMargin}cm" margin-top="{$vPageA4LandscapeMargin}cm" margin-right="{$vPageA4LandscapeMargin}cm" margin-bottom="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vPageA4LandscapeBodyLeftMargin}cm" margin-top="{$vPageA4LandscapeBodyTopMargin}cm" margin-bottom="{$vPageA4LandscapeBodyBottomMargin}cm" />
        <fo:region-before region-name="A4LandscapeHeader" background-color="white" extent="{$vPageA4LandscapeHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4LandscapeFooter" background-color="white" extent="{$vPageA4LandscapeFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>

    </fo:layout-master-set>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="handleStaticContentHeader">
    <xsl:choose>
      <!-- vertical report -->
      <xsl:when test ="$vIsA4VerticalReport=true()">
        <fo:static-content flow-name="A4VerticalHeader">
          <xsl:call-template name="displayHeader"/>
        </fo:static-content>
      </xsl:when>
      <!-- landscape report -->
      <xsl:otherwise>
        <fo:static-content flow-name="A4LandscapeHeader">
          <xsl:call-template name="displayHeader"/>
        </fo:static-content>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 1 columns (for Landscape and Vertical page) -->
  <!-- *************************************************** -->
  <xsl:template name="create1Column">
    <xsl:param name ="pColumnWidth1" select ="0"/>

    <xsl:variable name ="vColumnWidth1">

      <xsl:choose>
        <xsl:when test ="$pColumnWidth1=0">
          <xsl:value-of select =" $vPageWidth - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth1"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />

  </xsl:template>

  <!-- *************************************************** -->
  <!-- 2 columns (for Landscape and Vertical page) -->
  <!-- mandatory parameters: pColumnWidth1 -->
  <!-- optional parameters: pColumnWidth2 -->
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
  <!-- *************************************************** -->
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
  <!-- 5 columns (for Landscape and Vertical page)-->
  <!-- *************************************************** -->
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
  <!-- 6 columns (for Landscape and Vertical page)-->
  <!-- *************************************************** -->
  <xsl:template name="create6Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select="$pColumnWidth3"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select="$pColumnWidth4"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select="$pColumnWidth5"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth6">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth6=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth6"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 8 columns (for Landscape and Vertical page)-->
  <!-- mandatory parameters: first seven parameters  -->
  <!-- column8 width is dynamically calcultated when pColumnWidth8 param is missing -->
  <!-- else all columns are drawed with static width) -->
  <!-- *************************************************** -->
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

  <!-- *************************************************** -->
  <!-- 16 columns -->
  <!-- *************************************************** -->
  <xsl:template name="create16Columns">
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
    <xsl:param name ="pColumnWidth14" select ="0"/>
    <xsl:param name ="pColumnWidth15" select ="0"/>
    <xsl:param name ="pColumnWidth16" select ="0"/>


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
      <xsl:value-of select ="$pColumnWidth13"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth14">
      <xsl:value-of select ="$pColumnWidth14"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth15">
      <xsl:value-of select ="$pColumnWidth15"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth16">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth16=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 
                        - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - $vColumnWidth9 
                        - $vColumnWidth10 - $vColumnWidth11 - $vColumnWidth12 - $vColumnWidth13 - $vColumnWidth14 
                        - $vColumnWidth15 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth16"/>
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
    <fo:table-column column-width="{$vColumnWidth14}cm" column-number="14" />
    <fo:table-column column-width="{$vColumnWidth15}cm" column-number="15" />
    <fo:table-column column-width="{$vColumnWidth16}cm" column-number="16" />

  </xsl:template>

  <!--! =================================================================================== -->
  <!--!                      layout templates                                               -->
  <!--! =================================================================================== -->

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

  <xsl:template name="displayEmptyRow">
    <fo:table-body>
      <fo:table-row height="{$vRowHeight}cm">
        <fo:table-cell>
          <fo:block border="{$vBorder}"/>
        </fo:table-cell>
      </fo:table-row>
    </fo:table-body>
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
    <xsl:param name ="pPadding" select ="$vTableCellPadding"/>
    <xsl:param name ="pDisplayAlign" select ="$vTableCellDisplayAlign"/>
    <xsl:param name ="pFontSize" select ="$vFontSize"/>
    <xsl:param name ="pFontWeight" select ="'normal'"/>
    <xsl:param name ="pColor" select ="'black'"/>
    <xsl:param name ="pTextAlign"/>
    <xsl:param name ="pStartIndent" select ="0"/>
    <fo:table-cell border="{$vBorder}" padding="{$pPadding}cm" display-align="{$pDisplayAlign}" background-color="{$pBackgroundColor}" >
      <fo:block border="{$vBorder}" font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}" >
        <xsl:value-of select ="$pValue"/>
      </fo:block>
    </fo:table-cell>
  </xsl:template>

  <!-- draw table cell for body-->
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name ="displayTableCellBody">
    <xsl:param name ="pValue" select ="''"/>
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
      <fo:block font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}" >
        <xsl:call-template name="DisplayBorderAttribute">
          <xsl:with-param name="pTopStyle" select="$pBorderTopStyle"/>
          <xsl:with-param name="pTopWidth" select="$pBorderTopWidth"/>
        </xsl:call-template>
        <xsl:call-template name="DisplayBorderAttribute">
          <xsl:with-param name="pBottomStyle" select="$pBorderBottomStyle"/>
          <xsl:with-param name="pBottomWidth" select="$pBorderBottomWidth"/>
        </xsl:call-template>
        <xsl:value-of select ="$pValue"/>
      </fo:block>
    </fo:table-cell>

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
                <!-- to do add message error: this report is not in SPAN format-->
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

    <fo:block border="{$vBorder}" linefeed-treatment="preserve">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create3Columns">
          <xsl:with-param name="pColumnWidth1" select ="9"/>
          <xsl:with-param name="pColumnWidth2" select ="9"/>
          <xsl:with-param name="pColumnWidth3" select ="10"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block start-indent="1cm" border="{$vBorder}" text-align="left" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="normal">
                <xsl:value-of select ="$vBusinessDateTime"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="center" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="normal">
                <xsl:value-of select ="$vLabelReportName"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="center" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="normal">
                Page <fo:page-number/> of <fo:page-number-citation ref-id="EndOfDoc"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block/>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block/>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="right" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="bold" color="DarkGrey">
                <!-- EG 20160404 Migration vs2013 -->
                <!--<xsl:text>Powered by Spheres©</xsl:text>-->
                <xsl:text>Powered by Spheres - © 2024 EFS</xsl:text>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>

</xsl:stylesheet>
