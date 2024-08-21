<?xml version="1.0" encoding="utf-8"?>
<!--
/*==============================================================
/* Name     : RiskData_MEFF_V9_ContrGrp_Map.xsl
/*==============================================================
/* Summary  : Import MEFF V9 Contract SubGroup
/*==============================================================
/* Version  : 3.3
/* Date     : 20130411
/* Author   : PM
/* Comment  : Mapping de l'importation des sous groupes d'asset
/*            utilisés par le calcul de déposit MEFFCOM2.
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
  <xsl:variable name="vXSLFileName">RiskData_MEFF_V9_ContrGrp_Map.xsl</xsl:variable>
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
    
    <r uc="true">
      <tbl n="IMMEFFCONTRGRP_H" a="IU">
        <c n="BUSINESSDATE" dk="true" t="dt" v="{./d[@n='BD']/@v}">
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
        <c n="SUBGROUPCODE" dk="true" v="{./d[@n='SubGrp']/@v}"/>
        <c n="SPOTCODE" dku="true" v="{./d[@n='SubGrpUnl']/@v}"/>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </r>
  </xsl:template>

</xsl:stylesheet>
