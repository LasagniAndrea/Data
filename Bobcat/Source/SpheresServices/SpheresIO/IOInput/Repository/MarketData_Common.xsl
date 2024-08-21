<?xml version="1.0" encoding="utf-8"?>
<!--
/*==========================================================================*/
/* Summary : Common Import Referential                                      */
/*==========================================================================*/
/* File    : MarketData_Common.xsl                                          */
/* Version : v8.1.7737                                                      */
/* Date    : 20210308                                                       */
/* Author  : FL                                                             */
/* Description:                                                             */
/*  [22979] - Stock Future Cash IDEM - Metodo di liquidazione errato        */
/*             Updating GetSettltMethod template management of an           */
/*      exception for Stock Dividend Future on IDEM market.                 */
/*==========================================================================*/
/* File    : MarketData_Common.xsl                                          */
/* Version : v1.0.3.0                                                       */
/* Date    : 20100407                                                       */
/* Author  : GP                                                             */
/* Description: Template common for IDEM LSD Marketdata import              */
/* Comment :                                                                */
/*    Variables                                                             */
/*    - *NEW* gEquityAsset                                                  */
/*    - *NEW* gBond                                                         */
/*    - *NEW* gIndex                                                        */
/*    - *NEW* gRateIndex                                                    */
/*    - *NEW* gEQTY                                                         */
/*    - *NEW* gFut                                                          */
/*    - *NEW* gFS                                                           */
/*    - *NEW* gFI                                                           */
/*    - *NEW* gFD                                                           */
/*    Templates                                                             */
/*    - *NEW* CallPut                                                       */
/*    - *NEW* GetCurrency                                                   */
/*    - *NEW* GetAssetCategory                                              */
/*    - *NEW* GetFutValuationMethod                                         */
/*    - *NEW* GetUnderlyingGroup                                            */
/*    - *NEW* GetSettltMethod                                               */
/*==========================================================================*/
/* File    : MarketData_Common.xsl                                          */
/* Version : v1.0.1.0                                                       */
/* Date    : 20100220                                                       */
/* Author  : PM/MF/RD                                                       */
/* Description: Template common for LSD Marketdata import                   */
/*==========================================================================*/
/* Revision:                                                                */
/* Version : v5.1.6143.0                                                    */
/* Date    : 20161026  [34191]                                              */
/* Author  : FL                                                             */
/* Description: in template GetSettltMethod, management of an               */
/*      exception for Equity Options on IDEM market.                        */
/*      - Options contracts with contract symbol preceeded by               */
/*        4 are set as Cash Settlement method, otherwise as                 */
/*        Physical method                                                   */
/*==========================================================================*/
/* Revision:                                                                */
/*                                                                          */
/* Date    : 20100222                                                       */
/* Author  : v1.0.2.0                                                       */
/* Version :                                             	                  */
/* Comment :                                                                */
/*      - *NEW* DateBeautifier                                              */
/*==========================================================================*/
/* Revision:                                                                */
/*                                                                          */
/* Date    :  20100220                                                      */
/* Author  : v1.0.1.0                                                       */
/* Version :                                             	                  */
/* Comment :                                                                */
/*  - *UPD* DerivativeContractIdentifier - identifier built                 */
/*           with or without market code                                    */
/*  - *NEW* ContractGroupIdentifier template                                */
/*  - *NEW* ContractGroupDisplayName template                               */
/*  - *NEW* StrikePriceDiv template                                         */
/*                                                                          */
/*==========================================================================*/
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Global variables-->
  <xsl:decimal-format name="decimalFormat" decimal-separator="."/>
  <xsl:variable name="gAutomaticCompute">%%AUTOMATIC_COMPUTE%%</xsl:variable>
  <xsl:variable name="gNull1">null</xsl:variable>
  
  <!-- Variables partagées entre span_par et span_map pour identifier le type de 'row'-->
  <xsl:variable name="vDataTypeMarket" select="'M'"/>
  <xsl:variable name="vDataTypeContract" select="'C'"/>
  <xsl:variable name="vDataTypeExpiry" select="'E'"/>
  <xsl:variable name="vDataTypeAsset" select="'A'"/>

  <!-- AssetCategory: Equity Asset -->
  <xsl:variable name="gEquityAsset">
    <xsl:value-of select="'EquityAsset'"/>
  </xsl:variable>

  <!-- AssetCategory: Bond -->
  <xsl:variable name="gBond">
    <xsl:value-of select="'Bond'"/>
  </xsl:variable>

  <!-- AssetCategory: Index -->
  <xsl:variable name="gIndex">
    <xsl:value-of select="'Index'"/>
  </xsl:variable>

  <!-- AssetCategory: Index -->
  <xsl:variable name="gRateIndex">
    <xsl:value-of select="'RateIndex'"/>
  </xsl:variable>

  <!-- AssetCategory: Securities -->
  <xsl:variable name="gSecurities">
    <xsl:value-of select="'Securities'"/>
  </xsl:variable>

  <xsl:variable name="gEQTY">
    <xsl:value-of select="'EQTY'"/>
  </xsl:variable>

  <xsl:variable name="gFut">
    <xsl:value-of select="'FUT'"/>
  </xsl:variable>

  <!-- Enum UnderlyingAssetEnum FS (Stock-Equities) -->
  <xsl:variable name="gFS">
    <xsl:value-of select="'FS'"/>
  </xsl:variable>

  <!-- Eum UnderlyingAssetEnum FI (Indices) -->
  <xsl:variable name="gFI">
    <xsl:value-of select="'FI'"/>
  </xsl:variable>

  <!-- UnderlyingAssetEnum FD (Interest rate/notional debt securities) -->
  <xsl:variable name="gFD">
    <xsl:value-of select="'FD'"/>
  </xsl:variable>
  <!-- *************************************************************** -->
  <!-- Template à partager entre les différents imports LSD_MarketData -->
  <!-- *************************************************************** -->

  <!-- Templates généralistes -->
  <xsl:template name="IOTaskAtt">
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
  </xsl:template>
  <xsl:template name="IOTaskDetAtt">
    <xsl:attribute name="id">
      <xsl:value-of select="@id"/>
    </xsl:attribute>
    <xsl:attribute name="loglevel">
      <xsl:value-of select="@loglevel"/>
    </xsl:attribute>
    <xsl:attribute name="commitmode">
      <xsl:value-of select="@commitmode"/>
    </xsl:attribute>
  </xsl:template>
  <xsl:template name="IOInputAtt">
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
  </xsl:template>
  <xsl:template name="IOFileAtt">
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
  </xsl:template>
  <xsl:template name="IORowAtt">
    <xsl:param name="pId" select="@id"/>
    <xsl:param name="pSrc" select="@src"/>

    <xsl:attribute name="id">
      <xsl:value-of select="$pId"/>
    </xsl:attribute>
    <xsl:attribute name="src">
      <xsl:value-of select="$pSrc"/>
    </xsl:attribute>
  </xsl:template>

  <!-- Commun pour tous les Imports -->
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
  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="ioinput"/>
    </iotaskdet>
  </xsl:template>
  <xsl:template match="ioinput">
    <ioinput>
      <xsl:call-template name="IOInputAtt"/>
      <xsl:apply-templates select="file"/>
    </ioinput>
  </xsl:template>

  <xsl:template name="ExerciseStyle">
    <xsl:param name="pExerciseStyle"/>

    <xsl:choose>
      <xsl:when test="$pExerciseStyle='E'">
        <xsl:value-of select="'0'"/>
      </xsl:when>
      <xsl:when test="$pExerciseStyle='A'">
        <xsl:value-of select="'1'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="AssetIdentifier">
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAssetIdentifierFromFile"/>
    <xsl:param name="pAIICode"/>

    <xsl:choose>
      <xsl:when test="string-length($pAssetISINCode) > 0">
        <xsl:value-of select="$pAssetISINCode"/>
      </xsl:when>
      <xsl:when test="string-length($pAssetIdentifierFromFile) > 0">
        <xsl:value-of select="$pAssetIdentifierFromFile"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pAIICode"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="InstrumentIdentifier">
    <xsl:param name="pCategory"/>

    <xsl:choose>
      <xsl:when test="$pCategory='F'">
        <xsl:value-of select="'ExchangeTradedFuture'"/>
      </xsl:when>
      <xsl:when test="$pCategory='O'">
        <xsl:value-of select="'ExchangeTradedOption'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="FutValuationMethod">
    <xsl:param name="pCategory"/>

    <xsl:choose>
      <xsl:when test="$pCategory='F'">
        <xsl:value-of select="'FUT'"/>
      </xsl:when>
      <xsl:when test="$pCategory='O'">
        <xsl:value-of select="'EQTY'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="AssignmentMethod">
    <xsl:param name="pCategory"/>

    <xsl:choose>
      <xsl:when test="$pCategory='F'">
        null
      </xsl:when>
      <xsl:when test="$pCategory='O'">
        <xsl:value-of select="'R'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="MaturityRuleIdentifier">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:choose>
      <xsl:when test="$pExchangeSymbol = ''">
        <xsl:value-of select="$pContractSymbol"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(concat($pExchangeSymbol,'-'),$pContractSymbol)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  
  <xsl:template name="DerivativeContractIdentifier">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:choose>
      <xsl:when test="$pExchangeSymbol = ''">
        <xsl:value-of select="$pContractSymbol"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(concat($pExchangeSymbol,' '),$pContractSymbol)"/>
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>

  <xsl:template name="ContractGroupIdentifier">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:value-of select="concat(concat($pExchangeSymbol,' '),$pContractSymbol)"/>
  </xsl:template>

  <!--xsl:template name="MaturityDate">
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityDate"/>

    <xsl:choose>
      <xsl:when test="string-length($pMaturityMonthYear) = 8">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pMaturityDate"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template-->

  <xsl:template name="NewMaturityDate">
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pMaturityFormat" select="$gNull1"/>

    <xsl:variable name="vMaturityFormat">
      <xsl:choose>
        <xsl:when test="$pMaturityFormat = $gNull1">
          <xsl:call-template name="MaturityFormat">
            <xsl:with-param name="pMaturityMonthYear" select="$pMaturityMonthYear"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pMaturityFormat"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <xsl:choose>
      <xsl:when test="$vMaturityFormat = '1'">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pMaturityDate"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="MaturityFormat">
    <xsl:param name="pMaturityMonthYear"/>

    <xsl:choose>
      <xsl:when test="string-length($pMaturityMonthYear) = 6">0</xsl:when>
      <xsl:when test="(string-length($pMaturityMonthYear) = 8) and (substring($pMaturityMonthYear , 7, 1) = 'W')">2</xsl:when>
      <xsl:when test="string-length($pMaturityMonthYear) = 8">1</xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <xsl:template name="AIICode">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pAssetStrikePrice"/>

    <xsl:variable name="pFormattedMaturityDate">
      <xsl:call-template name="DateBeautyfier">
        <xsl:with-param name="pDate" select="$pMaturityDate"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$pCategory='F'">
        <xsl:value-of select="
                              concat( 
                                  substring( concat( $pExchangeSymbol, '    ' ), 1, 4 ), 
                                  substring( concat( $pContractSymbol, '            ' ), 1, 12 ), 
                                  'F ', 
                                  $pFormattedMaturityDate 
                              )"/>
      </xsl:when>
      <xsl:when test="$pCategory='O'">
        <xsl:value-of select="
                              concat( 
                                  substring( concat( $pExchangeSymbol, '    ' ), 1, 4 ), 
                                  substring( concat( $pContractSymbol, '            ' ), 1, 12 ) , 
                                  'O' , 
                                  $pPutCall, 
                                  $pFormattedMaturityDate, 
                                  $pAssetStrikePrice
                              )"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="DateBeautyfier">
    <xsl:param name="pDate"/>
      <xsl:choose>
        <xsl:when test="string-length($pDate) = 10 ">
          <xsl:value-of select="
                  concat( 
                      substring( $pDate, 1, 4 ), 
                      substring( $pDate, 6, 2 ), 
                      substring( $pDate, 9, 2) 
                  )"/>
        </xsl:when>
        <xsl:when test="string-length($pDate) = 8 ">
          <xsl:value-of select="
                  concat( 
                      substring( $pDate, 1, 4 ), 
                      '-', 
                      substring( $pDate, 5, 2 ), 
                      '-', 
                      substring( $pDate, 7, 2 ) 
                  )"/>
        </xsl:when>
        <xsl:when test="string-length($pDate) = 6 ">
          <xsl:value-of select="
                  concat( 
                      substring( $pDate, 1, 4 ), 
                      '-', 
                      substring( $pDate, 5, 2 ) 
                  )"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Unknown format'"/>
        </xsl:otherwise>
      </xsl:choose>
  </xsl:template>

  <xsl:template name="DisplayName">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pAssetStrikePrice"/>

    <xsl:variable name="pFormattedMaturityDate">
      <xsl:call-template name="DateBeautyfier">
        <xsl:with-param name="pDate" select="$pMaturityDate"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$pCategory='F'">
        <xsl:value-of select="
                              concat( 
                                  substring( concat( $pExchangeSymbol, '    ' ), 1, 4 ), 
                                  substring( concat( $pContractSymbol, '            ' ), 1, 12 ), 
                                  'F ', 
                                  $pFormattedMaturityDate
                              )"/>
      </xsl:when>
      <xsl:when test="$pCategory='O'">
        <xsl:value-of select="
                              concat( 
                                  substring( concat( $pExchangeSymbol, '    ' ), 1, 4 ), 
                                  substring( concat( $pContractSymbol, '            ' ), 1, 12 ) , 
                                  'O' , 
                                  $pPutCall, 
                                  $pFormattedMaturityDate,
                                  ' ',  
                                  $pAssetStrikePrice
                              )"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="ContractGroupDisplayName">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:value-of select="concat(concat($pExchangeSymbol,' '),$pContractSymbol)"/>
  </xsl:template>

  <!-- Compute strike price div fqctor -->
  <xsl:template name="StrikePriceDiv">
    <xsl:param name="pHowManyZero"/>
    <xsl:choose>
      <xsl:when test="number($pHowManyZero) > 0">
        <xsl:variable name="vRecursiveResult">
          <xsl:call-template name="StrikePriceDiv">
            <xsl:with-param name="pHowManyZero" select="number($pHowManyZero) - 1"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="10 * number($vRecursiveResult)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="1"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ===================================== -->
  <!--         Template CallPut              -->
  <!-- If Call then 1, if Put then 0         -->
  <!-- ===================================== -->
  <xsl:template name="CallPut">
    <xsl:param name="pCallPut"/>
    <xsl:choose>
      <xsl:when test="$pCallPut='C'">
        <xsl:value-of select="'1'"/>
      </xsl:when>
      <xsl:when test="$pCallPut='P'">
        <xsl:value-of select="'0'"/>
      </xsl:when>
      <xsl:otherwise>null</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- In the import file the currency is EU instead of EUR (?) -->
  <xsl:template name="GetCurrency">
    <xsl:param name="pCurrency"/>
    <xsl:choose>
      <xsl:when test="$pCurrency='EU'">
        <xsl:value-of select="'EUR'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pCurrency"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- EQTY	Premium style, FUT	Futures style mark-to-market, FUTDA	Futures style with cash adjustment -->
  <xsl:template name="GetFutValuationMethod">
    <xsl:param name="pProductStyle"/>
    <xsl:param name="pClassType"/>    
    <xsl:choose>
      <!-- Option style -->
      <xsl:when test="$pProductStyle='O'">
        <xsl:value-of select="$gEQTY"/>
      </xsl:when>
      <!-- Index -->
      <xsl:when test="$pProductStyle='F'">
        <xsl:value-of select="$gFut"/>
      </xsl:when>
      <!-- Si dans le fichier d'entrée la valuer est null ou différente de 'F' et 'O'  -->
      <!-- on appelle le template standard -->
      <xsl:otherwise>
        <xsl:call-template name="FutValuationMethod">
          <xsl:with-param name="pCategory" select="$pClassType"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Valeurs de l'enum UnderlyingGroupEnum en Spheres. F: Financial, C: Commodities -->
  <xsl:template name="GetUnderlyingGroup">
    <xsl:param name="pProductType"/>
    <xsl:choose>
      <!-- Equity -->
      <xsl:when test="$pProductType='E'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <!-- Index -->
      <xsl:when test="$pProductType='I'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <!-- Bond -->
      <xsl:when test="$pProductType='B'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <!-- Securities -->
      <xsl:when test="$pProductType='S'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Settlement Method                    -->
  <!-- Data non available in the IDEM files -->
  <xsl:template name="GetSettltMethod">
    <xsl:param name="pProductType"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:choose>
      <!--Index-->
      <xsl:when test="$pProductType='I'">
        <xsl:value-of select="'C'"/>
      </xsl:when>
      <!--Equity-->
      <xsl:when test="$pProductType='E'">
        <xsl:choose>
          <!-- FL 20161026 [34191]                                                   -->
          <!--  Management of an exception for Equity Options on IDEM market.        -->
          <!--   Options contracts with contract symbol preceeded by 4 are           -->
          <!--   set as Cash Settlement method, otherwise as Physical method         -->
          <xsl:when test="substring($pContractSymbol,1,1)='4'">
            <xsl:value-of select="'C'"/>
          </xsl:when>
          <!-- FL 20210308 [22979] -->
          <!--  Management of an exception for Stock Dividend Future on IDEM market. -->
          <!--   Options contracts with contract symbol preceeded by 1 are           -->
          <!--   set as Cash Settlement method                                       -->
          <xsl:when test="substring($pContractSymbol,1,1)='1'">
            <xsl:value-of select="'C'"/>
          </xsl:when>
          <!--   Otherwise as Physical method  -->
          <xsl:otherwise>
            <xsl:value-of select="'P'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!--Bond-->
      <xsl:when test="$pProductType='B'">
        <xsl:value-of select="'P'"/>
      </xsl:when>
      <!--Securities-->
      <xsl:when test="$pProductType='S'">
        <xsl:value-of select="'P'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Get asset category for the field ProductType of Italian Markets files -->
  <xsl:template name="GetAssetCategory">
    <xsl:param name="pProductType"/>
    <xsl:choose>
      <!-- Equity -->
      <xsl:when test="$pProductType='E'">
        <xsl:value-of select="$gEquityAsset"/>
      </xsl:when>
      <!-- Bond -->
      <xsl:when test="$pProductType='B'">
        <xsl:value-of select="$gBond"/>
      </xsl:when>
      <!-- Index -->
      <xsl:when test="$pProductType='I'">
        <xsl:value-of select="$gIndex"/>
      </xsl:when>
      <!-- RateIndex -->
      <xsl:when test="$pProductType='S'">
        <xsl:value-of select="$gRateIndex"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  

</xsl:stylesheet>
