<?xml version="1.0" encoding="utf-8"?>

<!-- 
  ==================================================================================================================================
  Summary      : Import des cours de cloture de la chambre de compensation OCC à partir d'un fichier provenant du marché CBOE.         
   
                  Tous ces caotations seront importés sur des DC dans un marché ayant pour Symbole du marché XUSS et Code ISO XUSS.
                  Ce marché est un marché fictif qui a été créé pour mettre dans un marché unique tous les DC
                  négociés sur tous les marchés de la chambre de compensation OCC de manière à pouvoir gérer
                  la problématique de fongibilité
   
  File         : ClosingPriceImport-CBOE-map.xsl
  ==================================================================================================================================
  Version : v3.1.0.0                                           
  Date    : 20130218
  Author  : BD 
  Comment : Création
  ==================================================================================================================================
  Version : v3.2.0.0                                           
  Date    : 20130405
  Author  : FL
  Comment : Rajout de commentaire
            Creation et gestion de la variable vExchangeAcronym
            Remplacement du marché 'CBOE' par 'XUSS'
  ==================================================================================================================================
  Version : v3.2.0.1                                           
  Date    : 20130416
  Author  : BD
  Comment : Remplacement de vExchangeAcronym par vISO10383
  ==================================================================================================================================
  -->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:include href="Quote_Common_SQL.xsl"/>
  
  <!-- Initialisation de la variable vISO10383 -->
  <xsl:variable name="vISO10383">XUSS</xsl:variable>

  <!-- iotask -->
  <xsl:template match="iotask">
    <iotask>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="pms"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- parameters -->
  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter">
        <parameter>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="@name"/>
          </xsl:attribute>
          <xsl:attribute name="displayname">
            <xsl:value-of select="@displayname"/>
          </xsl:attribute>
          <xsl:attribute name="direction">
            <xsl:value-of select="@direction"/>
          </xsl:attribute>
          <xsl:attribute name="datatype">
            <xsl:value-of select="@datatype"/>
          </xsl:attribute>
          <xsl:value-of select="."/>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>

  <!-- iotaskdet -->
  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="ioinput"/>
    </iotaskdet>
  </xsl:template>

  <!-- ioinput -->
  <xsl:template match="ioinput">
    <ioinput>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="file"/>
    </ioinput>
  </xsl:template>

  <!-- file -->
  <xsl:template match="file">
    <file>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="folder">
        <xsl:value-of select="@folder"/>
      </xsl:attribute>
      <xsl:attribute name="date">
        <xsl:value-of select="@date"/>
      </xsl:attribute>
      <xsl:attribute name="size">
        <xsl:value-of select="@size"/>
      </xsl:attribute>

      <xsl:apply-templates select="r" />
    </file>
  </xsl:template>

  <!-- Infos Importion -->
  <xsl:template name="infosImport">
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

  <xsl:template name="getIDASSET_UNL">
    <xsl:param name="pIDASSET_UNL"/>
    <xsl:param name="pISO10383" select="'null'"/>
    <xsl:param name="pASSETCATEGORY" select="'null'"/>
    <xsl:param name="pCONTRACTSYMBOL" select="'null'"/>
    <!-- FI 20130412 Add cache="true" -->
    <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
    <sql cd="select" cache="true">
      <xsl:attribute name="rt">
        <xsl:value-of select="$pIDASSET_UNL"/>
      </xsl:attribute>
      select dc.IDASSET_UNL
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on ( m.IDM = dc.IDM )
      where ( m.ISO10383_ALPHA4 = @ISO10383 )
      and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
      and ( dc.CATEGORY = 'O' )
      and ( dc.ASSETCATEGORY = @ASSETCATEGORY )
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <p n="DT" t="dt" v="{$gParamDtBusiness}"/>
      <p n="ISO10383" v="{$pISO10383}"/>
      <p n="CONTRACTSYMBOL" v="{$pCONTRACTSYMBOL}"/>
      <p n="ASSETCATEGORY" v="{$pASSETCATEGORY}"/>
    </sql>
  </xsl:template>

  <!-- row -->
  <xsl:template match="r">
    <r>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>


      <!-- Liste des parametres -->
      <pms>

        <!-- IDMARKETENV -->
        <pm n="marketenv" >
          <sql cd="select" rt="IDMARKETENV" cache="true">
            <![CDATA[
            select IDMARKETENV
            from dbo.MARKETENV
            where ISDEFAULT = 1
            ]]>
          </sql>
        </pm>

        <!-- IDVALSCENARIO -->
        <pm n="valscenario" >
          <sql cd="select" rt="IDVALSCENARIO" cache="true">
            <![CDATA[
            select v.IDVALSCENARIO, 1 as colorder
            from dbo.VALSCENARIO v
            inner join dbo.MARKETENV m on ( m.IDMARKETENV = v.IDMARKETENV ) and ( m.ISDEFAULT = 1)
            where v.ISDEFAULT = 1
            union all
            select v.IDVALSCENARIO, 2 as colorder
            from dbo.VALSCENARIO v
            where v.ISDEFAULT = 1 and v.IDMARKETENV is null
            order by colorder asc
            ]]>
          </sql>
        </pm>

        <!-- IDASSET -->
        <pm n="asset" >
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <!-- RD 20130529 : Correction du paramètre @DT-->
          <!-- FI 20131121 [19224] use parameter MATURITYMONTHYEAR -->
          <sql cd="select" rt="IDASSET" cache="true">
            <![CDATA[
            select distinct a.IDASSET
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
              and ( ( da.DTDISABLED is null ) or ( da.DTDISABLED >= @DT ) )
            inner join dbo.MATURITY ma on ( ma.IDMATURITY = da.IDMATURITY )
              and ( ( ma.DTDISABLED is null ) or ( ma.DTDISABLED >= @DT ) )
            inner join dbo.MATURITYRULE mr on ( mr.IDMATURITYRULE = ma.IDMATURITYRULE )
              and ( ( mr.DTDISABLED is null ) or ( mr.DTDISABLED >= @DT ) )
            inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )            
            inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
              and ( ( mk.DTDISABLED is null ) or ( mk.DTDISABLED >= @DT ) )
            inner join dbo.TRADE tr on ( tr.IDASSET = a.IDASSET )
            where ( mk.ISO10383_ALPHA4 = @ISO10383 )
              and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
              and ( dc.CATEGORY = @CATEGORY )
              and ( ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR )
              and ( ( a.DTDISABLED is null ) or ( a.DTDISABLED >= @DT ) )
              and a.PUTCALL = @PUTCALL
              and a.STRIKEPRICE = @STRIKEPRICE
            ]]>
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>

            <xsl:variable name="vMaturityMonthYear">
              <xsl:value-of select ="translate(d[@n='Maturity']/@v,'-','')" />
            </xsl:variable>

            <p n="DT" t="dt" v="{$gParamDtBusiness}"/>
            <p n="ISO10383" v="{$vISO10383}"/>
            <p n="CONTRACTSYMBOL" v="{d[@n='Group']/@v}"/>
            <p n="CATEGORY" v="O" />
            <!--<p n="MATURITYDATE" t="dt" v="{d[@n='Maturity']/@v}"/>-->
            <p n="MATURITYMONTHYEAR" v="{$vMaturityMonthYear}"/>
            <!-- FI 20131121 [19224]-->
            <!--<p n="MATURITYMONTHYEAR">
              <xsl:variable name="Month">
                <xsl:choose>
                  <xsl:when test="d[@n='Month']/@v = 'Jan'">01</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Feb'">02</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Mar'">03</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Apr'">04</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'May'">05</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Jun'">06</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Jul'">07</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Aug'">08</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Sep'">09</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Oct'">10</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Nov'">11</xsl:when>
                  <xsl:when test="d[@n='Month']/@v = 'Dec'">12</xsl:when>
                </xsl:choose>
              </xsl:variable>
              <xsl:attribute name="v">
                <xsl:value-of select="concat(20,d[@n='Year']/@v,$Month)"/>
              </xsl:attribute>
            </p>-->
            <p n="PUTCALL">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="d[@n='CP']/@v = 'C'">1</xsl:when>
                  <xsl:when test="d[@n='CP']/@v = 'P'">0</xsl:when>
                </xsl:choose>
              </xsl:attribute>
            </p>
            <p n="STRIKEPRICE" t="dc" v="{d[@n='Strike']/@v}"/>
          </sql>
        </pm>

        <!-- IDC_PRICE -->
        <pm n="currency">
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <!-- RD 20130529 : Correction du paramètre @DT-->
          <sql cd="select" rt="IDC_PRICE" cache="true">
            <![CDATA[
            select dc.IDC_PRICE
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on (m.IDM = dc.IDM)
            where ( m.ISO10383_ALPHA4 = @ISO10383 )
            and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
            and ( dc.CATEGORY = 'O' )
            and ( ( m.DTDISABLED is null ) or ( m.DTDISABLED >= @DT ) )
            ]]>
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <p n="ISO10383" v="{$vISO10383}"/>
            <p n="CONTRACTSYMBOL" v="{d[@n='Group']/@v}"/>
            <p n="DT" t="dt" v="{$gParamDtBusiness}"/>
          </sql>
        </pm>

        <!-- IDM -->
        <pm n="market">          
          <sql cd="select" rt="IDM" cache="true">
            <![CDATA[
            select m.IDM
            from dbo.MARKET m
            where ( m.ISO10383_ALPHA4 = @ISO10383 )
            and ( ( m.DTDISABLED is null ) or ( m.DTDISABLED >= @DT ) )
            ]]>
            <p n="ISO10383" v="{$vISO10383}"/>
            <p n="DT" t="dt" v="{$gParamDtBusiness}"/>
          </sql>
        </pm>
      </pms>

      <!-- Table QUOTE_ETD_H -->
      <tbl n="QUOTE_ETD_H" a="IU">

        <!-- IDMARKETENV -->
        <c n="IDMARKETENV" dk="true" v="parameters.marketenv">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>

        <!-- IDVALSCENARIO -->
        <c n="IDVALSCENARIO" dk="true" v="parameters.valscenario">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>

        <!-- IDASSET -->
        <c n="IDASSET" dk="true" t="i" v="parameters.asset">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>

        <!-- IDC -->
        <c n="IDC" dku="true" v="parameters.currency"/>

        <!-- IDM -->
        <c n="IDM" dku="true" t="i" v="parameters.market"/>

        <!-- TIME -->
        <c n="TIME" dk="true" t="dt" f="yyyy-MM-dd" v="{$gParamDtBusiness}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>

        <!-- VALUE -->
        <c n="VALUE" dku="true" t="dc" v="{d[@n='LastSale']/@v}"/>

        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>

        <!-- Infos Importation -->
        <xsl:call-template name="infosImport"/>

      </tbl>


      <!-- Table QUOTE_EQUITY_H -->
      <tbl n="QUOTE_EQUITY_H" a="IU">

        <c n="IDMARKETENV" dk="true" v="parameters.marketenv">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="IDVALSCENARIO" dk="true" v="parameters.valscenario">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="IDASSET" dk="true" dku="false" t="i">
          <xsl:call-template name="getIDASSET_UNL">
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pASSETCATEGORY" select="'EquityAsset'"/>
            <xsl:with-param name="pCONTRACTSYMBOL" select="d[@n='Group']/@v"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="TIME" dk="true" dku="false" t="dt" v="{$gParamDtBusiness}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>

        <!-- Insertion de StockValue dans VALUE -->
        <c n="VALUE" dku="true" t="dc" v="{d[@n='SValue']/@v}" />

        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>

        <!-- Infos Importation -->
        <xsl:call-template name="infosImport"/>

      </tbl>


      <!--Table QUOTE_INDEX_H-->
      <tbl n="QUOTE_INDEX_H" a="IU">

        <c n="IDMARKETENV" dk="true" v="parameters.marketenv">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="IDVALSCENARIO" dk="true" v="parameters.valscenario">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="IDASSET" dk="true" dku="false" t="i">
          <xsl:call-template name="getIDASSET_UNL">
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pASSETCATEGORY" select="'Index'"/>
            <xsl:with-param name="pCONTRACTSYMBOL" select="d[@n='Group']/@v"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="TIME" dk="true" dku="false" t="dt" v="{$gParamDtBusiness}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>

        <!--VALUE-->
        <c n="VALUE" dku="true" t="dc" v="{d[@n='SValue']/@v}" />

        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>

        <!--Infos Importation-->
        <xsl:call-template name="infosImport"/>

      </tbl>

      <!--Table QUOTE_EXTRDFUND_H-->
      <tbl n="QUOTE_EXTRDFUND_H" a="IU">

        <c n="IDMARKETENV" dk="true" v="parameters.marketenv">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="IDVALSCENARIO" dk="true" v="parameters.valscenario">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="IDASSET" dk="true" dku="false" t="i">
          <xsl:call-template name="getIDASSET_UNL">
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pASSETCATEGORY" select="'ExchangeTradedFund'"/>
            <xsl:with-param name="pCONTRACTSYMBOL" select="d[@n='Group']/@v"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="TIME" dk="true" dku="false" t="dt" v="{$gParamDtBusiness}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>

        <!--VALUE-->
        <c n="VALUE" dku="true" t="dc" v="{d[@n='SValue']/@v}" />

        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>

        <!--Infos Importation-->
        <xsl:call-template name="infosImport"/>

      </tbl>

    </r>
  </xsl:template>

</xsl:stylesheet>