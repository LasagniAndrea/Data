<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<!-- FxDeliverableLeg -->
	<xsl:template name="FxDeliverableLeg">
    <xsl:param name="pPosition" />
		<deliverableLeg>
			<keyGroup name="DeliverableLeg" key="DLV" />
      <itemGroup>
				<occurence to="All" isFieldOptional="true" field="deliverableLeg">efs_FxLeg</occurence>
				<key>
					<eventCode>EventCode(TradeDate)</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Deliverable</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate" />
        <endDateReference hRef="FXLegValueDate" />
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
			</itemGroup>
			
			<xsl:call-template name="ExchangedAndSideCurrency">
				<xsl:with-param name="pFxType" select="'Deliverable'" />
			</xsl:call-template>
			
			<xsl:call-template name="FwpDepreciableAmount" />

      <xsl:call-template name="FxMarginRatio" />

      <!-- LPC Amounts -->
      <xsl:if test = "$pPosition = '0'">
        <xsl:call-template name="FXLinkedProductClosing" />
      </xsl:if>

    </deliverableLeg>
	</xsl:template>		

	<!-- FxDeliverableLeg -->
	<xsl:template name="FxNonDeliverableLeg">
    <xsl:param name="pPosition" />
		<nonDeliverableLeg>
			<keyGroup name="NonDeliverableLeg" key="NDV" />
			<itemGroup>
				<occurence to="All" isFieldOptional="true" field="nonDeliverableLeg">efs_FxLeg</occurence>
				<key>
					<eventCode>EventCode(TradeDate)</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|NonDeliverable</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate" />
        <endDateReference hRef="FXLegValueDate" />
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
			</itemGroup>
			
			<xsl:call-template name="ExchangedAndSideCurrency">
				<xsl:with-param name="pFxType" select="'NonDeliverable'" />
			</xsl:call-template>
			
			<xsl:call-template name="SettlementCurrency" />
			
			<xsl:call-template name="FwpDepreciableAmount" />

      <xsl:call-template name="FxMarginRatio" />

      <!-- LPC Amounts -->
      <xsl:if test = "$pPosition = '0'">
        <xsl:call-template name="FXLinkedProductClosing" />
      </xsl:if>

    </nonDeliverableLeg>
	</xsl:template>		

	<xsl:template name="ExchangedAndSideCurrency">
		<xsl:param name="pFxType" />
		
		<!-- ExchangedCurrency StartPayment -->
		<xsl:call-template name="ExchangedCurrency">
			<xsl:with-param name="pCurrency"  select="'1'" />
			<xsl:with-param name="pEventCode" select="'Start'" />
		</xsl:call-template>
		<xsl:call-template name="ExchangedCurrency">
			<xsl:with-param name="pCurrency"  select="'2'" />
			<xsl:with-param name="pEventCode" select="'Start'" />
		</xsl:call-template>
		<!-- SideRates StartPayment -->
		<xsl:call-template name="SideRate">
			<xsl:with-param name="pCurrency"  select="'1'" />
			<xsl:with-param name="pEventCode" select="'Start'" />
		</xsl:call-template>
		<xsl:call-template name="SideRate">
			<xsl:with-param name="pCurrency"  select="'2'" />
			<xsl:with-param name="pEventCode" select="'Start'" />
		</xsl:call-template>
		<xsl:if test="$pFxType = 'Deliverable'">
			<!-- ExchangedCurrency MaturityPayment -->
			<xsl:call-template name="ExchangedCurrency">
				<xsl:with-param name="pCurrency"  select="'1'" />
				<xsl:with-param name="pEventCode" select="'Termination'" />
			</xsl:call-template>
			<xsl:call-template name="ExchangedCurrency">
				<xsl:with-param name="pCurrency"  select="'2'" />
				<xsl:with-param name="pEventCode" select="'Termination'" />
			</xsl:call-template>
			<!-- SideRates MaturityPayment -->
			<xsl:call-template name="SideRate">
				<xsl:with-param name="pCurrency" select="'1'" />
				<xsl:with-param name="pEventCode" select="'Termination'" />
			</xsl:call-template>
			<xsl:call-template name="SideRate">
				<xsl:with-param name="pCurrency" select="'2'" />
				<xsl:with-param name="pEventCode" select="'Termination'" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>		

	<!-- ExchangedCurrency -->
	<xsl:template name="ExchangedCurrency">
		<xsl:param name="pCurrency" />
		<xsl:param name="pEventCode" />
		
		<xsl:variable name="currency">
			<xsl:text>exchangedCurrency</xsl:text>
			<xsl:value-of select="$pCurrency" />
		</xsl:variable>
		<exchangeCurrency>
			<keyGroup name="ExchangeCurrency" key="CUR" />
			<itemGroup>
				<occurence to="Unique"><xsl:value-of select="$currency" /></occurence>
				<parameter id="ValueDate">ValueDate</parameter>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Currency<xsl:value-of select="$pCurrency" /></eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDate>ValueDate</endDate>
				<payer>PayerPartyReference</payer>
				<receiver>ReceiverPartyReference</receiver>
				<valorisation>PaymentAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PaymentCurrency</unit>
				<item_Details>
					<exchangeRate>ExchangeRate</exchangeRate>
				</item_Details>
				<xsl:if test="$pEventCode = 'Start'">
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
						<eventDateReference hRef="AdjustedTradeDate" />
					</subItem>
				</xsl:if>
				<xsl:if test="$pEventCode = 'Termination'">
          <!-- EG 20150630 [21124] ValueDate -->
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
            <eventDate>AdjustedPaymentDate</eventDate>
            <isPayment>1</isPayment>
          </subItem>
          <!-- PreSettlement -->
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
						<eventDate>AdjustedPreSettlementDate</eventDate>
					</subItem>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
						<eventDate>AdjustedPaymentDate</eventDate>
						<isPayment>1</isPayment>
					</subItem>
				</xsl:if>
			</itemGroup>
			<xsl:call-template name="SpotFixing" />
		</exchangeCurrency>
	</xsl:template>
	<!-- SideRate -->
	<xsl:template name="SideRate">
		<xsl:param name="pCurrency" />
		<xsl:param name="pEventCode" />
		<xsl:variable name="currency">
			<xsl:text>sideRateCurrency</xsl:text>
			<xsl:value-of select="$pCurrency" />
		</xsl:variable>
		<sideRate>
			<keyGroup name="SideRate" key="SDR" />
			<itemGroup>
				<occurence to="Unique"><xsl:value-of select="$currency" /></occurence>				
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|BaseCurrency<xsl:value-of select="$pCurrency" /></eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDate>ValueDate</endDate>
				<payer>PayerPartyReference</payer>
				<receiver>ReceiverPartyReference</receiver>
				<valorisation>BaseAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>BaseCurrency</unit>
				<item_Details>
					<sideRate>SideRate</sideRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
			</itemGroup>
		</sideRate>
	</xsl:template>

	<!-- SettlementCurrency -->
	<xsl:template name="SettlementCurrency">
		<settlementCurrency>
			<keyGroup name="SettlementCurrency" key="SCU" />
			<itemGroup>
				<occurence to="Unique">nonDeliverableForwardCurrency</occurence>
				<parameter id="NDFValueDate">ValueDate</parameter>
				<parameter id="AdjustedPaymentDate">AdjustedPaymentDate</parameter>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Termination</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|SettlementCurrency</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="NDFValueDate" />
				<unitType>[Currency]</unitType>
				<unit>SettlementCurrency</unit>
				<item_Details>
					<settlementRate>SettlementRate</settlementRate>
				</item_Details>
        <!-- EG 20150630 [21124] ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDateReference hRef="AdjustedPaymentDate" />
          <isPayment>1</isPayment>
        </subItem>
        <subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>AdjustedPreSettlementDate</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDateReference hRef="AdjustedPaymentDate" />
					<isPayment>1</isPayment>
				</subItem>
			</itemGroup>
			<xsl:call-template name="SettlementFixing">
				<xsl:with-param name="pFixing" select="'settlementFixing'" />
			</xsl:call-template>
			<xsl:call-template name="AvgSettlementFixing" />
			<xsl:call-template name="QuotedFixing">
				<xsl:with-param name="pFixing" select="'quotedFixing'" />
			</xsl:call-template>
			<xsl:call-template name="AvgQuotedFixing" />
		</settlementCurrency>
	</xsl:template>


	<!-- FwpDepreciableAmount -->
	<xsl:template name="FwpDepreciableAmount">
		<fwpDepreciableAmount>
			<keyGroup name="FwpDepreciableAmount" key="FWP" />
			<itemGroup>
				<occurence to="Unique">fwpDepreciableAmount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|DepreciableAmount</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|ForwardPoints</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDate>ValueDate</endDate>
				<payer>PayerPartyReference</payer>
				<receiver>ReceiverPartyReference</receiver>
				<valorisation>FwpAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>FwpCurrency</unit>
				<item_Details>
					<exchangeRate>ExchangeRate</exchangeRate>
				</item_Details>
        <!-- EG 20150630 [21124] ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedValueDate</eventDate>
          <isPayment>0</isPayment>
        </subItem>
        <subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>AdjustedPreSettlementDate</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDate>AdjustedValueDate</eventDate>
					<isPayment>0</isPayment>
				</subItem>
			</itemGroup>
		</fwpDepreciableAmount>
	</xsl:template>
	
	<!-- AvgSettlementFixing -->
	<xsl:template name="AvgSettlementFixing">
		<avgSettlementFixing>
			<keyGroup name="AvgSettlementFixing" key="AVGSTL" />
			<itemGroup>
				<occurence to="All" isOptional="true">avgSettlementFixing</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EventType</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="NDFValueDate" />
				<item_Details>
					<currencyPair>CurrencyPair</currencyPair>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Average</eventClass>
					<eventDateReference hRef="AdjustedPaymentDate" />
				</subItem>
			</itemGroup>
			<xsl:call-template name="SettlementFixing">
				<xsl:with-param name="pFixing" select="'fixing'" />
			</xsl:call-template>
		</avgSettlementFixing>
	</xsl:template>
	
	<!-- SettlementFixing -->
	<xsl:template name="SettlementFixing">
		<xsl:param name="pFixing" />
		<settlementFixing>
			<keyGroup name="SettlementFixing" key="STL" />
			<itemGroup>
				<occurence to="All" isOptional="true"><xsl:value-of select="$pFixing" /></occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EventType</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="NDFValueDate" />
				<item_Details>
					<asset>AssetFxRate</asset>
					<fixingRate>FixingRate</fixingRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
					<eventDate>FixingDate</eventDate>
				</subItem>
			</itemGroup>
		</settlementFixing>
	</xsl:template>
	
	<!-- AvgQuotedFixing -->
	<xsl:template name="AvgQuotedFixing">
		<avgQuotedFixing>
			<keyGroup name="AvgQuotedFixing" key="AVGQUO" />
			<itemGroup>
				<occurence to="All" isOptional="true">avgQuotedFixing</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EventType</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="NDFValueDate" />
				<item_Details>
					<currencyPair>CurrencyPair</currencyPair>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Average</eventClass>
					<eventDateReference hRef="AdjustedPaymentDate" />
				</subItem>
			</itemGroup>
			<xsl:call-template name="QuotedFixing">
				<xsl:with-param name="pFixing" select="'fixing'" />
			</xsl:call-template>
		</avgQuotedFixing>
	</xsl:template>
	
	<!-- QuotedFixing -->
	<xsl:template name="QuotedFixing">
		<xsl:param name="pFixing" />
		<quotedFixing>
			<keyGroup name="QuotedFixing" key="FXR" />
			<itemGroup>
				<occurence to="All" isOptional="true">
					<xsl:value-of select="$pFixing" />
				</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EventType</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="NDFValueDate" />
				<item_Details>
					<asset>AssetFxRate</asset>
					<fixingRate>FixingRate</fixingRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
					<eventDate>FixingDate</eventDate>
				</subItem>
			</itemGroup>
		</quotedFixing>
	</xsl:template>
	
	<!-- SpotFixing -->
	<xsl:template name="SpotFixing">
		<spotFixing>
			<keyGroup name="SpotFixing" key="SFX" />
			<itemGroup>
				<occurence to="Unique" isOptional="true">fixing</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|FxRate</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="ValueDate" />
				<item_Details>
					<asset>AssetFxRate</asset>
					<fixingRate>FixingRate</fixingRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
					<eventDate>FixingDate</eventDate>
				</subItem>
			</itemGroup>
		</spotFixing>
	</xsl:template>
	
	<!-- FxOptionPremium -->
	<xsl:template name="FxOptionPremium">
		<premium>
			<keyGroup name="Premium" key="PRE" />
			<itemGroup>
				<parameter id="fxOptionPremiumPayer">PayerPartyReference</parameter>
				<parameter id="fxOptionPremiumReceiver">ReceiverPartyReference</parameter>
				<occurence to="All" isOptional="true">fxOptionPremium</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Premium</eventType>
				</key>
				<startDate>SettlementDate</startDate>
				<endDate>ExpirationDate</endDate>
				<payerReference hRef="fxOptionPremiumPayer"/>
				<receiverReference hRef="fxOptionPremiumReceiver"/>
				<valorisation>PremiumAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PremiumCurrency</unit>
				
				<item_Details>
					<exchangeRate>ExchangeRate</exchangeRate>
					<premiumQuote>PremiumQuote</premiumQuote>
				</item_Details>
				
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
        <!-- EG 20150630 [21124] ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedSettlementDate</eventDate>
          <isPayment>0</isPayment>
        </subItem>

        <!-- 20070823 EG Ticket : 15643 -->
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>AdjustedPreSettlementDate</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDate>AdjustedSettlementDate</eventDate>
					<isPayment>1</isPayment>
				</subItem>
			</itemGroup>

			<originalPayment>
				<xsl:call-template name="OriginalFxPremium"/>
			</originalPayment>
		</premium>
	</xsl:template>

	<!-- FxOptionPayoutRecognition -->
	<xsl:template name="FxOptionPayoutRecognition">
		<xsl:param name="pDigitalOptionType" />
		<payout>
			<keyGroup name="Payout" key="POU" />
			<itemGroup>
				<occurence to="Unique">triggerPayout</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Payout</eventType>
				</key>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="TradeDate" />
				<payerReference hRef="{$pDigitalOptionType}Seller" />
				<receiverReference hRef="{$pDigitalOptionType}Buyer" />
				<valorisation>PayoutAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PayoutCurrency</unit>

				<item_Details>
					<exchangeRate>ExchangeRate</exchangeRate>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
				
				
			</itemGroup>

			<originalPayment>
				<xsl:call-template name="OriginalFxPayout">
					<xsl:with-param name="pOptionType" select="$pDigitalOptionType" />
					<xsl:with-param name="pPayoutType" select="'Payout'" />
				</xsl:call-template>
			</originalPayment>

		</payout>
	</xsl:template>

	<!-- FxDigitalOptionTrigger -->
	<xsl:template name="FxDigitalOptionTrigger">
		<xsl:param name="pDigitalOptionType" />
		<trigger>
			<keyGroup name="Trigger" key="TRG" />
			<itemGroup>
				<occurence to="All">trigger</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Trigger</eventCode>
					<eventType>TriggerType(<xsl:value-of select="$pDigitalOptionType" />SpotRate,AdjustedTradeDate)</eventType>
				</key>
				<xsl:choose>
					<xsl:when test="$pDigitalOptionType = 'ADO'">
						<startDate>ObservationStartDate(TradeDate)</startDate>
						<endDate>ObservationEndDate(<xsl:value-of select="$pDigitalOptionType" />ExpiryDate)</endDate>
					</xsl:when>
					<xsl:when test="$pDigitalOptionType = 'EDO'">
						<startDateReference hRef="TradeDate" />
						<endDateReference hRef="{$pDigitalOptionType}ExpiryDate" />
					</xsl:when>
				</xsl:choose>
				<item_Details>
					<triggerRate>TriggerRate</triggerRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|TriggerTouch</eventClass>
					<xsl:choose>
						<xsl:when test="$pDigitalOptionType = 'ADO'">
							<eventDate>AdjustedObservationEndDate(Adjusted<xsl:value-of select="$pDigitalOptionType" />ExpiryDate)</eventDate>
						</xsl:when>
						<xsl:when test="$pDigitalOptionType = 'EDO'">
							<eventDateReference hRef="Adjusted{$pDigitalOptionType}ExpiryDate" />
						</xsl:when>
					</xsl:choose>
				</subItem>
			</itemGroup>
		</trigger>
	</xsl:template>

	<!-- FxOptionType -->
	<xsl:template name="FxOptionType">
		<xsl:param name="pType" />
		<fxOptionType>
			<keyGroup name="FxOptionType" key="FOT" />
			<parameter id="ExpiryDate">ExpiryDate</parameter>
      <parameter id="FXLegValueDate">ExpiryDate</parameter>
      <parameter id="IMPayerPartyReference">InitialMarginPayerPartyReference</parameter>
      <parameter id="IMReceiverPartyReference">InitialMarginReceiverPartyReference</parameter>
      <parameter id="AdjustedExpiryDate">AdjustedExpiryDate</parameter>
			<parameter id="Buyer">BuyerPartyReference</parameter>
			<parameter id="Seller">SellerPartyReference</parameter>
			<parameter id="SpotRate">SpotRate</parameter>
			<parameter id="IsFxCapBarrier">IsFxCapBarrier</parameter>
			<parameter id="IsFxFloorBarrier">IsFxFloorBarrier</parameter>
			<parameter id="StrikePrice">StrikePrice</parameter>
			<itemGroup>
				<occurence to="Unique">efs_<xsl:value-of select="$pType" /></occurence>
				<key>
					<eventCode>EventCode</eventCode>
					<eventType>EventType</eventType>
				</key>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="ExpiryDate" />

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedExpiryDate" />
				</subItem>
			</itemGroup>
			<!-- Capital (CallCurrencyAmount/PutCurrencyAmount) -->
			<xsl:call-template name="FxCurrencyAmount">
				<xsl:with-param name="pType" select="'call'" />
			</xsl:call-template>
			<xsl:call-template name="FxCurrencyAmount">
				<xsl:with-param name="pType" select="'put'" />
			</xsl:call-template>

      <xsl:call-template name="FxMarginRatio" />

      <!-- LPC Amounts -->
      <xsl:call-template name="FXLinkedProductClosing" />

      <!-- FxOptionPayoutRecognition -->
			<xsl:call-template name="FxRebateRecognition"/>

			<!-- ExerciseDates -->
			<xsl:call-template name="FxOptionExerciseDates" />
			<!-- Barriers -->
			<xsl:call-template name="FxBarriers" />
			<xsl:call-template name="FxRebateBarriers" />
			<!-- ExerciseProcedure -->
			<xsl:call-template name="FxExerciseProcedure" />
		</fxOptionType>
	</xsl:template>

	<!-- FxCurrencyAmount -->
	<xsl:template name="FxCurrencyAmount">
		<xsl:param name="pType" />

		<xsl:variable name="currencyType">
			<xsl:choose>
				<xsl:when test="$pType = 'call'">Call</xsl:when>
				<xsl:when test="$pType = 'put'">Put</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<fxCurrencyAmount>
			<keyGroup name="FxCurrencyAmount" key="{$pType}Currency" />
			<itemGroup>
				<occurence to="Unique"><xsl:value-of select="$pType" />CurrencyAmount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$currencyType" />Currency</eventType>
				</key>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="TradeDate" />
				<xsl:choose>
					<xsl:when test="$pType = 'call'">
						<payerReference hRef="Seller" />
						<receiverReference hRef="Buyer" />
					</xsl:when>
					<xsl:when test="$pType = 'put'">
						<payerReference hRef="Buyer" />
						<receiverReference hRef="Seller" />
					</xsl:when>
				</xsl:choose>
				<valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>

				<item_Details>
					<strikePriceReference hRef="StrikePrice" />
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
			</itemGroup>
		</fxCurrencyAmount>
	</xsl:template>

	<!-- FxOptionExerciseDates -->
	<xsl:template name="FxOptionExerciseDates">
		<exerciseDates>
			<keyGroup name="FxExerciseDates" key="EXD" />
			<itemGroup>
				<occurence to="All">exerciseDates</occurence>
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

	<!-- FxBarriers -->
	<xsl:template name="FxBarriers">
		<xsl:param name="pPattern" select="''" />
		<barrier>
			<keyGroup name="FxBarrier" key="BAR" />
			<itemGroup>
				<occurence to="All" isOptional="true">fxBarrier</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
					<eventType>BarrierType(<xsl:value-of select="$pPattern" />SpotRate,AdjustedTradeDate,IsFxCapBarrier,IsFxFloorBarrier)</eventType>
				</key>
				<startDate>ObservationStartDate(TradeDate)</startDate>
				<endDate>ObservationEndDate(<xsl:value-of select="$pPattern" />ExpiryDate)</endDate>
				<item_Details>
					<triggerRate>TriggerRate</triggerRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|BarrierKnock</eventClass>
					<eventDate>AdjustedObservationEndDate(Adjusted<xsl:value-of select="$pPattern" />ExpiryDate)</eventDate>
				</subItem>
			</itemGroup>
		</barrier>
	</xsl:template>

	<!-- FxRebateBarriers -->
	<xsl:template name="FxRebateBarriers">
		<barrier>
			<keyGroup name="FxRebateBarrier" key="REB" />
			<itemGroup>
				<occurence to="Unique" isOptional="true">fxRebateBarrier</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
					<eventType>RebateBarrierType(SpotRate,AdjustedTradeDate)</eventType>
				</key>
				<startDate>ObservationStartDate(TradeDate)</startDate>
				<endDate>ObservationEndDate(ExpiryDate)</endDate>
				<item_Details>
					<triggerRate>TriggerRate</triggerRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|BarrierKnock</eventClass>
					<eventDate>AdjustedObservationEndDate(AdjustedExpiryDate)</eventDate>
				</subItem>
			</itemGroup>
		</barrier>
	</xsl:template>

	<!-- FxExerciseProcedure -->
	<xsl:template name="FxExerciseProcedure">
		<exerciseProcedure>
			<keyGroup name="FxExerciseProcedure" key="EXE" />
			<itemGroup>
				<occurence to="All" isOptional="true">exerciseProcedure</occurence>
				<key>
					<eventCode>ExerciseProcedureType</eventCode>
					<eventType>SettlementType</eventType>
				</key>
				<startDateReference hRef="ExpiryDate" />
				<endDateReference hRef="ExpiryDate" />
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDate>AddOneDayToExpiryDate(ExpiryDate)</eventDate>
				</subItem>
			</itemGroup>
		</exerciseProcedure>
	</xsl:template>

	<!-- FxOptionPayoutRecognition -->
	<xsl:template name="FxRebateRecognition">
		<rebate>
			<keyGroup name="Rebate" key="REB" />
			<itemGroup>
				<occurence to="Unique">triggerPayout</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Rebate</eventType>
				</key>
				<startDateReference hRef="TradeDate" />
				<endDateReference hRef="TradeDate" />
				<payerReference hRef="Seller" />
				<receiverReference hRef="Buyer" />
				<valorisation>PayoutAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PayoutCurrency</unit>

				<item_Details>
					<exchangeRate>ExchangeRate</exchangeRate>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
			</itemGroup>

			<originalPayment>
				<xsl:call-template name="OriginalFxPayout">
					<xsl:with-param name="pPayoutType" select="'Rebate'" />					
				</xsl:call-template>
			</originalPayment>
		</rebate>
	</xsl:template>

	<xsl:template name="OriginalFxPayout">
		<xsl:param name="pOptionType" select="''"/>
		<xsl:param name="pPayoutType"/>

		<keyGroup name="FxOriginalPayment" key="OPAO"/>					
		<itemGroup>
			<occurence to="Unique" isOptional="true">originalPayment</occurence>
			<key>
				<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
				<eventType>
					<xsl:choose>
						<xsl:when test="$pPayoutType = 'Payout'">EfsML.Enum.Tools.EventTypeFunc|Payout</xsl:when>
						<xsl:when test="$pPayoutType = 'Rebate'">EfsML.Enum.Tools.EventTypeFunc|Rebate</xsl:when>
					</xsl:choose>
				</eventType>
			</key>
			<startDateReference hRef="TradeDate" />
			<endDateReference hRef="TradeDate" />
			<payerReference hRef="{$pOptionType}Seller" />
			<receiverReference hRef="{$pOptionType}Buyer"/>
			<valorisation>PaymentAmount</valorisation>
			<unitType>[Currency]</unitType>
			<unit>PaymentCurrency</unit>

			<subItem>
				<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
				<eventDateReference hRef="AdjustedTradeDate"/>
			</subItem>
		</itemGroup>
	</xsl:template>

	<xsl:template name="OriginalFxPremium">
		<keyGroup name="OriginalPremium" key="OPRM"/>
		<itemGroup>
			<occurence to="Unique" field="originalPayment" isFieldOptional="true">efs_FxOptionPremium</occurence>
			<key>
				<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
				<eventType>EfsML.Enum.Tools.EventTypeFunc|Premium</eventType>
			</key>

			<startDate>SettlementDate</startDate>
			<endDate>ExpirationDate</endDate>
			<payerReference hRef="fxOptionPremiumPayer"/>
			<receiverReference hRef="fxOptionPremiumReceiver"/>
			<valorisation>PaymentAmount</valorisation>
			<unitType>[Currency]</unitType>
			<unit>PaymentCurrency</unit>

			<subItem>
				<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
				<eventDateReference hRef="AdjustedTradeDate"/>
			</subItem>
		</itemGroup>
	</xsl:template>

  <!-- FxMarginRatio -->
  <xsl:template name="FxMarginRatio">
    <marginRatio>
      <keyGroup name="FxMarginRatio" key="MGF"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">marginRatio</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|MarginRequirementRatio</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="TradeDate" />
        <endDateReference hRef="FXLegValueDate" />

        <valorisation>Amount</valorisation>
        <unitType>UnitType</unitType>
        <unit>Unit</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>

      </itemGroup>
    </marginRatio>
  </xsl:template>

  <!-- ESELinkedProductClosing -->
  <xsl:template name="FXLinkedProductClosing">
    <linkedProductClosingAmounts>
      <keyGroup name="FX_LinkedProductClosing" key="LPC"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">initialMargin</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>

        <startDateReference hRef="TradeDate" />
        <endDateReference hRef="FXLegValueDate" />

        <!--<item_Details>
          <assetReference hRef="ESE_Asset"/>
        </item_Details>-->

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <!-- Initial Margin -->
      <item>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|InitialMargin</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="TradeDate" />
        <endDateReference hRef="FXLegValueDate" />

        <payerReference hRef="IMPayerPartyReference"/>
        <receiverReference hRef="IMReceiverPartyReference"/>

        <valorisation>Amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>Currency</unit>

        <!--<valorisation>InitialMarginAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>InitialMarginCurrency</unit>-->

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </item>

    </linkedProductClosingAmounts>
  </xsl:template>
  
  <xsl:template name="FxOptionProvisions">
    <mandatoryEarlyTerminationProvision>
      <xsl:call-template name="FxOptionProvision">
        <xsl:with-param name="pProvision" select="'MandatoryEarlyTerminationProvision'"/>
      </xsl:call-template>
    </mandatoryEarlyTerminationProvision>
    <cancelableProvision>
      <xsl:call-template name="FxOptionProvision">
        <xsl:with-param name="pProvision" select="'Cancelable'"/>
      </xsl:call-template>
    </cancelableProvision>
    <extendibleProvision>
      <xsl:call-template name="FxOptionProvision">
        <xsl:with-param name="pProvision" select="'Extendible'"/>
      </xsl:call-template>
    </extendibleProvision>
    <earlyTerminationProvision>
      <xsl:call-template name="FxOptionProvision">
        <xsl:with-param name="pProvision" select="'OptionalEarlyTermination'"/>
      </xsl:call-template>
    </earlyTerminationProvision>
  </xsl:template>

  <!-- EG 20180301 [23803] New -->
  <xsl:template name="FxOptionProvision">
      <xsl:param name="pProvision"/>

      <xsl:variable name="occurence">
        <xsl:choose>
          <xsl:when test = "$pProvision = 'Cancelable'">cancelableProvision</xsl:when>
          <xsl:when test = "$pProvision = 'Extendible'">extendibleProvision</xsl:when>
          <xsl:when test = "$pProvision = 'OptionalEarlyTermination'">earlyTerminationOptional</xsl:when>
          <xsl:otherwise>None</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <keyGroup name="{$pProvision}Provision" key="{$pProvision}"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">
          <xsl:value-of select="$occurence"/>
        </occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Provision</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pProvision"/>Provision</eventType>
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
    
    </xsl:template>


</xsl:stylesheet>
