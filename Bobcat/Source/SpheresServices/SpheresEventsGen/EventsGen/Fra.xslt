<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='Fra']">
		<xsl:param name="pPosition" select="0"/>
		<fra>
			<!-- Product -->
			<keyGroup name="Fra" key="PRD"/>
			<parameter id="EffectiveDate">EffectiveDate</parameter>
			<parameter id="AdjustedEffectiveDate">AdjustedEffectiveDate</parameter>
			<parameter id="TerminationDate">TerminationDate</parameter>					
			<parameter id="AdjustedTerminationDate">AdjustedTerminationDate</parameter>					
			<parameter id="Seller">SellerPartyReference</parameter>
			<parameter id="Buyer">BuyerPartyReference</parameter>
			<parameter id="Notional">Notional</parameter>
			<parameter id="Currency">Currency</parameter>
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
				<startDateReference hRef="EffectiveDate"/>
				<endDateReference hRef="TerminationDate"/>
				-->
				<payerReference hRef="Buyer"/>
				<receiverReference hRef="Seller"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>
			
			<groupLevel>
				<keyGroup name="GroupLevel" key="GRP"/>
				<itemGroup>	
					<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|ForwardRateAgreement</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|ForwardRateAgreement</eventType>
					</key>
					<idStCalcul>[CALC]</idStCalcul>
					<startDateReference hRef="EffectiveDate"/>
					<endDateReference hRef="TerminationDate"/>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
						<eventDateReference hRef="AdjustedTradeDate"/>
					</subItem>
				</itemGroup>
				
				<!-- Nominal -->			
				<xsl:call-template name="NominalDates">
					<xsl:with-param name="pEventCode" select="'Start'"/>
				</xsl:call-template>
				<xsl:call-template name="NominalDates">
					<xsl:with-param name="pEventCode" select="'Termination'"/>
				</xsl:call-template>
				<xsl:call-template name="NominalDates">
					<xsl:with-param name="pEventCode" select="'NominalStep'"/>
				</xsl:call-template>
				
				<!-- PaymentDates -->
				<xsl:call-template name="PaymentFra"/>
				
				<!-- ResetDates -->
				<xsl:call-template name="ResetFra"/>
				
			</groupLevel>
		</fra>
	</xsl:template>
	
	<!-- NominalDates -->
	<xsl:template name="NominalDates">
		<xsl:param name="pEventCode"/>
		<nominalPeriods>
			<keyGroup name="NominalPeriods" key="NOM"/>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="EffectiveDate"/>
				<endDateReference hRef="TerminationDate"/>
				
				<xsl:if test = "$pEventCode = 'Start'"><payerReference hRef="Buyer"/></xsl:if>
				<xsl:if test = "$pEventCode = 'Termination'"><payerReference hRef="Seller"/></xsl:if>

				<xsl:if test = "$pEventCode = 'Start'"><receiverReference hRef="Seller"/></xsl:if>
				<xsl:if test = "$pEventCode = 'Termination'"><receiverReference hRef="Buyer"/></xsl:if>
				
				<valorisationReference hRef="Notional"/>
				<unitType>[Currency]</unitType>
				<unitReference hRef="Currency"/>
				<subItem>
					<eventClass>
						<xsl:if test = "$pEventCode = 'Start'">EfsML.Enum.Tools.EventClassFunc|Recognition</xsl:if>
						<xsl:if test = "$pEventCode = 'Termination'">EfsML.Enum.Tools.EventClassFunc|Recognition</xsl:if>
						<xsl:if test = "$pEventCode = 'NominalStep'">EfsML.Enum.Tools.EventClassFunc|GroupLevel</xsl:if>
					</eventClass>
					<xsl:if test = "$pEventCode = 'Start'"><eventDateReference hRef="AdjustedTradeDate"/></xsl:if>
					<xsl:if test = "$pEventCode = 'Termination'"><eventDateReference hRef="AdjustedTerminationDate"/></xsl:if>
					<xsl:if test = "$pEventCode = 'NominalStep'"><eventDateReference hRef="AdjustedEffectiveDate"/></xsl:if>
				</subItem>
			</itemGroup>
		</nominalPeriods>						
	</xsl:template>		

	<!-- PaymentDates -->
	
	<xsl:template name="PaymentFra">	
		<paymentDates>
			<keyGroup name="PaymentDates" key="PAY"/>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Termination</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Interest</eventType>
				</key>
				<startDateReference hRef="EffectiveDate"/>
				<endDateReference hRef="TerminationDate"/>
				<payerReference hRef="Seller"/>
				<receiverReference hRef="Buyer"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedEffectiveDate"/>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>AdjustedPreSettlementDate</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDate>AdjustedPaymentDate</eventDate>
					<isPayment>1</isPayment>
				</subItem>
			</itemGroup>
			<xsl:call-template name="CalculationPeriodFra"/>
		</paymentDates>
	</xsl:template>
		
	<!-- CalculationPeriodDates -->
	<xsl:template name="CalculationPeriodFra">
		<calculationPeriodDates>
			<keyGroup name="CalculationPeriodDates" key="PER"/>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|CalculationPeriod</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
				</key>
				<startDateReference hRef="EffectiveDate"/>
				<endDateReference hRef="TerminationDate"/>
				
				<payerReference hRef="Seller"/>
				<receiverReference hRef="Buyer"/>
				
				<item_Details>
					<asset>AssetRateIndex</asset>
					<dayCountFraction>DayCountFraction</dayCountFraction>
				</item_Details>
				
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedEffectiveDate"/>
				</subItem>
			</itemGroup>
			<xsl:call-template name="ResetFra"/>
		</calculationPeriodDates>
	</xsl:template>

	<!-- ResetDates -->											
	<xsl:template name="ResetFra">
		<resetDates>
			<keyGroup name="ResetDates" key="RES"/>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
				</key>
				<startDateReference hRef="EffectiveDate"/>
				<endDateReference hRef="TerminationDate"/>
				
				<item_Details>
					<asset>AssetRateIndex</asset>
					<dayCountFraction>DayCountFraction</dayCountFraction>
				</item_Details>
				
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
					<eventDate>AdjustedFixingDate</eventDate>
				</subItem>
			</itemGroup>
		</resetDates>			
	</xsl:template>		
</xsl:stylesheet>

  