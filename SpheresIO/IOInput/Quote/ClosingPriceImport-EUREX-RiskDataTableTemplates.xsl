<!--
/*==============================================================
/* Summary : Import EUREX parameters and underlying closing prices - Table Templates                          
/*==============================================================
/* File    : ClosingPriceImport-EUREX-RiskDataTableTemplates.xsl    
/* Version : 0.0.0.1
/* Date    : 20111024                                          
/* Author  : MF                                                 
/* Description: 
    

/*==============================================================*/
/*==============================================================
/* Revision:                                           
/*                                                            
/* Date    :                                          
/* Author  :                                                 
/* Version :                                             	      
/* Comment : 
                    
/*==============================================================*/
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="Quote_Common_SQL.xsl"/>

  <xsl:variable name="vVersionRiskDataTableTemplates">v0.0.0.1</xsl:variable>
  <xsl:variable name="vFileNameRiskDataTableTemplates">ClosingPriceImport-EUREX-RiskDataTableTemplates.xsl</xsl:variable>

  <!-- 
  ***********************************************
  ***********************************************
  SQL Template
  ***********************************************
  ***********************************************
  -->

  <!-- get the symbol from the PARAMSEUREX_CONTRACT table, 
  passing the symbol itself  -->
  <xsl:template name="SQL_CONTRACTSYMBOL">
    <xsl:param name="pContractSymbol"/>
    <sql cd="select" rt="CONTRACTSYMBOL">
      select CONTRACTSYMBOL
      from PARAMSEUREX_CONTRACT
      where
      CONTRACTSYMBOL = @CSYMB
      <p n="CSYMB" v="{$pContractSymbol}"/>
    </sql>
  </xsl:template>

  <!-- get the internal id of the maturity from the PARAMSEUREX_MATURITY table 
  passing a specific contract (symbol + putcall indicator), a maturitymonthyear and a business date -->
  <xsl:template name="SQL_IDPARAMSEUREX_MATURITY">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pDtMarket"/>
    <xsl:param name="pMaturityYearMonth"/>
    <xsl:variable name="vPutCall">
      <xsl:call-template name="NullableString">
        <xsl:with-param name="pCheckValue" select="$pPutCall"/>
      </xsl:call-template>
    </xsl:variable>
    <sql cd="select" rt="IDPARAMSEUREX_MATURITY">
      select pm.IDPARAMSEUREX_MATURITY
      from PARAMSEUREX_MATURITY pm
      inner join PARAMSEUREX_CONTRACT pc on pc.CONTRACTSYMBOL = pm.CONTRACTSYMBOL
      where
      pc.CONTRACTSYMBOL = @CSYMB
      <xsl:if test="$vPutCall != $gNull">
        and pm.PUTCALL = @PC
      </xsl:if>
      and pm.MATURITYYEARMONTH = @MYM
      and pm.DTMARKET = @DTMKT
      <p n="CSYMB" v="{$pContractSymbol}"/>
      <xsl:if test="$vPutCall != $gNull">
        <p n="PC" v="{$vPutCall}"/>
      </xsl:if>
      <p n="MYM" v="{$pMaturityYearMonth}"/>
      <p n="DTMKT" v="{$pDtMarket}" t="dt"/>
    </sql>
  </xsl:template>

  <!-- 
  ***********************************************
  ***********************************************
  Table Template
  ***********************************************
  ***********************************************
  -->

  <!-- Update a group/class element of the EUREX hierarchy (table PARAMSEUREX_CONTRACT) -->
  <!-- the group/class element must be exist -->
  <xsl:template name="LIGHT_PARAMSEUREX_CONTRACT_U">

    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- internal id of the derivative contract -->
    <xsl:param name="pIDDC"/>
    <!-- contract symbol -->
    <xsl:param name="pSym"/>
    <!-- tick size -->
    <xsl:param name="pPrdTk"/>
    <!-- tick value -->
    <xsl:param name="pPrdTkV"/>
    <!-- Margining style (F,T) -->
    <xsl:param name="pMgnStl"/>
    <!-- update date -->
    <xsl:param name="pDatetimeRDBMS"/>
    <!-- user performing the update -->
    <xsl:param name="pUserId"/>

    <tbl n="PARAMSEUREX_CONTRACT" a="U" sn="{$pSequenceNumber}">

      <c n="IDDC_Control" v="{$pIDDC}">
        <ctls>
          <!-- when the contract is null,it means that does not exists, the update can not be executed, and the row is silently rejected -->
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>

      <c n="CONTRACTSYMBOL" dk="true" v="{$pSym}">
        <ctls>
          <!-- when the contract symbol is null,it means that does not exists, the update can not be executed, 
          and the row is silently rejected -->
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="TICKSIZE" dku="true" t="dc" v="{$pPrdTk}"/>

      <c n="TICKVALUE" dku="true" t="dc" v="{$pPrdTkV}"/>

      <c n="MARGIN_STYLE" dku="true" v="{$pMgnStl}"/>

      <!--
      *********************************************
      Sys columns
      *********************************************
      -->

      <c n="DTUPD" dku="true" t="dt" v="{$pDatetimeRDBMS}"/>

      <c n="IDAUPD" dku="true" t="i" v="{$pUserId}"/>

    </tbl>

  </xsl:template>

  <!-- Insert a new entry inside of the PARAMSEUREX_MATURITY table, the entry is related to a specific maturity -->
  <xsl:template name="LIGHT_PARAMSEUREX_MATURITY_I">
    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- internal id of the table, used to check when the inserting element already exists in the table -->
    <xsl:param name="pIdParamsEurex_Maturity"/>
    <!-- identifier of the contract parameter (the contract symbol is a unique key in the FPTHED file), it may be NULL in roder to reject the row silently 
    when the contract parameter has not been inserted inside of the PARAMSEUREX_CONTRACT table-->
    <xsl:param name="pContractSymbol"/>
    <!-- putcall indicator, null for futures -->
    <xsl:param name="pPutCall"/>
    <!-- maturity code. Format: YYYYMM.
    Built by this concatenation: '20' + SERI-EXP-DAT-RMTHED or '20' + EXPI-YR-DAT-RMTHED + EXPI-MTH-DAT-RMTHED -->
    <xsl:param name="pMaturityYearMonth"/>
    <!-- current business date -->
    <xsl:param name="pDtMarket"/>
    <!-- Security free interest rate 
    INTR-RAT-PCT-RMTHED	-->
    <xsl:param name="pItrRat"/>
    <!-- Yield rate
    YIELD-RAT-PCT-RMTHED	-->
    <xsl:param name="pYldRat"/>
    <!-- insert/update date -->
    <xsl:param name="pDatetimeRDBMS"/>
    <!-- user performing the insert/update -->
    <xsl:param name="pUserId"/>

    <xsl:variable name="vPutCall">
      <xsl:call-template name="NullableString">
        <xsl:with-param name="pCheckValue" select="$pPutCall"/>
      </xsl:call-template>
    </xsl:variable>

    <tbl n="PARAMSEUREX_MATURITY" a="I" sn="{$pSequenceNumber}">

      <c n="IDPARAMSEUREX_MATURITY_Control" v="{$pIdParamsEurex_Maturity}">
        <ctls>
          <!-- when the element already exists then we silently reject the inserting row -->
          <ctl a="RejectRow" rt="false" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>

      <!-- it is possible to pass a NULL value to reject the row silently, IOW this column is also a control column -->
      <c n="CONTRACTSYMBOL" dk="true" v="{$pContractSymbol}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="PUTCALL" dk="true" v="{$vPutCall}"/>

      <c n="MATURITYYEARMONTH" dk="true" v="{$pMaturityYearMonth}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="DTMARKET" dk="true" t="dt" v="{$pDtMarket}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="THEORETICAL_INTEREST_RATE" t="dc" v="{$pItrRat}"/>

      <c n="THEORETICAL_YIELD_RATE" t="dc" v="{$pYldRat}"/>

      <!--
      *********************************************
      Sys columns
      *********************************************
      -->

      <c n="DTINS" t="dt" v="{$pDatetimeRDBMS}"/>

      <c n="IDAINS" t="i" v="{$pUserId}"/>

    </tbl>

  </xsl:template>

  <!-- Insert/Update a new entry inside of the PARAMSEUREX_ASSET table, the entry is related to a single asset ETD -->
  <xsl:template name="LIGHT_PARAMSEUREX_ASSETETD_IU">
    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- internal id of the asset -->
    <xsl:param name="pIdAsset"/>
    <!-- internal id of the eurex parameter maturity -->
    <xsl:param name="pIdParamsEurex_Maturity"/>
    <!-- Trade Unit (to compute the contract multiplier) 
    SECU-TRD-UNT-NO-RMTHED	-->
    <xsl:param name="pTrdUnt"/>
    <!-- Asset price
    SERI-REF-PRC-RMTHED	-->
    <xsl:param name="pPrice"/>
    <!-- Underlying price (pour EOD files it is the closing price), pour les Futures is the settlement price
    UND-REF-PRC-RMTHED	-->
    <xsl:param name="pUndPrice"/>
    <!-- Volatility
    VOL-RMTHED	-->
    <xsl:param name="pVol"/>
    <!-- insert/update date -->
    <xsl:param name="pDatetimeRDBMS"/>
    <!-- user performing the insert/update -->
    <xsl:param name="pUserId"/>

    <tbl n="PARAMSEUREX_ASSETETD" a="IU" sn="{$pSequenceNumber}">

      <c n="IDASSET" dk="true" t="i" v="{$pIdAsset}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="IDPARAMSEUREX_MATURITY" dk="true" t="i" v="{$pIdParamsEurex_Maturity}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="TRADE_UNIT" dku="true" t="dc" v="{$pTrdUnt}"/>

      <c n="VALUE_QUOTE_ASSETETD" dku="true" t="dc" v="{$pPrice}"/>

      <c n="VALUE_QUOTE_UNL" dku="true" t="dc" v="{$pUndPrice}"/>

      <c n="VOLATILITY" dku="true" t="dc" v="{$pVol}"/>

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

  <!-- Insert/Update asset theoretical values inside of the PARAMSEUREX_VOLATILITY table -->
  <xsl:template name="LIGHT_PARAMSEUREX_VOLATILITY_IU">
    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- internal id of the asset -->
    <xsl:param name="pIdAsset"/>
    <!-- internal id of the eurex parameter maturity -->
    <xsl:param name="pIdParamsEurex_Maturity"/>
    <!-- Index of the array/scenario
    RISK-ARRAY-INDEX-RMTHED-->
    <xsl:param name="pRskArrIdx"/>
    <!-- Underlying price, to evaluate the execution/assignation risk value (value = pUnlPrx - pStrike)
    CALC-UND-PRICE-RMTHED-->
    <xsl:param name="pUnlPrx"/>
    <!-- strike price of the pIdAsset serie, to evaluate the execution/assignation risk value (value = pUnlPrx - pStrike)
    EXEC-PRC-RMTHED-->
    <xsl:param name="pStrike"/>
    <!-- Indicator for the projected undelrlying price (0,1,2,3).
    STOCK-PRC-IND-RMTHED-->
    <xsl:param name="pQtUnlInd"/>
    <!-- Indicator whether the projected underlying price is lesser, equals or greater than the closing price (D,N,U). Used to choose
    which side column fill with the following values.
    PRICE-UND-IND-RMTHED -->
    <xsl:param name="pQtCmp"/>
    <!-- Flag to fill the right side value column (D -> DOWNXXX,N -> NTRLXXX,U -> UPXXX). 
    VOLA-UND-IND-RMTHED-->
    <xsl:param name="pQtCmpFlag"/>
    <!-- Adjusted Volatility
    ADJ-VOL-RMTHED-->
    <xsl:param name="pAdjVol"/>
    <!-- Side risk value
    ADJ-VOL-RMTHED-->
    <xsl:param name="pTheVal"/>
    <!-- Short option adjustement
    SHORT-OPT-THEO-RMTHED-->
    <xsl:param name="pSrtOptAdj"/>
    <!-- insert/update date -->
    <xsl:param name="pDatetimeRDBMS"/>
    <!-- user performing the insert/update -->
    <xsl:param name="pUserId"/>

    <xsl:variable name="vRiskValueExeAss" select="number($pUnlPrx) - number($pStrike)"/>

    <xsl:variable name="vSrtOptAdj">
      <xsl:call-template name="Nullable">
        <xsl:with-param name="pCheckValue" select="$pSrtOptAdj"/>
      </xsl:call-template>
    </xsl:variable>

    <tbl n="PARAMSEUREX_VOLATILITY" a="IU" sn="{$pSequenceNumber}">

      <c n="RISKARRAY_INDEX" dk="true" t="i" v="{$pRskArrIdx}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="IDASSET" dk="true" t="i" v="{$pIdAsset}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="IDPARAMSEUREX_MATURITY" dk="true" t="i" v="{$pIdParamsEurex_Maturity}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="ISNULL()"/>
          </ctl>
        </ctls>
      </c>

      <c n="QUOTE_UNL_INDICATOR" dku="true" v="{$pQtUnlInd}"/>

      <c n="QUOTE_ETD_UNL_COMPARE" dku="true" v="{$pQtCmp}"/>

      <c n="RISKVALUE_EXEASS" dku="true" t="dc" v="{$vRiskValueExeAss}"/>

      <xsl:choose>
        <xsl:when test="$pQtCmpFlag = 'U'">
          <c n="UPVOLATILITY" dku="true" t="dc" v="{$pAdjVol}"/>

          <c n="UPTHEORETICAL_VALUE" dku="true" t="dc" v="{$pTheVal}"/>

          <c n="UPSHORTOPTADJUSTMENT" dku="true" t="dc" v="{$vSrtOptAdj}"/>
        </xsl:when>

        <xsl:when test="$pQtCmpFlag = 'N'">
          <c n="NTRLVOLATILITY" dku="true" t="dc" v="{$pAdjVol}"/>

          <c n="NTRLTHEORETICAL_VALUE" dku="true" t="dc" v="{$pTheVal}"/>

          <c n="NTRLSHORTOPTADJUSTMENT" dku="true" t="dc" v="{$vSrtOptAdj}"/>
        </xsl:when>

        <xsl:when test="$pQtCmpFlag = 'D'">
          <c n="DOWNVOLATILITY" dku="true" t="dc" v="{$pAdjVol}"/>

          <c n="DOWNTHEORETICAL_VALUE" dku="true" t="dc" v="{$pTheVal}"/>

          <c n="DOWNSHORTOPTADJUSTMENT" dku="true" t="dc" v="{$vSrtOptAdj}"/>
        </xsl:when>
      </xsl:choose>

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

  <!-- 
  ***********************************************
  ***********************************************
  Tool Template
  ***********************************************
  ***********************************************
  -->
  <!-- RD 20130827 [18834] Gestion de la checkbox "Importation systématique des cours" -->
  <!-- Ajout du paramètre pQuoteSide -->
  <xsl:template name="InsertUnderlyingQuotes">
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <xsl:param name="pIdUndAsset" />
    <xsl:param name="pTableUndQuote" />
    <xsl:param name="pBusinessDate" />
    <xsl:param name="pCurrency" select="$gNull"/>
    <xsl:param name="pValue"/>
    <!-- Quote side: Ask, Bid, ... OfficialClose is the default -->
    <xsl:param name="pQuoteSide" select="'OfficialClose'"/>
    <xsl:param name="pDatetimeRDBMS" />
    <xsl:param name="pUserId" />

    <!-- BD & FL 20130530 : Ne pas alimenter QUOTE_INDEX_H quand pValue=0 -->
    <xsl:if test="($pTableUndQuote != 'QUOTE_INDEX_H') or (number($pValue) != 0)">
      <xsl:if test="$pTableUndQuote != 'QUOTE_ETD_H'">
        <xsl:if test="$pTableUndQuote != 'Null'">
          <xsl:call-template name="LIGHTQUOTE_H_IU">
            <xsl:with-param name="pTableName" select="$pTableUndQuote"/>
            <xsl:with-param name="pSequenceNumber" select="$pSequenceNumber"/>
            <xsl:with-param name="pIdAsset" select="$pIdUndAsset"/>
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pValue" select="$pValue"/>
            <xsl:with-param name="pQuoteSide" select="$pQuoteSide"/>
            <xsl:with-param name="pDatetimeRDBMS" select="$pDatetimeRDBMS"/>
            <xsl:with-param name="pUserId" select="$pUserId"/>
            <xsl:with-param name="pIsWithControl" select="$gTrue"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="BuildUnderlyerParameters">
    <xsl:param name="pMarket"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pVersion"/>
    <pm n="IdAsset_Unl_Bond">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_BOND'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
    <pm n="IdAsset_Unl_Deposit">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_DEPOSIT'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
    <pm n="IdAsset_Unl_Equity">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_EQUITY'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
    <pm n="IdAsset_Unl_FxRate">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_FXRATE'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
    <pm n="IdAsset_Unl_Index">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_INDEX'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
    <pm n="IdAsset_Unl_MutualFund">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_MUTUALFUND'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
    <pm n="IdAsset_Unl_RateIndex">
      <xsl:call-template name="LIGHTSQL_UnderlyingDatas_DERIVATIVECONTRACT">
        <xsl:with-param name="pSelect" select="'IDASSET_UNL_RATEINDEX'"/>
        <xsl:with-param name="pExchangeAcronym" select="$pMarket"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pContractAttribute" select="$pVersion"/>
      </xsl:call-template>
    </pm>
  </xsl:template>

</xsl:stylesheet>

