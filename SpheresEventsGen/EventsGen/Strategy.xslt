<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='Strategy']">
		<xsl:param name="pPosition" select="0"/>
		<strategy>
		
			<!-- Strategy Dates -->
			<keyGroup name="Strategy" key="STG"/>
			<itemGroup>
				<xsl:choose>
					<xsl:when test = "$pPosition = '0'"><occurence to="Unique">product</occurence></xsl:when>
					<xsl:otherwise><occurence to="Item" position="{$pPosition}">Item</occurence></xsl:otherwise>
				</xsl:choose>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Strategy</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Date</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>

			<!-- SubProduct (Products of Strategy) -->
			<xsl:for-each select="item">
				<xsl:apply-templates mode="product" select=".">
					<xsl:with-param name="pPosition" select="position()"/>
				</xsl:apply-templates>
			</xsl:for-each>
		</strategy>		
	</xsl:template>
</xsl:stylesheet>
  
  