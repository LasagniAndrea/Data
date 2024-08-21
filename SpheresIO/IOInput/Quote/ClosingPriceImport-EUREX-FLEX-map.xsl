<?xml version="1.0" encoding="utf-8"?>
<!-- 
  ===================================================================================================================================
  Summary      : Import des cours de cloture de la chambre de compensation XEUR, des contrats FLEX.         
  File         : ClosingPriceImport-EUREX-FLEX-map.xsl
  ===================================================================================================================================
 
  FI/FL 20131213 [19361] add 
  Comment : Création
      Attention, la première vesion de cette importation, n’importe uniquement que les cours de clôture sur les asset des DC FLEX.
      On avais envisagé d’intégrer aussi les cours des sous-jacents des DC «Flex» qui sont des EquityAsset(Table: QUOTE_EQUITY_H) ou 
      des Index(Table: QUOTE_INDEX_H) mai on ne fait rien pour l’instant car ces cotations ne sont pas les mêmes que celles des DC 
      «STANDARD» ayant le même sous-jacent que le DC «Flex»
 ====================================================================================================================================
  -->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
  
  <xsl:include href="Quote_Common_SQL.xsl"/>
  
  <!-- Initialisation de la variable vISO10383 -->
  <xsl:variable name="vISO10383">XEUR</xsl:variable>

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

  <!-- row -->
  <xsl:template match="r">
    <r>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>

      <xsl:variable name ="vBusinessDate" >
        <xsl:value-of select="../r[d[@n='RTC' and @v='*EOF*']]/d[@n='BusDt']/@v"/>
      </xsl:variable>

      <!-- Category : F for Future , O for Option  -->
      <xsl:variable name ="vCategory" >
        <xsl:choose>
          <xsl:when test="string-length(d[@n='PrdTyp']/@v) > 0">O</xsl:when>
          <xsl:when test="string-length(d[@n='PrdTyp']/@v) = 0">F</xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name ="vContractSymbol" >
        <xsl:value-of select ="d[@n='PrdCd']/@v"/>
      </xsl:variable>

      <xsl:variable name ="vVersion" >
        <xsl:value-of select ="d[@n='Ver']/@v"/>
      </xsl:variable>

      <xsl:variable name ="vMaturityMonthYear" >
        <xsl:value-of select="concat('20',d[@n='SerExp']/@v)"/>
      </xsl:variable>

      <xsl:variable name ="vPutCall" >
        <xsl:choose>
          <xsl:when test="d[@n='PrdTyp']/@v= 'P'">0</xsl:when>
          <xsl:when test="d[@n='PrdTyp']/@v= 'C'">1</xsl:when>
          <xsl:otherwise></xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name ="vStrike" >
        <xsl:value-of select="d[@n='Strk']/@v"/>
      </xsl:variable>

      <xsl:variable name ="vPrice" >
        <xsl:value-of select="d[@n='Price']/@v"/>
      </xsl:variable>

      <xsl:variable name ="vUndPrice" >
        <xsl:value-of select="d[@n='UndPrice']/@v"/>
      </xsl:variable>

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
          <xsl:call-template name ="getIDASSET">
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pCONTRACTSYMBOL" select="$vContractSymbol"/>
            <xsl:with-param name="pCATEGORY" select="$vCategory"/>
            <xsl:with-param name="pVERSION" select="$vVersion"/>
            <xsl:with-param name="pMATURITYMONTHYEAR" select="$vMaturityMonthYear"/>
            <xsl:with-param name="pPUTCALL" select="$vPutCall"/>
            <xsl:with-param name="pSTRIKEPRICE" select="$vStrike"/>
            <xsl:with-param name="pBUSINESSDATE" select="$vBusinessDate"/>
          </xsl:call-template>
        </pm>

        <!-- IDC_PRICE -->
        <pm n="currency">
          <sql cd="select" rt="IDC_PRICE" cache="true">
            <![CDATA[
            select dc.IDC_PRICE
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on (m.IDM = dc.IDM) and ( ( m.DTDISABLED is null ) or ( m.DTDISABLED >= @DT ) )
                                    and( m.ISO10383_ALPHA4 = @ISO10383 )
            where 
                ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
            and ( dc.CATEGORY = @CATEGORY )
            and ( isnull(dc.CONTRACTATTRIBUTE,'0') = @CONTRACTATTRIBUTE )
            ]]>
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <p n="ISO10383" v="{$vISO10383}"/>
            <p n="CONTRACTSYMBOL" v="{$vContractSymbol}"/>
            <p n="CATEGORY" v="{$vCategory}"/>
            <p n="CONTRACTATTRIBUTE" v="{$vVersion}"/>
            <p n="DT" t="dt" v="{$vBusinessDate}"/>
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
            <p n="DT" t="dt" v="{$vBusinessDate}"/>
          </sql>
        </pm>
      </pms>

      <!-- Table QUOTE_ETD_H (Importation des cours de clôture sur les asset des DC FLEX) -->
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
        <c n="TIME" dk="true" t="dt" f="yyyy-MM-dd" v="{$vBusinessDate}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>

        <!-- VALUE -->
        <c n="VALUE" dku="true" t="dc" v="{$vPrice}"/>
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

      <!-- Table QUOTE_EQUITY_H (Importation des cours des sous-jacents des DC «Flex» qui sont des EquityAsset) -->
      <!--<tbl n="QUOTE_EQUITY_H" a="IU">
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
            <xsl:with-param name="pCONTRACTSYMBOL" select="$vContractSymbol"/>
            <xsl:with-param name="pCATEGORY" select="$vCategory"/>
            <xsl:with-param name="pVERSION" select="$vVersion"/>
            <xsl:with-param name="pASSETCATEGORY" select="'EquityAsset'"/>
            <xsl:with-param name="pBUSINESSDATE" select ="$vBusinessDate"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="TIME" dk="true" dku="false" t="dt" v="{$vBusinessDate}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>
        <c n="VALUE" dku="true" t="dc" v="{$vUndPrice}" />
        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>
              
        <xsl:call-template name="infosImport"/>

      </tbl>-->

      <!-- Table QUOTE_INDEX_H (Importation des cours des sous-jacents des DC «Flex» qui sont des Index) -->
      <!--<tbl n="QUOTE_INDEX_H" a="IU">
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
            <xsl:with-param name="pCONTRACTSYMBOL" select="$vContractSymbol"/>
            <xsl:with-param name="pCATEGORY" select="$vCategory"/>
            <xsl:with-param name="pVERSION" select="$vVersion"/>
            <xsl:with-param name="pASSETCATEGORY" select="'Index'"/>
            <xsl:with-param name="pBUSINESSDATE" select ="$vBusinessDate"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <c n="TIME" dk="true" dku="false" t="dt" v="{$vBusinessDate}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li st="ERROR" ex="true" msg="Invalid TIME"/>
            </ctrl>
          </ctrls>
        </c>

        --><!--VALUE--><!--
        <c n="VALUE" dku="true" t="dc" v="$UndPrice" />

        <c n="SPREADVALUE" dk="false" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dk="false" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" dku="true" v="OfficialClose"/>
        <c n="QUOTETIMING" dk="false" dku="true" v="Close"/>
        <c n="ASSETMEASURE" dk="false" dku="false" v="MarketQuote"/>
        <c n="ISENABLED" dk="false" dku="true" t="b" v="1"/>
        <c n="SOURCE" dk="false" dku="false" v="ClearingOrganization"/>
       
        <xsl:call-template name="infosImport"/>

      </tbl>-->

    </r>
  </xsl:template>

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
    <xsl:param name="pISO10383" select="$gNull"/>
    <xsl:param name="pCONTRACTSYMBOL" select="$gNull"/>
    <xsl:param name="pCATEGORY" select="$gNull"/>
    <xsl:param name="pVERSION" select="$gNull"/>
    <xsl:param name="pASSETCATEGORY" select="$gNull"/>
    <xsl:param name="pBUSINESSDATE" select="$gNull"/>

    <sql cd="select" rt="IDASSET_UNL" cache="true">
      <![CDATA[
      select dc.IDASSET_UNL
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on ( m.IDM = dc.IDM )
      where ( m.ISO10383_ALPHA4 = @ISO10383 )
      and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
      and ( dc.CATEGORY = @CATEGORY )
      and ( dc.ASSETCATEGORY = @ASSETCATEGORY )
      and ( isnull(dc.CONTRACTATTRIBUTE,'0') = @CONTRACTATTRIBUTE )
      ]]>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <p n="DT" t="dt" v="{$pBUSINESSDATE}"/>
      <p n="ISO10383" v="{$pISO10383}"/>
      <p n="CONTRACTSYMBOL" v="{$pCONTRACTSYMBOL}"/>
      <p n="CATEGORY" v="{$pCATEGORY}"/>
      <p n="CONTRACTATTRIBUTE" v="{$pVERSION}"/>
      <p n="ASSETCATEGORY" v="{$pASSETCATEGORY}"/>
    </sql>
  </xsl:template>

  <xsl:template name ="getIDASSET">
    <xsl:param name="pISO10383" select="$gNull"/>
    <xsl:param name="pCONTRACTSYMBOL" select="$gNull"/>
    <xsl:param name="pCATEGORY" select="$gNull"/>
    <xsl:param name="pVERSION" select="$gNull"/>
    <xsl:param name="pMATURITYMONTHYEAR" select="$gNull"/>
    <xsl:param name="pPUTCALL" select="$gNull"/>
    <xsl:param name="pSTRIKEPRICE" select="$gNull"/>
    <xsl:param name="pBUSINESSDATE" select="$gNull"/>

    <sql cd="select" rt="IDASSET" cache="true">
      <![CDATA[
            select a.IDASSET
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
            inner join dbo.TRADEINSTRUMENT tr on ( tr.IDASSET = a.IDASSET )
            where ( mk.ISO10383_ALPHA4 = @ISO10383 )
              and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
              and ( dc.CATEGORY = @CATEGORY )
              and ( isnull(dc.CONTRACTATTRIBUTE,'0') = @CONTRACTATTRIBUTE )
              and ( ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR )
              and ( ( a.DTDISABLED is null ) or ( a.DTDISABLED >= @DT ) )]]>
      <xsl:if test ="$pCATEGORY='O'">
        <![CDATA[
              and a.PUTCALL = @PUTCALL
              and a.STRIKEPRICE = @STRIKEPRICE]]>
      </xsl:if>

      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <p n="DT" t="dt" v="{$pBUSINESSDATE}"/>
      <p n="ISO10383" v="{$pISO10383}"/>
      <p n="CONTRACTSYMBOL" v="{$pCONTRACTSYMBOL}"/>
      <p n="CATEGORY" v="{$pCATEGORY}"/>
      <p n="CONTRACTATTRIBUTE" v="{$pVERSION}"/>
      <p n="MATURITYMONTHYEAR" v="{$pMATURITYMONTHYEAR}"/>
      <xsl:if test = "$pCATEGORY = 'O'">
        <p n="PUTCALL" v="{$pPUTCALL}"/>
        <p n="STRIKEPRICE" t="dc" v="{$pSTRIKEPRICE}"/>
      </xsl:if>
    </sql>

  </xsl:template>

</xsl:stylesheet>