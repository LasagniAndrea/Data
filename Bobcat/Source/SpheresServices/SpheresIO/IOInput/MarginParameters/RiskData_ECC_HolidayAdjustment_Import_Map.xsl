<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>
  
  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_ECC_HolidayAdjustment_Import_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v1.0.0.0</xsl:variable>
  <xsl:variable name="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>
  <!-- Variable for file identification -->
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>

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

      <tbl n="IMSMHOLIDAYADJ_H" a="IU">
        <c n="CALCULATIONDATE" dk="true" t="dt" v="{./d[@n='IMSM_Calculation_Date']/@v}">
          <ctls>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDate()" f="yyyy-MM-dd"/>
              <li st="ERROR" ex="true">
                <xsl:attribute name="msg">
                  Calculation Date: Invalid Date  (<xsl:value-of select="./d[@n='IMSM_Calculation_Date']/@v"/>)
                </xsl:attribute>
              </li>
            </ctl>
          </ctls>
        </c>
        <c n="EFFECTIVEDATE" dk="true"  t="dt" v="{./d[@n='Effective_Date']/@v}">
          <ctls>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDate()" f="yyyy-MM-dd"/>
              <li st="ERROR" ex="true">
                <xsl:attribute name="msg">
                  Effective Date: Invalid Date  (<xsl:value-of select="./d[@n='Effective_Date']/@v"/>)
                </xsl:attribute>
              </li>
            </ctl>
          </ctls>
        </c>
        <c n="LAMBDAFACTOR" dku="true" t="dc">
          <xsl:call-template name="AttribValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='Holiday_Adjustment_Factor']/@v"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
              <li st="ERROR" ex="true">
                <xsl:attribute name="msg">
                  Holiday Adjustment Factor: Invalid Number  (<xsl:value-of select="./d[@n='Holiday_Adjustment_Factor']/@v"/>)
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
