<!--
================================================================================================
Summary : CCeG - REPOSITORY
File    : MarketData_CCeG_Equities_Map.xsl
================================================================================================
Version : v3.2.0.0                                           
Date    : 20130429
Author  : CC
Comment : File MarketData_Idem_Equity_Import_Map.xsl renamed to MarketData_CCeG_Equities_Map.xsl
================================================================================================
Version : v1.0.0.0                                           
Date    : 20110215                                           
Author  : Guido  
================================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>
  <xsl:include href="MarketData_Common_SQL.xsl"/>
  <!-- <xsl:include href="MarketData_ItalianMarkets-Shared.xsl"/> -->

  <!-- Exchange symbol for Italian Markets -->
  <xsl:variable name="gIDEM_ExchangeSymbol" select="'2'"/>
  <xsl:variable name="gEQUITY_ExchangeSymbol" select="'3'"/>
  <xsl:variable name="gBorsaItalianaMarketExchangeSymbol">
    <xsl:value-of select="'3'"/>
  </xsl:variable>

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

    <!-- =================================================================== -->
    <!--                    Import of Equities (file Class File Extended)    -->
    <!--                                                                     -->
    <!-- 1. Create Equities (table ASSET_EQUITY)                             -->
    <!-- =================================================================== -->

    <!-- BD 20130513 Modifications de la valeur des paramètres envoyés au template SQLTableASSET_EQUITY -->

    <xsl:variable name="vSymbol" select="normalize-space(data[@name='SY'])"/>
    <xsl:variable name="vIsincode" select="normalize-space(data[@name='UI'])"/>
    <xsl:variable name="vISO10383">XMIL</xsl:variable>
    <xsl:variable name="vLibelle">
      <xsl:choose>
        <!-- Quand le libellé fait plus de 37 caractères, on le tronc et rajoute '...'  -->
        <xsl:when test="normalize-space(data[@name='DE']) > 37">
          <xsl:value-of select="concat(substring(normalize-space(data[@name='DE']),0,37),'...')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space(data[@name='DE'])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- BD 20130604 : Nouvelles variables -->
    <xsl:variable name="vExtSQLFilterValues" select="'KEY_OP'"/>
    <xsl:variable name="vExtSQLFilterNames" select="$gIdemSpecial"/>

    <!-- Finally we call the template to feed table ASSET_EQUITY -->
    <xsl:call-template name="SQLTableASSET_EQUITY">
      <xsl:with-param name="pSymbol"      select="$vSymbol"/>
      <xsl:with-param name="pIsincode"    select="$vIsincode"/>

      <!-- Currency -->
      <xsl:with-param name="pIdc">
        <!-- NB: We call the template because in the import file there is EU for EUR -->
        <xsl:call-template name="GetCurrency">
          <xsl:with-param name="pCurrency" select="normalize-space(data[@name='CU'])"/>
        </xsl:call-template>
      </xsl:with-param>

      <!-- Market -->
      <xsl:with-param name="pIso10383" select="$vISO10383"/>
      <xsl:with-param name="pExchangeSymbolRelated" select="$gIDEM_ExchangeSymbol"/>

      <!-- BD 20130604 : Appel du template SQLTableASSET_EQUITY avec les param pExtSQLFilterValues & pExtSQLFilterNames -->
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>

      <xsl:with-param name="pAssetTitled" select="$vLibelle"/>
    </xsl:call-template>

  </xsl:template>

</xsl:stylesheet>