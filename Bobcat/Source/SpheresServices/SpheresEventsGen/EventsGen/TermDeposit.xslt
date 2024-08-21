<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='TermDeposit']">
		<xsl:param name="pPosition" select="0"/>
		<termDeposit>
		
			<!-- Product -->
			<keyGroup name="TermDeposit" key="PRD"/>
			<parameter id="InitialPayerReference">InitialPayerReference</parameter>
			<parameter id="InitialReceiverReference">InitialReceiverReference</parameter>
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
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
				</subItem>
			</itemGroup>
			
			<groupLevel>
				<keyGroup name="GroupLevel" key="GRP"/>
				<itemGroup>	
					<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|TermDeposit</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|TermDeposit</eventType>
					</key>
					<idStCalcul>[CALC]</idStCalcul>
					<subItem>
						<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
						<eventDateReference hRef="AdjustedTradeDate"/>
					</subItem>
				</itemGroup>
				
				<!-- Nominal -->			
				<xsl:call-template name="NominalTermDeposit">
					<xsl:with-param name="pEventCode" select="'Start'"/>
				</xsl:call-template>
				<xsl:call-template name="NominalTermDeposit">
					<xsl:with-param name="pEventCode" select="'Termination'"/>
				</xsl:call-template>
				<xsl:call-template name="NominalStepTermDeposit"/>

				<!-- Interest -->
				<xsl:call-template name="Interest"/>

				<!-- Payment -->
				<payment>
					<xsl:call-template name="Payment">
						<xsl:with-param name="pType" select="'TermDeposit'"/>
					</xsl:call-template>
				</payment>
			</groupLevel>
		</termDeposit>
	</xsl:template>
	
	<!-- NominalDates -->
	<xsl:template name="NominalTermDeposit">
		<xsl:param name="pEventCode"/>
		<nominalPeriod>
			<keyGroup name="NominalPeriods" key="NOM"/>
			<itemGroup>
				<occurence to="Unique" field="nominalPeriod">efs_TermDeposit</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>
				
				<xsl:if test = "$pEventCode = 'Start'"><payerReference hRef="InitialReceiverReference"/></xsl:if>
				<xsl:if test = "$pEventCode = 'Termination'"><payerReference hRef="InitialPayerReference"/></xsl:if>

				<xsl:if test = "$pEventCode = 'Start'"><receiverReference hRef="InitialPayerReference"/></xsl:if>
				<xsl:if test = "$pEventCode = 'Termination'"><receiverReference hRef="InitialReceiverReference"/></xsl:if>
				
				<valorisation>PeriodAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PeriodCurrency</unit>
			
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<xsl:if test = "$pEventCode = 'Start'"><eventDateReference hRef="AdjustedTradeDate"/></xsl:if>
					<xsl:if test = "$pEventCode = 'Termination'"><eventDate>UnadjustedEndPeriod</eventDate></xsl:if>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
					<eventDate>
						<xsl:if test = "$pEventCode = 'Start'">AdjustedPreSettlementStartDate</xsl:if>
						<xsl:if test = "$pEventCode = 'Termination'">AdjustedPreSettlementEndDate</xsl:if>
					</eventDate>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDate>
						<xsl:if test = "$pEventCode = 'Start'">AdjustedStartPeriod</xsl:if>
						<xsl:if test = "$pEventCode = 'Termination'">AdjustedEndPeriod</xsl:if>
					</eventDate>
					<isPayment>1</isPayment>
				</subItem>
			</itemGroup>
		</nominalPeriod>						
	</xsl:template>		

	<!-- NominalStep -->
	<xsl:template name="NominalStepTermDeposit">
		<nominalPeriod>
			<keyGroup name="NominalStep" key="NOS"/>
			<itemGroup>
				<occurence to="Unique" field="nominalPeriod">efs_TermDeposit</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>
				
				<valorisation>PeriodAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>PeriodCurrency</unit>
			
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDate>AdjustedStartPeriod</eventDate>
				</subItem>
			</itemGroup>
		</nominalPeriod>						
	</xsl:template>		

	<!-- PaymentDates -->
	
	<xsl:template name="Interest">	
		<interest>
			<keyGroup name="Interest" key="PAY"/>
			<itemGroup>
				<occurence to="Unique" field="interestPeriod">efs_TermDeposit</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Termination</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Interest</eventType>
				</key>
				<startDate>StartPeriod</startDate>
				<endDate>EndPeriod</endDate>
				
				<payerReference hRef="InitialPayerReference"/>
				<receiverReference hRef="InitialReceiverReference"/>
				
				<valorisation>Interest</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>

				<item_Details>
					<fixedRate>FixedRate</fixedRate>
					<dayCountFraction>DayCountFraction</dayCountFraction>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTradeDate"/>
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
		</interest>
	</xsl:template>
			
</xsl:stylesheet>