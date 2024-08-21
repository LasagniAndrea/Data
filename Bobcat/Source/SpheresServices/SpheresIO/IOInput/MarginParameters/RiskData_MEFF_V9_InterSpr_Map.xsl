<?xml version="1.0" encoding="utf-8"?>
<!--
/*==============================================================
/* Name     : RiskData_MEFF_V9_InterSpr_Map.xsl
/*==============================================================
/* Summary  : Import MEFF V9 Inter-commodity spreads
/*==============================================================
/* Version  : 3.3
/* Date     : 20130411
/* Author   : PM
/* Comment  : Mapping de l'importation des paramètres de spreads
/*            inter-commodity utilisés par le calcul de déposit
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
  <xsl:variable name="vXSLFileName">RiskData_MEFF_V9_InterSpr_Map.xsl</xsl:variable>
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

  <!-- ================================================================== -->
  <!-- ================== TEMPLATE PRINCIPALE R (ROW) =================== -->
  <xsl:template match="r">

    <xsl:variable name="vBusinessDate" select="./d[@n='BD']/@v"/>

    <r uc="true">
      <tbl n="IMMEFFINTERSPR_H" a="IU">
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
        <c n="PRIORITY" dk="true" v="{./d[@n='OffPri']/@v}"/>
        <c n="ARRAYCODE1" dku="true" v="{./d[@n='ArrayCd1']/@v}"/>
        <c n="OFFSETDISCOUNT1" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='GrpOffDis1']/@v"/>
          </xsl:call-template>
        </c>
        <c n="OFFSETMULTIPLIER1" dku="true" t="i">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='OffMltp1']/@v"/>
          </xsl:call-template>
        </c>
        <c n="ARRAYCODE2" dku="true" v="{./d[@n='ArrayCd2']/@v}"/>
        <c n="OFFSETDISCOUNT2" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='GrpOffDis2']/@v"/>
          </xsl:call-template>
        </c>
        <c n="OFFSETMULTIPLIER2" dku="true" t="i">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='OffMltp2']/@v"/>
          </xsl:call-template>
        </c>
        <c n="DISCOUNTTYPE" dku="true" v="{./d[@n='DisTyp']/@v}"/>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </r>
  </xsl:template>
  
</xsl:stylesheet>
