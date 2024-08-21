<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
				        xmlns:dt="http://xsltsl.org/date-time">

	<!-- 20080103 PL Add du paramètre suivant, car attendu par Lib_SWIFT.xslt, et donc pb avec .NET 2.0 -->
	<xsl:param name="pPartyIDFrom"/>

	<xsl:include href="..\Lib_SWIFT.xslt"/>
	
	<!-- ******************* -->
	<!-- MAIN                -->
	<!-- ******************* -->	
	<xsl:template match="/">			
		<xsl:call-template name="Swift-BasicHeader">
			<xsl:with-param name="pSendersLTAddress" 
				select="//header/sender/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorLTAdress']"/>
		</xsl:call-template>
		
		<xsl:call-template name="Swift-ApplicationHeader">
			<xsl:with-param name="pReceiversLTAddress" 
			select="//header/receiver/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorLTAdress']"/>
			<xsl:with-param name="pMessageType" select="'210'" />
		</xsl:call-template>
		
		<xsl:call-template name="MT210Fields"/>
	</xsl:template>		

	<!-- ******************* -->
	<!-- Message SWIFT MT210 -->
	<!-- ******************* -->
	<xsl:template name="MT210Fields">
		<xsl:text>{4:</xsl:text>
		
		<!--:20:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:20:</xsl:text>
		<xsl:value-of select="//header/@OTCmlId"/>
		
		<!--:30:-->
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>:30:</xsl:text>
		<!--transaction valueDate-->
		<xsl:call-template name="Swift-Date">
			<xsl:with-param name="pDate" select="//payment/valueDate"/>  
			<xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>				
			<xsl:with-param name="pDatePatternOut" select="'yymmjj'"/>				
		</xsl:call-template> 


		<!--Boucle pour n payment si cela existe -->
		<xsl:for-each select="//payment">
			
			<!--:21:-->
			<xsl:text>&#xA;</xsl:text>
			<xsl:text>:21:</xsl:text>
			<xsl:value-of select="current()/receiver/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']"/>
			
			<!--:32B:-->
			<xsl:text>&#xA;</xsl:text>
			<xsl:text>:32B:</xsl:text>
			<!--transaction currency-->
			<xsl:value-of select="current()/paymentAmount/currency"/>
			<!--transaction amount-->
			<xsl:call-template name="Swift-Amount">
				<xsl:with-param name="pSource" select="current()/paymentAmount/amount"/>
			</xsl:call-template>		
			
			<!--:52a:-->
			<xsl:call-template name="Swift-DisplayActor">
				<xsl:with-param name="pIdFields" select="52" />
				<xsl:with-param name="pActorNode" select="current()/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails"/>
			</xsl:call-template>

			<!--:56a:-->
			<xsl:if test="count( current()/payer/settlementInstruction/correspondentInformation ) >= 1 ">
				<xsl:call-template name="Swift-DisplayActor">
					<xsl:with-param name="pIdFields" select="56" />
					<xsl:with-param name="pActorNode" select="current()/payer/settlementInstruction/correspondentInformation/routingIdsAndExplicitDetails"/>
				</xsl:call-template>
			</xsl:if>
			
		</xsl:for-each>			
		
		
		<xsl:text>&#xA;</xsl:text>
		<xsl:text>-}</xsl:text>
	</xsl:template>

</xsl:stylesheet>