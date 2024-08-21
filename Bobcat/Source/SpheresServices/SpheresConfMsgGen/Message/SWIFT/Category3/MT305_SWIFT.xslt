<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
				        xmlns:dt="http://xsltsl.org/date-time">       
				
	<!-- xsl:param name="pPartyIDFrom"/ -->
  <xsl:variable name="pPartyIDFrom">
    <xsl:value-of select="//header/sendBy/partyRelativeTo/@href"/>
  </xsl:variable>

  <xsl:param name="pPartyIDTo"/>
  <xsl:param name="pSendersLTAddress"/>
  <xsl:param name="pReceiversLTAddress"/>
	<xsl:param name="pSwiftCommonReferenceBankLeft"/>
	<xsl:param name="pSwiftCommonReferenceBankRight"/>
	
	<xsl:include href="../Lib_SWIFT.xslt"/>
	
	<!-- ******************* -->
	<!-- MAIN                -->
	<!-- ******************* -->
	<xsl:template match="/">		
		<xsl:call-template name="Swift-BasicHeader">
			<xsl:with-param name="pSendersLTAddress" select="$pSendersLTAddress" />
		</xsl:call-template>
		
		<xsl:call-template name="Swift-ApplicationHeader">
			<xsl:with-param name="pReceiversLTAddress" select="$pReceiversLTAddress" />
			<xsl:with-param name="pMessageType" select="'305'" />
		</xsl:call-template>
								
		<xsl:call-template name="MT305Fields">
			<xsl:with-param name="pPartyIDFrom" select="$pPartyIDFrom"/>  
			<xsl:with-param name="pPartyIDTo" select="$pPartyIDTo"/>  
		</xsl:call-template>								
	</xsl:template>


	<!-- ******************* -->
	<!-- Message SWIFT MT305 -->
	<!-- ******************* -->
	<xsl:template name="MT305Fields">
		<xsl:param name="pPartyIDFrom"/>  
		<xsl:param name="pPartyIDTo"/>  
		
		<xsl:text>&#xA;</xsl:text>

		<xsl:text>{4:</xsl:text>
		
		<!--:20:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:20:</xsl:text>
		<xsl:call-template name="Swift-GetReferenceNumber">
			<xsl:with-param name="pPartyID" select="$pPartyIDFrom"/>  
		</xsl:call-template>

		<!--:21:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:21:</xsl:text>
		<xsl:text>NEW</xsl:text>
		
		<!--:22:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:22:</xsl:text>
		<xsl:text>NEW/</xsl:text>
		<xsl:call-template name="Swift-CommonReference">
			<xsl:with-param name="pBicCodeBank1" select="$pSwiftCommonReferenceBankLeft"/>  
			<xsl:with-param name="pBicCodeBank2" select="$pSwiftCommonReferenceBankRight"/>  
      <xsl:with-param name="pSeparatorCode" select="//dataDocument/trade/fxSimpleOption/fxStrikePrice/rate"/>
      <xsl:with-param name="pCommonReferenceType" select="'NUMBER'"/>				
		</xsl:call-template>

		<!--:23:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:23:</xsl:text>
		<xsl:call-template name="Swift-GetBuySellFromPartyID">
			<xsl:with-param name="pProductNode" select="//dataDocument/trade/fxSimpleOption"/>  		
			<xsl:with-param name="pPartyID" select="$pPartyIDFrom"/>  
		</xsl:call-template>
		<xsl:text>/</xsl:text>
		<xsl:variable name="varCallPut">
			<xsl:call-template name="Swift-GetCallPut">
				<xsl:with-param name="pProductNode" select="//dataDocument/trade/fxSimpleOption"/>
			</xsl:call-template>	
		</xsl:variable>			
		<xsl:value-of select="$varCallPut"/>
		<xsl:text>/</xsl:text>
		<xsl:call-template name="Swift-GetExerciseStyle">
			<xsl:with-param name="pProductNode" select="//dataDocument/trade/fxSimpleOption"/>  		
		</xsl:call-template>		
		<xsl:text>/</xsl:text>
		<xsl:if test="$varCallPut = 'CALL'">
			<xsl:value-of select="//dataDocument/trade/fxSimpleOption/CallCurrencyAmount/currency"/>
		</xsl:if>
		<xsl:if test="$varCallPut = 'PUT'">
			<xsl:value-of select="//dataDocument/trade/fxSimpleOption/putCurrencyAmount/currency"/>
		</xsl:if>
				
		<!--:30:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:30:</xsl:text>
		<!--Formatage de la date yyyy-mm-jj se transforme en yymmjj-->		
		<xsl:call-template name="Swift-Date">
			<xsl:with-param name="pDate" select="//dataDocument/trade/tradeHeader/tradeDate"/>  
			<xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>				
			<xsl:with-param name="pDatePatternOut" select="'yymmjj'"/>				
		</xsl:call-template>

		<!--:31G:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:31G:</xsl:text>
		<xsl:call-template name="Swift-ExpiryDetails">
			<xsl:with-param name="pProductNode" select="//dataDocument/trade/fxSimpleOption"/>  		
		</xsl:call-template>
							
		<!--:26F:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:26F:</xsl:text>		
		<xsl:call-template name="Swift-GetSettlementType">
			<xsl:with-param name="pProductNode" select="//dataDocument/trade/fxSimpleOption"/>  		
		</xsl:call-template>
			
		<!--:32B:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:32B:</xsl:text>
		<xsl:if test="$varCallPut = 'CALL'">
			<xsl:value-of select="//dataDocument/trade/fxSimpleOption/callCurrencyAmount/currency"/>
			<xsl:call-template name="Swift-Amount">
				<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/callCurrencyAmount/amount" /> 
			</xsl:call-template>												
		</xsl:if>
		<xsl:if test="$varCallPut = 'PUT'">
			<xsl:value-of select="//dataDocument/trade/fxSimpleOption/putCurrencyAmount/currency"/>			
			<xsl:call-template name="Swift-Amount">
				<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/putCurrencyAmount/amount" /> 
			</xsl:call-template>												
		</xsl:if>
		
		<!--:36:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:36:</xsl:text>
		<xsl:call-template name="Swift-Rate">
			<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/fxStrikePrice/rate" /> 
		</xsl:call-template>									
									
		<!--:33B:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:33B:</xsl:text>
		<xsl:if test="$varCallPut = 'PUT'">
			<xsl:value-of select="//dataDocument/trade/fxSimpleOption/callCurrencyAmount/currency"/>
			<xsl:call-template name="Swift-Amount">
				<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/callCurrencyAmount/amount" /> 
			</xsl:call-template>												
		</xsl:if>
		<xsl:if test="$varCallPut = 'CALL'">
			<xsl:value-of select="//dataDocument/trade/fxSimpleOption/putCurrencyAmount/currency"/>			
			<xsl:call-template name="Swift-Amount">
				<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/putCurrencyAmount/amount" /> 
			</xsl:call-template>												
		</xsl:if>
									
		<!--:37K:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:37K:</xsl:text>
		<xsl:variable name="varPremiumPricePrefix">
			<xsl:call-template name="Swift-GetPremiumPricePrefix">
				<xsl:with-param name="pProductNode" select="//dataDocument/trade/fxSimpleOption" /> 
			</xsl:call-template>									
		</xsl:variable>			
		
		<xsl:if test="string-length($varPremiumPricePrefix) &gt; 1">
			<xsl:value-of select="$varPremiumPricePrefix"/>
		
			<xsl:call-template name="Swift-Rate">
				<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/fxOptionPremium/premiumQuote/premiumValue" /> 
			</xsl:call-template>														
		</xsl:if>

		<!--:34a:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:34</xsl:text>
		<xsl:if test="//dataDocument/trade/fxSimpleOption/fxOptionPremium/payerPartyReference/@href = $pPartyIDFrom ">
			<xsl:text>P:</xsl:text>	
		</xsl:if>
		<xsl:if test="//dataDocument/trade/fxSimpleOption/fxOptionPremium/receiverPartyReference/@href = $pPartyIDFrom ">
			<xsl:text>R:</xsl:text>	
		</xsl:if>
		<xsl:call-template name="Swift-Date">
			<xsl:with-param name="pDate" select="//dataDocument/trade/fxSimpleOption/fxOptionPremium/premiumSettlementDate"/>  
			<xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>				
			<xsl:with-param name="pDatePatternOut" select="'yymmjj'"/>				
		</xsl:call-template>
		<xsl:value-of select="//dataDocument/trade/fxSimpleOption/fxOptionPremium/premiumAmount/currency"/>
		<xsl:call-template name="Swift-Amount">
			<xsl:with-param name="pSource" select="//dataDocument/trade/fxSimpleOption/fxOptionPremium/premiumAmount/amount" /> 
		</xsl:call-template>												
 
		<!--:57a:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:57A:</xsl:text>
		<xsl:value-of select="$pPartyIDFrom"/>
									
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>-}</xsl:text>
		
	</xsl:template>
	
	
</xsl:stylesheet>