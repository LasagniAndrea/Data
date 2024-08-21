<!--
==============================================================================================
Summary : CCeG - REPOSITORY
File    : MarketData_CCeG_Series_Map.xsl
==============================================================================================
Version : v3.2.0.0                                           
Date    : 20130429
Author  : CC
Comment : File MarketData_Idem_Series_Import_Map.xsl renamed to MarketData_CCeG_Series_Map.xsl
==============================================================================================
Version : v3.2.0.0                                           
Date    : 20130220
Author  : PL
Comment : IDEX: Management of ContractMultiplier
==============================================================================================
Version : v1.0.0.0                                           
Date    : 20110407                                           
Author  : GP
==============================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>
  <xsl:include href="MarketData_Common_SQL.xsl"/>

  <xsl:variable name="gIDEX_ExchangeSymbol" select="'5'"/>

  <!--Main template  -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- Specific template-->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>
      <xsl:apply-templates select="row"/>
    </file>
  </xsl:template>

  <xsl:template match="row">
    <row useCache="false">
      <xsl:call-template name="IORowAtt"/>
      <xsl:call-template name="rowStream"/>
    </row>
  </xsl:template>

  <!-- Spécifique à chaque Import -->
  <xsl:template name="rowStream">
    <!-- ================================================================================ -->
    <!--                              Shared variables                                    -->
    <!-- These variables are used both by Derivative Contract section and Series section  -->
    <!-- ================================================================================ -->
    <xsl:variable name="vDerivativeContractIdentifier">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:variable name="vExchangeSymbol">
      <xsl:value-of select="normalize-space(data[@name='MI'])"/>
    </xsl:variable>

    <xsl:variable name="vDescription">
      <xsl:value-of select="normalize-space(data[@name='DE'])"/>
    </xsl:variable>

    <!-- F=Future;O=Option -->
    <!-- (NB.: C=Equity;CEF,ETF;W=Warrant;V=Convertible pour les autres marchés) -->
    <xsl:variable name="vClassType">
      <xsl:value-of select="normalize-space(data[@name='CT'])"/>
    </xsl:variable>

    <xsl:variable name="vSymbol">
      <xsl:value-of select="normalize-space(data[@name='SY'])"/>
    </xsl:variable>

    <xsl:variable name="vMultiplier">
      <xsl:choose>
        <!-- For IDEX only: Contract Multiplier -->
        <xsl:when test="$vExchangeSymbol=$gIDEX_ExchangeSymbol">
          <xsl:value-of select="normalize-space(data[@name='MU'])"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- ================================================================================ -->
    <!--                              Series variables  1                                 -->
    <!-- For passing data to template SQLTableMATURITY_DERIVATIVEATTRIB                   -->
    <!-- ================================================================================ -->
    <xsl:variable name="vMaturityMonthYear">
      <xsl:value-of select="normalize-space(data[@name='EX'])"/>
    </xsl:variable>

    <xsl:variable name="vMaturityDate">
      <xsl:value-of select="normalize-space(data[@name='ED'])"/>
    </xsl:variable>

    <xsl:variable name="vLastTradingDay">
      <xsl:value-of select="normalize-space(data[@name='LS'])"/>
    </xsl:variable>

    <xsl:variable name="vExtSQLFilterValues" select="concat ('KEY_OP',',',$vClassType)"/>
    <xsl:variable name="vExtSQLFilterNames"  select="concat ($gIdemSpecial,',','CATEGORY')"/>

    <!-- ================================================================================ -->
    <!--           Call shared template SQLTableMATURITY_DERIVATIVEATTRIB                 -->
    <!-- Tables MATURITY and DERIVATIVEATTRIB                                             -->
    <!-- ================================================================================ -->
    <!-- BD / FL 20130527 : Ne plus faire d'Insert/Update sur les tables MATURITY et DERIVATIVEATTRIB -->
    <!--<xsl:call-template name="SQLTableMATURITY_DERIVATIVEATTRIB">
      <xsl:with-param name="pExchangeSymbol">
        <xsl:value-of select="$vExchangeSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="$vSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pMaturityMonthYear">
        <xsl:value-of select="$vMaturityMonthYear"/>
      </xsl:with-param>
      <xsl:with-param name="pMaturityDate">
        <xsl:value-of select="$vMaturityDate"/>
      </xsl:with-param>
      --><!-- Not available --><!--
      <xsl:with-param name="pDeliveryDate"/>
      <xsl:with-param name="pLastTradingDay">
        <xsl:value-of select="$vLastTradingDay"/>
      </xsl:with-param>
      --><!-- Not available --><!--
      <xsl:with-param name="pUnderlyingAssetSymbol"/>
      --><!-- Not available --><!--
      <xsl:with-param name="pUnderlyingContractSymbol"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames"  select="$vExtSQLFilterNames"/>
    </xsl:call-template>-->

    <!-- ================================================================================ -->
    <!--                              Series variables II                                 -->
    <!-- For passing data to template SQLTableASSET_ETD                                   -->
    <!-- ================================================================================ -->
    <!-- ExchangeTradedOption, ExchangeTradedFuture -->
    <xsl:variable name="vAssetIdentifier">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:variable name="vAssetDisplayName">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:variable name="vCodeUsedByMarket">
      <xsl:value-of select="normalize-space(data[@name='CD'])"/>
    </xsl:variable>

    <xsl:variable name="vAssetISINCode">
      <xsl:value-of select="normalize-space(data[@name='IC'])"/>
    </xsl:variable>

    <xsl:variable name="vAIICode">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:variable name="vPutCall">
      <xsl:call-template name="CallPut">
        <xsl:with-param name="pCallPut">
          <xsl:value-of select="normalize-space(data[@name='PC'])"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vStrike">
      <xsl:value-of select="normalize-space(data[@name='SP'])"/>
    </xsl:variable>

    <!-- ================================================================================ -->
    <!--                    Call shared template SQLTableASSET_ETD                        -->
    <!-- Table ASSET_ETD                                                                  -->
    <!-- ================================================================================ -->
    <xsl:call-template name="SQLTableASSET_ETD">
      <xsl:with-param name="pExchangeSymbol">
        <xsl:value-of select="$vExchangeSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="$vSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pMaturityMonthYear">
        <xsl:value-of select="$vMaturityMonthYear"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetIdentifier">
        <xsl:value-of select="$vAssetIdentifier"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetDisplayName">
        <xsl:value-of select="$vAssetDisplayName"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetSymbol">
        <xsl:value-of select="$vCodeUsedByMarket"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetISINCode">
        <xsl:value-of select="$vAssetISINCode"/>
      </xsl:with-param>
      <xsl:with-param name="pAIICode">
        <xsl:value-of select="$vAIICode"/>
      </xsl:with-param>
      <!-- For IDEM the series multiplier is always = to Derivative Contract multiplier, except for IDEX -->
      <xsl:with-param name="pAssetMultiplier" select="$vMultiplier"/>
      <xsl:with-param name="pAssetFactor" select="$vMultiplier"/>
      <!-- PL 20130201: Bug - Missing pCategory -->
      <xsl:with-param name="pCategory">
        <xsl:value-of select="$vClassType"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetPutCall">
        <xsl:value-of select="$vPutCall"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetStrikePrice">
        <xsl:value-of select="$vStrike"/>
      </xsl:with-param>
      <xsl:with-param name="pContractAttribute"/>
      <!-- BD 20130521 pUpdateOnly=gTrue pour ne pas créer les asset -->
      <xsl:with-param name="pUpdateOnly" select="$gTrue"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames"  select="$vExtSQLFilterNames"/>
    </xsl:call-template>
  </xsl:template>
</xsl:stylesheet>
