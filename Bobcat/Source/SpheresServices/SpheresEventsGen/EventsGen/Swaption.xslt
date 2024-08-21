<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='Swaption']">
		<xsl:param name="pPosition" select="0"/>
		<swaption>
		
			<!-- Product -->
			<keyGroup name="Swaption" key="PRD"/>
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
				<!--
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="TradeDate"/>
				-->
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>

			<!-- Premium -->
			<premium>
				<xsl:call-template name="Payment">
					<xsl:with-param name="pType" select="'Premium'"/>
				</xsl:call-template>
			</premium>
			<groupLevel>
				<keyGroup name="GroupLevel" key="GRP"/>
				<itemGroup>
					<key>
						<eventCode>EventCode</eventCode>
						<eventType>EventType</eventType>
					</key>
					<idStCalcul>[CALC]</idStCalcul>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
						<eventDateReference hRef="AdjustedTradeDate"/>
					</subItem>
				</itemGroup>

				<!-- Exercise -->
				<xsl:call-template name="Exercise">
					<xsl:with-param name="pType" select="'American'"/>
				</xsl:call-template>
				<xsl:call-template name="Exercise">
					<xsl:with-param name="pType" select="'Bermuda'"/>
				</xsl:call-template>
				<xsl:call-template name="Exercise">
					<xsl:with-param name="pType" select="'European'"/>
				</xsl:call-template>
				<!-- Underlyer -->
				<xsl:call-template name="SwaptionUnderlyer"/>
			</groupLevel>
		</swaption>
	</xsl:template>

	<!-- SwaptionUnderlyer -->
	<xsl:template name="SwaptionUnderlyer">
		<swaptionUnderlyer>
			<keyGroup name="SWP" key="SWP" />
			<parameter id="MinEffectiveDate">MinEffectiveDate</parameter>
			<parameter id="MaxTerminationDate">MaxTerminationDate</parameter>
			<itemGroup>
				<occurence to="Unique">swap</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|SwapUnderlyer</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Underlyer</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
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
		</swaptionUnderlyer>
	</xsl:template>

</xsl:stylesheet>