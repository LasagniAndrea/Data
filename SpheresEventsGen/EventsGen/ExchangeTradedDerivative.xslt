<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='ExchangeTradedDerivative']">
		<xsl:param name="pPosition" select="0"/>
		<exchangeTradedDerivative>

			<!-- Product -->
			<keyGroup name="ExchangeTradedDerivative" key="PRD"/>
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

      <!-- ExchangeTradedDerivativeStream -->
      <xsl:call-template name="ExchangeTradedDerivativeStream">
        <xsl:with-param name="pPosition" select="$pPosition"/>
      </xsl:call-template>

		</exchangeTradedDerivative>
	</xsl:template>

	<!-- ExchangeTradedDerivativeStream -->
	<xsl:template name="ExchangeTradedDerivativeStream">
    <xsl:param name="pPosition" />
		<exchangeTradedDerivativeStream>
			<keyGroup name="ExchangeTradedDerivativeStream" key="STD"/>
      <parameter id="ETD_IsNotPositionOpeningAndIsPositionKeeping">IsNotPositionOpeningAndIsPositionKeeping</parameter>
      <parameter id="ETD_ClearingBusinessDate">ClearingBusinessDate</parameter>
			<parameter id="ETD_AdjustedClearingBusinessDate">AdjustedClearingBusinessDate</parameter>
			<parameter id="ETD_MaturityDate">MaturityDate</parameter>
			<parameter id="ETD_AdjustedMaturityDate">AdjustedMaturityDate</parameter>
			<parameter id="ETD_Buyer">BuyerPartyReference</parameter>
			<parameter id="ETD_Seller">SellerPartyReference</parameter>
			<itemGroup>
				<occurence to="Unique" >efs_ExchangeTradedDerivative</occurence>
				<key>
					<eventCode>EventCode</eventCode>
					<eventType>EventType</eventType>
				</key>
				<startDate>ClearingBusinessDate</startDate>
				<endDate>MaturityDate</endDate>

				<item_Details>
					<asset>AssetETD</asset>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="ETD_AdjustedMaturityDate"/>
				</subItem>
			</itemGroup>

			<!-- Premium -->
			<xsl:call-template name="ETDPremium"/>

			<!-- ETDStartAmount -->
			<xsl:call-template name="ETDStartAmount">
				<xsl:with-param name="pEventType" select="'Nominal'"/>
			</xsl:call-template>

			<xsl:call-template name="ETDStartAmount">
				<xsl:with-param name="pEventType" select="'Quantity'"/>
			</xsl:call-template>

      <!-- LPC Amounts -->
      <xsl:if test = "$pPosition = '0'">
			  <xsl:call-template name="ETDLinkedProductClosing" />
      </xsl:if>
			
			<!-- ExerciseDates -->
			<xsl:call-template name="ETDOptionExerciseDates" />

		</exchangeTradedDerivativeStream>
	</xsl:template>

	<!-- ETDPremium -->
	<xsl:template name="ETDPremium">
		<premium>
			<keyGroup name="Premium" key="PRM" />
      <itemGroup>
				<occurence to="Unique" isOptional="true">premium</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EventType</eventType>
				</key>
        <startDateReference hRef="ETD_ClearingBusinessDate"/>
				<endDateReference hRef="ETD_MaturityDate"/>
				<payerReference hRef="ETD_Buyer"/>
				<receiverReference hRef="ETD_Seller"/>
				<valorisation>PremiumAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PremiumCurrency</unit>

				<item_Details>
					<etdPremium>PremiumCalculation</etdPremium>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <!-- RD 20110526 Ticket : 17469 -->
          <!-- Replace AdjustedTradeDate by ETD_AdjustedClearingBusinessDate -->
					<eventDateReference hRef="ETD_AdjustedClearingBusinessDate" />
				</subItem>
        
        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <conditionalReference hRef="ETD_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDateReference hRef="ETD_AdjustedClearingBusinessDate" />
        </subItem>
        
        <subItem>
          <conditionalReference hRef="ETD_IsNotPositionOpeningAndIsPositionKeeping"/>
				  <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
				  <eventDate>AdjustedPaymentDate</eventDate>
          <!-- EG 20130723 (18754) Premium avec IsPayment à false si FuturesStyleMarkToMarket-->
				  <isPayment>IsPayment</isPayment>
			  </subItem>
      </itemGroup>
      <!-- EG 20210812 [25173] New Additional Premium amounts -->
      <xsl:call-template name="ETDPremiumTheoretical"/>
      <xsl:call-template name="ETDPremiumCashResidual"/>

    </premium>
	</xsl:template>

  <!-- EG 20210812 [25173] New Additional Premium amounts -->
  <xsl:template name="ETDPremiumTheoretical">
    <premiumTheoretical>
      <keyGroup name="PremiumTheoretical" key="PRT" />
      <itemGroup>
        <occurence to="Unique" isOptional="true">premiumAdditionalAmount</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|PremiumTheoretical</eventType>
        </key>
        <startDateReference hRef="ETD_ClearingBusinessDate"/>
        <endDateReference hRef="ETD_MaturityDate"/>
        <payerReference hRef="ETD_Buyer"/>
        <receiverReference hRef="ETD_Seller"/>
        <valorisation>TheoreticalAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>TheoreticalAmountCurrency</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="ETD_AdjustedClearingBusinessDate" />
        </subItem>

      </itemGroup>

    </premiumTheoretical>
  </xsl:template>
  
  <!-- EG 20210812 [25173] New Additional Premium amounts -->
  <xsl:template name="ETDPremiumCashResidual">
    <premiumCashResidual>
      <keyGroup name="PremiumCashResidual" key="PCR" />
      <itemGroup>
        <occurence to="Unique" isOptional="true">premiumAdditionalAmount</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|PremiumCashResidual</eventType>
        </key>
        <startDateReference hRef="ETD_ClearingBusinessDate"/>
        <endDateReference hRef="ETD_MaturityDate"/>
        <payerReference hRef="ETD_Buyer"/>
        <receiverReference hRef="ETD_Seller"/>
        <valorisation>CashResidualAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>CashResidualAmountCurrency</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="ETD_AdjustedClearingBusinessDate" />
        </subItem>

      </itemGroup>

    </premiumCashResidual>
  </xsl:template>

  <!-- ETDStartAmount -->
	<xsl:template name="ETDStartAmount">
		<xsl:param name="pEventType" />

		<xsl:variable name="amountType">
			<xsl:choose>
				<xsl:when test="$pEventType = 'Nominal'">nominal</xsl:when>
				<xsl:when test="$pEventType = 'Quantity'">quantity</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<nominalQtyVariation>
			<xsl:variable name="paramEventType">
				ETD<xsl:value-of select="$pEventType"/>EventType
			</xsl:variable>
			<keyGroup name="ETDStartAmount" key="STAAMT" />
			<parameter id="ETD{$pEventType}EventType">
				EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pEventType"/>
			</parameter>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
					<eventTypeReference hRef="{$paramEventType}"/>
				</key>
				<startDateReference hRef="ETD_ClearingBusinessDate" />
				<endDateReference hRef="ETD_ClearingBusinessDate" />

				<xsl:choose>
					<xsl:when test = "$pEventType = 'Nominal'">
						<valorisation>Amount</valorisation>
						<unitType>[Currency]</unitType>
						<unit>Currency</unit>
						<payerReference hRef="ETD_Buyer"/>
						<receiverReference hRef="ETD_Seller"/>
					</xsl:when>
					<xsl:when test = "$pEventType = 'Quantity'">
						<valorisation>Quantity</valorisation>
						<unitType>[Qty]</unitType>
						<payerReference hRef="ETD_Seller"/>
						<receiverReference hRef="ETD_Buyer"/>
					</xsl:when>
				</xsl:choose>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <!-- RD 20110526 Ticket : 17469 -->
          <!-- Replace AdjustedTradeDate by ETD_AdjustedClearingBusinessDate -->
					<eventDateReference hRef="ETD_AdjustedClearingBusinessDate" />
				</subItem>
			</itemGroup>
		</nominalQtyVariation>
	</xsl:template>

	<!-- ETDLinkedProductClosing -->
	<xsl:template name="ETDLinkedProductClosing">
		<linkedProductClosingAmounts>
			<keyGroup name="ETD_LinkedProductClosing" key="LPI"/>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
				</key>
				<startDate>ClearingBusinessDate</startDate>
				<endDate>MaturityDate</endDate>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>
		</linkedProductClosingAmounts>
	</xsl:template>

	<!-- ETDOptionExerciseDates -->
	<xsl:template name="ETDOptionExerciseDates">
		<exerciseDates>
			<keyGroup name="ETDOptionExerciseDates" key="EXD" />
			<itemGroup>
				<occurence to="Unique">exerciseDates</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|ExerciseDates</eventCode>
					<eventType>ExerciseCode</eventType>
				</key>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>
				<subItem>
					<eventClass>ExerciseSettlementType</eventClass>
					<eventDate>AdjustedExerciseDate</eventDate>
				</subItem>
			</itemGroup>
		</exerciseDates>
	</xsl:template>

</xsl:stylesheet>