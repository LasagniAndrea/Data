<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<!-- RepoDuration -->
	<xsl:template name="RepoDuration">
		<xsl:param name="pProduct"/>
		<repoDuration>
			<keyGroup name="RepoDuration" key="{$pProduct}" />
			<parameter id="SRA_ValueDate"    hRef="MinEffectiveDate" />
			<parameter id="SRA_MaturityDate" hRef="MaxTerminationDate" />
			<itemGroup>
				<occurence to="Unique">efs_SaleAndRepurchaseAgreement</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pProduct"/></eventCode>
					<eventType>EventType</eventType>
				</key>
				<startDateReference hRef="SRA_ValueDate" />
				<endDateReference hRef="SRA_MaturityDate" />

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate" />
				</subItem>
			</itemGroup>

			<!-- CashStream -->
			<cashStream>
				<xsl:call-template name="Stream">
					<xsl:with-param name="pProduct" select="$pProduct"/>
				</xsl:call-template>
			</cashStream>

			<!-- SecurityLeg -->
			<xsl:call-template name="SecurityLeg">
				<xsl:with-param name="pLeg" select="'spot'"/>
			</xsl:call-template>
			<xsl:call-template name="SecurityLeg">
				<xsl:with-param name="pLeg" select="'forward'"/>
			</xsl:call-template>

		</repoDuration>
	</xsl:template>


	<!-- SecurityLeg -->
	<xsl:template name="SecurityLeg">
		<xsl:param name="pLeg"/>

		<securityLeg>
			<xsl:variable name="eventCode">
				<xsl:choose>
					<xsl:when test = "$pLeg = 'spot'">RepurchaseAgreementSpotLeg</xsl:when>
					<xsl:when test = "$pLeg = 'forward'">RepurchaseAgreementForwardLeg</xsl:when>
				</xsl:choose>
			</xsl:variable>
			<parameter id="{$pLeg}BuyerPartyReference">BuyerPartyReference</parameter>
			<parameter id="{$pLeg}SellerPartyReference">SellerPartyReference</parameter>
			<parameter id="{$pLeg}IssuerPartyReference">IssuerPartyReference</parameter>
			<parameter id="{$pLeg}MaxTerminationDate">MaxTerminationDate</parameter>
			<parameter id="{$pLeg}ValueDate">ValueDate</parameter>

			<keyGroup name="SecurityLeg" key="LEG"/>
			<itemGroup>
				<occurence to="All" isOptional="true"><xsl:value-of select="$pLeg"/>Leg</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$eventCode"/></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Securities</eventType>
				</key>
				<startDateReference hRef="SRA_ValueDate" />
				<endDateReference hRef="SRA_MaturityDate" />

				<payerReference hRef="{$pLeg}SellerPartyReference"/>
				<receiverReference hRef="{$pLeg}BuyerPartyReference"/>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>

			<!-- Amounts -->
			<debtSecurityTransactionAmounts>
				<xsl:call-template name="DebtSecurityTransactionAmounts">
					<xsl:with-param name="pLeg" select="$pLeg"/>
				</xsl:call-template>
			</debtSecurityTransactionAmounts>

			<!-- Stream -->
			<debtSecurityTransactionStream>
				<xsl:call-template name="DebtSecurityTransactionStream">
					<xsl:with-param name="pLeg" select="$pLeg"/>
				</xsl:call-template>
			</debtSecurityTransactionStream>

		</securityLeg>
	</xsl:template>

	<!-- DebtSecurityTransactionAmounts -->
	<xsl:template name="DebtSecurityTransactionAmounts">
		<xsl:param name="pLeg"/>
		<keyGroup name="DebtSecurityTransactionAmounts" key="DSA"/>
		<parameter id="DSA_UnadjustedPaymentDate">UnadjustedPaymentDate</parameter>
		<parameter id="DSA_PreSettlementPaymentDate">PreSettlementPaymentDate</parameter>
		<parameter id="DSA_AdjustedPaymentDate">AdjustedPaymentDate</parameter>
		<itemGroup>
			<occurence to="Unique">efs_DebtSecurityTransactionAmounts</occurence>
			<key>
				<eventCode>EfsML.Enum.Tools.EventCodeFunc|DebtSecurityTransaction</eventCode>
				<eventType>EfsML.Enum.Tools.EventTypeFunc|DebtSecurityTransactionAmounts</eventType>
			</key>
			<idStCalcul>[CALC]</idStCalcul>
			<startDateReference hRef="{$pLeg}ValueDate"/>
			<endDateReference hRef="{$pLeg}MaxTerminationDate"/>

			<valorisation>dirtyAmount</valorisation>
			<unitType>[Currency]</unitType>
			<unit>DirtyAmountCurrency</unit>
			<subItem>
				<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
				<eventDateReference hRef="AdjustedTradeDate"/>
			</subItem>
		</itemGroup>

		<xsl:call-template name="DebtSecurityTransactionGrossAmount">
			<xsl:with-param name="pLeg" select="$pLeg"/>
		</xsl:call-template>
		<xsl:call-template name="DebtSecurityTransactionAccruedInterestAmount">
			<xsl:with-param name="pLeg" select="$pLeg"/>			
		</xsl:call-template>
	</xsl:template>

	<!-- DebtSecurityTransactionGrossAmount -->
	<xsl:template name="DebtSecurityTransactionGrossAmount">
		<xsl:param name="pLeg"/>
		<grossAmount>
			<keyGroup name="DebtSecurityTransactionAmount" key="GAM"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true">grossAmount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|GrossAmount</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="{$pLeg}ValueDate"/>
				<endDateReference hRef="{$pLeg}ValueDate"/>

				<valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>

				<payerReference hRef="{$pLeg}BuyerPartyReference"/>
				<receiverReference hRef="{$pLeg}SellerPartyReference"/>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
        
        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDateReference hRef="DSA_AdjustedPaymentDate"/>
        </subItem>

        <subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDateReference hRef="DSA_PreSettlementPaymentDate"/>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDateReference hRef="DSA_AdjustedPaymentDate"/>
					<xsl:if test="$pLeg=''">
						<isPayment>1</isPayment>	
					</xsl:if>
				</subItem>
			</itemGroup>
		</grossAmount>
	</xsl:template>

	<!-- DebtSecurityTransactionAccruedInterestAmount -->
	<xsl:template name="DebtSecurityTransactionAccruedInterestAmount">
		<xsl:param name="pLeg"/>
		<accruedInterestAmount>
			<keyGroup name="DebtSecurityTransactionAccruedInterestAmount" key="AIN"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true">accruedInterestAmount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|AccruedInterestAmount</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="{$pLeg}ValueDate"/>
				<endDateReference hRef="{$pLeg}ValueDate"/>

				<valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>

				<payerReference hRef="{$pLeg}BuyerPartyReference"/>
				<receiverReference hRef="{$pLeg}SellerPartyReference"/>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDateReference hRef="DSA_AdjustedPaymentDate"/>
        </subItem>

        <subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDateReference hRef="DSA_PreSettlementPaymentDate"/>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDateReference hRef="DSA_AdjustedPaymentDate"/>
				</subItem>
			</itemGroup>
		</accruedInterestAmount>
	</xsl:template>


	<!-- DebtSecurityStream -->
	<xsl:template name="DebtSecurityTransactionStream">
		<xsl:param name="pLeg"/>
		<keyGroup name="Stream" key="STD"/>
		<itemGroup>
			<occurence to="All" field="debtSecurityStream">efs_DebtSecurityTransactionStream</occurence>
			<key>
				<eventCode>EventCode</eventCode>
				<eventType>EventType</eventType>
			</key>
			<idStCalcul>[CALC]</idStCalcul>
			<startDate>EffectiveDate</startDate>
			<endDate>TerminationDate</endDate>

			<subItem>
				<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
				<eventDate>AdjustedEffectiveDate</eventDate>
			</subItem>
		</itemGroup>

		<xsl:call-template name="DST_NominalPeriodsVariation">
			<xsl:with-param name="pEventType" select="'Nominal'"/>
			<xsl:with-param name="pLeg" select="$pLeg"/>
		</xsl:call-template>

		<xsl:call-template name="DST_NominalPeriodsVariation">
			<xsl:with-param name="pEventType" select="'Quantity'"/>
			<xsl:with-param name="pLeg" select="$pLeg"/>
		</xsl:call-template>

		<xsl:call-template name="DST_NominalPeriods">
			<xsl:with-param name="pEventType" select="'Nominal'"/>
		</xsl:call-template>
		<xsl:call-template name="DST_NominalPeriods">
			<xsl:with-param name="pEventType" select="'Quantity'"/>
		</xsl:call-template>

		<xsl:if test="$pLeg!='forward'">
			<xsl:call-template name="DST_PaymentDates">
				<xsl:with-param name="pLeg" select="$pLeg"/>
			</xsl:call-template>
		</xsl:if>

	</xsl:template>

	<!-- DST_NominalPeriods -->
	<xsl:template name="DST_NominalPeriodsVariation">
		<xsl:param name="pEventType"/>
		<xsl:param name="pLeg"/>
		
		<nominalPeriodsVariation>
			<xsl:variable name="paramEventType">
				<xsl:value-of select="$pLeg"/><xsl:value-of select="$pEventType"/>EventType
			</xsl:variable>
			<keyGroup name="NominalPeriodsVariation" key="NOM"/>
			<parameter id="{$pLeg}{$pEventType}EventType">EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pEventType"/></parameter>
			<itemGroup>
				<occurence to="All">nominal</occurence>
				<key>
					<eventCode>EventCode</eventCode>
					<eventTypeReference hRef="{$paramEventType}"/>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>

				<xsl:choose>
					<xsl:when test = "$pEventType = 'Nominal'">
						<valorisation>Amount</valorisation>
						<unitType>[Currency]</unitType>
						<unit>Currency</unit>
					</xsl:when>
					<xsl:when test = "$pEventType = 'Quantity'">
						<valorisation>Quantity</valorisation>
						<unitType>[Qty]</unitType>
					</xsl:when>
				</xsl:choose>

				<payer>PayerNominalReference(<xsl:value-of select="$pLeg"/><xsl:value-of select="$pEventType"/>EventType,<xsl:value-of select="$pLeg"/>BuyerPartyReference,<xsl:value-of select="$pLeg"/>SellerPartyReference,<xsl:value-of select="$pLeg"/>IssuerPartyReference)</payer>
				<receiver>ReceiverNominalReference(<xsl:value-of select="$pLeg"/><xsl:value-of select="$pEventType"/>EventType,<xsl:value-of select="$pLeg"/>BuyerPartyReference,<xsl:value-of select="$pLeg"/>SellerPartyReference,<xsl:value-of select="$pLeg"/>IssuerPartyReference)</receiver>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDate>RecognitionDate</eventDate>
				</subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>SettlementDate</eventDate>
        </subItem>

        <subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>PreSettlementDate</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDate>SettlementDate</eventDate>
					<!--<xsl:if test = "$pEventType = 'Quantity'">
						<isPayment>1</isPayment>
					</xsl:if>-->
          <isPayment>1</isPayment>
        </subItem>
			</itemGroup>
		</nominalPeriodsVariation>
	</xsl:template>

	<!-- DST_NominalPeriods -->
	<xsl:template name="DST_NominalPeriods">
		<xsl:param name="pEventType"/>
		<nominalPeriods>
			<keyGroup name="NominalPeriods" key="NOSNOM"/>
			<itemGroup>
				<occurence to="All">nominalStep</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pEventType"/></eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>

				<xsl:choose>
					<xsl:when test = "$pEventType = 'Nominal'">
						<valorisation>Amount</valorisation>
						<unitType>[Currency]</unitType>
						<unit>Currency</unit>
					</xsl:when>
					<xsl:when test = "$pEventType = 'Quantity'">
						<valorisation>Quantity</valorisation>
						<unitType>[Qty]</unitType>
					</xsl:when>
				</xsl:choose>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Date</eventClass>
					<eventDate>AdjustedStartPeriod</eventDate>
				</subItem>
			</itemGroup>
		</nominalPeriods>
	</xsl:template>


	<!-- DebtSecurity Transaction Stream PaymentDates -->
	<xsl:template name="DST_PaymentDates">
		<xsl:param name="pLeg"/>
		<paymentDates>
			<keyGroup name="PaymentDates" key="PAY"/>
			<itemGroup>
				<occurence to="All">interest</occurence>
				<key>
					<eventCode>EventCode</eventCode>
					<eventType>EventType</eventType>
					<idE_Source>IdE_Source</idE_Source>
				</key>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>

				<valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>

				<payerReference hRef="{$pLeg}IssuerPartyReference"/>
				<receiverReference hRef="{$pLeg}BuyerPartyReference"/>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDate>AdjustedEndPeriod</eventDate>
				</subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>SettlementDate</eventDate>
        </subItem>

        <subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>AdjustedPreSettlementDate</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDate>SettlementDate</eventDate>
					<isPayment>1</isPayment>
				</subItem>
			</itemGroup>
		</paymentDates>
	</xsl:template>


</xsl:stylesheet>