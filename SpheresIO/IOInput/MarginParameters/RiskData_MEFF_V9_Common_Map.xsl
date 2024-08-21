<?xml version="1.0" encoding="utf-8"?>
<!--
/*==============================================================
/* Name     : RiskData_MEFF_V9_Common_Map.xsl
/*==============================================================
/* Summary  : Import MEFF V9 Clearing files
/*==============================================================
/* Version  : 3.3
/* Date     : 20130411
/* Author   : PM
/* Comment  : Elements commun de mapping de l'importation des 
/*            fichiers utilisés par le calcul de déposit
/*            MEFFCOM2.
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>

  <!-- Variable for file identification -->
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>
  <!-- Date de bourse pour laquelle on veux importer les données -->
  <xsl:variable name ="vAskedBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
 
  <!-- ================================================================== -->
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
  <xsl:template name="InfoTable_U">
    <c n="DTUPD" t="dt">
      <sl fn="GetUTCDateTimeSys()"/>
    </c>
    <c n="IDAUPD" t="i">
      <sl fn="GetUserId()"/>
    </c>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- Petits templates utiles -->
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
  <!-- ================================================================== -->
  <!-- ============== IMMEFF_H ========================================== -->
  <xsl:template name="IMMEFF_H">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pContractGroup"/>
    <xsl:param name="pIOInputName"/>

    <r uc="true" id="r0" src="0">
      <tbl n="IMMEFF_H" a="IU">
        <!-- Business Date -->
        <c n="BUSINESSDATE" dk="true" t="dt" v="{$pBusinessDate}">
          <ctls>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDate()" f="yyyy-MM-dd"/>
              <li st="ERROR" ex="true">
                <xsl:attribute name="msg">
                  BusinessDate: Invalid Date  (<xsl:value-of select="$pBusinessDate"/>)
                </xsl:attribute>
              </li>
            </ctl>
            <ctl a="RejectRow" rt="true">
              <xsl:choose>
                <xsl:when test="$pBusinessDate!=$vAskedBusinessDate">
                  <xsl:attribute name="v">true</xsl:attribute>
                  <li st="ERROR" ex="true">
                    <code>SYS</code>
                    <number>5301</number>
                    <d1>
                      <xsl:value-of select="$pBusinessDate"/>
                    </d1>
                    <d2>
                      <xsl:value-of select="$vAskedBusinessDate"/>
                    </d2>
                    <d3>
                      <xsl:value-of select="$vImportedFileName"/>
                    </d3>
                    <d4>
                      <xsl:value-of select="$vImportedFileFolder"/>
                    </d4>
                    <d5>
                      <xsl:value-of select="$pIOInputName"/>
                    </d5>
                  </li>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="v">false</xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </ctl>
          </ctls>
        </c>
        <c n="CONTRACTGROUP" dk="true" v="{$pContractGroup}"/>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </r>
  </xsl:template>
  <!-- ================================================================== -->
</xsl:stylesheet>
