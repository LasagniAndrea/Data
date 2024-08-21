<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:exsl="http://exslt.org/common" extension-element-prefixes="exsl">

<!--
=============================================================================================
Summary : BCS Gateway tools for ETD imports (Trades, PosRequest, ...)
          It's based on Spheres standard tools and overrides some templates
          Some templates of this xsl are overrided by specific customer's xsl mapping files
          
File    : BCSTools.xsl
=============================================================================================
FI 2022XXXX [25699] Adaptation au format d'import V3 (dispo avec Spheres V12)
FI 20161005 [XXXXX] Rem ovrETD_SQLMarket and ovrSQLDerivativeContract
RD 20140324 [19704] Add ovrETD_SQLMarket and ovrSQLDerivativeContract
=============================================================================================
-->

  <xsl:import href="ImportTools.xsl"/>
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>
  <xsl:decimal-format name="decimalFormat" decimal-separator="."/>

  <xsl:template name ="DataMKT">
    <xsl:param name ="pMarketID"/>
    <!-- Market -->
    <data>
      <xsl:attribute name="name">MKT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="$pMarketID">XDMI</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="concat('XDMI-',number(data[@name='marketid']))"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- MarketIdent -->
    <data>
      <xsl:attribute name="name">MKTI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresSecurityExchange'"/>
    </data>

  </xsl:template>
  
  <xsl:template name ="DataCTR">
    <xsl:param name ="pIsOptionAction"/>
    <!-- DerivativeContract -->
    <data>
      <xsl:attribute name="name">CTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='symbol']"/>
    </data>

    <!-- DerivativeContractIdent -->
    <data>
      <xsl:attribute name="name">CTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresContractCode'"/>
    </data>

    <!-- DerivativeContractCategory -->
    <data>
      <xsl:attribute name="name">CTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="number(data[@name='strikeprice']) != 0">O</xsl:when>
        <xsl:otherwise>F</xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- PutCall -->
    <data>
      <xsl:attribute name="name">CTRPC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='putcall']"/>
    </data>

    <!-- Maturity -->
    <data>
      <xsl:attribute name="name">CTRM</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test ="$pIsOptionAction = 'true'">
          <xsl:value-of select="data[@name='expirationdate']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="data[@name='expirationmonth']"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- Strike -->
    <data>
      <xsl:attribute name="name">CTRS</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='strikeprice']"/>
    </data>
  </xsl:template>

  <xsl:template name ="DataBDT">
    <xsl:param name ="pDateYYYYMMDD"/>

    <xsl:variable name="year">
      <xsl:value-of select="substring($pDateYYYYMMDD,1,4)"/>
    </xsl:variable>
    <xsl:variable name="month">
      <xsl:value-of select="substring($pDateYYYYMMDD,5,2)"/>
    </xsl:variable>
    <xsl:variable name="day">
      <xsl:value-of select="substring($pDateYYYYMMDD,7,2)"/>
    </xsl:variable>

    <data name="BDT" datatype="date">
      <xsl:call-template name="DateFormatISO">
        <xsl:with-param name ="year" select ="$year"/>
        <xsl:with-param name ="month" select ="$month"/>
        <xsl:with-param name ="day" select ="$day"/>
      </xsl:call-template>
    </data>
  </xsl:template>

  <xsl:template name ="DataBSCO">
    <!-- ClearingOrganisation -->
    <data>
      <xsl:attribute name="name">BSCO</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'CCEGITRR'"/>
    </data>

    <!-- ClearingOrganisationIdent -->
    <data>
      <xsl:attribute name="name">BSCOI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'BIC'"/>
    </data>
  </xsl:template>

  <xsl:template name ="DataBSCOA">
    <!-- ClearingOrganisationAccount -->
    <data>
      <xsl:attribute name="name">BSCOA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="string-length(data[@name='subaccount'])>0 and data[@name='subaccount'] != '*OMN'">
          <xsl:value-of select="data[@name='subaccount']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="data[@name='accounttype']"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- ClearingntOrganisationAccountIdent -->
    <data>
      <xsl:attribute name="name">BSCOAI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>


  </xsl:template>

  <xsl:template name="DateFormatISO">
    <xsl:param name ="year"/>
    <xsl:param name ="month"/>
    <xsl:param name ="day"/>
    <xsl:value-of select="concat($year,'-',$month,'-',$day)"/>
  </xsl:template>

  <xsl:template name="BuildFile">
    <file>
      <xsl:attribute name="name"/>
      <xsl:attribute name="folder"/>
      <xsl:attribute name="date"/>
      <xsl:attribute name="size"/>
      <xsl:attribute name="status">success</xsl:attribute>
      <xsl:variable name="idprefix" select="'r'"/>
      <xsl:variable name="idposition" select="position()"/>

      <xsl:choose>
        <!-- 2 à n <BCSMessage>,il existe un tag <BCSMessageList>  -->
        <xsl:when test="BCSMessageList">
          <xsl:apply-templates select="BCSMessageList"/>
        </xsl:when>
        <!-- 1 seul <BCSMessage> -->
        <xsl:otherwise>
          <xsl:apply-templates select="BCSMessage"/>
        </xsl:otherwise>
      </xsl:choose>
    </file>
  </xsl:template>

  
</xsl:stylesheet>