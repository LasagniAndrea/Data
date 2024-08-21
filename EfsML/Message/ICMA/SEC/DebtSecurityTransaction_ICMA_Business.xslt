<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  version="1.0">

  <!--
	==============================================================
	Summary : DebtSecurityTransaction ICMA Business
	==============================================================
	File    : DebtSecurityTransaction_ICMA_Business.xslt
	Date    : 28.05.2009
	Author  : 
	Version : 2.3.0.4_1
	Description:
	==============================================================
	Revision:
	Date    :
	Author  :
	Version :
	Comment :
	==============================================================
	-->

  <!-- 
		Define the parties id (HREF) of the actors who pay and receive the financial flows: 
     - Party1 is the buyer on the stream 1
		 - Party2 is the seller on the stream 1
	-->

  <xsl:variable name="partyID_1" select="//dataDocument/trade/debtSecurityTransaction/buyerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="//dataDocument/trade/debtSecurityTransaction/sellerPartyReference/@href"/>

  <!-- IsStream1NotionalStepSchedule variable: "true" if the notional has some steps -->
  <xsl:variable name="IsStream1NotionalStepSchedule" select="//dataDocument/trade/debtSecurityTransaction/securityAsset/debtSecurity/debtSecurityStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step/node()">
  </xsl:variable>

  <!-- getStream1NotionalStepScheduleInitialValue variable: gets the initial value of the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue" select="//dataDocument/trade/debtSecurityTransaction/securityAsset/debtSecurity/debtSecurityStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue">
  </xsl:variable>

  <!-- getStream1NotionalSteps variable: gets the notional steps of the first stream -->
  <xsl:variable name="getStream1NotionalSteps" select="//dataDocument/trade/debtSecurityTransaction/securityAsset/debtSecurity/debtSecurityStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">
  </xsl:variable>

</xsl:stylesheet>