<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='FxSwap']">
		<xsl:param name="pPosition" select="0"/>
		<fxSwap>
		
			<!-- Product -->
			<keyGroup name="FxSwap" key="PRD"/>
			<itemGroup>
				<xsl:choose>
					<xsl:when test = "$pPosition = '0'"><occurence to="Unique">product</occurence></xsl:when>
					<xsl:otherwise><occurence to="Item" position="{$pPosition}">Item</occurence></xsl:otherwise>
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
			
			<xsl:call-template name="FxDeliverableLeg"/>
			<xsl:call-template name="FxNonDeliverableLeg"/>
		</fxSwap>
	</xsl:template>
	
	<xsl:template name="FxLegs">
		<xsl:param name="pProduct"/>
		<keyGroup name="Leg" key="PRD"/>
		<itemGroup>
			<occurence to="All">fxSingleLeg</occurence>
		</itemGroup>
		<xsl:call-template name="FxDeliverableLeg"/>
		<xsl:call-template name="FxNonDeliverableLeg"/>
	</xsl:template>
</xsl:stylesheet>