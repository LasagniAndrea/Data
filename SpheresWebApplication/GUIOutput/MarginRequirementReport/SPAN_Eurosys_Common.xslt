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
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 2)"/>
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
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 5)"/>
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
  <xsl:variable name ="vFontFamily">Helvetica</xsl:variable>
  <xsl:variable name ="vFontSize">7pt</xsl:variable>
  <xsl:variable name ="vFooterFontFamily">Helvetica</xsl:variable>
  <xsl:variable name ="vFooterFontSize">7pt</xsl:variable>
  <!-- ========================================================================== -->
  <!-- Block ==================================================================== -->
  <xsl:variable name ="vBlockLeftMargin">0.1</xsl:variable>
  <xsl:variable name ="vBlockRightMargin">1</xsl:variable>
  <xsl:variable name ="vBlockTopMargin">0.2</xsl:variable>
  <!-- ========================================================================== -->
  <!-- Table ==================================================================== -->
  <xsl:variable name ="vTablePadding">0.3mm</xsl:variable>
  <!-- =========================================================================== -->
  <!-- TableCell ================================================================= -->
  <xsl:variable name ="vTableCellBackgroundColor">White</xsl:variable>
  <xsl:variable name ="vTableCellBackgroundColorLight">LightGrey</xsl:variable>
  <xsl:variable name ="vTableCellFontWeight">normal</xsl:variable>
  <!-- hidden dotted dashed solid double groove ridge inset  outset   -->
  <!--thin thick medium-->
  <xsl:variable name ="vTableCellBorder">0.1pt solid black</xsl:variable>
  <xsl:variable name ="vTableCellBorderLeftStyle">solid</xsl:variable>
  <xsl:variable name ="vTableCellBorderLeftWidth">0.1pt</xsl:variable>
  <xsl:variable name ="vTableCellBorderRightStyle">solid</xsl:variable>
  <xsl:variable name ="vTableCellBorderRightWidth">0.1pt</xsl:variable>
  <xsl:variable name ="vTableCellBorderTopStyle">solid</xsl:variable>
  <xsl:variable name ="vTableCellBorderTopWidth">0.1pt</xsl:variable>
  <xsl:variable name ="vTableCellBorderBottomStyle">solid</xsl:variable>
  <xsl:variable name ="vTableCellBorderBottomWidth">0.1pt</xsl:variable>
  <!-- separate -->
  <xsl:variable name ="vBorderCollapse">0</xsl:variable>
  <xsl:variable name ="vBorderSeparation">0pt</xsl:variable>
  <xsl:variable name ="vTableCellPadding">0.06</xsl:variable>
  <xsl:variable name ="vTableCellDisplayAlign">center</xsl:variable>
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
  <xsl:variable name ="vLabelPerformanceBondSummaryReport">
    <xsl:text>PERFORMANCE BOND SUMMARY REPORT</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPerformanceBondReport">
    <xsl:text>PERFORMANCE BOND REPORT</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCompte">
    <xsl:text>Compte/Groupe</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTypeCompte">
    <xsl:text>Type de Compte SPAN</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTimeStamp">
    <xsl:text>Edité le</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPage">
    <xsl:text>Page</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelScenario">
    <xsl:text>Scénario</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMarche">
    <xsl:text>Marché</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelProduit">
    <xsl:text>Produit</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelGroupeSpan">
    <xsl:text>Groupe Span</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCodeSpan">
    <xsl:text>Code Span</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelScanRisk">
    <xsl:text>Scan Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelIntermonth">
    <xsl:text>Intermonth</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelDeliveryMonth">
    <xsl:text>Delivery Month</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTimeRisk">
    <xsl:text>Time Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelVolatilyRisk">
    <xsl:text>Volatily Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelWeightedRisk">
    <xsl:text>Weighted Risk</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelIntercomCdt">
    <xsl:text>Intercom.Cdt</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelDeltaRemain">
    <xsl:text>Delta Remain.</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelNetOptValue">
    <xsl:text>Net Opt.Value</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelShortOptMin">
    <xsl:text>Short Opt.Min</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelRiskMaint">
    <xsl:text>Risk Maint.</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelRiskInitial">
    <xsl:text>Risk Initial</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelDepositMaint">
    <xsl:text>Deposit Maint.</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelDepositInit">
    <xsl:text>Deposit Init.</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelExcLgOptValue">
    <xsl:text>Exc.Lg.Opt.Value</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCommodityRisk">
    <xsl:text>Commodity Risk</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRemarques">
    <xsl:text>Remarques:</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisqueDeScenario">
    <xsl:text>Risque de scénario = (valeur la plus grande de tous les scénarios) - Scan Risk</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisqueDeTemps">
    <xsl:text>Risque de temps = ((valeur du scénario 1 + valeur du scénario 2)/2) - Time Risk</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisqueVolatilite">
    <xsl:text>Risque ajusté en volatilité = ((risque de scénario - valeur de scénario couplé)/2) - Volatility Risk</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisquePondere">
    <xsl:text>Risque pondéré = (MAX(risque de scénario - risque de temps - risque de volatilité , 0) / delta net initial) - Weighted Risk</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisqueProduit">
    <xsl:text>Risque du produit = (risque de scénario + débit sur spread calendaire + charge de livraison) - Commodity Risk</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisqueMaintenance">
    <xsl:text>Risque de Maintenance = (risque du produit - crédit sur spread entre produits) - Risk Maint.</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vTextRisqueInitial">
    <xsl:text>Risque Initial = (risque de maintenance - crédit sur spread entre produits) - Risk Initial</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vMissingValue">
    <xsl:text>N/A</xsl:text>
  </xsl:variable>

  <!--! =================================================================================== -->
  <!--!   BUSINESS VARIABLES AND TEMPLATES                                                  -->
  <!--! =================================================================================== -->

  <!-- returns formatted number with 2 decimal pattern using current culture -->
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

  <!--abs-->
  <xsl:template name="abs">
    <xsl:param name="n" />
    <xsl:if test="string-length($n) > 0">
      <xsl:choose>
        <xsl:when test="number($n) >= 0">
          <xsl:value-of select="number($n)" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number(-1) * number($n)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!--return BusinessDate-->
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

  <!--  return Compte -->
  <xsl:variable name ="vCompte">
    <xsl:value-of select ="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:partyTradeIdentifier/efs:bookId/@bookName"/>
  </xsl:variable>

  <!-- return TypeCompte -->
  <xsl:variable name ="vTypeCompte">
    <xsl:choose>
      <xsl:when test ="//efs:EfsML/efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@accounttype">
        <xsl:value-of select ="//efs:EfsML/efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@accounttype"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vMissingValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

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

  <xsl:template name ="getScanRisk">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:scanrisk">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:scanrisk/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getIntracommoditycharge">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:intracommoditycharge">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:intracommoditycharge/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getDeliverycharge">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:deliverycharge">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:deliverycharge/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getCommodityRisk">
    <xsl:param name="pNode"/>

    <xsl:variable name ="vScanRisk">
      <xsl:value-of select="$pNode/efs:scanrisk/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vIntermonth">
      <xsl:value-of select="$pNode/efs:intracommoditycharge/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vDeliveryMonth">
      <xsl:value-of select="$pNode/efs:deliverycharge/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vCommodityRisk">
      <xsl:value-of select="$vScanRisk + $vIntermonth + $vDeliveryMonth"/>
    </xsl:variable>

    <xsl:call-template name ="formatMoney">
      <xsl:with-param name ="pAmount" select ="$vCommodityRisk"/>
    </xsl:call-template>

  </xsl:template>


  <xsl:template name ="getTimerisk">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:timerisk">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:timerisk/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getVolatilyrisk">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:volatilityrisk">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:volatilityrisk/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getWeightedrisk">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:weightedrisk">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:weightedrisk/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getIntercommoditycredit">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:intercommoditycredit">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:intercommoditycredit/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getDeltanetremaining">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:deltanetremaining">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:deltanetremaining"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getNov">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:nov">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:nov/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getSom">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:som">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:som/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getMaintenancemargin">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:maintenancemargin">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:maintenancemargin/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name ="getInitialmargin">
    <xsl:param name="pNode"/>
    <xsl:if test ="$pNode/efs:initialmargin">
      <xsl:call-template name ="formatMoney">
        <xsl:with-param name ="pAmount" select ="$pNode/efs:initialmargin/@amount"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!--
  <	&lt;	
  >	&gt;	
  -->

  <!--max(0, maintenancemargin - nov)-->
  <xsl:template name ="getDepositMaint">
    <xsl:param name="pNode"/>

    <xsl:variable name ="vMaintenancemargin">
      <xsl:value-of select ="$pNode/efs:maintenancemargin/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vAbsNov">
      <xsl:call-template name ="abs">
        <xsl:with-param name ="n" select ="$pNode/efs:nov/@amount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vDepositMaint">
      <xsl:value-of select ="$vMaintenancemargin - $vAbsNov"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="$vMaintenancemargin&gt;$vAbsNov">
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="$vDepositMaint"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="0"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--max(0, nov - maintenancemargin))-->
  <xsl:template name ="getExcLgOptValueMaint">
    <xsl:param name="pNode"/>

    <xsl:variable name ="vAbsNov">
      <xsl:call-template name ="abs">
        <xsl:with-param name ="n" select ="$pNode/efs:nov/@amount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vMaintenancemargin">
      <xsl:value-of select ="$pNode/efs:maintenancemargin/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vExcLgOptValueMaint">
      <xsl:value-of select ="$vAbsNov - $vMaintenancemargin"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="$vAbsNov&gt;$vMaintenancemargin">
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="$vExcLgOptValueMaint"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="0"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--max(0, initialmargin - nov)-->
  <xsl:template name ="getDepositInit">
    <xsl:param name="pNode"/>

    <xsl:variable name ="vInitialmargin">
      <xsl:value-of select ="$pNode/efs:initialmargin/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vAbsNov">
      <xsl:call-template name ="abs">
        <xsl:with-param name ="n" select ="$pNode/efs:nov/@amount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vDepositInit">
      <xsl:value-of select ="$vInitialmargin - $vAbsNov"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="$vInitialmargin&gt;$vAbsNov">
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="$vDepositInit"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="0"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--max(0, nov - initialmargin))-->
  <xsl:template name ="getExcLgOptValueInit">
    <xsl:param name="pNode"/>

    <xsl:variable name ="vAbsNov">
      <xsl:call-template name ="abs">
        <xsl:with-param name ="n" select ="$pNode/efs:nov/@amount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vInitialmargin">
      <xsl:value-of select ="$pNode/efs:initialmargin/@amount"/>
    </xsl:variable>

    <xsl:variable name ="vExcLgOptValueInit">
      <xsl:value-of select ="$vAbsNov - $vInitialmargin"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="$vAbsNov&gt;$vInitialmargin">
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="$vExcLgOptValueInit"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name ="formatMoney">
          <xsl:with-param name ="pAmount" select ="0"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>


  <xsl:template name ="getScanScenario">
    <xsl:param name="pNode"/>
    <xsl:param name="pScenario"/>

    <xsl:call-template name ="formatMoney">
      <xsl:with-param name ="pAmount" select ="$pNode/efs:scantiers/efs:scan[@scenario=$pScenario]/@value"/>
    </xsl:call-template>

  </xsl:template>


  <!-- =========================================================  -->
  <!-- LAYOUT TEMPLATES                                           -->
  <!-- =========================================================  -->

  <xsl:template name ="setPagesCaracteristics">

    <fo:layout-master-set>

      <!-- A4 VERTICAL -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin-left="{$vPageA4VerticalMargin}cm" margin-top="{$vPageA4VerticalMargin}cm" margin-right="{$vPageA4VerticalMargin}cm" margin-bottom="{$vPageA4VerticalMargin}cm">
        <fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMargin}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />
        <fo:region-before region-name="A4VerticalHeader" background-color="white" extent="{$vPageA4VerticalHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4VerticalFooter" background-color="white" extent="{$vPageA4VerticalFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>

      <!-- A4 LANDSCAPE -->
      <!-- EG 20160404 Migration vs2013 -->
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
  <!-- 9 columns (for Landscape and Vertical page)-->
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
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 -($vPageMargin * 2)"/>
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

  <!-- *************************************************** -->
  <!-- 11 columns (for Landscape and Vertical page)-->
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

  <!-- *************************************************** -->
  <!-- 14 columns -->
  <!-- *************************************************** -->
  <xsl:template name="create14Columns">
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
      <xsl:choose>
        <xsl:when test ="$pColumnWidth14=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 
                        - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - $vColumnWidth9 
                        - $vColumnWidth10 - $vColumnWidth11 - $vColumnWidth12 - $vColumnWidth13 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth14"/>
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
    <xsl:param name ="pBorder" select ="$vBorder"/>
    <xsl:param name ="pBackgroundColor" select ="'White'"/>
    <xsl:param name ="pPadding" select ="$vTableCellPadding"/>
    <xsl:param name ="pDisplayAlign" select ="$vTableCellDisplayAlign"/>
    <xsl:param name ="pFontSize" select ="$vFontSize"/>
    <xsl:param name ="pFontWeight" select ="'normal'"/>
    <xsl:param name ="pColor" select ="'black'"/>
    <!-- EG 20160404 Migration vs2013 -->
    <xsl:param name ="pTextAlign" select ="'left'"/>
    <xsl:param name ="pStartIndent" select ="0"/>
    <fo:table-cell border="{$pBorder}" padding="{$pPadding}cm" display-align="{$pDisplayAlign}" background-color="{$pBackgroundColor}" >
      <fo:block font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}" >
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
    <xsl:param name ="pBorderLeftStyle" />
    <xsl:param name ="pBorderLeftWidth" />
    <xsl:param name ="pBorderRightStyle" />
    <xsl:param name ="pBorderRightWidth" />
    <xsl:param name ="pBorderTopStyle" />
    <xsl:param name ="pBorderTopWidth" />
    <xsl:param name ="pBorderBottomStyle" />
    <xsl:param name ="pBorderBottomWidth" />
    <xsl:param name ="pFontSize" select ="$vFontSize"/>
    <xsl:param name ="pFontWeight" select ="'normal'"/>
    <xsl:param name ="pColor" select ="'#000'"/>
    <xsl:param name ="pTextAlign" select ="'left'"/>

    <!-- EG 20160404 Migration vs2013 -->
    <fo:table-cell padding="{$pPadding}cm" border="{$pBorder}" display-align="{$pDisplayAlign}" background-color="{$pBackgroundColor}" >
      <xsl:call-template name="DisplayBorderAttributes">
        <xsl:with-param name="pLeftStyle" select="$pBorderLeftStyle"/>
        <xsl:with-param name="pLeftWidth" select="$pBorderLeftWidth"/>
        <xsl:with-param name="pRightStyle" select="$pBorderRightStyle"/>
        <xsl:with-param name="pRightWidth" select="$pBorderRightStyle"/>
        <xsl:with-param name="pTopStyle" select="$pBorderTopStyle"/>
        <xsl:with-param name="pTopWidth" select="$pBorderTopWidth"/>
        <xsl:with-param name="pBottomStyle" select="$pBorderBottomStyle"/>
        <xsl:with-param name="pBottomWidth" select="$pBorderBottomWidth"/>
      </xsl:call-template>
      <fo:block font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}" >
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
        <xsl:call-template name ="create2Columns">
          <xsl:with-param name="pColumnWidth1" select ="15"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="left" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="normal">
                Page <fo:page-number/> / <fo:page-number-citation ref-id="EndOfDoc"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block/>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block/>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" display-align="center">
              <fo:block border="{$vBorder}" text-align="right" font-family="{$vFooterFontFamily}" font-size="{$vFooterFontSize}" font-weight="bold" color="DarkGrey">
                <!-- EG 20160404 Migration vs2013 -->
                <xsl:text>Powered by Spheres - © 2024 EFS</xsl:text>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>


</xsl:stylesheet>
