<?xml version="1.0" encoding="ISO-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <!--
	==============================================================
	Summary : CapFloor ISDA Business
	==============================================================
	Revision: 1	
	Date    : 31.08.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_2
	Updates : Delete IsCollar variable
	          Add isNotionalDifferent variable (always in false)
	==============================================================
	File    : CapFloor_ISDA_Business.xslt
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	==============================================================
	-->
  <!-- 
	Define the parties id (HREF) of the actors who pay and receive the financial flows: 
	     - Party1 is the payer on the stream 1
		 - Party2 is the receiver on the stream 1
	-->

  <!-- Parties variables - the "stream" path variable is defined into the specific XSL file of the product -->
  <xsl:variable name="partyID_1" select="$capFloorStream/payerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="$capFloorStream/receiverPartyReference/@href"/>

  <!-- IsStream1NotionalStepSchedule variable: "true" if the notional has some steps -->
  <xsl:variable name="isStream1NotionalStepSchedule" select="$capFloorStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step/node()">
  </xsl:variable>

  <!-- getStream1NotionalStepSchedule variable: gets the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepSchedule" select="$capFloorStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule">
  </xsl:variable>

  <!-- getStream1NotionalStepScheduleInitialValue variable: gets the initial value of the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue" select="$capFloorStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue">
  </xsl:variable>

  <!-- getStream1NotionalSteps variable: gets the notional steps of the first stream -->
  <xsl:variable name="getStream1NotionalSteps" select="$capFloorStream//calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">
  </xsl:variable>

  <xsl:variable name="capFloorStream" select="//dataDocument/trade/capFloor/capFloorStream"/>

  <!-- isNotionalDifferent variable: it is false always.  -->
  <xsl:variable name="isNotionalDifferent" select="'false'"/>

  <!--
	<xsl:variable name="IsCollar" select="$capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule/node()
	= $capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule/node()"/>
	-->

</xsl:stylesheet>