<?xml version="1.0" encoding="ISO-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!--
	==============================================================
	Summary : Fra ISDA Business
	==============================================================
	File    : Fra_ISDA_Business.xslt
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
  <xsl:variable name="partyID_1" select="//dataDocument/trade/fra/buyerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="//dataDocument/trade/fra/sellerPartyReference/@href"/>


  <!-- IsStream1NotionalStepSchedule variable: "true" if the notional has some steps-->
  <xsl:variable name="isStream1NotionalStepSchedule" select="//dataDocument/trade/fra/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step/node()">
  </xsl:variable>

  <!-- getStream1NotionalStepSchedule variable: gets the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepSchedule" select="//dataDocument/trade/fra/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule">
  </xsl:variable>

  <!-- getStream1NotionalStepScheduleInitialValue variable: gets the initial value of the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue" select="//dataDocument/trade/fra/notional/amount">
  </xsl:variable>

  <!-- getStream1NotionalSteps variable: gets the notional steps of the first stream -->
  <xsl:variable name="getStream1NotionalSteps" select="//dataDocument/trade/fra/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">
  </xsl:variable>

</xsl:stylesheet>