<?xml version="1.0" encoding="utf-8"?>
<!--
/*==============================================================
/* Name     : RiskData_MEFF_V9_ContrStat_Map.xsl
/*==============================================================
/* Summary  : Import MEFF V9 Contract daily data
/*==============================================================

FI 20140108 [19460] Les parameters SQL doivent être en majuscule 
PM 20130411 Mapping de l'importation des prix
-->


<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes -->
  <xsl:include href=".\RiskData_MEFF_V9_Common_Map.xsl"/>

  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_MEFF_V9_ContrStat_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v3.3.0.0</xsl:variable>
  <xsl:variable name ="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>

  <!-- FILE -->
  <xsl:template match="file">
    <file>
      <!-- CommonInput : IOFileAtt -->
      <xsl:call-template name="IOFileAtt"/>
      <!--  -->
      <xsl:call-template name="StartImport"/>
    </file>
  </xsl:template>

  <!-- Initialisation des variables provenant du fichier -->
  <!-- Lancement de l'import -->
  <xsl:template name="StartImport">
    <!-- Gestion des lignes suivantes -->
    <xsl:apply-templates select="./r">
    </xsl:apply-templates>
  </xsl:template>

  <!-- SQL de lecture d'informations sur l'Asset -->
  <xsl:template name="SqlAssetInformation">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pContractGroup"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetCategory"/>

    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <sql cd="select" cache="true">
      <xsl:attribute name="rt">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>

      select a.IDM, a.IDC, a.IDASSET, mc.ASSETCATEGORY
      from dbo.IMMEFFCONTRACT_H mc
      inner join dbo.ASSET_EQUITY a on (a.IDASSET = mc.IDASSET)
      where (mc.BUSINESSDATE = @BUSINESSDATE)
      and (mc.CONTRACTGROUP = @CONTRACTGROUP)
      and (mc.ASSETCODE = @ASSETCODE)
      and (mc.ASSETCATEGORY = @ASSETCATEGORY)
      union all
      select a.IDM, a.IDC, a.IDASSET, mc.ASSETCATEGORY
      from dbo.IMMEFFCONTRACT_H mc
      inner join dbo.ASSET_INDEX a on (a.IDASSET = mc.IDASSET)
      where (mc.BUSINESSDATE = @BUSINESSDATE)
      and (mc.CONTRACTGROUP = @CONTRACTGROUP)
      and (mc.ASSETCODE = @ASSETCODE)
      and (mc.ASSETCATEGORY = @ASSETCATEGORY)
      union all
      select dc.IDM, dc.IDC_PRICE as IDC, a.IDASSET, 'Future' as ASSETCATEGORY
      from dbo.IMMEFFCONTRACT_H mc
      inner join dbo.ASSET_ETD a on (a.IDASSET = mc.IDASSET)
      inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
      where (mc.BUSINESSDATE = @BUSINESSDATE)
      and (mc.CONTRACTGROUP = @CONTRACTGROUP)
      and (mc.ASSETCODE = @ASSETCODE)
      and (@ASSETCATEGORY = 'Future')
      and ((mc.ASSETCATEGORY is null) or (mc.ASSETCATEGORY = @ASSETCATEGORY))
      <p n="BUSINESSDATE" t="dt" f="yyyy-MM-dd" v="{$pBusinessDate}"/>
      <p n="CONTRACTGROUP" v="{$pContractGroup}"/>
      <p n="ASSETCODE" v="{$pAssetCode}"/>
      <p n="ASSETCATEGORY" v="{$pAssetCategory}"/>
    </sql>
  </xsl:template>

  <!-- ============== QUOTE_XXX_H ======================================= -->
  <!-- Insertion/Mise à jour d'une cotation dans les tables de quotation  -->
  <xsl:template name="QUOTE_XXX_H">
    <xsl:param name="pQuoteTable"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pContractGroup"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pPrice"/>
    <xsl:param name="pDelta" select="'null'"/>
    <xsl:param name="pVolatility" select="'null'"/>

    <xsl:if test="boolean($pQuoteTable)">

      <xsl:variable name="vAssetCategory">
        <xsl:choose>
          <xsl:when test="$pQuoteTable='QUOTE_EQUITY_H'">
            <xsl:value-of select="'EquityAsset'"/>
          </xsl:when>
          <xsl:when test="$pQuoteTable='QUOTE_INDEX_H'">
            <xsl:value-of select="'Index'"/>
          </xsl:when>
          <xsl:when test="$pQuoteTable='QUOTE_ETD_H'">
            <xsl:value-of select="'Future'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>

      <!-- Table QUOTE_XXX_H -->
      <tbl n="{$pQuoteTable}" a="IU">
        <!-- IDMARKETENV -->
        <c n="IDMARKETENV" dk="true" v="parameters.MarketEnv">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <!-- IDVALSCENARIO -->
        <c n="IDVALSCENARIO" dk="true" v="parameters.ValScenario">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <!-- IDASSET -->
        <c n="IDASSET" dk="true" t="i">
          <xsl:call-template name="SqlAssetInformation">
            <xsl:with-param name="pResultColumn" select="'IDASSET'"/>
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pContractGroup" select="$pContractGroup"/>
            <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
            <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <!-- IDC -->
        <c n="IDC" dku="true">
          <xsl:call-template name="SqlAssetInformation">
            <xsl:with-param name="pResultColumn" select="'IDC'"/>
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pContractGroup" select="$pContractGroup"/>
            <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
            <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
          </xsl:call-template>
        </c>
        <!-- IDM -->
        <c n="IDM" dku="true" t="i">
          <xsl:call-template name="SqlAssetInformation">
            <xsl:with-param name="pResultColumn" select="'IDM'"/>
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pContractGroup" select="$pContractGroup"/>
            <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
            <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
          </xsl:call-template>
        </c>
        <!-- TIME -->
        <c n="TIME" dk="true" t="dt" f="yyyy-MM-dd" v="{$pBusinessDate}">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <xsl:choose>
                <xsl:when test="$pBusinessDate!=$vAskedBusinessDate">
                  <xsl:attribute name="v">true</xsl:attribute>
                  <li st="WARNING" ex="true">
                    <xsl:attribute name="msg">
                      BusinessDate: Different from asked date  (File:<xsl:value-of select="$pBusinessDate"/>, Asked:<xsl:value-of select="$vAskedBusinessDate"/>)
                    </xsl:attribute>
                  </li>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="v">false</xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </ctl>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li s="ERROR" ex="true" msg="Invalid TIME"/>
            </ctl>
          </ctls>
        </c>
        <!-- VALUE -->
        <c n="VALUE" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="$pPrice"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <xsl:if test="$pQuoteTable='QUOTE_ETD_H'">
          <!-- DELTA -->
          <c n="DELTA" dku="true" t="dc">
            <xsl:call-template name="AttribValueOrNull">
              <xsl:with-param name="pValue" select="$pDelta"/>
            </xsl:call-template>
          </c>
          <!-- VOLATILITY -->
          <c n="VOLATILITY" dku="true" t="dc">
            <xsl:call-template name="AttribValueOrNull">
              <xsl:with-param name="pValue" select="$pVolatility"/>
            </xsl:call-template>
          </c>
        </xsl:if>
        <!-- . -->
        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </xsl:if>

  </xsl:template>

  <!-- ============== IMMEFFCONTRACT_H ================================== -->
  <!-- Mise à jour d'une cotation -->
  <xsl:template name="IMMEFFCONTRACT_H">
    <xsl:param name="pQuoteTable"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pContractGroup"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pPrice"/>

    <tbl n="IMMEFFCONTRACT_H" a="U">
      <c n="BUSINESSDATE" dk="true" t="dt" v="{$pBusinessDate}"/>
      <c n="CONTRACTGROUP" dk="true" v="{$pContractGroup}"/>
      <c n="ASSETCODE" dk="true" v="{$pAssetCode}"/>
      <c n="PRICE" dku="true" t="dc" v="{$pPrice}"/>
    </tbl>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- ================== TEMPLATE PRINCIPALE R (ROW) =================== -->
  <xsl:template match="r">

    <xsl:variable name="vBusinessDate" select="./d[@n='BD']/@v"/>

    <r uc="true">
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>

      <!-- Liste des parametres -->
      <pms>
        <!-- IDMARKETENV -->
        <pm n="MarketEnv" >
          <sql cd="select" rt="IDMARKETENV" cache="true">
            select IDMARKETENV
            from dbo.MARKETENV
            where ISDEFAULT = 1
          </sql>
        </pm>
        <!-- IDVALSCENARIO -->
        <pm n="ValScenario" >
          <sql cd="select" rt="IDVALSCENARIO" cache="true">
            select v.IDVALSCENARIO, 1 as colorder
            from dbo.VALSCENARIO v
            inner join dbo.MARKETENV m on ( m.IDMARKETENV = v.IDMARKETENV ) and ( m.ISDEFAULT = 1)
            where v.ISDEFAULT = 1
            union all
            select v.IDVALSCENARIO, 2 as colorder
            from dbo.VALSCENARIO v
            where v.ISDEFAULT = 1 and v.IDMARKETENV is null
            order by colorder asc
          </sql>
        </pm>
      </pms>

      <xsl:call-template name="IMMEFFCONTRACT_H">
        <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
        <xsl:with-param name="pContractGroup" select="./d[@n='CtrGrp']/@v"/>
        <xsl:with-param name="pAssetCode" select="./d[@n='ACd']/@v"/>
        <xsl:with-param name="pPrice" select="./d[@n='StlPrc']/@v"/>
      </xsl:call-template>

      <xsl:call-template name="QUOTE_XXX_H">
        <xsl:with-param name="pQuoteTable" select="'QUOTE_EQUITY_H'"/>
        <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
        <xsl:with-param name="pContractGroup" select="./d[@n='CtrGrp']/@v"/>
        <xsl:with-param name="pAssetCode" select="./d[@n='ACd']/@v"/>
        <xsl:with-param name="pPrice" select="./d[@n='StlPrc']/@v"/>
        <xsl:with-param name="pDelta" select="./d[@n='StlDelta']/@v"/>
        <xsl:with-param name="pVolatility" select="./d[@n='StlVolat']/@v"/>
      </xsl:call-template>

      <xsl:call-template name="QUOTE_XXX_H">
        <xsl:with-param name="pQuoteTable" select="'QUOTE_INDEX_H'"/>
        <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
        <xsl:with-param name="pContractGroup" select="./d[@n='CtrGrp']/@v"/>
        <xsl:with-param name="pAssetCode" select="./d[@n='ACd']/@v"/>
        <xsl:with-param name="pPrice" select="./d[@n='StlPrc']/@v"/>
        <xsl:with-param name="pDelta" select="./d[@n='StlDelta']/@v"/>
        <xsl:with-param name="pVolatility" select="./d[@n='StlVolat']/@v"/>
      </xsl:call-template>

      <xsl:call-template name="QUOTE_XXX_H">
        <xsl:with-param name="pQuoteTable" select="'QUOTE_ETD_H'"/>
        <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
        <xsl:with-param name="pContractGroup" select="./d[@n='CtrGrp']/@v"/>
        <xsl:with-param name="pAssetCode" select="./d[@n='ACd']/@v"/>
        <xsl:with-param name="pPrice" select="./d[@n='StlPrc']/@v"/>
        <xsl:with-param name="pDelta" select="./d[@n='StlDelta']/@v"/>
        <xsl:with-param name="pVolatility" select="./d[@n='StlVolat']/@v"/>
      </xsl:call-template>
    </r>
  </xsl:template>
</xsl:stylesheet>
