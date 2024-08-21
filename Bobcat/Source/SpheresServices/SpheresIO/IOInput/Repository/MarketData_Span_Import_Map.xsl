<?xml version="1.0" encoding="utf-8"?>
<!--
=============================================================================================
 Summary   : Import SPAN Referential 
 File      : MarketData_Span_Import_Map.xsl.xsl
=============================================================================================
 Version: v6.0.0.0    Date: 20170420    Author: FL/PLA
  Comment: [23064] - Derivative Contracts: Settled amount behavior for "Physical" delivery
  Add pPhysettltamount parameter on ovrSQLGetDerivativeContractDefaultValue template
=============================================================================================
FL 20161123 [20625]  
  Manage Last Maturity Rule Added in       
  template(GetMaturityRuleIdentifier_CME)
    - XCBT 26 F
    - XCEC QI F
    - XCEC QO F
    - XCME Fx Futures Standards (Monthly)
    - XCME RU F
    - XCME Index Mini Options Standard
=============================================================================================
FL 20160701 [22084]
  Création of template GetExerciseRule
  Manage Last Maturity Rule Added in template(GetMaturityRuleIdentifier_CME)                     
    - XCBT SDF O
=============================================================================================
FL 20140304 [19648] 
  Manage DERIVATIVECONTRACT.UNDERLYINGGROUP & DERIVATIVECONTRACT.UNDERLYINGASSET
  in template (ovrSQLGetDerivativeContractDefaultValue)
=============================================================================================
FI 20131129 [19284]
  Manage DERIVATIVECONTRACT.FINALSETTLTSIDE
  in template (ovrSQLGetDerivativeContractDefaultValue)
=============================================================================================
FL 20131113 [18812]-[19170]  
  Manage Last Maturity Rule Added in       
  template(GetMaturityRuleIdentifier_CME)
    - XCME Fx Options Standard
    - XCBT 07 F
    - XCME ES O
	  - XCEC Metals Futures
     (DC : SI Fut (COMEX 5000 SILVER FUTURES) Phys (Identifiant: SI F))
  Manage Last Maturity Rule Added in       
  template(GetMaturityRuleIdentifier_CME)
      - XCEC Metals Futures
  Manage Last Maturity Rule Added in       
  template(GetMaturityRuleIdentifier_CME)
    - XCBT 06 F
    - XCBT 06 O
    - XCBT C O
    - XCBT S O
    - XCBT W O
  Manage Last Maturity Rule Added in       
  template(GetMaturityRuleIdentifier_CME)                     
    - XCBT US Treasury Options                                
    - XCME Index Futures Standard                             
    - XCME Fx Futures Standard                                
=============================================================================================
 FI 20130222 [18419]
  new integration of derivative Contracts  
