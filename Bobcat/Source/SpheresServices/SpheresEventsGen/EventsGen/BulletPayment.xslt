<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='BulletPayment']">
		<xsl:param name="pPosition" select="0"/>
		<bulletPayment>
			<!-- Product -->		
			<keyGroup name="BulletPayment" key="PRD"/>
			<parameter declaringType="Payment" id="PaymentDate">PaymentDate</parameter>
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
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="PaymentDate"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>
			
			<groupLevel>
				<keyGroup name="GroupLevel" key="GRP"/>
				<itemGroup>	
					<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|BulletPayment</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|BulletPayment</eventType>
					</key>
					<idStCalcul>[CALC]</idStCalcul>
					<startDateReference hRef="TradeDate"/>
					<endDateReference hRef="PaymentDate"/>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
						<eventDateReference hRef="AdjustedTradeDate"/>
					</subItem>
				</itemGroup>
				<!-- Payment -->
				<payment>
					<xsl:call-template name="Payment">
						<xsl:with-param name="pType" select="'BulletPayment'"/>
					</xsl:call-template>
				</payment>
			</groupLevel>
		</bulletPayment>
	</xsl:template>
</xsl:stylesheet>