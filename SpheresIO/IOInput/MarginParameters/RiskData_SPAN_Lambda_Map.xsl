<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes -->
  <xsl:include href=".\RiskData_SPAN_Import_Common.xsl"/>
  
  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_SPAN_Lambda_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v5.1.0.0</xsl:variable>
  <xsl:variable name="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>
  <!-- Variable for file identification -->
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>
  
  <!-- Parameters -->
  <xsl:variable name="vAskedBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
  <!-- Le paramètre PRICEONLY active/désactive l'import des données de calcul de risque  -->
  <xsl:variable name ="vIsImportPriceOnly" >
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='PRICEONLY']='true'">true</xsl:when>
      <xsl:otherwise>false</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <!-- FILE -->
  <xsl:template match="file">
    <file>
      <!-- CommonInput : IOFileAtt -->
      <xsl:call-template name="IOFileAtt"/>
      <!--  -->
      <xsl:if test="$vIsImportPriceOnly != 'true'">
        <xsl:call-template name="StartImport"/>
      </xsl:if>
    </file>
  </xsl:template>

  <!-- Lancement de l'import -->
  <xsl:template name="StartImport">
    <xsl:variable name="vBusinessDate" select="./r[1]/d[@n='BD']/@v"/>

    <!-- Gestion des record -->
    <xsl:apply-templates select="./r">
      <xsl:with-param name="pFileFormat" select="'UP'"/>
      <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
      <xsl:with-param name="pExchangeComplex" select="'MONEP'"/>
    </xsl:apply-templates>

  </xsl:template>

  <xsl:template match="r">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pExchangeComplex"/>

      <!--Traitement en fonction du type d'enregistrement RT-->
    <xsl:choose>
      <!--Record Type = '00000'-->
      <xsl:when test="./d[@n='RT']/@v = '00000'">
        <r uc="true" id="{./@id}">
          <xsl:call-template name="CheckDate">
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
        </r>
      </xsl:when>
      <!--Record Type = '00020'-->
      <xsl:when test="./d[@n='RT']/@v = '00020'">
        <r uc="true" id="{./@id}">
          <xsl:call-template name="IMSPANGRPCTR_H">
            <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
        </r>
      </xsl:when>
      <!--Record Type = '99999'-->
      <xsl:when test="./d[@n='RT']/@v = '99999'">
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CheckDate">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pExchangeComplex"/>

    <tbl n="IMSPAN_H" a="U">
      <!-- Business Date Time -->
      <c n="ASKEDDTBUSINESSTIME" v="{$vAskedBusinessDate}">
        <ctls>
          <ctl a="RejectRow" rt="true" v="true">
            <xsl:choose>
              <xsl:when test="$pBusinessDate!=$vAskedBusinessDate">
                <xsl:attribute name="lt">Error</xsl:attribute>
                <li st="err" ex="true">
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
                    <xsl:value-of select="$vIOInputName"/>
                  </d5>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="lt">None</xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
          </ctl>
        </ctls>
      </c>
    </tbl>
  </xsl:template>

  <xsl:template name="IMSPANGRPCTR_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pExchangeComplex"/>

    <tbl n="IMSPANGRPCTR_H" a="U">
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_Last_IDIMSPAN_H">
        <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
        <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <!-- Combined Commodity Code -->
      <c n="COMBCOMCODE" dk="true" v="{./d[@n='CC']/@v}">
        <ctls>
          <ctl a="RejectRow" rt="true">
            <sl fn="IsEmpty()"/>
            <li st="REJECT" ex="true">
              <c>SYS</c>
              <n>2001</n>
              <d1>
                <xsl:value-of select="$vXSLFileName"/>
              </d1>
              <d2>
                <xsl:value-of select="$vXSLVersion"/>
              </d2>
            </li>
          </ctl>
        </ctls>
      </c>
      <!-- Use of Lambda -->
      <c n="ISUSELAMBDA" dku="true" t="b">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="./d[@n='LActiv']/@v = 'Y'">1</xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <!-- Lambda Minimum -->
      <c n="LAMBDAMIN" dku="true" t="dc">
        <xsl:call-template name="AttribDecLoc">
          <xsl:with-param name="pValue" select="concat(./d[@n='LMinS']/@v, ./d[@n='LMin']/@v)"/>
          <xsl:with-param name="pDecLoc" select="./d[@n='LMinDL']/@v"/>
        </xsl:call-template>
      </c>
      <!-- Lambda Maximum -->
      <c n="LAMBDAMAX" dku="true" t="dc">
        <xsl:call-template name="AttribDecLoc">
          <xsl:with-param name="pValue" select="concat(./d[@n='LMaxS']/@v, ./d[@n='LMax']/@v)"/>
          <xsl:with-param name="pDecLoc" select="./d[@n='LMaxDL']/@v"/>
        </xsl:call-template>
      </c>
    </tbl>
  </xsl:template>

  <!-- Lire la colonne IDIMSPAN_H la plus récente de la table IMSPAN_H pour la date et chambre spécifiée -->
  <xsl:template name="Col_Last_IDIMSPAN_H">
    <xsl:param name="pFileFormat"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pExchangeComplex"/>
    
    <c n="IDIMSPAN_H" dk="true" t="i">
      <sql cd="select" rt="IDIMSPAN_H" cache="true">
        select s.IDIMSPAN_H
        from dbo.IMSPAN_H s
        where (s.DTBUSINESS = @BUSINESSDATE)
        and (s.EXCHANGECOMPLEX = @EXCHANGECOMPLEX)
        and (s.FILEFORMAT = @FILEFORMAT)
        and (((s.DTUPD is null) and (s.DTINS =
        (select max(case when (sm.DTUPD is null) then sm.DTINS else sm.DTUPD end) dt
        from dbo.IMSPAN_H sm
        where (sm.DTBUSINESS = s.DTBUSINESS)
        and (sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX))))
        or (s.DTUPD =
        (select max(case when (sm.DTUPD is null) then sm.DTINS else sm.DTUPD end) dt
        from dbo.IMSPAN_H sm
        where (sm.DTBUSINESS = s.DTBUSINESS)
        and (sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX))))
        <p n="BUSINESSDATE" t="d" f="yyyy-MM-dd" v="{$pBusinessDate}"/>
        <p n="EXCHANGECOMPLEX" v="{$pExchangeComplex}"/>
        <p n="FILEFORMAT" v="{$pFileFormat}"/>
      </sql>
      <ctls>
        <ctl a="RejectRow" rt="true">
          <sl fn="IsNull()"/>
          <li st="INFO" ex="false"/>
        </ctl>
      </ctls>
    </c>
  </xsl:template>
  
</xsl:stylesheet>