=============================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:exsl="http://exslt.org/common" extension-element-prefixes="exsl" version="1.0">

  <!-- Imports -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>
  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes"
              media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:param name="pPassNumber" select="1"/>
  <xsl:param name="pPassRowsToProcess" select="100000"/>

  <!-- Global variables-->
  <xsl:variable name="gStartRowNumber">
    <xsl:copy-of select="$pPassRowsToProcess * ($pPassNumber - 1)"/>
  </xsl:variable>
  <xsl:variable name="gEndRowNumber">
    <xsl:copy-of select="$pPassRowsToProcess * $pPassNumber"/>
  </xsl:variable>

  <!--Main template  -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- Specific template-->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>
      <xsl:call-template name="IORow"/>
    </file>
  </xsl:template>

  <!-- Spécifique à chaque Import -->
  <xsl:template name="IORow">
    <!-- Toutes les lignes MARKET-->
    <xsl:for-each select="row[starts-with(@id,$vDataTypeMarket)]">
      <xsl:variable name="vMarketRow">
        <xsl:copy-of select="."/>
      </xsl:variable>

      <xsl:variable name="vClearingOrgSymbol">
        <xsl:value-of select="normalize-space(exsl:node-set($vMarketRow)/row/data[@name='COC'])"/>
      </xsl:variable>

      <xsl:variable name="vExchangeSymbol">
        <xsl:value-of select="normalize-space(exsl:node-set($vMarketRow)/row/data[@name='EC'])"/>
      </xsl:variable>

      <xsl:variable name="vISO10383">
        <xsl:call-template name="GetISO10383">
          <xsl:with-param name="pClearingOrgSymbol" select="$vClearingOrgSymbol"/>
          <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- La dernière ligne CONTRACT-->
      <!--FI 20120220[18419] Mise en commentaire, integration des DC uniquement -->
      <!--<xsl:if test="($pPassNumber > 1)">

        <xsl:variable name="vPrecedingContractRow">
          <xsl:copy-of select="../row[($gStartRowNumber >= position()) and starts-with(@id,$vDataTypeContract)][last()]"/>
        </xsl:variable>

        <xsl:variable name="vContractId">
          <xsl:call-template name="GetRowDataId2">
            <xsl:with-param name="pId" select="exsl:node-set($vPrecedingContractRow)/row/@id"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vCategory">
          <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingContractRow)/row/data[@name='CAT'])"/>
        </xsl:variable>

        <xsl:variable name="vContractSymbol">
          <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingContractRow)/row/data[@name='CS'])"/>
        </xsl:variable>

        <xsl:variable name="vUnderlyingContractSymbol">
          <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingContractRow)/row/data[@name='UCS'])"/>
        </xsl:variable>

        -->
      <!--FI 20120220[18419] Mise en commentaire, integration des DC uniquement -->
      <!--
        -->
      <!--<xsl:call-template name="rowStreamExpiry">
          <xsl:with-param name="pExchangeSymbol">
            <xsl:value-of select="$vExchangeSymbol"/>
          </xsl:with-param>
          <xsl:with-param name="pContractId">
            <xsl:value-of select="$vContractId"/>
          </xsl:with-param>
          <xsl:with-param name="pContractSymbol">
            <xsl:value-of select="$vContractSymbol"/>
          </xsl:with-param>
          <xsl:with-param name="pUnderlyingContractSymbol">
            <xsl:value-of select="$vUnderlyingContractSymbol"/>
          </xsl:with-param>
          <xsl:with-param name="pCategory" select="$vCategory"/>
          <xsl:with-param name="pIsPrecedingContractRow" select="'1'"/>
        </xsl:call-template>-->
      <!--
      </xsl:if>-->

      <!-- Toutes les lignes CONTRACT-->
      <xsl:for-each select="../row[(position() > $gStartRowNumber) and ($gEndRowNumber >= position()) and starts-with(@id,$vDataTypeContract)]">

        <xsl:variable name="vContractRow">
          <xsl:copy-of select="."/>
        </xsl:variable>

        <xsl:variable name="vContractId">
          <xsl:call-template name="GetRowDataId2">
            <xsl:with-param name="pId" select="exsl:node-set($vContractRow)/row/@id"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vCategory">
          <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='CAT'])"/>
        </xsl:variable>

        <!-- Phil : additional category filter  to identify contracts -->
        <xsl:variable name="vExtSQLFilterValues" select="
                  concat(
                  'KEY_OP',',',
                  $vCategory
                  )"/>

        <xsl:variable name="vExtSQLFilterNames" select="
                  concat(
                  'SPAN_SPECIAL',',',
                  'CATEGORY')"/>

        <xsl:variable name="vContractSymbol">
          <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='CS'])"/>
        </xsl:variable>

        <xsl:variable name="vUnderlyingContractSymbol">
          <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='UCS'])"/>
        </xsl:variable>

        <xsl:variable name="vFirstExpiryRow">
          <xsl:copy-of select="../row[(position() > $gStartRowNumber) and starts-with(@id,$vDataTypeExpiry) and (substring-after(substring-after(@id,'_'),'_') = $vContractId)][1]"/>
        </xsl:variable>

        <xsl:variable name="vMaturityMonthYear">
          <xsl:value-of select="exsl:node-set($vFirstExpiryRow)/row/data[@name='MMY']"/>
        </xsl:variable>

        <xsl:variable name="vMaturityFormat">
          <xsl:call-template name="MaturityFormat">
            <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
          </xsl:call-template>
        </xsl:variable>

        <row>
          <xsl:call-template name="IORowAtt">
            <xsl:with-param name="pId" select="exsl:node-set($vContractRow)/row/@id"/>
            <xsl:with-param name="pSrc" select="exsl:node-set($vContractRow)/row/@src"/>
          </xsl:call-template>

          <xsl:variable name="vDerivativeContractIdentifier">
            <xsl:value-of select="$gAutomaticCompute"/>
          </xsl:variable>

          <xsl:variable name="vContractDisplayName">
            <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='CN'])"/>
          </xsl:variable>
          <xsl:variable name="vInstrumentIdentifier">
            <xsl:call-template name="InstrumentIdentifier">
              <xsl:with-param name="pCategory" select="$vCategory"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCurrency">
            <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='CU'])"/>
          </xsl:variable>
          <xsl:variable name="vExerciseStyle">
            <xsl:variable name="vExercise">
              <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='ES'])"/>
            </xsl:variable>
            <xsl:if test="string-length($vExercise) > 0">
              <xsl:call-template name="ExerciseStyle">
                <xsl:with-param name="pExerciseStyle">
                  <xsl:value-of select="substring($vExercise,1,1)"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="vSettlMethod">
            <xsl:call-template name="GetSettlMethodFromSM">
              <xsl:with-param name="pSettlMethod"
                                select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='SM'])"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vFutValuationMethod">
            <xsl:call-template name="FutValuationMethod">
              <xsl:with-param name="pCategory" select="$vCategory"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vContractFactor">
            <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='FA'])"/>
          </xsl:variable>
          <xsl:variable name="vContractMultiplier">
            <xsl:value-of select="$vContractFactor"/>
          </xsl:variable>
          <xsl:variable name="vNominalValue">
            <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='NV'])"/>
          </xsl:variable>
          <xsl:variable name="vUnderlyingGroup">
            <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='UG'])"/>
          </xsl:variable>
          <xsl:variable name="vUnderlyingAsset">
            <xsl:value-of select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='UA'])"/>
          </xsl:variable>
          <xsl:variable name="vAssignmentMethod">
            <xsl:call-template name="AssignmentMethod">
              <xsl:with-param name="pCategory" select="$vCategory"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vMaturityRule">
            <xsl:call-template name="GetMaturityRuleIdentifier_CME">
              <xsl:with-param name="pISO10383" select="$vISO10383"/>
              <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
              <xsl:with-param name="pCategory" select="$vCategory"/>
            </xsl:call-template>
          </xsl:variable>
          <!--FL 20160701 [22084]-->
          <xsl:variable name="vExerciseRule">
            <xsl:call-template name="GetExerciseRule">
              <xsl:with-param name="pClearingOrgSymbol" select="$vClearingOrgSymbol"/>
              <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
              <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
              <xsl:with-param name="pCategory" select="$vCategory"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vAssetCategory">
            <xsl:call-template name="GetAssetCategoryFromUTC">
              <xsl:with-param name="pUnderlyingContractType"
                              select="normalize-space(exsl:node-set($vContractRow)/row/data[@name='UCT'])"/>
            </xsl:call-template>
          </xsl:variable>


          <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
            <xsl:with-param name="pClearingOrgSymbol">
              <xsl:value-of select="$vClearingOrgSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pISO10383">
              <xsl:value-of select="$vISO10383"/>
            </xsl:with-param>
            <xsl:with-param name="pExchangeSymbol">
              <xsl:value-of select="$vExchangeSymbol"/>
            </xsl:with-param>            
            <xsl:with-param name="pMaturityRuleIdentifier">
              <xsl:choose>
                <xsl:when test="string-length($vMaturityRule)>0">
                  <xsl:value-of select="$vMaturityRule"/>
                </xsl:when>
                <xsl:otherwise>Default Rule</xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="pContractSymbol">
              <xsl:value-of select="$vContractSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pDerivativeContractIdentifier">
              <xsl:value-of select="$vDerivativeContractIdentifier"/>
            </xsl:with-param>
            <xsl:with-param name="pContractDisplayName">
              <xsl:value-of select="$vContractDisplayName"/>
            </xsl:with-param>
            <xsl:with-param name="pInstrumentIdentifier">
              <xsl:value-of select="$vInstrumentIdentifier"/>
            </xsl:with-param>
            <xsl:with-param name="pCurrency">
              <xsl:value-of select="$vCurrency"/>
            </xsl:with-param>
            <xsl:with-param name="pCategory">
              <xsl:value-of select="$vCategory"/>
            </xsl:with-param>
            <xsl:with-param name="pExerciseStyle">
              <xsl:value-of select="$vExerciseStyle"/>
            </xsl:with-param>
            <!--FL 20160701 [22084]-->
            <xsl:with-param name="pExerciseRule" select="$vExerciseRule"/>
            <xsl:with-param name="pSettlMethod">
              <xsl:value-of select="$vSettlMethod"/>
            </xsl:with-param>
            <xsl:with-param name="pFutValuationMethod">
              <xsl:value-of select="$vFutValuationMethod"/>
            </xsl:with-param>
            <xsl:with-param name="pContractFactor">
              <xsl:value-of select="$vContractFactor"/>
            </xsl:with-param>
            <xsl:with-param name="pContractMultiplier">
              <xsl:value-of select="$vContractMultiplier"/>
            </xsl:with-param>
            <xsl:with-param name="pNominalValue">
              <xsl:value-of select="$vNominalValue"/>
            </xsl:with-param>
            <xsl:with-param name="pUnderlyingGroup">
              <xsl:value-of select="$vUnderlyingGroup"/>
            </xsl:with-param>
            <xsl:with-param name="pUnderlyingAsset">
              <xsl:value-of select="$vUnderlyingAsset"/>
            </xsl:with-param>
            <xsl:with-param name="pAssignmentMethod">
              <xsl:value-of select="$vAssignmentMethod"/>
            </xsl:with-param>
            <xsl:with-param name="pMaturityFormat" select="$vMaturityFormat"/>
            <xsl:with-param name="pInsertMaturityRule">
              <xsl:value-of select="$gFalse"/>
            </xsl:with-param>
            <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
            <xsl:with-param name="pUnderlyingContractSymbol">
              <xsl:value-of select="$vUnderlyingContractSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pDerivativeContractIsAutoSetting" >
              <xsl:value-of select="$gTrue"/>
            </xsl:with-param>
            <xsl:with-param name="pInstumentDen" select="$gNull"/>
            <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
            <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
          </xsl:call-template>


        </row>

        <!--FI 20120220[18419] Mise en commentaire, integration des DC uniquement -->
        <!--<xsl:call-template name="rowStreamExpiry">
          <xsl:with-param name="pExchangeSymbol">
            <xsl:value-of select="$vExchangeSymbol"/>
          </xsl:with-param>
          <xsl:with-param name="pContractId">
            <xsl:value-of select="$vContractId"/>
          </xsl:with-param>
          <xsl:with-param name="pContractSymbol">
            <xsl:value-of select="$vContractSymbol"/>
          </xsl:with-param>
          <xsl:with-param name="pUnderlyingContractSymbol">
            <xsl:value-of select="$vUnderlyingContractSymbol"/>
          </xsl:with-param>
          <xsl:with-param name="pCategory" select="$vCategory"/>

          <xsl:with-param name="pExtSQLFilterValues" select="
                    $vExtSQLFilterValues"/>
          <xsl:with-param name="pExtSQLFilterNames" select="
                    $vExtSQLFilterNames"/>

        </xsl:call-template>-->
      </xsl:for-each>

    </xsl:for-each>
  </xsl:template>

  <xsl:template name="rowStreamExpiry">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractId"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pUnderlyingContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pIsPrecedingContractRow" select="'0'"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>


    <!-- La dreniére ligne EXPIRY-->
    <xsl:if test="($pIsPrecedingContractRow = 1) and $pCategory != 'F'">

      <xsl:variable name="vPrecedingExpiryRow">
        <xsl:copy-of select="../row[($gStartRowNumber >= position()) and starts-with(@id,$vDataTypeExpiry)][last()]"/>
      </xsl:variable>

      <xsl:variable name="vMaturityMonthYear">
        <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingExpiryRow)/row/data[@name='MMY'])"/>
      </xsl:variable>
      <xsl:variable name="vMaturityDate">
        <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingExpiryRow)/row/data[@name='MD'])"/>
      </xsl:variable>
      <xsl:variable name="vNewMaturityDate">
        <xsl:call-template name="NewMaturityDate">
          <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
          <xsl:with-param name="pMaturityDate" select="$vMaturityDate"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vAssetMultiplier">
        <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingExpiryRow)/row/data[@name='MLT'])"/>
      </xsl:variable>
      <xsl:variable name="vAssetFirstQuotationDay">
        <xsl:value-of select="normalize-space(exsl:node-set($vPrecedingExpiryRow)/row/data[@name='FTD'])"/>
      </xsl:variable>

      <xsl:call-template name="rowStreamAsset">
        <xsl:with-param name="pISO10383" select="$pISO10383"/>
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pCategory" select="$pCategory"/>
        <xsl:with-param name="pExpiryRow" select="$vPrecedingExpiryRow"/>
        <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
        <xsl:with-param name="pMaturityDate" select="$vMaturityDate"/>
        <xsl:with-param name="pNewMaturityDate" select="$vNewMaturityDate"/>
        <xsl:with-param name="pAssetMultiplier" select="$vAssetMultiplier"/>
        <xsl:with-param name="pAssetFirstQuotationDay" select="$vAssetFirstQuotationDay"/>

        <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
        <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterValues"/>

      </xsl:call-template>

    </xsl:if>

    <!-- Toutes les lignes EXPIRY-->
    <xsl:for-each select="../row[(position() > $gStartRowNumber) and ($gEndRowNumber >= position()) and starts-with(@id,$vDataTypeExpiry)]">
      <xsl:variable name="vExpiryRow">
        <xsl:copy-of select="."/>
      </xsl:variable>

      <xsl:variable name="vExpiryParentId">
        <xsl:call-template name="GetRowParentId">
          <xsl:with-param name="pId" select="exsl:node-set($vExpiryRow)/row/@id"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:if test="$vExpiryParentId = $pContractId">

        <xsl:variable name="vMaturityMonthYear">
          <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='MMY'])"/>
        </xsl:variable>
        <xsl:variable name="vMaturityDate">
          <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='MD'])"/>
        </xsl:variable>
        <xsl:variable name="vNewMaturityDate">
          <xsl:call-template name="NewMaturityDate">
            <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
            <xsl:with-param name="pMaturityDate" select="$vMaturityDate"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="vAssetMultiplier">
          <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='MLT'])"/>
        </xsl:variable>
        <xsl:variable name="vAssetFirstQuotationDay">
          <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='FTD'])"/>
        </xsl:variable>

        <!-- Create SQL script for tables MATURITY-->
        <row>
          <xsl:call-template name="IORowAtt">
            <xsl:with-param name="pId" select="exsl:node-set($vExpiryRow)/row/@id"/>
            <xsl:with-param name="pSrc" select="exsl:node-set($vExpiryRow)/row/@src"/>
          </xsl:call-template>

          <xsl:variable name="vDeliveryDate">
            <xsl:choose>
              <xsl:when test="$pCategory='F'">
                <xsl:variable name="vSettlementDate">
                  <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='SD'])"/>
                </xsl:variable>

                <xsl:choose>
                  <xsl:when test="format-number($vSettlementDate,'0')!=0">
                    <xsl:value-of select="$vSettlementDate"/>
                  </xsl:when>
                  <xsl:otherwise>
                    null
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:when test="$pCategory='O'">
                <xsl:value-of select="$vMaturityDate"/>
              </xsl:when>
              <xsl:otherwise>
                null
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vLastTradingDay">
            <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='LTD'])"/>
          </xsl:variable>
          <xsl:variable name="vUnderlyingAssetSymbol">
            <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='UCM'])"/>
          </xsl:variable>

          <xsl:call-template name="SQLTableMATURITY_DERIVATIVEATTRIB2">
            <xsl:with-param name="pISO10383">
              <xsl:value-of select="$pISO10383"/>
            </xsl:with-param>
            <xsl:with-param name="pExchangeSymbol">
              <xsl:value-of select="$pExchangeSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pContractSymbol">
              <xsl:value-of select="$pContractSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pMaturityMonthYear">
              <xsl:value-of select="$vMaturityMonthYear"/>
            </xsl:with-param>
            <xsl:with-param name="pMaturityDate">
              <xsl:value-of select="$vMaturityDate"/>
            </xsl:with-param>
            <xsl:with-param name="pDeliveryDate">
              <xsl:value-of select="$vDeliveryDate"/>
            </xsl:with-param>
            <xsl:with-param name="pLastTradingDay">
              <xsl:value-of select="$vLastTradingDay"/>
            </xsl:with-param>
            <xsl:with-param name="pUnderlyingAssetSymbol">
              <xsl:value-of select="$vUnderlyingAssetSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pUnderlyingContractSymbol">
              <xsl:value-of select="$pUnderlyingContractSymbol"/>
            </xsl:with-param>

            <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
            <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterNames"/>

          </xsl:call-template>

          <xsl:if test="$pCategory = 'F'">

            <xsl:variable name="vAssetSymbol">
              <xsl:value-of select="normalize-space(exsl:node-set($vExpiryRow)/row/data[@name='AS'])"/>
            </xsl:variable>

            <xsl:call-template name="rowStream">
              <xsl:with-param name="pCategory" select="$pCategory"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
              <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
              <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
              <xsl:with-param name="pMaturityDate" select="$vMaturityDate"/>
              <xsl:with-param name="pNewMaturityDate" select="$vNewMaturityDate"/>
              <xsl:with-param name="pAssetSymbol" select="$vAssetSymbol"/>
              <xsl:with-param name="pAssetMultiplier" select="$vAssetMultiplier"/>
              <xsl:with-param name="pAssetFirstQuotationDay" select="$vAssetFirstQuotationDay"/>

              <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
              <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterNames"/>

            </xsl:call-template>

          </xsl:if>

        </row>

        <xsl:if test="$pCategory = 'O'">
          <xsl:call-template name="rowStreamAsset">
            <xsl:with-param name="pISO10383" select="$pISO10383"/>
            <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
            <xsl:with-param name="pCategory" select="$pCategory"/>
            <xsl:with-param name="pExpiryRow" select="$vExpiryRow"/>
            <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
            <xsl:with-param name="pMaturityDate" select="$vMaturityDate"/>
            <xsl:with-param name="pNewMaturityDate" select="$vNewMaturityDate"/>
            <xsl:with-param name="pAssetMultiplier" select="$vAssetMultiplier"/>
            <xsl:with-param name="pAssetFirstQuotationDay" select="$vAssetFirstQuotationDay"/>

            <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
            <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterNames"/>

          </xsl:call-template>
        </xsl:if>

      </xsl:if>

    </xsl:for-each>

  </xsl:template>

  <xsl:template name="rowStreamAsset">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExpiryRow"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pNewMaturityDate"/>
    <xsl:param name="pAssetMultiplier"/>
    <xsl:param name="pAssetFirstQuotationDay"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vExpiryId">
      <xsl:call-template name="GetRowDataId2">
        <xsl:with-param name="pId" select="exsl:node-set($pExpiryRow)/row/@id"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Toutes les lignes ASSET-->
    <xsl:for-each select="../row[(position() > $gStartRowNumber) and ($gEndRowNumber >= position()) and starts-with(@id,$vDataTypeAsset)]">
      <xsl:variable name="vAssetRow">
        <xsl:copy-of select="."/>
      </xsl:variable>

      <xsl:variable name="vAssetParentId">
        <xsl:call-template name="GetRowParentId">
          <xsl:with-param name="pId" select="exsl:node-set($vAssetRow)/row/@id"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:if test="$vAssetParentId = $vExpiryId">
        <row>
          <xsl:call-template name="IORowAtt">
            <xsl:with-param name="pId" select="exsl:node-set($vAssetRow)/row/@id"/>
            <xsl:with-param name="pSrc" select="exsl:node-set($vAssetRow)/row/@src"/>
          </xsl:call-template>

          <xsl:variable name="vAssetSymbol">
            <xsl:value-of select="normalize-space(exsl:node-set($vAssetRow)/row/data[@name='AS'])"/>
          </xsl:variable>

          <xsl:call-template name="rowStream">
            <xsl:with-param name="pISO10383" select="$pISO10383"/>
            <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
            <xsl:with-param name="pCategory" select="$pCategory"/>
            <xsl:with-param name="pMaturityMonthYear" select="$pMaturityMonthYear"/>
            <xsl:with-param name="pMaturityDate" select="$pMaturityDate"/>
            <xsl:with-param name="pNewMaturityDate" select="$pNewMaturityDate"/>
            <xsl:with-param name="pAssetSymbol" select="$vAssetSymbol"/>
            <xsl:with-param name="pAssetMultiplier" select="$pAssetMultiplier"/>
            <xsl:with-param name="pAssetFirstQuotationDay" select="$pAssetFirstQuotationDay"/>
            <xsl:with-param name="pAssetRow" select="$vAssetRow"/>

            <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
            <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterNames"/>

          </xsl:call-template>
        </row>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="rowStream">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pNewMaturityDate"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetMultiplier"/>
    <xsl:param name="pAssetFirstQuotationDay"/>
    <xsl:param name="pAssetRow"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vPutCall">
      <xsl:value-of select="normalize-space(exsl:node-set($pAssetRow)/row/data[@name='PC'])"/>
    </xsl:variable>
    <xsl:variable name="vAssetPutCall">
      <xsl:choose>
        <xsl:when test="$vPutCall='C'">
          <xsl:value-of select="'1'"/>
        </xsl:when>
        <xsl:when test="$vPutCall='P'">
          <xsl:value-of select="'0'"/>
        </xsl:when>
        <xsl:otherwise>
          null
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAssetStrikePrice">
      <xsl:value-of select="normalize-space(exsl:node-set($pAssetRow)/row/data[@name='SP'])"/>
    </xsl:variable>

    <xsl:variable name="vAIICode">
      <xsl:value-of select="$gAutomaticCompute"/>
      <!--<xsl:call-template name="AIICode">
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pCategory" select="$pCategory"/>
        <xsl:with-param name="pPutCall" select="$vPutCall"/>
        <xsl:with-param name="pMaturityDate" select="$pNewMaturityDate"/>
        <xsl:with-param name="pAssetStrikePrice" select="$vAssetStrikePrice"/>
      </xsl:call-template>-->
    </xsl:variable>
    <xsl:variable name="vAssetDisplayName">
      <xsl:value-of select="$gAutomaticCompute"/>
      <!--<xsl:call-template name="DisplayName">
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
        <xsl:with-param name="pCategory" select="$pCategory"/>
        <xsl:with-param name="pPutCall" select="$vPutCall"/>
        <xsl:with-param name="pMaturityDate" select="$pNewMaturityDate"/>
        <xsl:with-param name="pAssetStrikePrice" select="$vAssetStrikePrice"/>
      </xsl:call-template>-->
    </xsl:variable>
    <xsl:variable name="vAssetISINCode">
    </xsl:variable>
    <xsl:variable name="vAssetIdentifier">
      <xsl:value-of select="$gAutomaticCompute"/>
      <!--<xsl:call-template name="AssetIdentifier">
        <xsl:with-param name="pAssetISINCode" select="$vAssetISINCode"/>
        <xsl:with-param name="pAIICode" select="$vAIICode"/>
      </xsl:call-template>-->
    </xsl:variable>
    <xsl:variable name="vAssetSymbol">
      <xsl:value-of select="$pAssetSymbol"/>
    </xsl:variable>

    <!-- Call SQLTableASSET_ETD2 for SQL query generation-->
    <xsl:call-template name="SQLTableASSET_ETD2">
      <xsl:with-param name="pISO10383">
        <xsl:value-of select="$pISO10383"/>
      </xsl:with-param>
      <xsl:with-param name="pExchangeSymbol">
        <xsl:value-of select="$pExchangeSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="$pContractSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pMaturityMonthYear">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetIdentifier">
        <xsl:value-of select="$vAssetIdentifier"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetDisplayName">
        <xsl:value-of select="$vAssetDisplayName"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetSymbol">
        <xsl:value-of select="$vAssetSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetISINCode">
        <xsl:value-of select="$vAssetISINCode"/>
      </xsl:with-param>
      <xsl:with-param name="pAIICode">
        <xsl:value-of select="$vAIICode"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetMultiplier">
        <xsl:value-of select="$pAssetMultiplier"/>
      </xsl:with-param>
      <xsl:with-param name="pCategory" select="$pCategory"/>
      <xsl:with-param name="pAssetPutCall">
        <xsl:value-of select="$vAssetPutCall"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetStrikePrice">
        <xsl:value-of select="$vAssetStrikePrice"/>
      </xsl:with-param>
      <xsl:with-param name="pAssetFirstQuotationDay">
        <xsl:value-of select="$pAssetFirstQuotationDay"/>
      </xsl:with-param>

      <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterNames"/>

    </xsl:call-template>

  </xsl:template>

  <xsl:template name="GetRowType">
    <xsl:param name="pId"/>
    <xsl:value-of select="substring-before($pId,'_')"/>
  </xsl:template>

  <xsl:template name="GetRowDataId">
    <xsl:param name="pId"/>
    <xsl:value-of select="substring-before(substring-after($pId,'_'),'_')"/>
  </xsl:template>

  <xsl:template name="GetRowDataId2">
    <xsl:param name="pId"/>
    <xsl:value-of select="substring-after($pId,'_')"/>
  </xsl:template>

  <xsl:template name="GetRowParentId">
    <xsl:param name="pId"/>
    <xsl:value-of select="substring-after(substring-after($pId,'_'),'_')"/>
  </xsl:template>

  <!-- BD & FL 20132806 [18785]: Création du template GetMaturityRuleIdentifier_CME -->
  <xsl:template name="GetMaturityRuleIdentifier_CME">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>

    <!-- Affectation des règles d'échéances sur la base de la matrice suivante
          A partir du fichier Excel BCLEAR
  
       ISO10383       Category        ContractSymbol        Identifier de la Maturity Rule
       =====================================================================================
       XCBT           F               YM                    XCBT Index Mini Futures Standard
       XCBT           F               14                    XCBT 14 F
       XCBT           F               17                    XCBT 17 F
       XCBT           F               21                    XCBT 21 F
       XCBT           F               25                    XCBT 25 F
       XCBT           F               26                    XCBT 26 F
       XCBT           F               C                     XCBT C F
       XCBT           F               S                     XCBT S F
       XCBT           F               W                     XCBT W F 
       XCBT           F               06                    XCBT 06 F
       XCBT           O               06                    XCBT 06 O
       XCBT           O               C                     XCBT C O
       XCBT           O               S                     XCBT S O
       XCBT           O               SDF                   XCBT SDF O
       XCBT           O               W                     XCBT W O
       XCBT           O               17,21,25,26           XCBT US Treasury Options
       XCBT           F               07                    XCBT 07 F 
       XCBT           O               07                    XCBT 07 O (BD 20130701 Actuellement non géré, à faire en même temps que
                                                                        la reprise de toutes les MR qui on été crée pour SIGMATERME)
                                               
       XCME           F               ES, ME, NQ            XCME Index Mini Futures Standard
       XCME           F               SP, ND                XCME Index Futures Standard
       XCME           F               AD, BP, E1, E7, EC    XCME Fx Futures Standard
                                      RF, RP, J1, NE
       XCME           F               MP, RA                XCME Fx Futures Standard (Monthly)
       XCME           F               C1                    XCME Fx Futures CAD
       XCME           F               NK                    XCME NK F
       XCME           F               ED                    XCME ED F
       XCME           F               RU                    XCME RU F
       XCME           O               ES                    XCME ES O
       XCME           O               AD, BP, C1, E1, EC    XCME Fx Options Standard
                                      YT
       XCME           O               NQ                    XCME Index Mini Options Standard
    
       XNYM           F               QG                    XNYM QG F
       XNYM           F               QM                    XNYM QM F
       XNYM           F               CL                    XNYM CL F
       XNYM           F               NG                    XNYM NG F
       XNYM           F               PL                    XNYM PL F
       XNYM           F               HO                    XNYM HO F
       XNYM           F               RB                    XNYM RB F
      
       XCEC           F               GC, HG, SI            XCEC Metals Futures
       XCEC           F               QI                    XCEC QI F
       XCEC           F               QO                    XCEC QO F
       
              
       Si sur un DC considéré aucune des ces conditions ci-dessus est respectées on aplique une Règle d'échéance
       par défaut intitulée : Default Rule. -->

    <!-- En fonction du code ISO10383 -->
    <xsl:choose>

      <!-- XCBT -->
      <xsl:when test="$pISO10383 = 'XCBT'">

        <!-- En fonction de la Category -->
        <xsl:choose>

          <!-- Category = 'F' -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <!-- ContractSymbol : YM  -->
              <xsl:when test="$pContractSymbol = 'YM'">XCBT Index Mini Futures Standard</xsl:when>
              <!-- ContractSymbol : 14 -->
              <xsl:when test="$pContractSymbol = '14'">XCBT 14 F</xsl:when>
              <!-- ContractSymbol : 17 -->
              <xsl:when test="$pContractSymbol = '17'">XCBT 17 F</xsl:when>
              <!-- ContractSymbol : 21 -->
              <xsl:when test="$pContractSymbol = '21'">XCBT 21 F</xsl:when>
              <!-- ContractSymbol : 25 -->
              <xsl:when test="$pContractSymbol = '25'">XCBT 25 F</xsl:when>
              <!-- ContractSymbol : 26 -->
              <xsl:when test="$pContractSymbol = '26'">XCBT 26 F</xsl:when>
              <!-- ContractSymbol : C -->
              <xsl:when test="$pContractSymbol = 'C'">XCBT C F</xsl:when>
              <!-- ContractSymbol : S -->
              <xsl:when test="$pContractSymbol = 'S'">XCBT S F</xsl:when>
              <!-- ContractSymbol : 06 -->
              <xsl:when test="$pContractSymbol = '06'">XCBT 06 F</xsl:when>
              <!-- ContractSymbol : W -->
              <xsl:when test="$pContractSymbol = 'W'">XCBT W F</xsl:when>
              <!-- ContractSymbol : 07 -->
              <xsl:when test="$pContractSymbol = '07'">XCBT 07 F</xsl:when>
            </xsl:choose>
          </xsl:when>

          <!-- Category = 'O' -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <!-- ContractSymbol : 17, 21, 25, 26 -->
              <xsl:when test="$pContractSymbol = '17' or 
                              $pContractSymbol = '21' or 
                              $pContractSymbol = '25' or 
                              $pContractSymbol = '26'">XCBT US Treasury Options</xsl:when>
              <!-- ContractSymbol : C -->
              <xsl:when test="$pContractSymbol = 'C'">XCBT C O</xsl:when>
              <!-- ContractSymbol : S -->
              <xsl:when test="$pContractSymbol = 'S'">XCBT S O</xsl:when>
              <!-- ContractSymbol : SDF -->
              <xsl:when test="$pContractSymbol = 'SDF'">XCBT SDF O</xsl:when>
              <!-- ContractSymbol : W -->
              <xsl:when test="$pContractSymbol = 'W'">XCBT W O</xsl:when>
              <!-- ContractSymbol : 06 -->
              <xsl:when test="$pContractSymbol = '06'">XCBT 06 O</xsl:when>
              <!-- ContractSymbol : 07 -->
              <!-- BD 20130701 Actuellement non géré, à faire en même temps 
                   que la reprise de toutes les MR qui on été crée pour SIGMATERME)-->
              <!--<xsl:when test="$pContractSymbol = '07'">XCBT 07 O</xsl:when>-->
            </xsl:choose>
          </xsl:when>

        </xsl:choose>

      </xsl:when>

      <!-- XCME -->
      <xsl:when test="$pISO10383 = 'XCME'">

        <!-- En fonction de la Category -->
        <xsl:choose>

          <!-- Category = 'F' -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <!-- ContractSymbol : ES, ME, NQ -->
              <xsl:when test="$pContractSymbol = 'ES' or
                              $pContractSymbol = 'ME' or
                              $pContractSymbol = 'NQ'">XCME Index Mini Futures Standard</xsl:when>
              <!-- ContractSymbol : SP, ND -->
              <xsl:when test="$pContractSymbol = 'SP' or
                              $pContractSymbol = 'ND'">XCME Index Futures Standard</xsl:when>
              <!-- ContractSymbol : AD, BP, E1, E7, EC, RF, RP, J1, NE -->
              <xsl:when test="$pContractSymbol = 'AD' or
                              $pContractSymbol = 'BP' or
                              $pContractSymbol = 'E1' or
                              $pContractSymbol = 'E7' or
                              $pContractSymbol = 'EC' or
                              $pContractSymbol = 'RF' or
                              $pContractSymbol = 'RP' or
                              $pContractSymbol = 'J1' or
                              $pContractSymbol = 'NE'">XCME Fx Futures Standard</xsl:when>
              <!-- ContractSymbol : MP, RA -->
              <xsl:when test="$pContractSymbol = 'MP' or
                              $pContractSymbol = 'RA'">XCME Fx Futures Standard (Monthly)</xsl:when>
              <!-- ContractSymbol : C1 -->
              <xsl:when test="$pContractSymbol = 'C1'">XCME Fx Futures CAD</xsl:when>
              <!-- ContractSymbol : NK -->
              <xsl:when test="$pContractSymbol = 'NK'">XCME NK F</xsl:when>
              <!-- ContractSymbol : ED -->
              <xsl:when test="$pContractSymbol = 'ED'">XCME ED F</xsl:when>
              <!-- ContractSymbol : RU -->
              <xsl:when test="$pContractSymbol = 'RU'">XCME RU F</xsl:when>
            </xsl:choose>
          </xsl:when>

          <!-- Category = 'O' -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <!-- ContractSymbol : ES -->
              <xsl:when test="$pContractSymbol = 'ES'">XCME ES O</xsl:when>
              <!-- ContractSymbol : AD, BP, C1, E1, EC, YT -->
              <xsl:when test="$pContractSymbol = 'AD' or
                              $pContractSymbol = 'BP' or
                              $pContractSymbol = 'C1' or
                              $pContractSymbol = 'E1' or
                              $pContractSymbol = 'EC' or
                              $pContractSymbol = 'YT'">XCME Fx Options Standard</xsl:when>
              <!-- ContractSymbol : NQ -->
              <xsl:when test="$pContractSymbol = 'NQ'">XCME Index Mini Options Standard</xsl:when>
            </xsl:choose>
          </xsl:when>

        </xsl:choose>

      </xsl:when>

      <!-- XNYM -->
      <xsl:when test="$pISO10383 = 'XNYM'">

        <!-- En fonction de la Category -->
        <xsl:choose>

          <!-- Category = 'F' -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <!-- ContractSymbol : QG -->
              <xsl:when test="$pContractSymbol = 'QG'">XNYM QG F</xsl:when>
              <!-- ContractSymbol : QM -->
              <xsl:when test="$pContractSymbol = 'QM'">XNYM QM F</xsl:when>
              <!-- ContractSymbol : CL -->
              <xsl:when test="$pContractSymbol = 'CL'">XNYM CL F</xsl:when>
              <!-- ContractSymbol : NG -->
              <xsl:when test="$pContractSymbol = 'NG'">XNYM NG F</xsl:when>
              <!-- ContractSymbol : PL -->
              <xsl:when test="$pContractSymbol = 'PL'">XNYM PL F</xsl:when>
              <!-- ContractSymbol : HO -->
              <xsl:when test="$pContractSymbol = 'HO'">XNYM HO F</xsl:when>
              <!-- ContractSymbol : RB -->
              <xsl:when test="$pContractSymbol = 'RB'">XNYM RB F</xsl:when>
            </xsl:choose>
          </xsl:when>

        </xsl:choose>

      </xsl:when>

      <!-- XCEC -->
      <xsl:when test="$pISO10383 = 'XCEC'">

        <!-- En fonction de la Category -->
        <xsl:choose>

          <!-- Category = 'F' -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <!-- ContractSymbol : GC, HG, SI -->
              <xsl:when test="$pContractSymbol = 'GC' or
                              $pContractSymbol = 'HG' or
                              $pContractSymbol = 'SI'">XCEC Metals Futures</xsl:when>
              <!-- ContractSymbol : QI -->
              <xsl:when test="$pContractSymbol = 'QI'">XCEC QI F</xsl:when>
              <!-- ContractSymbol : QO -->
              <xsl:when test="$pContractSymbol = 'QO'">XCEC QO F</xsl:when>
            </xsl:choose>
          </xsl:when>

        </xsl:choose>

      </xsl:when>

    </xsl:choose>


  </xsl:template>

  <xsl:template name="GetISO10383">
    <xsl:param name="pClearingOrgSymbol"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:choose>
      <xsl:when test="$pClearingOrgSymbol = 'CME'">
        <xsl:choose>
          <xsl:when test="$pExchangeSymbol = 'CBT'">
            <xsl:value-of select ="'XCBT'"/>
          </xsl:when>
          <xsl:when test="$pExchangeSymbol = 'CMX'">
            <xsl:value-of select ="'XCEC'"/>
          </xsl:when>
          <xsl:when test="$pExchangeSymbol = 'CME'">
            <xsl:value-of select ="'XCME'"/>
          </xsl:when>
          <xsl:when test="$pExchangeSymbol = 'NYM'">
            <xsl:value-of select ="'XNYM'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat('Market Symbol ', $pExchangeSymbol,' is not managed') "/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat('Clearing Organisation Symbol ', $pClearingOrgSymbol,' is not managed') "/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Override of SQLGetDerivativeContractDefaultValue -->
  <!-- FL 20140220 [19648] add UNDERLYINGGROUP & UNDERLYINGASSET(Retourne tjs les valeurs contenues dans la table SPANPRODUCT) -->
  <!-- FI 20131121 [19216] add CONTRACTTYPE -->
  <!-- FI 20131129 [19284] add FINALSETTLTSIDE -->
  <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
  <xsl:template name="ovrSQLGetDerivativeContractDefaultValue">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:choose>
      <xsl:when test=" $pResult='CONTRACTMULTIPLIER' or
                       $pResult='PRICEDECLOCATOR' or
                       $pResult='STRIKEDECLOCATOR' or
                       $pResult='PRICEALIGNCODE' or
                       $pResult='STRIKEALIGNCODE' or
                       $pResult='CABINETOPTVALUE' or
                       $pResult='SETTLTMETHOD' or
                       $pResult='PHYSETTLTAMOUNT' or
                       $pResult='PRICEQUOTEMETHOD' or
                       $pResult='FUTVALUATIONMETHOD' or
                       $pResult='IDC_PRICE' or 
                       $pResult='IDC_NOMINAL' or
                       $pResult='EXERCISESTYLE' or 
                       $pResult='CONTRACTMULTIPLIER' or
                       $pResult='INSTRUMENTDEN' or 
                       $pResult='CONTRACTTYPE' or
                       $pResult='FINALSETTLTSIDE' or
                       $pResult='UNDERLYINGGROUP' or
                       $pResult='UNDERLYINGASSET'
                ">

        <xsl:variable name ="columnResult">
          <xsl:choose>
            <xsl:when test ="$pResult = 'IDC_PRICE' or $pResult = 'IDC_NOMINAL'">
              <xsl:value-of select ="'IDC'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="$pResult"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <!-- FL 20130228 colonne FUTVALUATIONMETHOD est renseignée à 'FUT' puisque obligatoire dans la table DERIVATIVECONTRACT -->
        <SQL command="select" result="{$columnResult}" cache="true">
          <xsl:text>
          <![CDATA[
            select '0' as LEVELSORT,
            CONTRACTMULTIPLIER,PRICEDECLOCATOR,STRIKEDECLOCATOR,PRICEALIGNCODE,STRIKEALIGNCODE,CABINETOPTVALUE,
            SETTLTMETHOD, case when SETTLTMETHOD = 'C' then 'NA' else 'None' end as PHYSETTLTAMOUNT, PRICEQUOTEMETHOD,FUTVALUATIONMETHOD,IDC,EXERCISESTYLE,
            case when PRICEALIGNCODE in ('C','9','B') then 32
                 when PRICEALIGNCODE in ('K','7') then 64
                 else 100 
            end as INSTRUMENTDEN, 'STD' as CONTRACTTYPE, 
            case when CATEGORY='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            UNDERLYINGGROUP, UNDERLYINGASSET
            from dbo.SPANPRODUCT
            where CLEARINGORGACRONYM = @CLEARINGORGSYMBOLEXL
              and EXCHANGEACRONYM = @EXCHANGESYMBOLEXL
              and CONTRACTSYMBOL = @CONTRACTSYMBOLEXL
              and CATEGORY = @CATEGORYEXL
              and TYPE not in ('1','3')
            union all
            select '1' as LEVELSORT, 
            null as CONTRACTMULTIPLIER, null as PRICEDECLOCATOR, null as STRIKEDECLOCATOR, null as PRICEALIGNCODE, null as STRIKEALIGNCODE, null as CABINETOPTVALUE,
            'C' as SETTLTMETHOD, 'NA' as PHYSETTLTAMOUNT, null as PRICEQUOTEMETHOD, 'FUT' as FUTVALUATIONMETHOD, null as IDC, '1' as EXERCISESTYLE, 
            100 as INSTRUMENTDEN, 'STD' as CONTRACTTYPE, 
            case when @CATEGORYEXL='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            null as UNDERLYINGGROUP, null as UNDERLYINGASSET
            from DUAL
            order by LEVELSORT
          ]]>
          </xsl:text>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
        </SQL>
      </xsl:when>
      <xsl:otherwise>null</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetAssetCategoryFromUTC">
    <xsl:param name="pUnderlyingContractType"/>
    <xsl:choose>
      <xsl:when test="$pUnderlyingContractType='FUT'">
        <xsl:value-of select="'Future'"/>
      </xsl:when>
      <xsl:when test="$pUnderlyingContractType='PHY'">
        <xsl:value-of select="'Commodity'"/>
      </xsl:when>
      <xsl:when test="$pUnderlyingContractType='EQTY'">
        <xsl:value-of select="'EquityAsset'"/>
      </xsl:when>
      <xsl:when test="$pUnderlyingContractType='DEBT'">
        <xsl:value-of select="'Bond'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetSettlMethodFromSM">
    <xsl:param name="pSettlMethod"/>
    <xsl:choose>
      <xsl:when test="$pSettlMethod='DELIV'">
        <xsl:value-of select="'P'"/>
      </xsl:when>
      <xsl:when test="$pSettlMethod='CASH'">
        <xsl:value-of select="'C'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gNull"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FL 20160701 [22084]: Création du template GetExerciseRule -->
  <!-- Affectation ExerciseRule sur la base de la matrice suivante
   ExchangeSymbol Category        ContractSymbol   ExerciseRule
   ====================================================================================================
   CBT           O               SDF              X -> The option contract exercises into the November(X)
                                                       futures contract that is nearest.
                                                         
   CMX           O               OG               GJMQVZ -> The option contract exercises into the futures
                                                              contract that is nearest on an even months:
                                                              Feb(G),Apr(J),Jun(M),Aug(Q),Oct(V),Dec(Z).
           
   Si sur un DC considéré aucune des ces conditions ci-dessus est respectées on aplique
   une données Contrat dérivé Future[Column ExerciseRule] à NULL
   ==================================================================================================== -->
  <xsl:template name="GetExerciseRule">
    <xsl:param name="pClearingOrgSymbol"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:choose>
      <xsl:when test="$pClearingOrgSymbol = 'CME'">
        <xsl:choose>
          <xsl:when test="$pExchangeSymbol = 'CBT'">
            <xsl:choose>
              <xsl:when test="$pContractSymbol = 'SDF' and $pCategory = 'O'">
                <xsl:value-of select ="'X'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gNull"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pExchangeSymbol = 'CMX'">
            <xsl:choose>
              <xsl:when test="$pContractSymbol = 'OG' and $pCategory = 'O'">
                <xsl:value-of select ="'GJMQVZ'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gNull"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pExchangeSymbol = 'CME'">
            <xsl:value-of select ="$gNull"/>
          </xsl:when>
          <xsl:when test="$pExchangeSymbol = 'NYM'">
            <xsl:value-of select ="$gNull"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gNull"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gNull"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
