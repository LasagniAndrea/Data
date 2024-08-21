<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
				        xmlns:dt="http://xsltsl.org/date-time">       
				
	<xsl:param name="pPartyIDFrom"/>
	<xsl:param name="pPartyIDTo"/>
  <xsl:param name="pSendersLTAddress"/>
  <xsl:param name="pReceiversLTAddress"/>
	<xsl:param name="pSwiftCommonReferenceBankLeft"/>
	<xsl:param name="pSwiftCommonReferenceBankRight"/>

  <xsl:include href="..\Lib_SWIFT.xslt"/>
	
	<!-- ******************* -->
	<!-- MAIN                -->
	<!-- ******************* -->	
	<xsl:template match="/">		
		<xsl:call-template name="Swift-BasicHeader">
			<xsl:with-param name="pSendersLTAddress" select="$pSendersLTAddress" />
		</xsl:call-template>
		
		<xsl:call-template name="Swift-ApplicationHeader">
			<xsl:with-param name="pReceiversLTAddress" select="$pReceiversLTAddress" />
			<xsl:with-param name="pMessageType" select="'300'" />
		</xsl:call-template>
								
		<xsl:call-template name="MT300Fields">
			<xsl:with-param name="pPartyIDFrom" select="$pPartyIDFrom"/>  
			<xsl:with-param name="pPartyIDTo" select="$pPartyIDTo"/>  
		</xsl:call-template>								
	</xsl:template>


	<!-- ******************* -->
	<!-- Message SWIFT MT300 -->
	<!-- ******************* -->
	<xsl:template name="MT300Fields">
		<xsl:param name="pPartyIDFrom"/>  
		<xsl:param name="pPartyIDTo"/>  
		
		<xsl:text>&#xA;</xsl:text>

		<xsl:text>{4:</xsl:text>
		
		<!-- Mandatory Sequence A General Information -->
		<!--:15A:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:15A:</xsl:text>
		
		<!--:20:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:20:</xsl:text>
		<xsl:call-template name="Swift-GetReferenceNumber">
			<xsl:with-param name="pPartyID" select="$pPartyIDFrom"/>  
		</xsl:call-template>

		<!--:22A:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:22A:</xsl:text>
		<xsl:text>NEWT</xsl:text>
		
		<!--:22C:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:22C:</xsl:text>
		<xsl:call-template name="Swift-CommonReference">
			<xsl:with-param name="pBicCodeBank1" select="$pSwiftCommonReferenceBankLeft"/>  
			<xsl:with-param name="pBicCodeBank2" select="$pSwiftCommonReferenceBankRight"/>  
			<xsl:with-param name="pSeparatorCode" select="//dataDocument/trade/fxSingleLeg/exchangeRate/rate"/>				
			<xsl:with-param name="pCommonReferenceType" select="'NUMBER'"/>				
		</xsl:call-template>

		<!--:82a:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:82A:</xsl:text>
		<xsl:value-of select="$pPartyIDFrom"/>
		
		<!--:87a:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:87A:</xsl:text>
		<xsl:value-of select="$pPartyIDTo"/>
		
		<!-- Mandatory Sequence B Transaction Details -->
		<!--:15B:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:15B:</xsl:text>
		
		<!--:30T:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:30T:</xsl:text>
		<!--Formatage de la date yyyy-mm-jj se transforme en yyyymmjj-->		
		<xsl:call-template name="Swift-Date">
			<xsl:with-param name="pDate" select="//dataDocument/trade/tradeHeader/tradeDate"/>  
			<xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>				
			<xsl:with-param name="pDatePatternOut" select="'yyyymmjj'"/>				
		</xsl:call-template>
		
		<!--:30V:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:30V:</xsl:text>
		<!--Formatage de la date yyyy-mm-jj se transforme en yyyymmjj-->		
		<xsl:call-template name="Swift-Date">
			<xsl:with-param name="pDate" select="//dataDocument/trade/fxSingleLeg/valueDate"/>  
			<xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>				
			<xsl:with-param name="pDatePatternOut" select="'yyyymmjj'"/>				
		</xsl:call-template>
		
		<!--:36:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:36:</xsl:text>
		<xsl:call-template name="Swift-Rate">
			<xsl:with-param name="pSource" select="//dataDocument/trade/fxSingleLeg/exchangeRate/rate" /> 
		</xsl:call-template>									
						
		<!-- Mandatory Subsequence B1 Amount Bought -->
		<!--:32B:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:32B:</xsl:text>
		<xsl:value-of select="//dataDocument/trade/fxSingleLeg/exchangedCurrency1/paymentAmount/currency" />
		<xsl:call-template name="Swift-Amount">
			<xsl:with-param name="pSource" select="//dataDocument/trade/fxSingleLeg/exchangedCurrency1/paymentAmount/amount" /> 
		</xsl:call-template>

		<!--:57A:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:57A:</xsl:text>
		<xsl:value-of select="$pPartyIDFrom"/>

		<!-- Mandatory Subsequence B2 Amount Sold -->
		<!--:33B:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:33B:</xsl:text>
		<xsl:value-of select="//dataDocument/trade/fxSingleLeg/exchangedCurrency2/paymentAmount/currency" />
		<xsl:call-template name="Swift-Amount">
			<xsl:with-param name="pSource" select="//dataDocument/trade/fxSingleLeg/exchangedCurrency2/paymentAmount/amount" /> 
		</xsl:call-template>

		<!--:57A:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:57A:</xsl:text>
		<xsl:value-of select="$pPartyIDTo"/>

		<!-- Optional Sequence C Optional General Information -->
		<!--:15C:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:15C:</xsl:text>

		<!--:15A:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:24D:</xsl:text>
		<xsl:text>ELEC</xsl:text>
		
		<!-- Optional Sequence D Split Settlement Details -->

						
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>-}</xsl:text>
		
	</xsl:template>
	
</xsl:stylesheet>