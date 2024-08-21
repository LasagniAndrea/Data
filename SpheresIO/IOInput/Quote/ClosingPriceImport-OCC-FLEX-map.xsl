<?xml version="1.0" encoding="utf-8"?>

<!-- 
  ===================================================================================================================================
  Summary      : Import des cours de cloture de la chambre de compensation OCC, des contrats FLEX.         
  File         : ClosingPriceImport-OCC-FLEX-map.xsl
  ===================================================================================================================================
  FL 20140115[19463] add 
  Comment : Création
      Attention, la première vesion de cette importation, n’importe uniquement que les cours de clôture sur les asset des DC FLEX.
      On avais envisagé d’intégrer aussi les cours des sous-jacents des DC «Flex» qui sont des EquityAsset(Table: QUOTE_EQUITY_H) ou 
      des Index(Table: QUOTE_INDEX_H) mai on ne fait rien pour l’instant car ces cotations ne sont pas les mêmes que celles des DC 
      «STANDARD» ayant le même sous-jacent que le DC «Flex»
      Pour plus d'info : Cf. Ticket 19463 (EFS Thread: 2)
 ====================================================================================================================================
  -->
<!-- FI 20131128 [19255] OfficialSettlement or OfficialClose management -->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:include href="Quote_Common_SQL.xsl"/>

  <!-- Initialisation de la variable vISO10383 relative au marché XUSS-->
  <xsl:variable name="vISO10383">XUSS</xsl:variable>

  <!-- Initialisation de la variable VCategory : F for Future , O for Option -->
  <!-- Rq : Dans les fichiers relatifs à ces importation, il n'y a que des Options -->
  <xsl:variable name="vCategory">O</xsl:variable>

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

      <!-- Initialisation de la variable vBusinessDate : -->
      <!--  Cette date est dans le premier Row du Fichier de parsing 
            Attention pour que cette recherche fonctionne : 
            Dans les éléments d’importation utilisant ce XSL, 
            il ne faut pas d’indice mettre d’indice de volumétrie de manière
            à n’avoirs qu’un seule fichier de Mapping XML. -->

      <xsl:variable name="vBusinessDate">
        <xsl:value-of select="./r[1]/d[@n='Busdt']/@v"/>
      </xsl:variable>

      <xsl:apply-templates select="r">
        <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
      </xsl:apply-templates>
    </file>
  </xsl:template>

  <!-- row -->
  <xsl:template match="r">
    <xsl:param name="pBusinessDate"/>

    <xsl:if test="./d[@n='Symbol']">
      <r>
        <xsl:attribute name="id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>
        <xsl:attribute name="src">
          <xsl:value-of select="@src"/>
        </xsl:attribute>

        <xsl:variable name ="vContractSymbol" >
          <xsl:value-of select ="d[@n='Symbol']/@v"/>
        </xsl:variable>

        <xsl:variable name ="vMaturityMonthYear" >
          <xsl:value-of select="concat(d[@n='Year']/@v,d[@n='Month']/@v,d[@n='Day']/@v)"/>
        </xsl:variable>

        <xsl:variable name ="vPutCall" >
          <xsl:choose>
            <xsl:when test="d[@n='CP']/@v= 'P'">0</xsl:when>
            <xsl:when test="d[@n='CP']/@v= 'C'">1</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name ="vStrike" >
          <xsl:value-of select="d[@n='Strike']/@v"/>
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
              <xsl:with-param name="pMATURITYMONTHYEAR" select="$vMaturityMonthYear"/>
              <xsl:with-param name="pPUTCALL" select="$vPutCall"/>
              <xsl:with-param name="pSTRIKEPRICE" select="$vStrike"/>
              <xsl:with-param name="pBUSINESSDATE" select="$pBusinessDate"/>
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
            ]]>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <p n="ISO10383" v="{$vISO10383}"/>
              <p n="CONTRACTSYMBOL" v="{$vContractSymbol}"/>
              <p n="CATEGORY" v="{$vCategory}"/>
              <p n="DT" t="dt" v="{$pBusinessDate}"/>
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
              <p n="DT" t="dt" v="{$pBusinessDate}"/>
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
          <c n="TIME" dk="true" t="dt" f="yyyy-MM-dd" v="{$pBusinessDate}">
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

      </r>
    </xsl:if>
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
     ]]>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <p n="DT" t="dt" v="{$pBUSINESSDATE}"/>
      <p n="ISO10383" v="{$pISO10383}"/>
      <p n="CONTRACTSYMBOL" v="{$pCONTRACTSYMBOL}"/>
      <p n="CATEGORY" v="{$pCATEGORY}"/>
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
            inner join dbo.TRADE tr on ( tr.IDASSET = a.IDASSET )
            where ( mk.ISO10383_ALPHA4 = @ISO10383 )
              and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
              and ( dc.CATEGORY = @CATEGORY )
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
      <p n="MATURITYMONTHYEAR" v="{$pMATURITYMONTHYEAR}"/>
      <xsl:if test = "$pCATEGORY = 'O'">
        <p n="PUTCALL" v="{$pPUTCALL}"/>
        <p n="STRIKEPRICE" t="dc" v="{$pSTRIKEPRICE}"/>
      </xsl:if>
    </sql>

  </xsl:template>

</xsl:stylesheet>