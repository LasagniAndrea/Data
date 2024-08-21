<?xml version="1.0" encoding="ISO-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!--
	==============================================================
	Summary : FxLeg ISDA HTML
	==============================================================
	File    : FxLeg_ISDA_HTML.xslt
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

  <xsl:import href="..\Shared\Shared_ISDA_Business.xslt"/>
  <xsl:import href="..\Shared\Shared_ISDA_HTML.xslt"/>
  <xsl:import href="FxLeg_ISDA_Business.xslt"/>
  <xsl:import href="..\..\Custom/Custom_Message_HTML.xslt"/>

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
    <xsl:call-template name="displayFxConfirmationText">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <xsl:call-template name="displayFxSwapLegTerms">
      <xsl:with-param name="pFxLegContainer" select="//trade" />
      <xsl:with-param name="pIsNonDeliverable" select="//dataDocument/trade/fxSingleLeg/nonDeliverableForward/node()" />
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <!-- Call template displayIrdCalculationAgent  -->
    <xsl:call-template name="displayIrdCalculationAgent">
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
    <xsl:call-template name="BreakLine"/>
    <xsl:if test="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage' and payerPartyReference/@href = $sendToID]">
      <xsl:call-template name="displayBrokerage">
        <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
      </xsl:call-template>
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