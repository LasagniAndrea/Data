<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='InvoiceSettlement']">
		<xsl:param name="pPosition" select="0"/>
		<invoiceSettlement>
			<!-- Product -->
			<keyGroup name="InvoiceSettlement" key="PRD"/>
			<parameter id="ReceptionDate">ReceptionDate</parameter>
			<parameter id="AdjustedReceptionDate">AdjustedReceptionDate</parameter>
			<parameter id="InvoiceSettlementPayer">PayerPartyReference</parameter>
			<parameter id="InvoiceSettlementReceiver">ReceiverPartyReference</parameter>
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
					<eventDateReference hRef="AdjustedReceptionDate"/>
				</subItem>
			</itemGroup>

			<groupLevel>
				<keyGroup name="GroupLevel" key="IST"/>
				<parameter id="FxGainOrLossType">FxGainOrLossType</parameter>
        <parameter id="FxGainOrLossPayer">FxGainOrLossPayerReference</parameter>
        <parameter id="FxGainOrLossReceiver">FxGainOrLossReceiverReference</parameter>
        <parameter id="InvoiceSettlementBank">BankPartyReference</parameter>
        <parameter id="FxGainOrLossAmount">FxGainOrLossAmount</parameter>
        <itemGroup>
					<occurence to="Unique">efs_InvoiceSettlement</occurence>
					<key>
						<eventCode>EfsML.Enum.Tools.EventCodeFunc|InvoiceSettlement</eventCode>
						<eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
					</key>
					<idStCalcul>[CALC]</idStCalcul>
					<startDateReference hRef="ReceptionDate"/>
					<endDateReference hRef="ReceptionDate"/>
					<valorisation>SettlementAmount</valorisation>
					<unitType>[Currency]</unitType>
					<unit>SettlementCurrency</unit>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
						<eventDateReference hRef="AdjustedReceptionDate"/>
					</subItem>
				</itemGroup>

				<!-- InvoiceSettlementAmount -->
				<xsl:call-template name="InvoiceSettlementAmount">
					<xsl:with-param name="pAmount" select="'netCash'"/>
				</xsl:call-template>
				<xsl:call-template name="InvoiceSettlementAmount">
					<xsl:with-param name="pAmount" select="'bankCharges'"/>
				</xsl:call-template>
				<xsl:call-template name="InvoiceSettlementAmount">
					<xsl:with-param name="pAmount" select="'vatBankCharges'"/>
				</xsl:call-template>
				<xsl:call-template name="InvoiceSettlementAmount">
					<xsl:with-param name="pAmount" select="'fxGainOrLoss'"/>
				</xsl:call-template>
				<xsl:call-template name="InvoiceSettlementAmount">
					<xsl:with-param name="pAmount" select="'unallocated'"/>
				</xsl:call-template>

				<!-- AllocatedAmountDates -->
				<xsl:call-template name="AllocatedAmountDates"/>

			</groupLevel>

		</invoiceSettlement>
	</xsl:template>


	<!-- InvoiceSettlementAmount -->
	<xsl:template name="InvoiceSettlementAmount">
		<xsl:param name="pAmount"/>
		<xsl:variable name="eventClass">
			<xsl:choose>
				<xsl:when test = "$pAmount = 'fxGainOrLoss' or $pAmount = 'unallocated'">Recognition</xsl:when>
				<xsl:otherwise>Settlement</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<invoiceSettlementAmount>
			<keyGroup name="AllocatedAmountDates" key="ALD"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true"><xsl:value-of select="$pAmount"/>Amount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
					<xsl:choose>
						<xsl:when test = "$pAmount = 'netCash'">
							<eventType>EfsML.Enum.Tools.EventTypeFunc|CashSettlement</eventType>
						</xsl:when>
						<xsl:when test = "$pAmount = 'bankCharges'">
							<eventType>EfsML.Enum.Tools.EventTypeFunc|Fee</eventType>
						</xsl:when>
						<xsl:when test = "$pAmount = 'vatBankCharges'">
							<eventType>EfsML.Enum.Tools.EventTypeFunc|ValueAddedTax</eventType>
						</xsl:when>
						<xsl:when test = "$pAmount = 'unallocated'">
							<eventType>EfsML.Enum.Tools.EventTypeFunc|NonAllocatedAmount</eventType>
						</xsl:when>
						<xsl:when test = "$pAmount = 'fxGainOrLoss'">
							<eventTypeReference hRef="FxGainOrLossType"/>
						</xsl:when>
						<xsl:otherwise>None</xsl:otherwise>
					</xsl:choose>
				</key>
				<startDateReference hRef="ReceptionDate"/>
				<endDateReference hRef="ReceptionDate"/>
				<xsl:choose>
					<xsl:when test = "$pAmount = 'bankCharges' or $pAmount = 'vatBankCharges'">
						<payerReference hRef="InvoiceSettlementReceiver"/>
						<receiverReference hRef="InvoiceSettlementBank"/>
					</xsl:when>
          <xsl:when test = "$pAmount = 'fxGainOrLoss'">
            <payerReference hRef="FxGainOrLossPayer"/>
            <receiverReference hRef="FxGainOrLossReceiver"/>
          </xsl:when>
          <xsl:otherwise>
						<payerReference hRef="InvoiceSettlementPayer"/>
						<receiverReference hRef="InvoiceSettlementReceiver"/>
					</xsl:otherwise>
				</xsl:choose>
        <xsl:choose>
          <xsl:when test = "$pAmount = 'fxGainOrLoss'">
            <valorisationReference hRef="FxGainOrLossAmount"/>
          </xsl:when>
          <xsl:otherwise>
            <valorisation>Amount</valorisation>
          </xsl:otherwise>
        </xsl:choose>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|<xsl:value-of select="$eventClass"/></eventClass>
					<eventDateReference hRef="AdjustedReceptionDate"/>
					<xsl:choose>
						<xsl:when test = "$pAmount = 'fxGainOrLoss' or $pAmount = 'unallocated'"><isPayment>0</isPayment></xsl:when>
						<xsl:otherwise><isPayment>1</isPayment></xsl:otherwise>
					</xsl:choose>
				</subItem>
			</itemGroup>
		</invoiceSettlementAmount>
	</xsl:template>

	<!-- AllocatedAmountDates -->
	<xsl:template name="AllocatedAmountDates">
		<xsl:param name="pAmountType"/>
		<allocatedAmountDates>
			<keyGroup name="AllocatedAmountDates" key="ALD"/>
			<itemGroup>
				<occurence to="All" isOptional="true">allocatedInvoice</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|AllocatedInvoiceDates</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|CashSettlement</eventType>
					<idE_Source>InvoiceIdT</idE_Source> <!-- Utilisation de ce tag pour stoker l'IdT de la facture allouée -->
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="ReceptionDate"/>
				<endDateReference hRef="ReceptionDate"/>

				<payerReference hRef="InvoiceSettlementPayer"/>
				<receiverReference hRef="InvoiceSettlementReceiver"/>

				<valorisation>AccountingAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>AccountingCurrency</unit>
				
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedReceptionDate"/>
				</subItem>
			</itemGroup>
		</allocatedAmountDates>
	</xsl:template>

</xsl:stylesheet>