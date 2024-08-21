<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='FxBarrierOption']">
		<xsl:param name="pPosition" select="0"/>
		<fxBarrierOption>
			<keyGroup name="FxBarrierOption" key="PRD"/>
			<itemGroup>
				<xsl:choose>
					<xsl:when test = "$pPosition = '0'"><occurence to="Unique">product</occurence></xsl:when>
					<xsl:otherwise><occurence to="Item" position="{$pPosition}">Item</occurence></xsl:otherwise>
				</xsl:choose>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Product</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Date</eventType>
				</key>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>
			
			<!-- Premium -->
			<xsl:call-template name="FxOptionPremium"/>
			<xsl:call-template name="FxOptionType">
				<xsl:with-param name="pType" select="'FxBarrierOption'"/>
			</xsl:call-template>

      <xsl:call-template name="FxOptionProvisions" />
    
		</fxBarrierOption>
	</xsl:template>
</xsl:stylesheet>