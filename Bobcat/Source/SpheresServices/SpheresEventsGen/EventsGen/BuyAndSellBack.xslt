<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='BuyAndSellBack']">
		<xsl:param name="pPosition" select="0"/>
		<buyAndSellBack>

			<!-- Product -->
			<keyGroup name="BuyAndSellBack" key="PRD"/>
			<parameter id="MinEffectiveDate">MinEffectiveDate</parameter>
			<parameter id="MaxTerminationDate">MaxTerminationDate</parameter>

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

			<xsl:call-template name="RepoDuration">
				<xsl:with-param name="pProduct" select="'BuyAndSellBack'"/>
			</xsl:call-template>
					
		</buyAndSellBack>
	</xsl:template>
</xsl:stylesheet>