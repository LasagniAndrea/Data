<?xml version="1.0" encoding="utf-8"?>
<!-- 
/*============================================================================
/* Name     : RiskData_ECC_MrkVol_Map.xsl
/*============================================================================
/* Summary  : Importation des données Market Volume nécessaires au calcul du 
/*            Concentraion Risk Margin de l'ECC
-->
<!-- PM 20190801 [24717] Création -->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>

  <!-- Variable d'identification de version -->
  <xsl:variable name="vXSLFileName">RiskData_ECC_MrkVol_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v8.1.0.0</xsl:variable>
  <!-- Variable d'identification du fichier -->
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>
  <!-- Date de bourse pour laquelle on veux importer les données -->
  <xsl:variable name ="vAskedBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>

  <!-- ================================================================================ -->
  <!-- InfoTable_IU (Pour DTUPD, IDAUPD, DTINS, IDAINS) -->
  <xsl:template name="InfoTable_IU">
    <c n="DTUPD" t="dt">
      <sl fn="GetUTCDateTimeSys()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsInsert()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="IDAUPD" t="i">
      <sl fn="GetUserId()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsInsert()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="DTINS" t="dt">
      <sl fn="GetUTCDateTimeSys()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsUpdate()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="IDAINS" t="i">
      <sl fn="GetUserId()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsUpdate()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
  </xsl:template>

  <!-- ================================================================================ -->
  <!-- Templates outils -->
  <xsl:template name="AttribValueOrNull">
    <xsl:param name="pValue"/>

    <xsl:attribute name="v">
      <xsl:choose>
        <xsl:when test="normalize-space($pValue)=''">null</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pValue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:attribute>
  </xsl:template>

  <!-- ================================================================================ -->
  <!-- FILE -->
  <xsl:template match="file">
    <file>
      <!-- CommonInput : IOFileAtt -->
      <xsl:call-template name="IOFileAtt"/>
      <!--  -->
      <xsl:apply-templates select="./r"/>
    </file>
  </xsl:template>

  <!-- ================================================================================ -->
  <!-- ========================= TEMPLATE PRINCIPALE R (ROW) ========================== -->
  <xsl:template match="r">

    <xsl:variable name="vBusinessDate" select="$vAskedBusinessDate"/>

    <r id="{@id}" src="{@src}" uc="true">

      <tbl n="IMECCCONR_H" a="IU">
        <c n="DTBUSINESS" dk="true" t="dt" v="{$vBusinessDate}"/>
        <c n="COMBCOMSTRESS" dk="true" v="{./d[@n='CombComStress']/@v}"/>
        <c n="MARKETVOLUME" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='MrkVol']/@v"/>
          </xsl:call-template>
        </c>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </r>
  </xsl:template>
</xsl:stylesheet>