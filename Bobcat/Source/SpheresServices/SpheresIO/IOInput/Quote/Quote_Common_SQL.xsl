<?xml version="1.0" encoding="utf-8"?>
<!--
/*==============================================================*/
/* Summary : Common SQL Quotes                    
/*==============================================================*/
/* File    : Quote_Common_SQL.xsl                      
/* Version : v0.0.0.1                                  
/* Date    : 20110711                                  
/* Author  : MF                                        
/* Description: SQL Template common for table QUOTES   
/*==============================================================*/
Version : v3.4.0.0                                           
Date    : 20130529
Author  : EG
Comment : - Appel template "SysIns" de MarketData_Common.sql pour DTENABLED/DTINS/IDAINS.
*==============================================================*/
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:include href="..\Repository\MarketData_Common_SQL.xsl"/>

  <xsl:variable name="vVersionQuoteIdemImport">v0.0.0.1</xsl:variable>
  <xsl:variable name="vFileNameQuoteIdemImport">Quote_Common_SQL.xsl</xsl:variable>

  <!-- 
  ***********************************************
  ***********************************************
  SQL Template
  ***********************************************
  ***********************************************
  -->

  <!-- 
  Get an id asset, the used criterias depend by the pCategory parameter value  -->
  <xsl:template name="SelectIDASSET">
    <xsl:param name="pISO10383" select="$gNull"/>
    <xsl:param name="pExchangeSymbol" select="$gNull"/>
    <!-- contract symbol or identifier of the asset -->
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pContractSymbolSuffix" select="$gNull"/>
    <!-- category of the asset, values: 'O' -> Option 'F' -> Future 'I' -> Index 'E' -> Equity 'FX' -> Exchange rate -->
    <xsl:param name="pCategory" select="$gNull"/>
    <xsl:param name="pMaturityMonthYear" select="$gNull"/>
    <xsl:param name="pPutCall" select="$gNull"/>
    <xsl:param name="pStrike" select="$gNull"/>

    <!-- PL 20130215 Add cache="true" -->
    <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
    <SQL command="select" result="IDASSET" cache="true">
      <xsl:choose>
        <xsl:when test="$pCategory = 'F' or $pCategory = 'O'">
          <xsl:choose>
            <xsl:when test="$pISO10383 != $gNull">
              select a.IDASSET
              from dbo.ASSET_ETD a
              inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB
              inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY
              inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CATEGORY = @CATEGORY)
              inner join dbo.MARKET m on m.IDM = dc.IDM and (ISO10383_ALPHA4=@ISO10383_ALPHA4) and (m.EXCHANGESYMBOL = @EXCHANGESYMBOL)
              where (1=1)
              <xsl:if test="$pMaturityMonthYear != $gNull">
                and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
              </xsl:if>
              <xsl:if test="$pCategory = 'O'">
                and (a.PUTCALL = @PUTCALL) and (a.STRIKEPRICE = @STRIKE)
              </xsl:if>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <Param name="DT" datatype="date">
                <xsl:value-of select="$gParamDtBusiness"/>
              </Param>
              <Param name="ISO10383_ALPHA4" datatype="string">
                <xsl:value-of select="$pISO10383" />
              </Param>
              <Param name="EXCHANGESYMBOL" datatype="string">
                <xsl:value-of select="$pExchangeSymbol" />
              </Param>
              <Param name="CONTRACTSYMBOL" datatype="string">
                <xsl:value-of select="$pContractSymbol" />
              </Param>
              <Param name="CATEGORY" datatype="string">
                <xsl:value-of select="$pCategory" />
              </Param>
              <xsl:if test="$pMaturityMonthYear != $gNull">
                <Param name="MATURITYMONTHYEAR" datatype="string">
                  <xsl:value-of select="$pMaturityMonthYear" />
                </Param>
              </xsl:if>
              <xsl:if test="$pCategory = 'O'">
                <Param name="PUTCALL" datatype="string">
                  <xsl:value-of select="$pPutCall" />
                </Param>
                <Param name="STRIKE" datatype="decimal">
                  <xsl:value-of select="$pStrike" />
                </Param>
              </xsl:if>
            </xsl:when>

            <xsl:otherwise>
              select a.IDASSET
              from dbo.ASSET_ETD a
              inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB
              inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY
              inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.CATEGORY = @CATEGORY)
              inner join dbo.MARKET m on m.IDM = dc.IDM and (m.EXCHANGESYMBOL = @EXCHANGESYMBOL)
              where (1=1)
              <xsl:if test="$pMaturityMonthYear != $gNull">
                and (ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
              </xsl:if>
              <xsl:if test="$pCategory = 'O'">
                and (a.PUTCALL = @PUTCALL) and (a.STRIKEPRICE = @STRIKE)
              </xsl:if>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <Param name="DT" datatype="date">
                <xsl:value-of select="$gParamDtBusiness"/>
              </Param>
              <Param name="EXCHANGESYMBOL" datatype="string">
                <xsl:value-of select="$pExchangeSymbol" />
              </Param>
              <Param name="CONTRACTSYMBOL" datatype="string">
                <xsl:value-of select="$pContractSymbol" />
              </Param>
              <Param name="CATEGORY" datatype="string">
                <xsl:value-of select="$pCategory" />
              </Param>
              <xsl:if test="$pMaturityMonthYear != $gNull">
                <Param name="MATURITYMONTHYEAR" datatype="string">
                  <xsl:value-of select="$pMaturityMonthYear" />
                </Param>
              </xsl:if>
              <xsl:if test="$pCategory = 'O'">
                <Param name="PUTCALL" datatype="string">
                  <xsl:value-of select="$pPutCall" />
                </Param>
                <Param name="STRIKE" datatype="decimal">
                  <xsl:value-of select="$pStrike" />
                </Param>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <xsl:when test="$pCategory = 'E' or $pCategory = 'C'">
          select aeq.IDASSET
          from dbo.ASSET_EQUITY aeq
          where (aeq.SYMBOL = @SYMBOL)
          <xsl:if test="$pContractSymbolSuffix != $gNull">
            and (aeq.SYMBOLSUFFIX = @SYMBOLSUFFIX)
            <Param name="SYMBOLSUFFIX" datatype="string">
              <xsl:value-of select="$pContractSymbolSuffix" />
            </Param>
          </xsl:if>

          <Param name="SYMBOL" datatype="string">
            <xsl:value-of select="$pContractSymbol" />
          </Param>
        </xsl:when>

        <xsl:when test="$pCategory = 'I'">
          select distinct aid.IDASSET
          from dbo.ASSET_INDEX aid
          where (aid.IDENTIFIER = @IDENTIFIER)
          <Param name="IDENTIFIER" datatype="string">
            <xsl:value-of select="$pContractSymbol"/>
          </Param>
        </xsl:when>

        <xsl:when test="$pCategory = 'FX'">
          select distinct afx.IDASSET
          from dbo.ASSET_FXRATE afx
          where (afx.IDENTIFIER = @IDENTIFIER)
          <Param name="IDENTIFIER" datatype="string">
            <xsl:value-of select="$pContractSymbol"/>
          </Param>
        </xsl:when>

        <xsl:otherwise>
          <xsl:value-of select="$gNull"/>
        </xsl:otherwise>
      </xsl:choose>
    </SQL>
  </xsl:template>

  <!-- HACK MF 20111026 - the LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT is a shortcut 
  to have some underlying infos just passing by the contracts table.
  It is right (actually) for the EUREX market where the underlyer is specified at the contract level. -->

  <!-- 
  Get some underlying informations (id: IDASSET_UNL, table name: QUOTETABLENAME, id type: IDASSET_UNL_XXX) about a derivative contract.
  To get the underlying datas you need to pass the derivative contract symbol and its version (contract attribute, 
  needed by corporate actions, beacause any corporate action could produce a new underlying asset).
  Implemented categories:
  Bond (DebtSecurity), Deposit, EquityAsset, FxRateAsset, Index, MutualFund, RateIndex.
  Compliant markets: 
  - 'XEUR', ...
  -->
  <xsl:template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
    <!-- Mining field you want to retrieve: IDASSET_UNL, QUOTETABLENAME, IDASSET_UNL_XXX  -->
    <xsl:param name="pSelect"/>
    <!-- Exchange acronym of the market (Ex 'EUR' -> 'EUREX'), seealso column MARKET.EXCHANGEACRONYM-->
    <xsl:param name="pExchangeAcronym"/>
    <!-- derivative contract symbol  -->
    <xsl:param name="pContractSymbol"/>
    <!-- derivative contract version, it should be used for corporate actions only  -->
    <xsl:param name="pContractAttribute"/>
    <xsl:choose>
      <!-- list of the compliant markets, add other acronyms using 'or' condition -->
      <xsl:when test="$pExchangeAcronym = 'EUR'">
        <!-- BD 20130429 : Ajout de la condition du DTDISABLED pour sélectionner un DC vivant -->
        <sql cd="select" rt="{$pSelect}">
          select
          IDASSET_UNL,
          case ASSETCATEGORY
          when 'Bond' then 'QUOTE_DEBTSEC_H'
          when 'Deposit'then 'QUOTE_DEPOSIT_H'
          when 'EquityAsset' then 'QUOTE_EQUITY_H'
          when 'FxRateAsset' then 'QUOTE_FXRATE_H'
          when 'Index' then 'QUOTE_INDEX_H'
          when 'MutualFund' then 'QUOTE_MUTUALFUND_H'
          when 'RateIndex' then 'QUOTE_RATEINDEX_H'
          else null
          /*
          not managed asset categories:
          'Cash'
          'Commodity'
          'Future' 'ExchangeTradedContract'
          'ConvertibleBond'
          'ExchangeTradedFund'
          'SimpleCreditDefaultSwap'
          'SimpleFra'
          'SimpleIRSwap'*/
          end as QUOTETABLENAME,
          case ASSETCATEGORY
          when 'Bond' then IDASSET_UNL
          else null
          end as IDASSET_UNL_BOND,
          case ASSETCATEGORY
          when 'Deposit' then IDASSET_UNL
          else null
          end as IDASSET_UNL_DEPOSIT,
          case ASSETCATEGORY
          when 'EquityAsset' then IDASSET_UNL
          else null
          end as IDASSET_UNL_EQUITY,
          case ASSETCATEGORY
          when 'FxRateAsset' then IDASSET_UNL
          else null
          end as IDASSET_UNL_FXRATE,
          case ASSETCATEGORY
          when 'Index' then IDASSET_UNL
          else null
          end as IDASSET_UNL_INDEX,
          case ASSETCATEGORY
          when 'MutualFund' then IDASSET_UNL
          else null
          end as IDASSET_UNL_MUTUALFUND,
          case ASSETCATEGORY
          when 'RateIndex' then IDASSET_UNL
          else null
          end as IDASSET_UNL_RATEINDEX
          from dbo.DERIVATIVECONTRACT
          where (CONTRACTSYMBOL = @CSYMB) and (CONTRACTATTRIBUTE = @CATTR)
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <p n="DT" t="dt">
            <xsl:attribute name="v">
              <xsl:value-of select="$gParamDtBusiness"/>
            </xsl:attribute>
          </p>
          <p n="CSYMB" v="{$pContractSymbol}"/>
          <p n="CATTR" v="{$pContractAttribute}"/>
        </sql>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gNull"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- 
  ***********************************************
  ***********************************************
  Table Template
  ***********************************************
  ***********************************************
  -->

  <!--	Insert/Update a quote inside a QUOTE_XXX_H table
        Actually supported (validated) tables : 
        QUOTE_ETD_H / 
        QUOTE_INDEX_H / QUOTE_EQUITIES_H / QUOTE_FXRATE_H.
        Mandatory parameters :
        - Underlying not ETD : pIdAsset OR {pContractSymbol, pCategory} 
        - Underlying ETD : pIdAsset OR {pContractSymbol, pCategory, pMaturityMonthYear, pPutCall, pStrike}
        - pValue
        - pBusinessDate
        -->
  <xsl:template name="QUOTE_H_IU">
    <xsl:param name="pTableName" select="'QUOTE_UNK'"/>
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <xsl:param name="pISO10383" select="$gNull"/>
    <!-- BD 20130417 - Ajout du param pIDM -->
    <xsl:param name="pIDM" select="0"/>
    <!-- Identifier/symbol of the asset we need to insert/update the current quote pValue  -->
    <xsl:param name="pContractSymbol" select="$gNull"/>
    <!-- internal id of the asset we need to insert/update the current quote pValue  -->
    <xsl:param name="pIdAsset" select="$gNull"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pContractSymbolSuffix" select="$gNull"/>
    <xsl:param name="pCategory" select="$gNull"/>
    <xsl:param name="pMaturityMonthYear" select="$gNull"/>
    <xsl:param name="pPutCall" select="$gNull"/>
    <xsl:param name="pStrike" select="$gNull"/>
    <xsl:param name="pCurrency" select="$gNull"/>
    <xsl:param name="pExchangeSymbol" select="$gNull"/>
    <!-- Quote value -->
    <xsl:param name="pValue"/>
    <!-- Quote unit: Rate, Price, Exchange Rate ... Price is the default -->
    <xsl:param name="pQuoteUnit" select="'Price'"/>
    <!-- Quote side: Ask, Bid, ... OfficialClose is the default -->
    <xsl:param name="pQuoteSide" select="'OfficialClose'"/>
    <!-- Quote source, it should be the same value of PRIMARYRATESRC of the reference asset ... ClearingOrganization is the default -->
    <xsl:param name="pRateSource" select="'ClearingOrganization'"/>
    <xsl:param name="pIsWithControl" select="$gTrue"/>

    <table name="{$pTableName}" action="IU" sequenceno="{$pSequenceNumber}">

      <column name="IDMARKETENV" datakey="true" datatype="string">
        <xsl:value-of select="'DEFAULT_MARKET_ENV'"/>
      </column>

      <column name="IDVALSCENARIO" datakey="true" datatype="string">
        <xsl:value-of select="'EOD_VALUATION'"/>
      </column>

      <column name="IDASSET" datakey="true" datatype="integer">
        <xsl:choose>
          <xsl:when test="$pIdAsset = $gNull">
            <xsl:call-template name="SelectIDASSET">
              <xsl:with-param name="pISO10383" select="$pISO10383"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
              <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
              <xsl:with-param name="pContractSymbolSuffix" select="$pContractSymbolSuffix"/>
              <xsl:with-param name="pCategory" select="$pCategory"/>
              <xsl:with-param name="pMaturityMonthYear" select="$pMaturityMonthYear"/>
              <xsl:with-param name="pPutCall" select="$pPutCall"/>
              <xsl:with-param name="pStrike" select="$pStrike"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pIdAsset"/>
          </xsl:otherwise>
        </xsl:choose>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="NONE"/>
          </control>
        </controls>
      </column>

      <column name="TIME" datakey="true" datatype="date" dataformat="yyyy-MM-dd">
        <xsl:value-of select="$pBusinessDate"/>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>3001</number>
              <data1>
                <xsl:value-of select="$vFileNameQuoteIdemImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionQuoteIdemImport"/>
              </data2>
            </logInfo>
          </control>
          <control action="RejectRow" return="false" >
            <SpheresLib function="IsDate()" />
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>3002</number>
              <data1>
                <xsl:value-of select="$vFileNameQuoteIdemImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionQuoteIdemImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="IDBC" datakey="true"  datatype="string" >
        <xsl:value-of select="$gNull"/>
      </column>

      <column name="QUOTESIDE" datakey="true" datatype="string">
        <xsl:value-of select="$pQuoteSide"/>
      </column>

      <column name="CASHFLOWTYPE" datakey="true"  datatype="string">
        <xsl:value-of select="$gNull"/>
      </column>

      <column name="IDM" datakeyupd="true" datatype="integer">
        <xsl:choose>
          <!-- BD 20130417 - Ajout du param pIDM -->
          <xsl:when test="number($pIDM) != 0">
            <xsl:value-of select="$pIDM"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="SQLIdMarket2">
              <xsl:with-param name="pISO10383" select="$pISO10383"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </column>

      <column name="IDC" datakeyupd="true" datatype="string" >
        <xsl:value-of select="$pCurrency"/>
      </column>

      <column name="VALUE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pValue"/>
      </column>

      <column name="SPREADVALUE" datakeyupd="true" datatype="decimal">0</column>

      <column name="QUOTEUNIT" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pQuoteUnit"/>
      </column>

      <column name="QUOTETIMING" datakeyupd="true" datatype="string">Close</column>

      <column name="ASSETMEASURE" datatype="string">MarketQuote</column>

      <column name="ISENABLED" datakeyupd="true" datatype="bool">1</column>

      <column name="SOURCE" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pRateSource"/>
      </column>

      <column name="DTINS" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>
      <column name="IDAINS" datatype="int">
        <SpheresLib function="GetUserID()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>

    </table>
  </xsl:template>

  <!--	Insert/Update a quote inside a QUOTE_XXX_H table
        Actually supported (validated) tables : 
        QUOTE_ETD_H / 
        QUOTE_INDEX_H / QUOTE_EQUITIES_H / QUOTE_DEBTSEC_H / QUOTE_FXRATE_H / QUOTE_DEPOSIT_H / QUOTE_MUTUALFUND_H / QUOTE_RATEINDEX_H.
        Mandatory parameters :
        - pIdAsset 
        - pValue
        - pBusinessDate
        -->
  <xsl:template name="LIGHTQUOTE_H_IU">

    <xsl:param name="pTableName" select="'QUOTE_UNK'"/>
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- internal id of the asset we need to insert/update the current quote pValue  -->
    <xsl:param name="pIdAsset" select="$gNull"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pCurrency" select="$gNull"/>
    <!-- Quote value -->
    <xsl:param name="pValue"/>
    <!-- Quote unit: Rate, Price, Exchange Rate ... Price is the default -->
    <xsl:param name="pQuoteUnit" select="'Price'"/>
    <!-- Quote side: Ask, Bid, ... OfficialClose is the default -->
    <xsl:param name="pQuoteSide" select="'OfficialClose'"/>
    <!-- Quote source, it should be the same value of PRIMARYRATESRC of the reference asset ... ClearingOrganization is the default -->
    <xsl:param name="pRateSource" select="'ClearingOrganization'"/>
    <!-- insert date -->
    <xsl:param name="pDatetimeRDBMS"/>
    <!-- user performing the insert -->
    <xsl:param name="pUserId"/>
    <xsl:param name="pIsWithControl" select="$gTrue"/>

    <tbl n="{$pTableName}" a="IU" sn="{$pSequenceNumber}">

      <c n="IDMARKETENV" dk="true" v="DEFAULT_MARKET_ENV"/>

      <c n="IDVALSCENARIO" dk="true" v="EOD_VALUATION"/>

      <c n="IDASSET" dk="true" t="i" v="{$pIdAsset}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
          <!-- BD 20130225 Rejet de la ligne si IDASSET vaut 0 -->
          <ctl a="RejectRow" rt="0" lt="None">
            <xsl:value-of select="$pIdAsset"/>
          </ctl>
        </ctls>
      </c>

      <c n="TIME" dk="true" t="dt" v="{$pBusinessDate}" f="yyyy-MM-dd">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>

        <!-- UNDONE MF 20111106 Message loginfo pas disponibles dans SPheresIO pour la version allégée-->

        <!--<controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>3001</number>
              <data1>
                <xsl:value-of select="$vFileNameQuoteIdemImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionQuoteIdemImport"/>
              </data2>
            </logInfo>
          </control>
          <control action="RejectRow" return="false" >
            <SpheresLib function="IsDate()" />
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>3002</number>
              <data1>
                <xsl:value-of select="$vFileNameQuoteIdemImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionQuoteIdemImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>-->
      </c>

      <c n="IDBC" dk="true" v="{$gNull}"/>

      <c n="QUOTESIDE" dk="true" v="{$pQuoteSide}"/>

      <c n="CASHFLOWTYPE" dk="true" v="{$gNull}"/>

      <c n="IDM" dku="true" t="i" v="{$gNull}"/>

      <c n="IDC" dku="true" v="{$pCurrency}"/>

      <c n="VALUE" dku="true" t="dc" v="{$pValue}"/>

      <c n="SPREADVALUE" dku="true" t="dc" v="0"/>

      <c n="QUOTEUNIT" dku="true" v="{$pQuoteUnit}"/>

      <c n="QUOTETIMING" dku="true" v="Close"/>

      <c n="ASSETMEASURE" dku="true" v="MarketQuote"/>

      <c n="ISENABLED" dku="true" t="b" v="1"/>

      <c n="SOURCE" dku="true" v="{$pRateSource}"/>

      <!--
      *********************************************
      Sys columns
      *********************************************
      -->

      <c n="DTUPD" dku="true" t="dt" v="{$pDatetimeRDBMS}">
        <ctls>
          <ctl a="RejectColumn" rt="true" >
            <sl fn="IsInsert()" />
          </ctl>
        </ctls>
      </c>
      <c n="IDAUPD" dku="true" t="i" v="{$pUserId}">
        <ctls>
          <ctl a="RejectColumn" rt="true" >
            <sl fn="IsInsert()" />
          </ctl>
        </ctls>
      </c>

      <c n="DTINS" t="dt" v="{$pDatetimeRDBMS}">
        <ctls>
          <ctl a="RejectColumn" rt="true" >
            <sl fn="IsUpdate()" />
          </ctl>
        </ctls>
      </c>
      <c n="IDAINS" t="i" v="{$pUserId}">
        <ctls>
          <ctl a="RejectColumn" rt="true" >
            <sl fn="IsUpdate()" />
          </ctl>
        </ctls>
      </c>

    </tbl>

  </xsl:template>

  <!-- Insert/Update an Asset FXRATE -->
  <xsl:template name="ASSET_FXRATE_IU">

    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- first curreny of the exchange rate and reference currency (IDC) -->
    <xsl:param name="pCurrency1"/>
    <!-- second currency -->
    <xsl:param name="pCurrency2"/>
    <!-- asset identifier -->
    <xsl:param name="pIdentifier"/>
    <!-- extendedn exchange rate description -->
    <xsl:param name="pDescription"/>
    <!-- identifier of the business center -->
    <xsl:param name="pIDBC"/>
    <!-- rate source identifier -->
    <xsl:param name="pPrimaryRateSource"/>
    <!-- web portal of the rate source -->
    <xsl:param name="pPrimaryRateSourcePage"/>
    <!-- sys columns controls enabled/disabled -->
    <xsl:param name="pIsWithControl" select="$gTrue"/>

    <table name="ASSET_FXRATE" action="IU" sequenceno="{$pSequenceNumber}">

      <column name="IDASSET" datakey="true" datatype="integer">
        <xsl:call-template name="SelectIDASSET">
          <xsl:with-param name="pContractSymbol" select="$pIdentifier"/>
          <xsl:with-param name="pCategory" select="'FX'"/>
        </xsl:call-template>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">true</control>
        </controls>
      </column>

      <column name="IDENTIFIER" datakey="true">
        <xsl:value-of select="$pIdentifier"/>
      </column>

      <column name="DISPLAYNAME">
        <xsl:value-of select="$pIdentifier"/>
      </column>

      <column name="DESCRIPTION" datakeyupd="true">
        <xsl:value-of select="$pDescription"/>
      </column>

      <xsl:variable name="vParamCurrency1">
        <Param name="CURRENCY" datatype="string">
          <xsl:value-of select="$pCurrency1"/>
        </Param>
      </xsl:variable>

      <column name="QCP_IDC1" datakey="true">
        <xsl:call-template name="SQLIdCurrency">
          <xsl:with-param name="pParamCurrency">
            <xsl:copy-of select="$vParamCurrency1"/>
          </xsl:with-param>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>5302</number>
              <data1>
                <xsl:value-of select="$pCurrency1"/>
              </data1>
              <data2>
                <xsl:value-of select="$pIdentifier"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:variable name="vParamCurrency2">
        <Param name="CURRENCY" datatype="string">
          <xsl:value-of select="$pCurrency2"/>
        </Param>
      </xsl:variable>

      <column name="QCP_IDC2" datakey="true">
        <xsl:call-template name="SQLIdCurrency">
          <xsl:with-param name="pParamCurrency">
            <xsl:copy-of select="$vParamCurrency2"/>
          </xsl:with-param>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>5302</number>
              <data1>
                <xsl:value-of select="$pCurrency2"/>
              </data1>
              <data2>
                <xsl:value-of select="$pIdentifier"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <!-- HACK : hardcoded Currency1PerCurrency2 -->
      <column name="QCP_QUOTEBASIS" datakey="true">
        <xsl:value-of select="'Currency1PerCurrency2'"/>
      </column>

      <!-- HACK : hardcoded false -->
      <column name="ISDEFAULT" datatype="bool">
        <xsl:value-of select="'0'"/>
      </column>

      <!-- HACK : hardcoded 15:14 - standard Spheres for the exchange rates -->
      <column name="TIMERATESRC">
        <xsl:value-of select="'14:15'"/>
      </column>

      <column name="IDBCRATESRC" datakeyupd="true">
        <xsl:value-of select="$pIDBC"/>
      </column>

      <column name="PRIMARYRATESRC" datakeyupd="true">
        <xsl:value-of select="$pPrimaryRateSource"/>
      </column>

      <column name="PRIMARYRATESRCPAGE" datakeyupd="true">
        <xsl:value-of select="$pPrimaryRateSourcePage"/>
      </column>

      <column name="PRIMARYRATESRCHEAD">
        <xsl:value-of select="$pCurrency1"/>
      </column>

      <!--
      *********************************************
      Sys columns
      *********************************************
      -->

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
        <xsl:with-param name="pUseDtBusiness">
          <xsl:copy-of select="$gTrue"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>

    </table>

  </xsl:template>

  <!-- 
  ***********************************************
  ***********************************************
  Tool Template
  ***********************************************
  ***********************************************
  -->

  <!-- Block the IO task when the two input dates are different -->
  <xsl:template name="FileDateCheck">

    <!-- date parsed from the source file -->
    <xsl:param name="pFileDate"/>
    <!-- task parameter date -->
    <xsl:param name="pControlDate"/>
    <!-- source file name -->
    <xsl:param name="pFileName" select="'N/A'"/>
    <!-- source file path -->
    <xsl:param name="pFilePath" select="'N/A'"/>
    <!-- IO element -->
    <xsl:param name="pIOElemId" select="'N/A'"/>

    <table name="FILEDATECONTROL" action="U" sequenceno="0">

      <column name="FILEDATECONTROL">
        <xsl:choose>
          <xsl:when test="$pFileDate = $pControlDate">
            <xsl:value-of select="$pFileDate"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gNull"/>
          </xsl:otherwise>
        </xsl:choose>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>5301</number>
              <data1>
                <xsl:value-of select="$pFileDate"/>
              </data1>
              <data2>
                <xsl:value-of select="$pControlDate"/>
              </data2>
              <data3>
                <xsl:value-of select="$pFileName"/>
              </data3>
              <data4>
                <xsl:value-of select="$pFilePath"/>
              </data4>
              <data5>
                <xsl:value-of select="$pIOElemId"/>
              </data5>
            </logInfo>
          </control>
          <control action="RejectRow" return="false" >
            <SpheresLib function="ISNULL()" />
            <logInfo status="NA" isexception="false">
              <code>LOG</code>
              <number>5301</number>
              <data1>
                <xsl:value-of select="$pFileDate"/>
              </data1>
            </logInfo>
          </control>
        </controls>
      </column>

    </table>

  </xsl:template>

</xsl:stylesheet>