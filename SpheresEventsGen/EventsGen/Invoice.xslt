<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='Invoice']">
		<xsl:param name="pPosition" select="0"/>
		<invoice>
			<!-- Product -->
			<keyGroup name="Invoice" key="PRD"/>
			<parameter id="InvoiceDate">InvoiceDate</parameter>
			<parameter id="AdjustedInvoiceDate">AdjustedInvoiceDate</parameter>
			<parameter id="AdjustedTaxInvoiceDate">AdjustedTaxInvoiceDate</parameter>
			<parameter id="AdjustedSettlementDate">AdjustedSettlementDate</parameter>
			<parameter id="InvoicePayer">PayerPartyReference</parameter>
			<parameter id="InvoiceReceiver">ReceiverPartyReference</parameter>
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
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<xsl:variable name="Invoice">
				<xsl:text>LinkedProductPayment</xsl:text>
			</xsl:variable>

			<groupLevel>
				<keyGroup name="GroupLevel" key="INV"/>
				<parameter id="{$Invoice}_TurnOverIssueRate">TurnOverIssueRate</parameter>
				<parameter id="{$Invoice}_TurnOverIssueBase">TurnOverIssueBase</parameter>
				<parameter id="{$Invoice}_TurnOverIssueAmount">TurnOverIssueAmount</parameter>
				<parameter id="{$Invoice}_TurnOverIssueCurrency">TurnOverIssueCurrency</parameter>
				<parameter id="{$Invoice}_TurnOverAccountingRate">TurnOverAccountingRate</parameter>
				<parameter id="{$Invoice}_TurnOverAccountingBase">TurnOverAccountingBase</parameter>
				<parameter id="{$Invoice}_TurnOverAccountingAmount">TurnOverAccountingAmount</parameter>
				<parameter id="{$Invoice}_TurnOverAccountingCurrency">TurnOverAccountingCurrency</parameter>
				<parameter id="{$Invoice}_TaxAmount">TaxAmount</parameter>
				<parameter id="{$Invoice}_TaxCurrency">TaxCurrency</parameter>
				<parameter id="{$Invoice}_TaxIssueBase">TaxIssueBase</parameter>
				<parameter id="{$Invoice}_TaxIssueAmount">TaxIssueAmount</parameter>
				<parameter id="{$Invoice}_TaxIssueCurrency">TaxIssueCurrency</parameter>
				<parameter id="{$Invoice}_TaxAccountingBase">TaxAccountingBase</parameter>
				<parameter id="{$Invoice}_TaxAccountingAmount">TaxAccountingAmount</parameter>
				<parameter id="{$Invoice}_TaxAccountingCurrency">TaxAccountingCurrency</parameter>

				<itemGroup>
					<occurence to="Unique">efs_Invoice</occurence>
					<key>
						<eventCode>EfsML.Enum.Tools.EventCodeFunc|InvoicingDates</eventCode>
						<eventType>EfsML.Enum.Tools.EventTypeFunc|Period</eventType>
					</key>
					<idStCalcul>[CALC]</idStCalcul>
					<startDateReference hRef="TradeDate"/>
					<endDateReference hRef="InvoiceDate"/>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
						<eventDateReference hRef="AdjustedInvoiceDate"/>
					</subItem>
				</itemGroup>

				<!-- GrossTurnOverAmount -->
				<xsl:call-template name="TurnOverAndTaxAmount">
					<xsl:with-param name="pInvoiceSection" select="$Invoice"/>
					<xsl:with-param name="pTurnOverType" select="'Gross'"/>
				</xsl:call-template>
				
				<!-- RebateAmount -->
				<xsl:call-template name="InvoiceRebateAmount">
					<xsl:with-param name="pInvoiceSection" select="$Invoice"/>
				</xsl:call-template>

				<!-- GlobalNetTurnOver -->
				<xsl:call-template name="GlobalNetTurnOverAndTax">
					<xsl:with-param name="pInvoiceSection" select="$Invoice"/>
				</xsl:call-template>

			</groupLevel>

		</invoice>
	</xsl:template>
</xsl:stylesheet>