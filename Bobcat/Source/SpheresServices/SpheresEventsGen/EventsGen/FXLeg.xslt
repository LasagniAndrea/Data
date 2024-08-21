<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='FxLeg']">
		<xsl:param name="pPosition" select="0"/>
		<fxSingleLeg>
		
			<!-- Product -->
			<keyGroup name="FxLeg" key="PRD"/>
      <parameter id="FXLegValueDate">ValueDate</parameter>
      <parameter id="IMPayerPartyReference">InitialMarginPayerPartyReference</parameter>
      <parameter id="IMReceiverPartyReference">InitialMarginReceiverPartyReference</parameter>
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

      <xsl:call-template name="FxDeliverableLeg">
        <xsl:with-param name="pPosition" select="$pPosition"/>
      </xsl:call-template>

      <xsl:call-template name="FxNonDeliverableLeg">
        <xsl:with-param name="pPosition" select="$pPosition"/>
      </xsl:call-template>

		</fxSingleLeg>
	</xsl:template>
</xsl:stylesheet>