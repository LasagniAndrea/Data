<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='FxDigitalOption']">
		<xsl:param name="pPosition" select="0"/>
		<fxDigitalOption>		
			<!-- Product -->
			<keyGroup name="FxDigitalOption" key="PRD"/>
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
			
			<!-- AmericanDigitalOption  -->
			<xsl:call-template name="DigitalOption">
				<xsl:with-param name="pFxType" select="'American'"/>
			</xsl:call-template>
			<!-- EuropeanDigitalOption-->
			<xsl:call-template name="DigitalOption">
				<xsl:with-param name="pFxType" select="'European'"/>
			</xsl:call-template>

      <xsl:call-template name="FxOptionProvisions" />
    
		</fxDigitalOption>
	</xsl:template>

	
	<!-- DigitalOption -->
	<xsl:template name="DigitalOption">
		<xsl:param name="pFxType"/>
		
		<xsl:variable name="field">
			<xsl:choose>
				<xsl:when test = "$pFxType = 'American'">americanDigitalOption</xsl:when>				
				<xsl:when test = "$pFxType = 'European'">europeanDigitalOption</xsl:when>					
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="digitalOptionType">
			<xsl:choose>
				<xsl:when test = "$pFxType = 'American'">ADO</xsl:when>				
				<xsl:when test = "$pFxType = 'European'">EDO</xsl:when>					
			</xsl:choose>
		</xsl:variable>

		<digitalOptionType>
			<keyGroup name="digitalOptionType" key="{$digitalOptionType}"/>
			<parameter id="{$digitalOptionType}ExpiryDate">ExpiryDate</parameter>
			<parameter id="Adjusted{$digitalOptionType}ExpiryDate">AdjustedExpiryDate</parameter>
			<parameter id="{$digitalOptionType}Buyer">BuyerPartyReference</parameter>
			<parameter id="{$digitalOptionType}Seller">SellerPartyReference</parameter>
			<parameter id="{$digitalOptionType}SpotRate">SpotRate</parameter>
			<itemGroup>
				<occurence to="Unique" isFieldOptional="true" field="{$field}">efs_FxDigitalOption</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pFxType"/>DigitalOption</eventCode>
					<eventType>DigitalOptionType</eventType>
				</key>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="Adjusted{$digitalOptionType}ExpiryDate"/>
				</subItem>
			</itemGroup>

			<!-- Barriers -->
			<xsl:call-template name="FxBarriers">
				<xsl:with-param name="pPattern" select="$digitalOptionType"/>
			</xsl:call-template>
			
			<!-- Trigger -->
			<xsl:call-template name="FxDigitalOptionTrigger">
				<xsl:with-param name="pDigitalOptionType" select="$digitalOptionType"/>
			</xsl:call-template>
			
			<!-- FxOptionPayoutRecognition -->
			<xsl:call-template name="FxOptionPayoutRecognition">
				<xsl:with-param name="pDigitalOptionType" select="$digitalOptionType"/>
			</xsl:call-template>
			
			<!-- ExerciseDates -->
			<xsl:call-template name="FxOptionExerciseDates" />

   
		</digitalOptionType>
	</xsl:template>
</xsl:stylesheet>