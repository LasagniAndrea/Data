<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

	<xsl:decimal-format name="decimalFormat" decimal-separator="." />

	<xsl:template match="/iotask">
		<iotask>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="name">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:attribute name="displayname">
				<xsl:value-of select="@displayname"/>
			</xsl:attribute>
			<xsl:attribute name="loglevel">
				<xsl:value-of select="@loglevel"/>
			</xsl:attribute>
			<xsl:attribute name="commitmode">
				<xsl:value-of select="@commitmode"/>
			</xsl:attribute>
			<xsl:apply-templates select="parameters"/>
			<xsl:apply-templates select="iotaskdet"/>
		</iotask>
	</xsl:template>

	<xsl:template match="parameters">
		<parameters>
			<xsl:for-each select="parameter" >
				<parameter>
					<xsl:attribute name="id">
						<xsl:value-of select="@id"/>
					</xsl:attribute>
					<xsl:attribute name="name">
						<xsl:value-of select="@name"/>
					</xsl:attribute>
					<xsl:attribute name="displayname">
						<xsl:value-of select="@displayname"/>
					</xsl:attribute>
					<xsl:attribute name="direction">
						<xsl:value-of select="@direction"/>
					</xsl:attribute>
					<xsl:attribute name="datatype">
						<xsl:value-of select="@datatype"/>
					</xsl:attribute>
					<xsl:value-of select="."/>
				</parameter>
			</xsl:for-each>
		</parameters>
	</xsl:template>

	<xsl:template match="iotaskdet">
		<iotaskdet>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="loglevel">
				<xsl:value-of select="@loglevel"/>
			</xsl:attribute>
			<xsl:attribute name="commitmode">
				<xsl:value-of select="@commitmode"/>
			</xsl:attribute>
			<xsl:apply-templates select="ioinput"/>
		</iotaskdet>
	</xsl:template>

	<xsl:template match="ioinput">
		<ioinput>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="name">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:attribute name="displayname">
				<xsl:value-of select="@displayname"/>
			</xsl:attribute>
			<xsl:attribute name="loglevel">
				<xsl:value-of select="@loglevel"/>
			</xsl:attribute>
			<xsl:attribute name="commitmode">
				<xsl:value-of select="@commitmode"/>
			</xsl:attribute>
			<xsl:apply-templates select="file"/>
		</ioinput>
	</xsl:template>

	<xsl:template match="file">
		<file>
			<xsl:attribute name="name">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:attribute name="folder">
				<xsl:value-of select="@folder"/>
			</xsl:attribute>
			<xsl:attribute name="date">
				<xsl:value-of select="@date"/>
			</xsl:attribute>
			<xsl:attribute name="size">
				<xsl:value-of select="@size"/>
			</xsl:attribute>
			<xsl:apply-templates select="row[@status='success']"/>
		</file>
	</xsl:template>

	<xsl:template match="row">
		<row>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="src">
				<xsl:value-of select="@src"/>
			</xsl:attribute>
			<tradeImport>
				<settings>
					<importMode>New</importMode>
					<parameters>
						<parameter name="http://www.efs.org/otcml/tradeImport/identifier" datatype="string">
							<xsl:value-of select="data[@name='TRADEIDENTIFIER']"/>
						</parameter>
						<parameter name="http://www.efs.org/otcml/tradeImport/instrumentIdentifier" datatype="string">debtSecurity</parameter>
						<parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string">debtSecurity</parameter>
						<!-- screen from instrument default -->
						<parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string">DebtSecurityOTCml</parameter>
						<!--/////-->
						<parameter name="http://www.efs.org/otcml/tradeImport/displayname" datatype="string">Import Sample</parameter>
						<parameter name="http://www.efs.org/otcml/tradeImport/description" datatype="string">Import Sample</parameter>
						<parameter name="http://www.efs.org/otcml/tradeImport/extllink" datatype="string" />
						<!--/////-->
						<parameter name="http://www.efs.org/otcml/tradeImport/isApplyFeeCalculation" datatype="bool">false</parameter>
						<parameter name="http://www.efs.org/otcml/tradeImport/isApplyPartyTemplate" datatype="bool">false</parameter>
						<parameter name="http://www.efs.org/otcml/tradeImport/isPostToEventsGen" datatype="bool">true</parameter>
					</parameters>
				</settings>
				<tradeInput>
					<customCaptureInfos>
						<customCaptureInfo clientId="tradeHeader_party1_actor" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='ACTOR1']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_localization_countryOfIssue" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='COUNTRY']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_currency" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='CURRENCY']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_localization_stateOrProvinceOfIssue" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='stateOrProvinceOfIssue']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_localization_localeOfIssue" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='localeOfIssue']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodDates_terminationDate" dataType="date">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='terminationDate']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream1_calculationPeriodAmount_calculation_rate" dataType="string" regex="RegexRate">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='calculation_rate']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue" dataType="decimal">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='floatingRateMultiplierSchedule_initialValue']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue" dataType="decimal" >
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='spreadSchedule_initialValue']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_couponType" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='couponType']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_priceRateType" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='priceRateType']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodDates_calculationPeriodFrequency_periodFrequency" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='periodFrequency']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodAmount_calculation_dayCountFraction" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='dayCountFraction']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_instrumentId1_scheme" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='instrumentId1_scheme']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_instrumentId1_value" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='instrumentId1_value']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_instrumentId2_scheme" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='instrumentId2_scheme']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_instrumentId2_value" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='instrumentId2_value']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_instrumentId3_scheme" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='instrumentId3_scheme']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_instrumentId3_value" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='instrumentId3_value']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_classification_debtSecurityClass" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='debtSecurityClass']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_classification_cfiCode" dataType="string" regex="RegexCFIIdentifier">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='cfiCode']"/>
							</xsl:element>							
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_classification_productTypeCode" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='productTypeCode']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_classification_financialInstrumentProductTypeCode" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='financialInstrumentProductTypeCode']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue" dataType="decimal">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='notionalStepSchedule_initialValue']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='notionalStepSchedule_currency']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_numberOfIssuedSecurities" dataType="integer" regex="RegexInteger">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='numberOfIssuedSecurities']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_faceAmount_amount" dataType="decimal">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='faceAmount_amount']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_faceAmount_currency" dataType="string">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='faceAmount_currency']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_price_issuePricePercentage" dataType="decimal">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='issuePricePercentage']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_security_price_redemptionPricePercentage" dataType="decimal">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='redemptionPricePercentage']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="tradeHeader_tradeDate" dataType="date">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='tradeDate']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodDates_effectiveDate" dataType="date">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='effectiveDate']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_calculationPeriodDates_firstPeriodStartDate" dataType="date">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='firstPeriodStartDate']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream_paymentDates_firstPaymentDate" dataType="date">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='firstPaymentDate']"/>
							</xsl:element>
						</customCaptureInfo>
						<customCaptureInfo clientId="debtSecurity_debtSecurityStream1_calculationPeriodDates_terminationDate" dataType="date">
							<xsl:element name ="value">
								<xsl:value-of select="data[@name='debtSecurityStream1_terminationDate']"/>
							</xsl:element>
						</customCaptureInfo>						
					</customCaptureInfos>
				</tradeInput>
			</tradeImport>
		</row>
	</xsl:template >

</xsl:stylesheet>
