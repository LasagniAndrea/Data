<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='Swap']">
		<xsl:param name="pPosition" select="0"/>
		<swap>
	
			<!-- Product -->
			<keyGroup name="Swap" key="PRD"/>
			<parameter id="MinEffectiveDate">MinEffectiveDate</parameter>
			<parameter id="MaxTerminationDate">MaxTerminationDate</parameter>					
			
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
		
			<!-- Stream -->			
			<swapStream>
				<xsl:call-template name="Stream">
					<xsl:with-param name="pProduct" select="'Swap'"/>
				</xsl:call-template>
			</swapStream>
			
			<!-- Provisions -->
			<mandatoryEarlyTerminationProvision>
				<xsl:call-template name="MandatoryEarlyTerminationProvision"/>
			</mandatoryEarlyTerminationProvision>
			<earlyTerminationProvision>
				<xsl:call-template name="Provision">
					<xsl:with-param name="pProvision" select="'OptionalEarlyTermination'"/>
				</xsl:call-template>
			</earlyTerminationProvision>
			<cancelableProvision>
				<xsl:call-template name="Provision">
					<xsl:with-param name="pProvision" select="'Cancelable'"/>
				</xsl:call-template>
			</cancelableProvision>				
			<extendibleProvision>
				<xsl:call-template name="Provision">
					<xsl:with-param name="pProvision" select="'Extendible'"/>
				</xsl:call-template>
			</extendibleProvision>				
			<stepUpProvision>
				<xsl:call-template name="Provision">
					<xsl:with-param name="pProvision" select="'StepUp'"/>
				</xsl:call-template>
			</stepUpProvision>				
			<!-- Additional Payment -->
			<additionalPayment>
				<xsl:call-template name="Payment">
					<xsl:with-param name="pType" select="'AdditionalPayment'"/>
				</xsl:call-template>
			</additionalPayment>
			
		</swap>
	</xsl:template>
	
	

</xsl:stylesheet>