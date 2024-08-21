<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format"
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" version="1.0">

  <!--
	==============================================================
	Summary : Swap ISDA HTML
	==============================================================
	File    : Swap_ISDA_HTML.xslt
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	Description:
	==============================================================
	Revision:
	Date    :
	Author  :
	Version :
	Comment :
	==============================================================
	-->

  <xsl:import href="..\Shared\Shared_ISDA_HTML.xslt"/>
  <xsl:import href="..\Shared\Shared_ISDA_Business.xslt"/>
  <xsl:import href="Swap_ISDA_Business.xslt"/>
  <xsl:import href="..\..\Custom\Custom_Message_HTML.xslt"/>

  <!-- xslt output -->
  <xsl:output method="html" indent="yes" encoding="UTF-16"/>


  <!-- xslt includes -->
  <xsl:include href="..\..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\..\Library\StrFunc.xslt"/>
  <xsl:include href="..\..\..\Library\xsltsl\date-time.xsl"/>

  <!-- Global Parameters -->
  <xsl:param name="pCurrentCulture" select="'en-GB'"/>
  <xsl:param name="pTradeExtlLink" select="''"/>
  <xsl:param name="pProduct" select="''"/>
  <xsl:param name="pInstrument" />

  <xsl:variable name="outputMethod" select="'html'"/>
  <!-- Display Web Page: template match-->
  <xsl:template match="/">
    <html>
      <head>
        <title>
          <xsl:call-template name="getTitle"/>
          <xsl:text>&#x20;</xsl:text>
          <xsl:call-template name="getNCS"/>
        </title>
        <STYLE TYPE="text/css">
          <xsl:call-template name="displayCSS"/>
        </STYLE>
      </head>
      <body style="margin: 1px; padding: 0px; font-family: Verdana; font-size: 11pt;">
        <xsl:call-template name="displayLogo"/>
        <br/>
        <table style="width: 780px;vertical-align:top; line-height:11pt" cellpadding="0" align="center" border="0" bgcolor="white" height="100%">
          <!-- 
					Enable this section to display the logo on each page (in this case take out it on "displayConfirmationBody" template )
					<thead>
					<xsl:call-template name="DisplayLogo"/>
					<xsl:call-template name="BreakLine"/>
					</thead>
					-->
          <tbody>
            <xsl:call-template name="displayConfirmationBody">
              <xsl:with-param name="pTDCSS" select="'font-size:9pt;line-height:11pt;'"/>
            </xsl:call-template>
          </tbody>
          <tfoot>
            <xsl:call-template name="displayFooter"/>
          </tfoot>
        </table>
      </body>
    </html>
  </xsl:template>


  <!-- ========================================================================================== -->
  <!-- BEGIN  of Region Display Confirmation Body    -->
  <!-- ========================================================================================== -->
  <!-- displayTradeBody: outline the trade section of the confirmation -->
  <xsl:template name="displayConfirmationBody">
    <xsl:param name="pTDCSS"/>
    <xsl:call-template name="displayTitle"/>
    <xsl:call-template name="BreakLine"/>
    <xsl:call-template name="BreakLine"/>
    <xsl:call-template name="displayHeader">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <xsl:call-template name="displayIrdConfirmationText">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <!-- Call template Display Terms (notional amount, trade date, etc.) -->
    <xsl:call-template name="displayIrdTerms">
      <xsl:with-param name="pStreams" select="//dataDocument/trade/swap/swapStream"/>
      <xsl:with-param name="pIsCrossCurrency" select="boolean($IsCrossCurrency)"/>
      <xsl:with-param name="pIsNotionalDifferent" select="boolean($IsNotionalDifferent)"/>
      <!-- to use for a swaption: when the condition is in "true" it display the Title of the section "Underlying Swap". -->
      <xsl:with-param name="pIsUnderlyngSwap" select="false()"/>
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <!-- Call template handleStreams  -->
    <xsl:call-template name="handleIrdStreams">
      <xsl:with-param name="pStreams" select="//dataDocument/trade/swap/swapStream"/>
      <xsl:with-param name="pIsCrossCurrency" select="boolean($IsCrossCurrency)"/>
      <!-- To display the title in Floating and Fixing Amount Region. When the condition is in "true" it Display the Title-->
      <xsl:with-param name="pIsDisplayTitle" select="true()"/>
      <!-- To display the detail in Floating and Fixing Amount Region. -->
      <!-- when the condition is "true", it displays detailled informations about payment dates (Rate Payer/Rate Option/Spread/Day Count/Payment Dates/Reset Dates/Compounding)-->
      <!-- when the condition is "false", it displays only basic information (Rate Payer/Rate Option/Maturity/Spread/Day Count/Compounding)-->
      <xsl:with-param name="pIsDisplayDetail" select="true()"/>
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <!-- Call template Display Exchange with parameter Initial -->
    <xsl:if test="//dataDocument/trade/swap/swapStream/principalExchanges/initialExchange">
      <xsl:call-template name="displayExchange">
        <xsl:with-param name="pExchange" select="'Initial'"/>
        <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
      </xsl:call-template>
      <xsl:call-template name="BreakLine"/>
    </xsl:if>
    <!-- Call template Display Exchange with parameter Final  -->
    <xsl:if test="//dataDocument/trade/swap/swapStream/principalExchanges/finalExchange">
      <xsl:call-template name="displayExchange">
        <xsl:with-param name="pExchange" select="'Final'"/>
        <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
      </xsl:call-template>
      <xsl:call-template name="BreakLine"/>
    </xsl:if>
    <!-- Call template displayIrdCalculationAgent  -->
    <xsl:call-template name="displayIrdCalculationAgent">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <xsl:if test="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage' and payerPartyReference/@href = $sendToID]">
      <xsl:call-template name="displayBrokerage"/>
      <xsl:call-template name="BreakLine"/>
    </xsl:if>

    <xsl:call-template name="displayFooter_Product">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <xsl:call-template name="displaySignature">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ========================================================================================== -->
  <!-- END  of Region Display Confirmation Body    -->
  <!-- ========================================================================================== -->

</xsl:stylesheet>