<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:template name="InvoiceCorrection">
		<!-- Avoir / Facture complémentaire -->
		<xsl:variable name="Invoice">
			<xsl:text>LinkedProductPayment</xsl:text>
		</xsl:variable>
		<finalInvoice>
			<keyGroup name="GroupLevel" key="INV"/>
			<parameter id="{$Invoice}_TurnOverIssueRate">TurnOverIssueRate</parameter>
			<parameter id="{$Invoice}_TurnOverIssueBase">TurnOverIssueBase</parameter>
			<parameter id="{$Invoice}_TurnOverIssueAmount">TurnOverIssueAmount</parameter>
			<parameter id="{$Invoice}_TurnOverIssueCurrency">TurnOverIssueCurrency</parameter>
			<parameter id="{$Invoice}_TurnOverAccountingRate">TurnOverAccountingRate</parameter>
			<parameter id="{$Invoice}_TurnOverAccountingBase">TurnOverAccountingBase</parameter>
			<parameter id="{$Invoice}_TurnOverAccountingAmount">TurnOverAccountingAmount</parameter>
			<parameter id="{$Invoice}_TurnOverAccountingCurrency">TurnOverAccountingCurrency</parameter>
			<parameter id="{$Invoice}_TaxAmount">TaxAmount</parameter>
			<parameter id="{$Invoice}_TaxCurrency">TaxCurrency</parameter>
			<parameter id="{$Invoice}_TaxIssueBase">TaxIssueBase</parameter>
			<parameter id="{$Invoice}_TaxIssueAmount">TaxIssueAmount</parameter>
			<parameter id="{$Invoice}_TaxIssueCurrency">TaxIssueCurrency</parameter>
			<parameter id="{$Invoice}_TaxAccountingBase">TaxAccountingBase</parameter>
			<parameter id="{$Invoice}_TaxAccountingAmount">TaxAccountingAmount</parameter>
			<parameter id="{$Invoice}_TaxAccountingCurrency">TaxAccountingCurrency</parameter>
			<itemGroup>
				<occurence to="Unique">efs_Invoice</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|InvoicingDates</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Period</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- RebateAmount -->
			<xsl:call-template name="RebateAmount">
				<xsl:with-param name="pInvoiceSection" select="$Invoice"/>
			</xsl:call-template>

			<!-- GlobalNetTurnOverAndTax -->
			<xsl:call-template name="GlobalNetTurnOverAndTax">
				<xsl:with-param name="pInvoiceSection" select="$Invoice"/>
			</xsl:call-template>

			<xsl:call-template name="InitialInvoice"/>
			<xsl:call-template name="BaseInvoice"/>
			<xsl:call-template name="TheoricInvoice"/>

		</finalInvoice>

	</xsl:template>

	<xsl:template name="InitialInvoice">
		<xsl:variable name="InvoiceMaster">
			<xsl:text>InvoiceMaster</xsl:text>
		</xsl:variable>
		<!-- Facture d'origine -->
		<initialInvoice>
			<keyGroup name="GroupLevel" key="IMS"/>
			<parameter id="{$InvoiceMaster}_TurnOverIssueRate">TurnOverIssueRate</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverIssueBase">TurnOverIssueBase</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverIssueAmount">TurnOverIssueAmount</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverIssueCurrency">TurnOverIssueCurrency</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverAccountingRate">TurnOverAccountingRate</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverAccountingBase">TurnOverAccountingBase</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverAccountingAmount">TurnOverAccountingAmount</parameter>
			<parameter id="{$InvoiceMaster}_TurnOverAccountingCurrency">TurnOverAccountingCurrency</parameter>
			<parameter id="{$InvoiceMaster}_TaxAmount">TaxAmount</parameter>
			<parameter id="{$InvoiceMaster}_TaxCurrency">TaxCurrency</parameter>
			<parameter id="{$InvoiceMaster}_TaxIssueBase">TaxIssueBase</parameter>
			<parameter id="{$InvoiceMaster}_TaxIssueAmount">TaxIssueAmount</parameter>
			<parameter id="{$InvoiceMaster}_TaxIssueCurrency">TaxIssueCurrency</parameter>
			<parameter id="{$InvoiceMaster}_TaxAccountingBase">TaxAccountingBase</parameter>
			<parameter id="{$InvoiceMaster}_TaxAccountingAmount">TaxAccountingAmount</parameter>
			<parameter id="{$InvoiceMaster}_TaxAccountingCurrency">TaxAccountingCurrency</parameter>

			<itemGroup>
				<occurence to="Unique">efs_InitialInvoice</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$InvoiceMaster" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Period</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- GrossTurnOverAmount -->
			<xsl:call-template name="TurnOverAndTaxAmount">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceMaster"/>
				<xsl:with-param name="pTurnOverType" select="'Gross'"/>
			</xsl:call-template>

			<!-- RebateAmount -->
			<xsl:call-template name="RebateAmount">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceMaster"/>
			</xsl:call-template>

			<!-- GlobalNetTurnOverAndTax -->
			<xsl:call-template name="GlobalNetTurnOverAndTax">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceMaster"/>
			</xsl:call-template>

		</initialInvoice>
	</xsl:template>

	<xsl:template name="BaseInvoice">
		<xsl:variable name="InvoiceMasterBase">
			<xsl:text>InvoiceMasterBase</xsl:text>
		</xsl:variable>
		<!-- Facture d'origine -->
		<baseInvoice>
			<keyGroup name="GroupLevel" key="IMB"/>
			<parameter id="{$InvoiceMasterBase}_TurnOverIssueRate">TurnOverIssueRate</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverIssueBase">TurnOverIssueBase</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverIssueAmount">TurnOverIssueAmount</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverIssueCurrency">TurnOverIssueCurrency</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverAccountingRate">TurnOverAccountingRate</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverAccountingBase">TurnOverAccountingBase</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverAccountingAmount">TurnOverAccountingAmount</parameter>
			<parameter id="{$InvoiceMasterBase}_TurnOverAccountingCurrency">TurnOverAccountingCurrency</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxAmount">TaxAmount</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxCurrency">TaxCurrency</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxIssueBase">TaxIssueBase</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxIssueAmount">TaxIssueAmount</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxIssueCurrency">TaxIssueCurrency</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxAccountingBase">TaxAccountingBase</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxAccountingAmount">TaxAccountingAmount</parameter>
			<parameter id="{$InvoiceMasterBase}_TaxAccountingCurrency">TaxAccountingCurrency</parameter>
			<itemGroup>
				<occurence to="Unique">efs_BaseInvoice</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$InvoiceMasterBase" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Period</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- GlobalNetTurnOverAndTax -->
			<xsl:call-template name="GlobalNetTurnOverAndTax">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceMasterBase"/>
			</xsl:call-template>

		</baseInvoice>
	</xsl:template>

	<xsl:template name="TheoricInvoice">
		<xsl:variable name="InvoiceAmended">
			<xsl:text>InvoiceAmended</xsl:text>
		</xsl:variable>
		<!-- Facture corrigée -->
		<theoricInvoice>
			<keyGroup name="GroupLevel" key="IAM"/>
			<parameter id="{$InvoiceAmended}_TurnOverIssueRate">TurnOverIssueRate</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverIssueBase">TurnOverIssueBase</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverIssueAmount">TurnOverIssueAmount</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverIssueCurrency">TurnOverIssueCurrency</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverAccountingRate">TurnOverAccountingRate</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverAccountingBase">TurnOverAccountingBase</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverAccountingAmount">TurnOverAccountingAmount</parameter>
			<parameter id="{$InvoiceAmended}_TurnOverAccountingCurrency">TurnOverAccountingCurrency</parameter>
			<parameter id="{$InvoiceAmended}_TaxAmount">TaxAmount</parameter>
			<parameter id="{$InvoiceAmended}_TaxCurrency">TaxCurrency</parameter>
			<parameter id="{$InvoiceAmended}_TaxIssueBase">TaxIssueBase</parameter>
			<parameter id="{$InvoiceAmended}_TaxIssueAmount">TaxIssueAmount</parameter>
			<parameter id="{$InvoiceAmended}_TaxIssueCurrency">TaxIssueCurrency</parameter>
			<parameter id="{$InvoiceAmended}_TaxAccountingBase">TaxAccountingBase</parameter>
			<parameter id="{$InvoiceAmended}_TaxAccountingAmount">TaxAccountingAmount</parameter>
			<parameter id="{$InvoiceAmended}_TaxAccountingCurrency">TaxAccountingCurrency</parameter>

			<itemGroup>
				<occurence to="Unique">efs_TheoricInvoice</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$InvoiceAmended" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Period</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- GrossTurnOverAmount -->
			<xsl:call-template name="TurnOverAndTaxAmount">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceAmended"/>
				<xsl:with-param name="pTurnOverType" select="'Gross'"/>
			</xsl:call-template>

			<!-- RebateAmount -->
			<xsl:call-template name="InvoiceRebateAmount">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceAmended"/>
			</xsl:call-template>

			<!-- GlobalNetTurnOverAndTax -->
			<xsl:call-template name="GlobalNetTurnOverAndTax">
				<xsl:with-param name="pInvoiceSection" select="$InvoiceAmended"/>
			</xsl:call-template>

		</theoricInvoice>
	</xsl:template>

	<!-- GlobalNetTurnOverAndTax -->
	<xsl:template name="GlobalNetTurnOverAndTax">
		<xsl:param name="pInvoiceSection"/>

		<!-- NetTurnOverAmount -->
		<xsl:call-template name="TurnOverAndTaxAmount">
			<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
			<xsl:with-param name="pTurnOverType" select="'Net'"/>
		</xsl:call-template>
		<!-- NetTurnOverIssueAmount -->
		<xsl:call-template name="NetTurnOverAndTaxAmount">
			<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
			<xsl:with-param name="pTurnOverType" select="'Issue'"/>
		</xsl:call-template>
		<!-- NetTurnOverAccountingAmount -->
		<xsl:call-template name="NetTurnOverAndTaxAmount">
			<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
			<xsl:with-param name="pTurnOverType" select="'Accounting'"/>
		</xsl:call-template>

	</xsl:template>

	<!-- TurnOverAndTaxAmount -->
  <!-- EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
			 Gestion des parties (Cas REBATE avec payeur qui devient l'entité) 
  -->
	<xsl:template name="TurnOverAndTaxAmount">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTurnOverType"/>
		<turnOverAmount>
			<keyGroup name="TurnOverAmount" key="TOA"/>
      <parameter id="{$pInvoiceSection}GrossTurnOverPayer">GrossTurnOverPayer</parameter>
      <parameter id="{$pInvoiceSection}GrossTurnOverReceiver">GrossTurnOverReceiver</parameter>
      <itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pTurnOverType"/>TurnOverAmount</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

        <xsl:if test = "$pTurnOverType = 'Gross'">
					<payerReference  hRef="{$pInvoiceSection}GrossTurnOverPayer"/>
					<receiverReference  hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>
        </xsl:if>

        <xsl:if test = "$pTurnOverType = 'Net'">
          <payerReference hRef="InvoicePayer"/>
          <receiverReference hRef="InvoiceReceiver"/>
        </xsl:if>

        <valorisation><xsl:value-of select="$pTurnOverType"/>TurnOverAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit><xsl:value-of select="$pTurnOverType"/>TurnOverCurrency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<xsl:if test = "$pTurnOverType = 'Gross'">
				<!-- DetailTurnOverAmount-->
				<xsl:call-template name="DetailTurnOverAmount">
					<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
					<xsl:with-param name="pTurnOverType" select="$pTurnOverType"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test = "$pTurnOverType = 'Net'">
				<!-- TaxAmount-->
				<xsl:call-template name="TaxAmount">
					<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
					<xsl:with-param name="pTaxType" select="''"/>
				</xsl:call-template>
			</xsl:if>

		</turnOverAmount>
	</xsl:template>

	<!-- DetailTurnOverAmount -->
  <!-- EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
			 Gestion des parties 
  -->
  <xsl:template name="DetailTurnOverAmount">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTurnOverType"/>
		<detailTurnOverAmount>
			<keyGroup name="DetailTurnOverAmount" key="DTO"/>
			<parameter id="Detail{$pInvoiceSection}_Payer">InvoicePayer</parameter>
			<itemGroup>
				<occurence to="All" isOptional="true">detailGrossTurnOver</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>FeeType</eventType>
					<idE_Source>IdE_Source</idE_Source>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDate>FeeDate</startDate>
				<endDate>FeeDate</endDate>

				<payerReference hRef="Detail{$pInvoiceSection}_Payer"/>
        <receiverReference hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>

				<valorisation>FeeAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>FeeCurrency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- DetailTurnOverAccountingAmount -->
			<xsl:call-template name="DetailTurnOverAccountingAmount">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
				<xsl:with-param name="pTurnOverType" select="'Accounting'"/>
			</xsl:call-template>

		</detailTurnOverAmount>
	</xsl:template>

	<!-- RebateAmount -->
  <!-- EG 20240202 [WI640] Gestion des parties (Cas REBATE avec payeur qui devient l'entité) -->
  <xsl:template name="RebateAmount">
		<xsl:param name="pInvoiceSection"/>
		<rebateAmount>
			<keyGroup name="RebateAmount" key="REB"/>
			<itemGroup>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|GlobalRebate</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

        <payerReference  hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>
        <receiverReference  hRef="{$pInvoiceSection}GrossTurnOverPayer"/>

				<valorisation>RebateAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>RebateCurrency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>
		</rebateAmount>
	</xsl:template>

	<!-- InvoiceRebateAmount -->
  <!-- EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
			 Gestion des parties 
  -->
  <xsl:template name="InvoiceRebateAmount">
		<xsl:param name="pInvoiceSection"/>
		<invoiceRebateAmount>
			<keyGroup name="RebateAmount" key="REB"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true">invoiceRebate</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|GlobalRebate</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

        <payerReference  hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>
        <receiverReference  hRef="{$pInvoiceSection}GrossTurnOverPayer"/>

        <valorisation>TotalRebateAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>TotalRebateCurrency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- RebateCapAmount -->
			<xsl:call-template name="RebateCapAmount">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
			</xsl:call-template>
			<!-- RebateBracketAmount -->
			<xsl:call-template name="RebateBracketAmount">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
			</xsl:call-template>
		</invoiceRebateAmount>
	</xsl:template>

	<!-- RebateCapAmount -->
  <!-- EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
			 Gestion des parties 
  -->
  <xsl:template name="RebateCapAmount">
		<xsl:param name="pInvoiceSection"/>
		<rebateCapAmount>
			<keyGroup name="RebateCapAmount" key="CRB"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true">netTurnOverInExcessAmount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|CapRebate</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="InvoiceDate"/>
				<endDateReference hRef="InvoiceDate"/>

        <payerReference  hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>
        <receiverReference  hRef="{$pInvoiceSection}GrossTurnOverPayer"/>

        <valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>
		</rebateCapAmount>
	</xsl:template>

	<!-- RebateBracketAmount -->
  <!-- EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
			 Gestion des parties 
  -->
  <xsl:template name="RebateBracketAmount">
		<xsl:param name="pInvoiceSection"/>
		<rebateBracketAmount>
			<keyGroup name="RebateBracketAmount" key="BRK"/>
			<parameter id="RebateBracketType">RebateBracketType</parameter>
			<itemGroup>
				<occurence to="Unique" isOptional="true">rebateBracket</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|BracketRebate</eventType>
				</key>
				<idStCalcul>[CALC]</idStCalcul>
				<startDateReference hRef="InvoiceDate"/>
				<endDateReference hRef="InvoiceDate"/>

        <payerReference  hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>
        <receiverReference  hRef="{$pInvoiceSection}GrossTurnOverPayer"/>

        <valorisation>TotalRebateBracketAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>TotalRebateBracketCurrency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- rebateBracketAmountDetail -->
			<xsl:call-template name="RebateBracketAmountDetail">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
			</xsl:call-template>
		</rebateBracketAmount>
	</xsl:template>

	<!-- RebateBracketAmountDetail -->
  <!-- EG 20240205 [WI640] Gestion de l'application de plafonds négatifs (Rebate) / Reverse du Payeur/Receiver si GTO < 0 (Cas d'un Rebate > GTO de base)
			 Gestion des parties 
  -->
  <xsl:template name="RebateBracketAmountDetail">
		<xsl:param name="pInvoiceSection"/>
		<rebateBracketAmountDetail>
			<keyGroup name="RebateBracketDetail" key="DBK"/>
			<itemGroup>
				<occurence to="all">details</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventTypeReference hRef="RebateBracketType"></eventTypeReference>
				</key>
				<idStCalcul>[CALC]</idStCalcul>

        <payerReference  hRef="{$pInvoiceSection}GrossTurnOverReceiver"/>
        <receiverReference  hRef="{$pInvoiceSection}GrossTurnOverPayer"/>

        <startDateReference hRef="InvoiceDate"/>
				<endDateReference hRef="InvoiceDate"/>

				<valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>Currency</unit>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>
		</rebateBracketAmountDetail>
	</xsl:template>

	<!-- NetTurnOverAndTaxAmount -->
	<xsl:template name="NetTurnOverAndTaxAmount">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTurnOverType"/>
		<turnOverAmount>
			<keyGroup name="NetTurnOverTypeAmount" key="NTO"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true">netTurnOver<xsl:value-of select="$pTurnOverType"/>Amount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|NetTurnOver<xsl:value-of select="$pTurnOverType"/>Amount</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

				<payerReference hRef="InvoicePayer"/>
				<receiverReference hRef="InvoiceReceiver"/>


				<valorisationReference hRef="{$pInvoiceSection}_TurnOver{$pTurnOverType}Amount"/>
				<unitType>[Currency]</unitType>
				<unitReference hRef="{$pInvoiceSection}_TurnOver{$pTurnOverType}Currency"/>

				<item_Details>
					<invoicingAmountBaseReference hRef="{$pInvoiceSection}_TurnOver{$pTurnOverType}Base"/>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
					<eventDateReference hRef="AdjustedSettlementDate"/>
				</subItem>
			</itemGroup>

			<!-- turnOverAmountFixing -->
			<xsl:call-template name="TurnOverAmountFixing">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
				<xsl:with-param name="pTurnOverType" select="$pTurnOverType"/>
			</xsl:call-template>

			<!-- Tax -->
			<xsl:call-template name="TaxAmount">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
				<xsl:with-param name="pTaxType" select="$pTurnOverType"/>
			</xsl:call-template>

		</turnOverAmount>
	</xsl:template>

	<!-- TaxAmount -->
	<xsl:template name="TaxAmount">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTaxType"/>
		<taxAmount>
			<keyGroup name="TaxTypeAmount" key="TXO"/>
			<itemGroup>
				<xsl:choose>
					<xsl:when test = "$pTaxType != ''">
						<occurence to="Unique" isOptional="true">taxAmount</occurence>
					</xsl:when>
					<xsl:otherwise>
						<occurence to="Unique" isOptional="true">tax</occurence>
					</xsl:otherwise>
				</xsl:choose>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|Tax<xsl:value-of select="$pTaxType"/>Amount</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

				<payerReference hRef="InvoicePayer"/>
				<receiverReference hRef="InvoiceReceiver"/>


				<valorisationReference hRef="{$pInvoiceSection}_Tax{$pTaxType}Amount"/>
				<unitType>[Currency]</unitType>
				<unitReference hRef="{$pInvoiceSection}_Tax{$pTaxType}Currency"/>


				<item_Details>
					<invoicingAmountBaseReference hRef="{$pInvoiceSection}_Tax{$pTaxType}Base"/>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTaxInvoiceDate"/>
				</subItem>
				
			</itemGroup>

			<!-- TaxAmountDetail -->
			<xsl:call-template name="TaxAmountDetail">
				<xsl:with-param name="pInvoiceSection" select="$pInvoiceSection"/>
				<xsl:with-param name="pTaxType" select="$pTaxType"/>
			</xsl:call-template>

		</taxAmount>
	</xsl:template>

	<!-- TaxAmountDetail -->
	<xsl:template name="TaxAmountDetail">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTaxType"/>
		<taxDetails>
			<keyGroup name="TaxAmountDetail" key="TXD"/>
			<itemGroup>
				<occurence to="all">details</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EventType</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

				<payerReference hRef="InvoicePayer"/>
				<receiverReference hRef="InvoiceReceiver"/>


				<valorisation>Amount</valorisation>
				<unitType>[Currency]</unitType>
				<unitReference hRef="{$pInvoiceSection}_Tax{$pTaxType}Currency"/>

				<item_Details>
					<taxSource>TaxSource</taxSource>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedTaxInvoiceDate"/>
				</subItem>

			</itemGroup>
		</taxDetails>
	</xsl:template>

	<!-- TurnOverAmountFixing -->
	<xsl:template name="TurnOverAmountFixing">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTurnOverType"/>
		<turnOverAmountFixing>
			<keyGroup name="TurnOverAmountFixing" key="FXG"/>
			<itemGroup>
				<occurence to="Unique" isOptional="true">turnOverFixing</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|FxRate</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>

				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>

				<valorisationReference hRef="{$pInvoiceSection}_TurnOver{$pTurnOverType}Rate"/>
				<unitType>[Rate]</unitType>

				<item_Details>
					<fixedRateReference hRef="{$pInvoiceSection}_TurnOver{$pTurnOverType}Rate"/>
					<asset>AssetFxRate</asset>
					<fixingRate>FixingRate</fixingRate>
				</item_Details>
				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
					<eventDate>FixingDate</eventDate>
				</subItem>
			</itemGroup>
		</turnOverAmountFixing>
	</xsl:template>

	<!-- DetailTurnOverAccountingAmount -->
	<xsl:template name="DetailTurnOverAccountingAmount">
		<xsl:param name="pInvoiceSection"/>
		<xsl:param name="pTurnOverType"/>

		<xsl:variable name="DetailInvoice">
			Detail<xsl:value-of select="$pInvoiceSection" />
		</xsl:variable>
		<detailTurnOverAccountingAmount>
			<keyGroup name="DetailTurnOverAccountingAmount" key="DTA"/>
			<parameter id="Detail{$pInvoiceSection}_TurnOver{$pTurnOverType}Rate">TurnOverRate</parameter>
			<itemGroup>
				<occurence to="Unique" isOptional="true">feeAccountingAmount</occurence>
				<key>
					<eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pInvoiceSection" /></eventCode>
					<eventType>EfsML.Enum.Tools.EventTypeFunc|FeeAccountingAmount</eventType>
				</key>
				<idStCalcul>[CALCREV]</idStCalcul>
				<startDateReference hRef="TradeDate"/>
				<endDateReference hRef="InvoiceDate"/>
				<payerReference hRef="Detail{$pInvoiceSection}_Payer"/>
				<receiverReference hRef="InvoiceReceiver"/>

				<valorisation>TurnOverAmount</valorisation>
				<unitType>[Currency]</unitType>
				<unit>TurnOverCurrency</unit>

				<item_Details>
					<invoicingAmountBase>TurnOverBase</invoicingAmountBase>
				</item_Details>

				<subItem>
					<eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
					<eventDateReference hRef="AdjustedInvoiceDate"/>
				</subItem>
			</itemGroup>

			<!-- turnOverAmountFixing -->
			<xsl:call-template name="TurnOverAmountFixing">
				<xsl:with-param name="pInvoiceSection" select="$DetailInvoice"/>
				<xsl:with-param name="pTurnOverType" select="$pTurnOverType"/>
			</xsl:call-template>

		</detailTurnOverAccountingAmount>
	</xsl:template>

</xsl:stylesheet>