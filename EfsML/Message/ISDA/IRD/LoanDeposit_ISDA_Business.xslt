<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  version="1.0">

  <!--
	==============================================================
	Summary : LoanDeposit ISDA Business
	==============================================================
	File    : LoanDeposit_ISDA_Business.xslt
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

  <!-- 
		Define the parties id (HREF) of the actors who pay and receive the financial flows: 
	     - Party1 is the payer on the stream 1
		 - Party2 is the receiver on the stream 1
	-->

  <xsl:variable name="partyID_1" select="//dataDocument/trade/loanDeposit/loanDepositStream/payerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="//dataDocument/trade/loanDeposit/loanDepositStream/receiverPartyReference/@href"/>

  <!-- IsCrossCurrency variable: "true" if the swap is a cross currency swap -->
  <xsl:variable name="isCrossCurrency" select="(//dataDocument/trade/loanDeposit/loanDepositStream[id='loanDepositStream2']/node())
			and
			(//dataDocument/trade/loanDeposit/loanDepositStream/calculationPeriodAmount/calculation/node())
			and
			(//dataDocument/trade/loanDeposit/loanDepositStream[id='loanDepositStream1']/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency
			!=//dataDocument/trade/loanDeposit/loanDepositStream[id='loanDepositStream2']//calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency)">
  </xsl:variable>

  <!-- IsStream1NotionalStepSchedule variable: "true" if the notional has some steps -->
  <xsl:variable name="isStream1NotionalStepSchedule" select="//dataDocument/trade/loanDeposit/loanDepositStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step/node()">
  </xsl:variable>

  <!-- getStream1NotionalStepSchedule variable: gets the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepSchedule" select="//dataDocument/trade/loanDeposit/loanDepositStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule">
  </xsl:variable>

  <!-- getStream1NotionalStepScheduleInitialValue variable: gets the initial value of the NotionalStepSchedule of the first stream -->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue" select="//dataDocument/trade/loanDeposit/loanDepositStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue">
  </xsl:variable>

  <!-- getStream1NotionalSteps variable: gets the notional steps of the first stream -->
  <xsl:variable name="getStream1NotionalSteps" select="//dataDocument/trade/loanDeposit/loanDepositStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">
  </xsl:variable>

  <!-- IsNotionalDifferent variable: "true" if distinct streams have different notional amounts -->
  <xsl:variable name="isNotionalDifferent" select="(//dataDocument/trade/loanDeposit/loanDepositStream[id='loanDepositStream2']/node())
			and
			(//dataDocument/trade/loanDeposit/loanDepositStream/calculationPeriodAmount/calculation/node())
			and
			(//dataDocument/trade/loanDeposit/loanDepositStream[id='loanDepositStream1']/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue
			!=//dataDocument/trade/loanDeposit/loanDepositStream[id='loanDepositStream2']/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue)">
  </xsl:variable>
</xsl:stylesheet>