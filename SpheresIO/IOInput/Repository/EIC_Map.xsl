<?xml version="1.0" encoding="utf-8"?>
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->
<!--  
=============================================================================================
Summary : Mapping du fichier Energy Identification Codes (EICs)
File    : EIC_Map.xsl
=============================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>

  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">EIC_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v1.0.0.0</xsl:variable>
  <xsl:variable name ="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>

  <!-- File template-->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>
      <xsl:apply-templates select="r"/>
    </file>
  </xsl:template>

  <xsl:template match="r">
    <r uc="false">
      <xsl:call-template name="IORowAtt"/>
      <tbl n="DELIVERYPOINT" a="IU">
        <c n="IDENTIFIER" dku="true" v="{d[@n='display']/@v}"/>
        <c n="DISPLAYNAME" dku="true" v="{d[@n='longName']/@v}"/>
        <c n="DESCRIPTION" dku="true" v="{d[@n='longName']/@v}"/>
        
        <!-- Verify existing country -->
        <xsl:variable name ="idCountry" select="d[@n='country']/@v"/>
        <xsl:if test="normalize-space($idCountry)!=''">
          <c n="IDCOUNTRY" dku="true" v="{$idCountry}">
            <ctls>
              <ctl a="RejectColumn" rt="false" lt="None">
                <sql cd="select" rt="EXIST" cache="true">
                  select 1 as EXIST
                  from dbo.COUNTRY c
                  where (c.IDCOUNTRY = @IDCOUNTRY)
                  <p n="IDCOUNTRY" v="{$idCountry}"/>
                </sql>
              </ctl>
            </ctls>
          </c>
        </xsl:if>
        
        <!-- Energy Identification Code -->
        <c n="EICCODE" dk="true" v="{d[@n='mRID']/@v}"/>
        
        <c n="FUNCTIONCODE" dku="true" v="{d[@n='fnCode']/@v}"/>
        <c n="FUNCTIONNAME" dku="true" v="{d[@n='fnName']/@v}"/>

        <xsl:choose>
          <xsl:when test="normalize-space(d[@n='dtLastRequest']/@v)!=''">
            <c n="DTENABLED" dku="false" t="dt" v="{d[@n='dtLastRequest']/@v}">
              <ctls>
                <ctl a="RejectRow" rt="false">
                  <sl fn="IsDate()" f="YYYY-MM-DD"/>
                  <li st="err" ex="true" msg="lastRequest_DateAndOrTime.date: Invalid Date">
                    <xsl:attribute name="msg">
                      &lt;b&gt;lastRequest_DateAndOrTime.date: Invalid Date.&lt;/b&gt;
                      <xsl:text>&#xa;</xsl:text>xsl: <xsl:value-of select="$vXSLFileName"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vXSLVersion"/><xsl:text>&#xa;</xsl:text>
                    </xsl:attribute>
                  </li>
                </ctl>
              </ctls>
            </c>
          </xsl:when>
          <xsl:otherwise>
            <c n="DTENABLED" dku="false" t="dt">
              <sl fn="GetDateSys()"/>
            </c>              
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="normalize-space(d[@n='dtDeactivateRequest']/@v)!=''">
          <c n="DTDISABLED" dku="false" t="dt" v="{d[@n='dtDeactivateRequest']/@v}">
            <ctls>
              <ctl a="RejectColumn" rt="true" lt="None">
                <sl fn="IsDate()" f="YYYY-MM-DD"/>
              </ctl>
            </ctls>
          </c>
        </xsl:if>

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
      </tbl>
    </r>
  </xsl:template>
  
</xsl:stylesheet>
