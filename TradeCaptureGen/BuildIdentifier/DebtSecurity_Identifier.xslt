<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;" />
  <xsl:param name="pCurrentCulture" select="'en-GB'" />
  <xsl:param name="pProduct" />
  <!--<xsl:param name="pInstrument" select="'BOND'"/>-->
  <xsl:param name="pInstrument" />
  <xsl:param name="pIdStEnvironment" />
  <!-- xslt includes -->
  <xsl:include href="..\Library\DtFunc.xslt"/>
  <xsl:include href="..\Library\Resource.xslt"/>
  <xsl:include href="..\Library\StrFunc.xslt"/>
  <xsl:include href="..\Library\xsltsl\date-time.xsl"/>
  <!-- Begin region: instrument or class -->
  <xsl:variable name="varClassIdentificationScheme">http://www.euro-finance-systems.fr/otcml/debtSecurityClass</xsl:variable>
  <!-- PL 20190709 New variable $varInstrument -->
  <xsl:variable name="varInstrument">
    <xsl:choose>
      <xsl:when test="substring($pInstrument,1,4) = 'PERP'">
        <xsl:value-of select="substring-after( $pInstrument, 'PERP' )" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pInstrument" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="varClassorInstrument">
    <xsl:choose>
      <xsl:when test="normalize-space(/EfsML/trade/debtSecurity/security/classification/debtSecurityClass[@identificationScheme=$varClassIdentificationScheme])">
        <xsl:value-of select="/EfsML/trade/debtSecurity/security/classification/debtSecurityClass[@identificationScheme=$varClassIdentificationScheme]" />
      </xsl:when>
      <!-- if  class does not exist, get the instrument -->
      <xsl:otherwise>
        <xsl:value-of select="$varInstrument" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- End region: instrument or class -->
  <!-- Begin region: actor identifier -->
  <xsl:variable name="varIssuerHref">
    <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/payerPartyReference/@href" />
  </xsl:variable>
  <xsl:variable name="varActorBicScheme">http://www.euro-finance-systems.fr/otcml/actorBic</xsl:variable>
  <xsl:variable name="varActorIdentifierScheme">http://www.euro-finance-systems.fr/otcml/actorIdentifier</xsl:variable>
  <xsl:variable name="varActorIso18773Part1Scheme">http://www.euro-finance-systems.fr/otcml/actorIso18773Part1</xsl:variable>
  <xsl:variable name="varInstrumentIdSchemeISIN">http://www.euro-finance-systems.fr/spheres-enum/FIX/SecurityIDSource?V=4&amp;EV=ISIN</xsl:variable>
  <!-- This is the variable to be used (varActorId)-->
  <xsl:variable name="varLongActorId">
    <xsl:choose>
      <xsl:when test="normalize-space(/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorBicScheme])">
        <xsl:value-of select="/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorBicScheme]" />
      </xsl:when>
      <!-- if the BIC-code does not exist, get the first-4 characters of the actors identifier -->
      <xsl:when test="normalize-space(/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorIdentifierScheme])">
        <xsl:value-of select="/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorIdentifierScheme]" />
      </xsl:when>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="varShortActorId">
    <xsl:choose>
      <xsl:when test="normalize-space(/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorIso18773Part1Scheme])">
        <xsl:value-of select="/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorIso18773Part1Scheme]" />
      </xsl:when>
      <!-- if the iso identifier does not exist, get the first-4 characters of the BIC code -->
      <xsl:when test="normalize-space(/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorBicScheme])">
        <xsl:value-of select="substring( /EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorBicScheme], 1, 4)" />
      </xsl:when>
      <!-- if the BIC-code does not exist, get the first-4 characters of the actors identifier -->
      <xsl:when test="normalize-space(/EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorIdentifierScheme])">
        <xsl:value-of select="substring( /EfsML/party[@id=$varIssuerHref]/partyId[@partyIdScheme=$varActorIdentifierScheme], 1, 4)" />
      </xsl:when>
    </xsl:choose>
  </xsl:variable>
  <!-- End region: actor identifier -->
  <xsl:variable name="varSecurityCurrency">
    <xsl:value-of select="EfsML/trade/debtSecurity/security/currency" />
  </xsl:variable>
  <xsl:variable name="varFixedRateValue">
    <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/calculationPeriodAmount/calculation/fixedRateSchedule/initialValue" />
  </xsl:variable>
  <!-- Begin region: floating stream info -->
  <xsl:variable name="varFloatingRateIndex">
    <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/calculationPeriodAmount/calculation/floatingRateCalculation/floatingRateIndex" />
  </xsl:variable>
  <!-- 
		"EUR-EONIA-OIS-COMPOUND" -> "EONIA-OIS", 
		"USD-OIS-COMPOUND"       -> "OIS", 
		"EUR-EURIBOR-BBA"        -> "EURIBOR", 
		"GBP-LIBOR-BBA"          -> "LIBOR",  
	-->
  <xsl:variable name="varShortFloatingRateIndexCode">
    <xsl:choose>
      <xsl:when test="contains( substring-after( substring-after( $varFloatingRateIndex, '-'), '-'), 'OIS')">
        <xsl:value-of select="substring-before( substring-after( $varFloatingRateIndex, '-'), 'OIS')" />
        <xsl:text>OIS</xsl:text>
      </xsl:when>
      <xsl:when test="contains( substring-after( $varFloatingRateIndex, '-'), '-')">
        <xsl:value-of select="substring-before( substring-after( $varFloatingRateIndex, '-'), '-')" />
      </xsl:when>
      <xsl:when test="contains( $varFloatingRateIndex, '-')">
        <xsl:value-of select="substring-after( $varFloatingRateIndex, '-')" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$varFloatingRateIndex" />
      </xsl:otherwise>
    </xsl:choose>
    <!-- <xsl:value-of select="substring( $varFloatingRateIndex, 1, 10)"/>-->
  </xsl:variable>
  <xsl:variable name="varPeriodMultiplier">
    <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/calculationPeriodAmount/calculation/floatingRateCalculation/indexTenor/periodMultiplier" />
  </xsl:variable>
  <xsl:variable name="varPeriod">
    <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/calculationPeriodAmount/calculation/floatingRateCalculation/indexTenor/period" />
  </xsl:variable>
  <xsl:variable name="varSpread">
    <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/calculationPeriodAmount/calculation/floatingRateCalculation/spreadSchedule/initialValue" />
  </xsl:variable>
  <!-- End region: floating stream info -->
  <xsl:variable name="varFaceRate">
    <xsl:choose>
      <!-- Display fixed rate information (if it a fixed rate stream) -->
      <xsl:when test="normalize-space( $varFixedRateValue )">
        <xsl:call-template name="format-fixed-rate2">
          <xsl:with-param name="fixed-rate" select="$varFixedRateValue" />
        </xsl:call-template>
      </xsl:when>
      <!-- Display floating rate information (if it a floating rate stream) -->
      <xsl:when test="normalize-space($varFloatingRateIndex)">
        <xsl:value-of select="substring( $varShortFloatingRateIndexCode, 1, 10)" />
        <!-- Display tenor (3M, 6M, etc. )-->
        <xsl:if test="normalize-space($varPeriodMultiplier)">
          <xsl:value-of select="$varPeriodMultiplier" />
          <xsl:value-of select="$varPeriod" />
        </xsl:if>
        <!-- Display spread (+0.25%, -0,25%) -->
        <xsl:if test="normalize-space($varSpread)">
          <xsl:if test="normalize-space($varSpread)">
            <xsl:if test="substring( $varSpread, 1, 1) != '-'">
              <xsl:text>+</xsl:text>
            </xsl:if>
            <xsl:call-template name="format-fixed-rate2">
              <xsl:with-param name="fixed-rate" select="$varSpread" />
            </xsl:call-template>
          </xsl:if>
        </xsl:if>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="varTerminationDate">
    <!-- PL 20190709 Add CHOOSE and TEST on Perpetual instrument -->
    <!-- EG 20190823 [FIXEDINCOME] Gestion Libellé pour Perpetual DebtSecurity -->
    <xsl:choose>
      <xsl:when test="EfsML/trade/debtSecurity/debtSecurityType = 'Perpetual'">
        <xsl:value-of select="'Perpetual'" />
      </xsl:when>
      <xsl:when test="substring($pInstrument,1,4) = 'PERP'">
        <xsl:text>Perpetual</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="EfsML/trade/debtSecurity/debtSecurityStream[1]/calculationPeriodDates/terminationDate/unadjustedDate" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="varIdentifier">
    <!-- Display instrument-->
    <xsl:value-of select="$varInstrument" />
    <xsl:text> </xsl:text>
    <!-- Display actor identifier -->
    <xsl:value-of select="$varShortActorId" />
    <xsl:text> </xsl:text>
    <xsl:value-of select="$varFaceRate" />
    <xsl:text> </xsl:text>
    <!-- Display termination date (yyyy-MM-dd, 2017-10-25) -->
    <xsl:value-of select="$varTerminationDate" />
    <xsl:text> </xsl:text>
    <!-- Display currency -->
    <xsl:value-of select="$varSecurityCurrency" />
  </xsl:variable>
  <xsl:variable name="varDisplayName">
    <!-- Display instrument-->
    <xsl:value-of select="$varClassorInstrument" />
    <xsl:text> </xsl:text>
    <!-- Display actor identifier -->
    <xsl:value-of select="$varLongActorId" />
    <xsl:text> </xsl:text>
    <xsl:value-of select="$varFaceRate" />
    <xsl:text> </xsl:text>
    <!-- Display termination date (yyyy-MM-dd, 2017-10-25) -->
    <xsl:value-of select="$varTerminationDate" />
    <!-- Additional information for DisplayName (Currency and ISIN, ...) -->
    <xsl:text> </xsl:text>
    <xsl:value-of select="/EfsML/trade/debtSecurity/security/currency" />
  </xsl:variable>
  <xsl:variable name="varDescription">
    <!-- Display instrument-->
    <xsl:value-of select="$varInstrument" />
    <xsl:text> </xsl:text>
    <!-- Display actor identifier -->
    <xsl:value-of select="$varLongActorId" />
    <xsl:text> </xsl:text>
    <xsl:value-of select="$varFaceRate" />
    <xsl:text> </xsl:text>
    <!-- Display termination date (yyyy-MM-dd, 2017-10-25) -->
    <xsl:value-of select="$varTerminationDate" />
    <!-- Additional information for Description (Currency and ISIN, ...) -->
    <xsl:text> </xsl:text>
    <xsl:value-of select="/EfsML/trade/debtSecurity/security/currency" />
    <xsl:text> </xsl:text>
    <xsl:choose>
      <xsl:when test="normalize-space(/EfsML/trade/debtSecurity/security/instrumentId[@instrumentIdScheme=$varInstrumentIdSchemeISIN])">
        <xsl:value-of select="/EfsML/trade/debtSecurity/security/instrumentId[@instrumentIdScheme=$varInstrumentIdSchemeISIN]" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/EfsML/trade/debtSecurity/security/instrumentId" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:template match="/">
    <TradeIdentification>
      <Identifier>
        <xsl:value-of select="substring( $varIdentifier, 1, 64)" />
      </Identifier>
      <DisplayName>
        <xsl:value-of select="substring( $varDisplayName, 1, 64)" />
      </DisplayName>
      <Description>
        <xsl:value-of select="substring( $varDescription, 1, 128)" />
      </Description>
      <ExtlLink />
    </TradeIdentification>
  </xsl:template>
  <!-- no more used templates 	
	<xsl:template match="party">
    <xsl:apply-templates select="partyId[@partyIdScheme=$varActorIso18773Part1Scheme]"/>
	</xsl:template>
  
	<xsl:template match="partyId">
     <xsl:value-of select="."/>
	</xsl:template>-->
</xsl:stylesheet>