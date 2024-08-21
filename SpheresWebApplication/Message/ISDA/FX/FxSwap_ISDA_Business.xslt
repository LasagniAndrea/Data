<?xml version="1.0" encoding="ISO-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  version="1.0">

  <!--
	==============================================================
	Summary : FxSwap ISDA Business
	==============================================================
	File    : FxSwap_ISDA_Business.xslt
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	==============================================================
	Revision:
	Date    :
	Author  :
	Version :
	Updates :
	==============================================================
	-->

  <!-- Parties variables -->
  <xsl:variable name="partyID_1" select="//dataDocument/trade/fxSwap/fxSingleLeg[1]/exchangedCurrency1/payerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="//dataDocument/trade/fxSwap/fxSingleLeg[1]/exchangedCurrency1/receiverPartyReference/@href"/>
  <!-- variable in false -->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue" select="false"/>
  <xsl:variable name="getStream1NotionalStepSchedule" select="false"/>
  <xsl:variable name="isStream1NotionalStepSchedule" select="false"/>
  <xsl:variable name="getStream1NotionalSteps" select="false"/>

</xsl:stylesheet>