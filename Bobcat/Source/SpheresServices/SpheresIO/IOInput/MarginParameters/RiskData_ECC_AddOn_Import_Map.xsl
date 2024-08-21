<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>

  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_ECC_AddOn_Import_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v1.0.0.0</xsl:variable>
  <xsl:variable name="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>
  <!-- Variable for file identification -->
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>

  <xsl:variable name ="vFileDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
  
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

  <!-- ================================================================================ -->
  <!-- ========================= TEMPLATE PRINCIPALE R (ROW) ========================== -->
  <xsl:template match="r">

    <r id="{@id}" src="{@src}" uc="true">

      <tbl n="IMSMADDON_H" a="IU">
        <c n="IDIMSMSET_H" dk="true" t="i">
          <sql cd="select" rt="IDIMSMSET_H" cache="true">
            select ims.IDIMSMSET_H
            from dbo.IMSMSET_H ims
            where (ims.IDENTIFIER = 'ECC_IMSM_ADDON')
              and (ims.DTENABLED &lt;= @DTBUSINESS) and ((ims.DTDISABLED is null) or (ims.DTDISABLED >@DTBUSINESS))
            <p n="DTBUSINESS" v="{$vFileDate}"/>
          </sql>
        </c>
        <c n="DATAPOINTNUMBER" dk="true" t="i">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="format-number(./d[@n='Num_Datapoints']/@v, '0')"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
              <li st="ERROR" ex="true">
                <xsl:attribute name="msg">
                  DataPoint: Invalid Number  (<xsl:value-of select="format-number(./d[@n='Num_Datapoints']/@v, '0')"/>)
                </xsl:attribute>
              </li>
            </ctl>
          </ctls>
        </c>
        <c n="SECURITYADDON" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='Security_Addon']/@v"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
              <li st="ERROR" ex="true">
                <xsl:attribute name="msg">
                  Security Addon: Invalid Number  (<xsl:value-of select="./d[@n='Security_Addon']/@v"/>)
                </xsl:attribute>
              </li>
            </ctl>
          </ctls>
        </c>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </r>
  </xsl:template>
  <!-- ================================================================================ -->

</xsl:stylesheet>
