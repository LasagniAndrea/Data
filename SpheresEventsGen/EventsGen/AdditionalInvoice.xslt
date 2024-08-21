<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='AdditionalInvoice']">
		<xsl:param name="pPosition" select="0"/>
		<additionalInvoice>

			<!-- Product -->
			<keyGroup name="AdditionalInvoice" key="PRD"/>
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

			<!-- InvoiceCorrection -->
			<xsl:call-template name="InvoiceCorrection"/>
		</additionalInvoice>
	</xsl:template>


</xsl:stylesheet>