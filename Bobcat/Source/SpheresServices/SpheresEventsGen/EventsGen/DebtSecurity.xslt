<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='DebtSecurity']">
		<xsl:param name="pPosition" select="0"/>
		<debtSecurity>

			<!-- Product -->
      <!--EG 20190823 [FIXEDINCOME] Parameter IsPerpetual-->
			<keyGroup name="DebtSecurity" key="PRD"/>
			<parameter id="MinEffectiveDate">MinEffectiveDate</parameter>
			<parameter id="MaxTerminationDate">MaxTerminationDate</parameter>
			<parameter id="IsDiscount">IsDiscount</parameter>
			<parameter id="IsNotDiscount">IsNotDiscount</parameter>
      <parameter id="IsPerpetual">IsPerpetual</parameter>

			<itemGroup>
				<xsl:choose>
					<xsl:when test = "$pPosition = '0'">
						<occurence to="Unique">product</occurence>
					</xsl:when>
					<xsl:otherwise>
						<occurence to="Item" position="{$pPosition}">Item</occurence>
					</xsl:otherwise>
				</xsl:choose>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Product</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Date</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>

			<!-- Stream -->
			<debtSecurityStream>
				<xsl:call-template name="DebtSecurityStream" />
			</debtSecurityStream>

		</debtSecurity>
	</xsl:template>

	<!-- DebtSecurityStream -->
  <!-- [WI828] AssetDebtSecurity: La génération des évènements est en erreur -->
	<xsl:template name="DebtSecurityStream">
		<keyGroup name="Stream" key="STD"/>
		<itemGroup>	
			<occurence to="All">debtSecurityStream</occurence>
			<parameter id="Payer">GetPayerPartyReference</parameter>
			<parameter id="Receiver">GetReceiverPartyReference</parameter>
			<parameter id="CalculationPeriodAmount">CalculationPeriodAmount</parameter>
			<parameter id="Rate">Rate('None')</parameter>
			<parameter id="RateInitialStub">Rate('Initial')</parameter>
			<parameter id="RateFinalStub">Rate('Final')</parameter>
			<parameter id="DCF">DayCountFraction</parameter>
			<parameter id="IsInitialExchange">IsInitialExchange</parameter>
			<parameter id="IsIntermediateExchange">IsIntermediateExchange</parameter>
			<parameter id="IsFinalExchange">IsFinalExchange</parameter>
			<parameter id="StreamEffectiveDate">EffectiveDate</parameter>
			<parameter id="StreamTerminationDate">TerminationDate</parameter>
			<parameter id="StreamAdjustedEffectiveDate">AdjustedEffectiveDate</parameter>
			<parameter id="StreamAdjustedTerminationDate">AdjustedTerminationDate</parameter>
			<key>
				<eventCode>EfsML.Enum.Tools.EventCodeFunc|DebtSecurityStream</eventCode>
				<eventType>EventType2('DebtSecurity',IsDiscount)</eventType>
			</key>
			<idStCalcul>[CALC]</idStCalcul>
			<startDateReference hRef="StreamEffectiveDate"/>
			<endDateReference hRef="StreamTerminationDate"/>
			<payerReference hRef="Payer"/>
			<receiverReference hRef="Receiver"/>
			<subItem>
				<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
				<eventDateReference hRef="StreamAdjustedEffectiveDate"/>
			</subItem>
		</itemGroup>


		<!-- NominalPeriodsVariation -->
		<xsl:call-template name="NominalPeriodsVariation">
			<xsl:with-param name="pEventCode" select="'Start'"/>
		</xsl:call-template>
    
	    <xsl:call-template name="NominalPeriodsVariation">
			<xsl:with-param name="pEventCode" select="'Intermediary'"/>
		</xsl:call-template>
    
	    <xsl:call-template name="NominalPeriodsVariation">
			<xsl:with-param name="pEventCode" select="'Termination'"/>
		</xsl:call-template>

		 <!--NominalPeriods-->
	    <xsl:call-template name="NominalPeriods">
			<xsl:with-param name="pEventCode" select="'NominalStep'"/>
		</xsl:call-template>
    
		<!-- PaymentDates -->
    <xsl:call-template name="PaymentDates">
			<xsl:with-param name="pEventCode" select="'Intermediary'"/>
		</xsl:call-template>
		<xsl:call-template name="PaymentDates">
			<xsl:with-param name="pEventCode" select="'Termination'"/>
		</xsl:call-template>
    
	</xsl:template>



</xsl:stylesheet>