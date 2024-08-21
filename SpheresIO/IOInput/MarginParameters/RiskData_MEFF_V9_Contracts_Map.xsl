<?xml version="1.0" encoding="utf-8"?>
<!--
/*==============================================================
/* Name     : RiskData_MEFF_V9_Contracts_Map.xsl
/*==============================================================
/* Summary  : Import MEFF V9 Contracts
/*==============================================================
/* Version  : 3.3
/* Date     : 20130411
/* Author   : PM
/* Comment  : Mapping de l'importation des paramètres des
/*            contrats utilisés par le calcul de déposit
/*            MEFFCOM2.
/*==============================================================
/* Revision : 
/* Date     : 
/* Author   : 
/* Version  : 
/* Comment  : 
/*==============================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes -->
  <xsl:include href=".\RiskData_MEFF_V9_Common_Map.xsl"/>

  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_MEFF_V9_Contracts_Map.xsl</xsl:variable>
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

    <!-- S'il existe au moins un enregistrement -->
    <xsl:if test="./r[1]">

      <!-- Insertion d'un enregistrement avec la date et le groupe de contrat -->
      <xsl:call-template name="IMMEFF_H">
        <xsl:with-param name="pBusinessDate" select="./r[1]/d[@n='BD']/@v"/>
        <xsl:with-param name="pContractGroup" select="./r[1]/d[@n='CtrGrp']/@v"/>
        <xsl:with-param name="pIOInputName" select="$vIOInputName"/>
      </xsl:call-template>

      <!-- Gestion des lignes suivantes -->
      <xsl:apply-templates select="./r">
      </xsl:apply-templates>
    </xsl:if>

  </xsl:template>

  <!-- ================================================================== -->
  <!-- ================== TEMPLATE PRINCIPALE R (ROW) =================== -->
  <xsl:template match="r">

    <xsl:variable name="vBusinessDate" select="./d[@n='BD']/@v"/>

    <r id="{@id}" src="{@src}" uc="true">
      
      <xsl:variable name="vIdAsset" select="./d[@n='IDASSET']/@v"/>
      <xsl:variable name="vAssetCategory" select="./d[@n='ASSETCAT']/@v"/>
      <xsl:variable name="vISINCode" select="./d[@n='ISIN']/@v"/>

      <tbl n="IMMEFFCONTRACT_H" a="IU">
        <c n="IDASSET" dku="true" t="i">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="$vIdAsset"/>
          </xsl:call-template>
        </c>
        <c n="ASSETCATEGORY" dku="true">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="$vAssetCategory"/>
          </xsl:call-template>
        </c>
        <c n="BUSINESSDATE" dk="true" t="dt" v="{$vBusinessDate}">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <xsl:choose>
                <xsl:when test="$vBusinessDate!=$vAskedBusinessDate">
                  <xsl:attribute name="v">true</xsl:attribute>
                  <li st="WARNING" ex="true">
                    <xsl:attribute name="msg">
                      BusinessDate: Different from asked date  (File:<xsl:value-of select="$vBusinessDate"/>, Asked:<xsl:value-of select="$vAskedBusinessDate"/>)
                    </xsl:attribute>
                  </li>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="v">false</xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </ctl>
          </ctls>
        </c>
        <c n="CONTRACTGROUP" dk="true" v="{./d[@n='CtrGrp']/@v}"/>
        <c n="ASSETCODE" dk="true" v="{./d[@n='ACd']/@v}"/>
        <c n="SUBGROUPCODE" dku="true" v="{./d[@n='SubGrp']/@v}"/>
        <c n="CONTRACTTYPECODE" dku="true" v="{./d[@n='TypCd']/@v}"/>
        <c n="STRIKEPRICE" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='StrkPrice']/@v"/>
          </xsl:call-template>
        </c>
        <c n="MATURITYDATE" dku="true" t="dt" v="{./d[@n='MatDt']/@v}"/>
        <c n="LASTTRADINGDAY" dku="true" t="dt" v="{./d[@n='LTD']/@v}"/>
        <c n="EXEUNLASSETCODE" dku="true" v="{./d[@n='ExeUnderCd']/@v}"/>
        <c n="MGRUNLASSETCODE" dku="true" v="{./d[@n='MrgUnderCd']/@v}"/>
        <c n="ARRAYCODE" dku="true" v="{./d[@n='ArrayCd']/@v}"/>
        <c n="EXPIRYSPAN" dku="true" v="{./d[@n='ExpirySpan']/@v}"/>
        <c n="MATURITYMONTHYEAR" dku="true" v="{./d[@n='MMY']/@v}"/>
        <c n="ISINCODE" dku="true" v="{$vISINCode}"/>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
      <!-- RD 20171009 [23258] Mise à jour code ISIN pour les assets ETD-->
      <xsl:if test="string-length($vIdAsset) > 0 
              and string-length($vISINCode) > 0 
              and (string-length($vAssetCategory) = 0 or $vAssetCategory = 'Future')">
        
        <tbl n="ASSET_ETD" a="U">
          <c n="IDASSET" dk="true" t="i" v="{$vIdAsset}"/>
          <c n="ISINCODE" dku="true" v="{$vISINCode}"/>
          <!-- InfoTable_U -->
          <xsl:call-template name="InfoTable_U"/>
        </tbl>
      </xsl:if>
    </r>
  </xsl:template>


</xsl:stylesheet>
