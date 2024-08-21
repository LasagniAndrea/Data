<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">

  <!--
	==============================================================
	Summary : Bond Option ISDA Business
	==============================================================
	==============================================================
	File    : BondOption_ISDA_PDF.xslt
	Date    : 22.04.2015
	Author  : Pony LA
	Version : 4.6.0.0
	Description: 
	==============================================================
	-->

  <!-- Define the parties id (HREF) of the actors who pay and receive the financial flows -->

  <!-- Parties variables -->
  <xsl:variable name="partyID_1" select="//dataDocument/trade/bondOption/buyerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="//dataDocument/trade/bondOption/sellerPartyReference/@href"/>

  <!-- variable in false -->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue" select="false"/>
  <xsl:variable name="getStream1NotionalStepSchedule" select="false"/>
  <xsl:variable name="isStream1NotionalStepSchedule" select="false"/>
  <xsl:variable name="getStream1NotionalSteps" select="false"/>


</xsl:stylesheet>