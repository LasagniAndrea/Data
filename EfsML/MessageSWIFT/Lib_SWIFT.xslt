<?xml version="1.0" encoding="UTF-8" ?>
<!--
=============================================================================================================================
Summary : Lib SWIFT
================================================================================================================
Version : v3.7.5155
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking
================================================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
				        xmlns:dt="http://xsltsl.org/date-time"
	      			xmlns:msxsl="urn:schemas-microsoft-com:xslt">       
				
	<xsl:output method="text" encoding='ISO-8859-1'/>

	<!-- ************************************************************************** -->
	<!-- Declaration des Variables													-->		
	<!-- ************************************************************************** -->
	<xsl:decimal-format name="SWIFT" decimal-separator=","  grouping-separator="." />	
	
	<xsl:variable name="swiftAmountPattern">
		<xsl:value-of select="'##############0,00#'"/>
	</xsl:variable>
	
	<xsl:variable name="swiftRatePattern">
		<xsl:value-of select="'######0,00########'"/>
	</xsl:variable>
	
	<!-- ************************************************************************** -->
	<!-- Définition du type de Product												-->		
	<!-- ************************************************************************** -->
	<xsl:variable name="isSwap" select="//dataDocument/trade/swap"/>	
	<xsl:variable name="isCap" select="( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule ) and not( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule )"/>	
	<xsl:variable name="isFloor" select="not( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule ) and ( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule )"/>	
	<xsl:variable name="isCollar" select="( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule ) and ( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule )"/>	

	
	<!-- ************************************************************************** -->
	<!-- Get Trade Stream															-->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetStream">
		<xsl:choose>
			<xsl:when test="$isSwap">
				<xsl:copy-of select="//dataDocument/trade/swap/swapStream/*" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="//dataDocument/trade/capFloor/capFloorStream/*" />			
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

		
	<!-- ************************************************************************** -->
	<!-- Template: Swift-GetMasterAgreementType                       							-->
	<!--																			                                      -->
	<!-- Return MasterAgreementType       											                    -->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetMasterAgreementType">
    <xsl:param name="pMasterAgreementAgreementType"/>
    <xsl:choose>
			<xsl:when test = "contains($pMasterAgreementAgreementType, 'ISDA')">
				<xsl:value-of select="'ISDA'"/>
			</xsl:when>
			<xsl:when test = "contains($pMasterAgreementAgreementType, 'AFB')">
        <xsl:value-of select="'AFB'"/>
			</xsl:when>
			<xsl:when test = "contains($pMasterAgreementAgreementType, 'BBAIRS')">
        <xsl:value-of select="'BBAIRS'"/>
			</xsl:when>
			<xsl:otherwise>
        <xsl:value-of select="$pMasterAgreementAgreementType"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<!-- ************************************************************************** -->
	<!-- Template: Get Cap, Floor, Collar Identification							              -->
	<!--																			                                      -->
	<!-- Return Ientification of Swap Code											                    -->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetCapFloorIdentification">		
		<!-- PartyA ( Buys ) en tant que payerPartyReference ou receiverPartyReference -->
		<xsl:if test="( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule/buyer='Payer' and //dataDocument/trade/capFloor/capFloorStream/payerPartyReference/@href=$pPartyIDFrom ) or
						( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule/buyer='Receiver' and //dataDocument/trade/capFloor/capFloorStream/receiverPartyReference/@href=$pPartyIDFrom )">
				<xsl:if test="$isCap">
					<xsl:text>CAPBUYER</xsl:text>
				</xsl:if>						
				<xsl:if test="$isFloor">
					<xsl:text>FLOORBUYER</xsl:text>
				</xsl:if>						
				<xsl:if test="$isCollar">
					<xsl:text>COLLARBYER</xsl:text>
				</xsl:if>						
		</xsl:if>

		<!-- PartyA ( Sells ) en tant que payerPartyReference ou receiverPartyReference -->
		<xsl:if test="( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule/seller='Payer' and //dataDocument/trade/capFloor/capFloorStream/payerPartyReference/@href=$pPartyIDFrom ) or				
						( //dataDocument/trade/capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule/seller='Receiver' and //dataDocument/trade/capFloor/capFloorStream/receiverPartyReference/@href=$pPartyIDFrom ) ">
				<xsl:if test="$isCap">
					<xsl:text>CAPSELLER</xsl:text>
				</xsl:if>						
				<xsl:if test="$isFloor">
					<xsl:text>FLOORSLLER</xsl:text>
				</xsl:if>						
				<xsl:if test="$isCollar">
					<xsl:text>COLLARSLLR</xsl:text>
				</xsl:if>						
		</xsl:if>			
	</xsl:template>

	<!-- ****************************************************************************************** -->
	<!-- Template: Get Swap type Float or Fixed														-->
	<!--																							-->
	<!-- Return String PartyAPayerFloat, PartyAPayerFixed, PartyAReceiverFloat, PartyAReceiverFixed	-->		
	<!-- ****************************************************************************************** -->
	<xsl:template name="Swift-GetSwapFloatFixed">
		<xsl:for-each select="//dataDocument/trade/swap/swapStream">
			<xsl:if test="boolean( $pPartyIDFrom = current()/payerPartyReference/@href ) and boolean( current()/resetDates )">
				<xsl:copy-of select="'PartyAPayerFloat'"/>
			</xsl:if>
			<xsl:if test="boolean( $pPartyIDFrom = current()/payerPartyReference/@href ) and not( boolean( current()/resetDates ) )">
				<xsl:copy-of select="'PartyAPayerFixed'"/>
			</xsl:if>
			<xsl:if test="boolean( $pPartyIDFrom = current()/receiverPartyReference/@href ) and boolean( current()/resetDates )">
				<xsl:copy-of select="'PartyAReceiverFloat'"/>
			</xsl:if>
			<xsl:if test="boolean( $pPartyIDFrom = current()/receiverPartyReference/@href ) and not( boolean( current()/resetDates ) )">
				<xsl:copy-of select="'PartyAReceiverFixed'"/>
			</xsl:if>
		</xsl:for-each>			
	</xsl:template>		

	<!-- ************************************************************************** -->
	<!-- Template: Get Premium Price Prefix											-->
	<!-- Parameter     : //dataDocument/trade/fxSimpleOption/									-->
	<!--																			-->
	<!-- Display Values: PCT or Currency											-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetPremiumPricePrefix">
		<xsl:param name="pProductNode"/>  
		
		<xsl:if test="contains($pProductNode/fxOptionPremium/premiumQuote/premiumQuoteBasis, 'PercentageOfPutCurrencyAmount') or contains($pProductNode/fxOptionPremium/premiumQuote/premiumQuoteBasis, 'PercentageOfCallCurrencyAmount') ">
			<xsl:copy-of select="'PCT'"/>
		</xsl:if>
		<xsl:if test="contains($pProductNode/fxOptionPremium/premiumQuote/premiumQuoteBasis, 'CallCurrencyPerPutCurrency')">
			<xsl:copy-of select="$pProductNode/callCurrencyAmount/currency"/>
		</xsl:if>
		<xsl:if test="contains($pProductNode/fxOptionPremium/premiumQuote/premiumQuoteBasis, 'PutCurrencyPerCallCurrency') ">
			<xsl:copy-of select="$pProductNode/putCurrencyAmount/currency"/>
		</xsl:if>
	</xsl:template>		

	<!-- ************************************************************************** -->
	<!-- Template: Get Type of Settlement											-->
	<!-- Parameter     : //dataDocument/trade/fxSimpleOption/									-->
	<!--																			-->
	<!-- Display Values: PRINCIPAL or NETCASH										-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetSettlementType">
		<xsl:param name="pProductNode"/>  
		
		<xsl:if test="$pProductNode/cashSettlementTerms">
			<xsl:text>NETCASH</xsl:text>	
		</xsl:if>
		<xsl:if test="not( $pProductNode/cashSettlementTerms )">
			<xsl:text>PRINCIPAL</xsl:text>	
		</xsl:if>
	</xsl:template>		
	
	<!-- ************************************************************************** -->
	<!-- Template: Get Type of American or European Style							-->
	<!-- Parameter     : //dataDocument/trade/fxSimpleOption/									-->
	<!--																			-->
	<!-- Display Values: A for American E for European								-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetExerciseStyle">
		<xsl:param name="pProductNode"/>  
		
		<xsl:if test="$pProductNode/exerciseStyle = 'American'">
			<xsl:text>A</xsl:text>	
		</xsl:if>
		<xsl:if test="$pProductNode/exerciseStyle = 'European'">
			<xsl:text>E</xsl:text>	
		</xsl:if>
	</xsl:template>		

	<!-- ************************************************************************** -->
	<!-- Template: Get Type of Call or Put											-->
	<!-- Parameter     : //dataDocument/trade/fxSimpleOption/									-->
	<!--																			-->
	<!-- Display Values: CALL or PUT												-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetCallPut">
		<xsl:param name="pProductNode"/>  
		
		<xsl:if test="not( $pProductNode/cashSettlementTerms )">
			<xsl:copy-of select="'PUT'"/>
		</xsl:if>
		<xsl:if test="$pProductNode/cashSettlementTerms/settlementCurrency = $pProductNode/putCurrencyAmount/currency">
			<xsl:copy-of select="'CALL'"/>
		</xsl:if>
		<xsl:if test="$pProductNode/cashSettlementTerms/settlementCurrency = $pProductNode/callCurrencyAmount/currency">
			<xsl:copy-of select="'PUT'"/>
		</xsl:if>		
	</xsl:template>		

	
	<!-- ************************************************************************** -->
	<!-- Template: Get From PartyID	Buyer or Seller Type	 						-->
	<!-- Parameter     : //dataDocument/trade/fxSimpleOption/									-->
	<!--																			-->
	<!-- Display Values: BUY or SELL												-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetBuySellFromPartyID">
		<xsl:param name="pProductNode"/>  
		<xsl:param name="pPartyID"/>  

		<xsl:if test="$pProductNode/buyerPartyReference/@href = $pPartyID ">
			<xsl:text>BUY</xsl:text>	
		</xsl:if>
		<xsl:if test="$pProductNode/sellerPartyReference/@href = $pPartyID ">
			<xsl:text>SELL</xsl:text>	
		</xsl:if>
	</xsl:template>		

	<!-- ************************************************************************** -->
	<!-- Swift-ExpiryDetails														-->
	<!--																			-->
	<!-- Parameter     : //dataDocument/trade/fxSimpleOption/									-->
	<!--																			-->
	<!-- Display Values: Date/Hour/BuinessCenter									-->		
	<!-- Display Format: yymmjj/hhmm/BuinessCenterCode								-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-ExpiryDetails">
		<xsl:param name="pProductNode"/>  
		
		<!--Expiry Date-->		
		<xsl:call-template name="Swift-Date">
			<xsl:with-param name="pDate" select="$pProductNode/expiryDateTime/expiryDate"/>  
			<xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>				
			<xsl:with-param name="pDatePatternOut" select="'yymmjj'"/>				
		</xsl:call-template>
		<xsl:text>/</xsl:text>
		
		<!--Expiry Time-->		
		<xsl:call-template name="Swift-Time">
			<xsl:with-param name="pTime" select="$pProductNode/expiryDateTime/expiryTime/hourMinuteTime"/>
			<xsl:with-param name="pTimePatternOut" select="'hhmm'"/>
		</xsl:call-template>
		<xsl:text>/</xsl:text>
		
		<!--Business Center-->		
		<xsl:value-of select="$pProductNode/expiryDateTime/expiryTime/businessCenter"/>
		
	</xsl:template>		
	
	<!-- ************************************************************************** -->
	<!-- Template: Get Reference Number	From PartyID							    -->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetReferenceNumber">
		<xsl:param name="pPartyID"/>  

		<xsl:for-each select="//dataDocument/trade/tradeHeader/partyTradeIdentifier/partyReference/@href">
			<xsl:if test="current() = $pPartyID ">
				<xsl:value-of select="//dataDocument/trade/tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']"/>
			</xsl:if>
		</xsl:for-each>			
	</xsl:template>		
	
	<!-- ************************************************************************** -->
	<!-- Delete Right Zero Number													-->
	<!-- ************************************************************************** -->
	<xsl:template name="SupRightZero">
		<xsl:param name="pNumber"/>  
		
		<xsl:if test="substring($pNumber,string-length($pNumber),1)='0'">				
			<xsl:call-template name="SupRightZero">
				<xsl:with-param name="pNumber" select="substring($pNumber,1, string-length($pNumber)-1)"/>
			</xsl:call-template>	
		</xsl:if>
		
		<xsl:if test="substring($pNumber,string-length($pNumber),1) != '0'">				
			<xsl:copy-of select="$pNumber"/>
		</xsl:if>		
	</xsl:template>		
		
	<!-- ************************************************************************** -->
	<!-- Swift Common Reference													    -->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-CommonReference">
		<xsl:param name="pBicCodeBank1"/>  
		<xsl:param name="pBicCodeBank2"/>  
		<xsl:param name="pSeparatorCode"/>  
		<xsl:param name="pCommonReferenceType"/> 
		

		<xsl:if test="$pCommonReferenceType = 'NUMBER'">				

			<!-- Recherche si le nombre est un decimal -->
			<xsl:if test="contains($pSeparatorCode,'.')">				
			
				<!-- Traitement partie decimal -->
				<xsl:variable name="varPartieDecimal">
						<xsl:value-of select="substring-after($pSeparatorCode,'.')"/>
				</xsl:variable>
				<xsl:variable name="varPartieDecimal2" select="normalize-space( $varPartieDecimal )"/>
			
				<!-- Traitement partie entière -->
				<xsl:variable name="varPartieEntier">
					<xsl:value-of select="substring-before($pSeparatorCode,'.')"/>
				</xsl:variable>
				<xsl:variable name="varPartieEntier2" select="number( $varPartieEntier ) + 10000000 "/>		

				<!-- Traitement Fusion de la partie entière et la partie decimal sans la virgule -->
				<xsl:variable name="varPartieEntier3" select="concat( $varPartieEntier2, $varPartieDecimal2 ) "/>		

				<!-- Traitement de suppression des zeros à droite du nombre -->
				<xsl:variable name="varNumberWithoutRightZero">
					<xsl:call-template name="SupRightZero">
						<xsl:with-param name="pNumber" select="$varPartieEntier3"/>
					</xsl:call-template>	
				</xsl:variable>			

				<!-- On ne garde que 4 chiffre en partant de la droite pour former le Séparator Code -->
				<xsl:variable name="varSeparatorCode" select="substring($varNumberWithoutRightZero, string-length($varNumberWithoutRightZero)-3, 4 )"/>		
				
				<!-- Common Reference Code -->
				<xsl:value-of select="concat( $pBicCodeBank1, $varSeparatorCode, $pBicCodeBank2 ) "/>	
				
			</xsl:if>
			
			
			<!-- Si le nombre est un entier sans partie decimale -->
			<xsl:if test="not( contains($pSeparatorCode,'.'))">				
				
				<xsl:variable name="varEntier" select="number( $pSeparatorCode ) + 10000000 "/>		
				
				<!-- Traitement de suppression des zeros à droite du nombre -->
				<xsl:variable name="varNumberWithoutRightZeros">
					<xsl:call-template name="SupRightZero">
						<xsl:with-param name="pNumber" select="$varEntier"/>
					</xsl:call-template>	
				</xsl:variable>			
				
				<!-- On ne garde que 4 chiffre en partant de la droite pour former le Séparator Code -->
				<xsl:variable name="varSeparatorCodes" select="substring($varNumberWithoutRightZeros, string-length($varNumberWithoutRightZeros)-3, 4 )"/>		
				
				<!-- Traitement Resultant du Common Reference Code -->
				<xsl:value-of select="concat( $pBicCodeBank1, $varSeparatorCodes, $pBicCodeBank2 ) "/>	
				
			</xsl:if>

		</xsl:if>
		
		<xsl:if test="$pCommonReferenceType = 'DATE'">				
			<!-- Traitement Resultant du Common Reference Code -->
			<xsl:value-of select="concat( $pBicCodeBank1, concat( substring($pSeparatorCode, 3,2 ), substring($pSeparatorCode, 6,2 ) ), $pBicCodeBank2 ) "/>	
		</xsl:if>
		
	</xsl:template>		

	<!-- ************************************************************************** -->
	<!-- Fonction de recherche d'un code BIC									    -->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-GetCodeBic">
		<xsl:param name="pActorNode"/>  

		<!-- Recherche s'il existe un code BIC -->
		<xsl:for-each select="$pActorNode/routingIds/routingId">
			<xsl:if test="current()/@routingIdCodeScheme = 'http://www.euro-finance-systems.fr/otcml/actorBic' ">				
				<xsl:copy-of select="current()"/>
			</xsl:if>
		</xsl:for-each>			
	</xsl:template>		

	<!-- ************************************************************************** -->
	<!-- Fonction de gestion d'affichage d'un acteur								-->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-DisplayActor"> 
		<xsl:param name="pIdFields"/>        <!-- 56, 57, 58-->
		<xsl:param name="pActorNode"/>  
		
		<!-- Test de l'existance du code BIC -->
		<xsl:variable name="codeBic">
			<xsl:call-template name="Swift-GetCodeBic">
				<xsl:with-param name="pActorNode" select="$pActorNode"/>
			</xsl:call-template>	
		</xsl:variable>			
		
		<!-- S'il existe un code BIC alors on génère un Tag A sinon un Tag B-->
		<xsl:if test="string-length($codeBic) &gt;= 1 ">
			<xsl:text>&#xA;</xsl:text>
			<xsl:text>:</xsl:text>
			<xsl:value-of select="$pIdFields"/>
			<xsl:text>A:</xsl:text>
			<xsl:value-of select="$codeBic"/>
		</xsl:if>		
		
		<!-- Tag B le fait de l'absence du code BIC on affichera à la place les info des adresses -->
		<xsl:if test="string-length($codeBic) = 0 ">
			<xsl:text>&#xA;</xsl:text>
			<xsl:text>:</xsl:text>
			<xsl:value-of select="$pIdFields"/>
			<xsl:text>D:</xsl:text>
			
			<!-- S'il n'existe pas d'adresse paramétrée alors on affiche routingName -->
			<xsl:if test="not($pActorNode/routingAddress/streetAddress/streetLine)">
				<xsl:value-of select="$pActorNode/routingName | 
                              $pActorNode/routingName"/>
			</xsl:if>				
			
			<!-- S'il existe une adresse paramétrée alors on affiche l'adresse -->
			<xsl:if test="$pActorNode/routingAddress/streetAddress/streetLine or $pActorNode/routingAddress/streetAddress/streetLine">
				<xsl:for-each select="$pActorNode/routingAddress/streetAddress/streetLine | 
                              $pActorNode/routingAddress/streetAddress/streetLine">
					<xsl:if test="position() &gt; 1">
						<xsl:text>&#xA;</xsl:text>
					</xsl:if>
					<xsl:value-of select="current()"/>
				</xsl:for-each>
			</xsl:if>				
			
		</xsl:if>				
	</xsl:template>	
	
	<!-- ************************************************************************** -->
	<!-- Swift-Date																	-->
	<!-- Formatage de la date														-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-Date">
		<xsl:param name="pDate"/>
		<xsl:param name="pDatePatternIn"/>
		<xsl:param name="pDatePatternOut"/>
		
		<xsl:choose>
			<xsl:when test = "$pDatePatternIn = 'yyyymmjj'">
				<xsl:choose>
					<xsl:when test = "$pDatePatternOut = 'yymmjj'">
						<xsl:value-of select="substring( $pDate, 3, 2 )"/>
						<xsl:value-of select="substring( $pDate, 5, 2 )"/>
						<xsl:value-of select="substring( $pDate, 7, 2 )"/>	
					</xsl:when>
				</xsl:choose>
			</xsl:when>
			<xsl:when test = "$pDatePatternIn = 'yyyy-mm-jj'">
				<xsl:choose>
					<xsl:when test = "$pDatePatternOut = 'yyyymmjj'">
						<xsl:value-of select="substring( $pDate, 1, 4 )"/>
						<xsl:value-of select="substring( $pDate, 6, 2 )"/>
						<xsl:value-of select="substring( $pDate, 9, 2 )"/>	
					</xsl:when>
					<xsl:when test = "$pDatePatternOut = 'yymmjj'">
						<xsl:value-of select="substring( $pDate, 3, 2 )"/>
						<xsl:value-of select="substring( $pDate, 6, 2 )"/>
						<xsl:value-of select="substring( $pDate, 9, 2 )"/>	
					</xsl:when>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'Date Pattern Not Found !'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>	
	
	<!-- ************************************************************************** -->
	<!-- Swift-Time																	-->
	<!-- Formatage de l'heure														-->		
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-Time">
		<xsl:param name="pTime"/>
		<xsl:param name="pTimePatternOut"/>
		
		<xsl:choose>
			<xsl:when test = "$pTimePatternOut = 'hhmmss'">
				<xsl:value-of select="substring( $pTime, 1, 2 )"/>
				<xsl:value-of select="substring( $pTime, 4, 2 )"/>
				<xsl:value-of select="substring( $pTime, 7, 2 )"/>
			</xsl:when>
			<xsl:when test = "$pTimePatternOut = 'hhmm'">
				<xsl:value-of select="substring( $pTime, 1, 2 )"/>
				<xsl:value-of select="substring( $pTime, 4, 2 )"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'Time Pattern Not Ready Yet !'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<!-- ************************************************************************** -->
	<!-- Swift-Amount																-->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-Amount">
		<xsl:param name="pSource"/>
		<xsl:value-of select='format-number($pSource, $swiftAmountPattern,"SWIFT")'/>
	</xsl:template>	

	<!-- ************************************************************************** -->
	<!-- Swift-Rate																-->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-Rate">
		<xsl:param name="pSource"/>
		<xsl:value-of select='format-number($pSource, $swiftRatePattern,"SWIFT")'/>
	</xsl:template>	
	

	<!-- ************************************************************************** -->
	<!-- BasicHeader du message SWIFT												-->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-BasicHeader">
		<xsl:param name="pSendersLTAddress"/>  
		
		<xsl:variable name="varBlocIdentifier" select="'1'"/>
		<xsl:variable name="varApplicationIdentifier" select="'F'"/>
		<xsl:variable name="varServiceIdentifier" select="'01'"/>
		<xsl:variable name="varSessionNumber" select="'0000'"/>
		<xsl:variable name="varSequenceNumber" select="'000000'"/>
		
		<xsl:text>{</xsl:text>
			<xsl:value-of select="$varBlocIdentifier"/><xsl:text>:</xsl:text>
			<xsl:value-of select="$varApplicationIdentifier"/>
			<xsl:value-of select="$varServiceIdentifier"/>
			<xsl:value-of select="$pSendersLTAddress"/>
			<xsl:value-of select="$varSessionNumber"/>
			<xsl:value-of select="$varSequenceNumber"/>
		<xsl:text>}</xsl:text>
	</xsl:template>
	
	<!-- ************************************************************************** -->
	<!-- ApplicationHeader du message SWIFT											-->
	<!-- ************************************************************************** -->
	<xsl:template name="Swift-ApplicationHeader">
		<xsl:param name="pReceiversLTAddress"/>  
		<xsl:param name="pMessageType"/>  <!-- 202, 210, 300, ... -->
		
		<xsl:variable name="varBlocIdentifier" select="'2'"/>
		<xsl:variable name="varInputOutputIdentifier" select="'I'"/>
		<xsl:variable name="varMessageType" select="$pMessageType"/>
		<xsl:variable name="varMessagePriority" select="'N'"/>
		
		<xsl:text>{</xsl:text>
			<xsl:value-of select="$varBlocIdentifier"/><xsl:text>:</xsl:text>
			<xsl:value-of select="$varInputOutputIdentifier"/>
			<xsl:value-of select="$varMessageType"/>
			<xsl:value-of select="$pReceiversLTAddress"/>
			<xsl:value-of select="$varMessagePriority"/>
		<xsl:text>}</xsl:text>
	</xsl:template>

</xsl:stylesheet>