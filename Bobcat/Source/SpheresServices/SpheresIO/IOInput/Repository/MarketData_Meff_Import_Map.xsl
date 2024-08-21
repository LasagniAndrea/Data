<!--
=======================================================================================================================
Summary : BMECLEARING - REPOSITORY
File    : MarketData_Meff_Import_Map.xsl
=======================================================================================================================
Version: v6.0.6488   Date: 20171006   Author: FL
Comment: [23495] - Import: BMECLEARING - REPOSITORY (New feature)
Fixed various problems following recipe for setting up this import into production.
  - Add pPhysettltamount parameter on SQLTableDERIVATIVECONTRACT template
  - Creation of template GetMEFF_MaturityRule and manage in this template this MR
      XMRV Spanish Stock Futures
      XMRV Stock Options
      XMRV Index Futures
      XMRV Index Options
      XMRV Dividend Futures
  - Manage the creation of equity (ASSET_EQUITY, ASSET_EQUITY_RDCMK) and index (ASSET_INDEX) FamilyMnemonicCode = 0240
  - Manage the creation in table EXCHANGELIST
=======================================================================================================================
Version: v1.0.4.0    Date: 20100310    Author: MF
Comment :    - *UPD* introducing SQLTableMATURITYRULE_DERIVATIVECONTRACT
             - *UPD* defined common parameters for BuildASSETS and BuildDERIVATIVECONTRACTS templates
=======================================================================================================================                    
Version: v1.0.3.0    Date: 20100305    Author: MF
Comment :  - *NEW* MEFFCONTRTYP and MEFFCONTRGRP helper tables 
             (calling SQLWhereClauseExtension and SelectColumnValueWhereClauseExtension)
=======================================================================================================================                                        
Version: v1.0.2.0    Date: 20100226    Author: MF
Comment :   - *NEW* table CCONTRTYP 
            - *NEW* table CONTRACTG
            - *NEW* table GCONTRACTROLE
=======================================================================================================================                                        
Version: v1.0.0.1    Date: 20100210    Author: MF
Comment : Filter for Index/Contado elements  (0.25%-0.2% of the original data volume saved )
=======================================================================================================================                                        
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>
  <xsl:include href="MarketData_Common_SQL.xsl"/>

  <xsl:template name="GeneralizeCFICodeXOptions">
    <xsl:param name="pCFICode"/>
    <xsl:choose>
      <xsl:when test="substring($pCFICode, 1, 2) = 'FF' or substring($pCFICode, 1, 2) = 'FC'">
        <xsl:value-of select="$pCFICode"/>
      </xsl:when>
      <xsl:when test="substring($pCFICode, 1, 2) = 'OP' or substring($pCFICode, 1, 2) = 'OC'">
        <xsl:value-of select="
                      concat(
                      substring($pCFICode,1,1),
                      'X',
                      substring($pCFICode,3,string-length($pCFICode)))"/>
      </xsl:when>

      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="ContractCategory">
    <xsl:param name="pCFICode"/>

    <xsl:choose>
      <xsl:when test="substring($pCFICode, 1, 2) = 'FF' or substring($pCFICode, 1, 2) = 'FC'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <xsl:when test="substring($pCFICode, 1, 2) = 'OP'">
        <xsl:value-of select="'O'"/>
      </xsl:when>
      <xsl:when test="substring($pCFICode, 1, 2) = 'OC'">
        <xsl:value-of select="'O'"/>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="ContractCategoryFromAsset">
    <xsl:param name="pAssetIdentifierFromFile"/>

    <xsl:choose>
      <xsl:when test="substring($pAssetIdentifierFromFile, 1, 1) = 'F'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <xsl:when test="
                substring($pAssetIdentifierFromFile, 1, 1) = 'P' 
                or substring($pAssetIdentifierFromFile, 1, 1) = 'C'">
        <xsl:value-of select="'O'"/>
      </xsl:when>

      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="ExtractExerciseStyle">
    <xsl:param name="pCFICode"/>
    <xsl:param name="NoTransCode" select="false()"/>

    <xsl:variable name="vTempCategoryGroup" select="substring($pCFICode, 1, 2)"/>

    <xsl:choose>
      <xsl:when test="$vTempCategoryGroup = 'FF'"/>
      <xsl:when test="$vTempCategoryGroup = 'OP' or $vTempCategoryGroup = 'OC'">

        <xsl:variable name="vTempAttribute1" select="substring($pCFICode, 3, 1)"/>

        <xsl:choose>

          <xsl:when test="$NoTransCode = false()">
            <xsl:call-template name="ExerciseStyle">
              <xsl:with-param name="pExerciseStyle" select="$vTempAttribute1"/>
            </xsl:call-template>
          </xsl:when>

          <xsl:otherwise>
            <xsl:value-of select="$vTempAttribute1"/>
          </xsl:otherwise>

        </xsl:choose>

      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="ExtractSettlMethod">
    <xsl:param name="pCFICode"/>

    <xsl:variable name="vTempCategoryGroup" select="substring($pCFICode, 1, 2)"/>

    <xsl:choose>
      <xsl:when test="$vTempCategoryGroup = 'FF'">

        <xsl:value-of select="substring($pCFICode, 4, 1)"/>

      </xsl:when>
      <xsl:when test="$vTempCategoryGroup = 'OP' or $vTempCategoryGroup = 'OC'">

        <xsl:value-of select="substring($pCFICode, 5, 1)"/>

      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="ExtractContractSymbol">
    <xsl:param name="pAssetIdentifierFromFile"/>
    <xsl:value-of select="substring($pAssetIdentifierFromFile, 2, 3)"/>
  </xsl:template>

  <!--<xsl:template name="UnderlyingAssetEnumFuture">
    <xsl:param name="pAttribute"/>

    <xsl:choose>
      <xsl:when test="$pAttribute = 'S'">
        <xsl:value-of select="'Stock'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'I'">
        <xsl:value-of select="'Index'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'D'">
        <xsl:value-of select="'Debt'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'C'">
        <xsl:value-of select="'Currency'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'O'">
        <xsl:value-of select="'option'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'F'">
        <xsl:value-of select="'Future'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'T'">
        <xsl:value-of select="'Commodity'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'W'">
        <xsl:value-of select="'Swap'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'B'">
        <xsl:value-of select="'Basket'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'M'">
        <xsl:value-of select="'Other'"/>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="UnderlyingAssetEnumOption">
    <xsl:param name="pAttribute"/>

    <xsl:choose>
      <xsl:when test="$pAttribute = 'A'">
        <xsl:value-of select="'Agricultural'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'E'">
        <xsl:value-of select="'Extraction'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'I'">
        <xsl:value-of select="'Industrial'"/>
      </xsl:when>
      <xsl:when test="$pAttribute = 'S'">
        <xsl:value-of select="'Agricultural'"/>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>
  </xsl:template>-->

  <!-- GetUnderlyingAsset 
       ******************************************************************************************************** 
       Matrice donnant la Valeur retournée en fonction des CFI code Suivants: 
       ========================================================================================================
       pCFICode               UnderlyingAssetGroup retournée
       ========================================================================================================
        FFSCNX                S
        FFSCSX                S
        FFSPNX                S
        FFSPSX                S
        FFMCSX                M (Stock Dividend)
        OCASPN                S
        OPASPN                S
        OCASPS                S
        OPASPS                S
        OCESCN                S
        OPESCN                S
        OCESPN                S
        OPESPN                S
        OCESPS                S
        OPESPS                S
        ========================================================================================================-->
  <xsl:template name="GetUnderlyingAsset">
    <xsl:param name="pCFICode"/>

    <xsl:variable name="vTempCategoryGroup" select="substring($pCFICode, 1, 2)"/>

    <xsl:choose>
      <xsl:when test="$vTempCategoryGroup = 'FF' or $vTempCategoryGroup = 'FC'">
        <xsl:value-of select="substring($pCFICode, 3, 1)"/>
      </xsl:when>
      <xsl:when test="$vTempCategoryGroup = 'OP' or $vTempCategoryGroup = 'OC'">
        <xsl:value-of select="substring($pCFICode, 4, 1)"/>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>


  </xsl:template>

  <!-- GetUnderlyingAssetGroup 
       ******************************************************************************************************** 
       Matrice donnant la Valeur retournée en fonction des CFI code Suivants: 
       ========================================================================================================
       pCFICode               UnderlyingAssetGroup retournée
       ========================================================================================================
        FFSCNX                F
        FFSCSX                F
        FFSPNX                F
        FFSPSX                F
        FFMCSX                F
        OCASPN                F
        OPASPN                F
        OCASPS                F
        OPASPS                F
        OCESCN                F
        OPESCN                F
        OCESPN                F
        OPESPN                F
        OCESPS                F
        OPESPS                F
       ========================================================================================================-->
  <xsl:template name="GetUnderlyingAssetGroup">
    <xsl:param name="pCFICode"/>

    <xsl:variable name="vTempCategoryGroup" select="substring($pCFICode, 1, 2)"/>

    <xsl:choose>
      <xsl:when test="$vTempCategoryGroup = 'FF' or $vTempCategoryGroup = 'FC'">
        <xsl:value-of select="substring($pCFICode, 2, 1)"/>
      </xsl:when>
      <xsl:when test="$vTempCategoryGroup = 'OP' or $vTempCategoryGroup = 'OC'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>


  </xsl:template>

  <!-- FL 20171006 [23064] - Creation of template GetMEFF_MaturityRule  *************************************** -->
  <!-- GetMeff_MaturityRule - return Meff MaturityRule Identifier from an Meff Derivative Contract         
       ******************************************************************************************************** 
       Affectation des règles d'échéances sur le Meff sur la base de la matrice suivante:
       ========================================================================================================
       pUnderlyingAsset         pCategory         Règle d'échéance retournée
       ========================================================================================================
       S: Stock                 F: Future         XMRV Spanish Stock Futures
                                O: Options        XMRV Stock Options
       
       I: Index                 F: Future         XMRV Index Futures
                                O: Options        XMRV Index Options
                                
       M: Dividend              F: Future         XMRV Dividend Futures
       
       Si sur un DC considéré aucune des conditions ci-dessus n'est respectées on applique une Règle d'échéance
       par défaut intitulée : Default Rule -->
  <xsl:template name="GetMEFF_MaturityRule">
    <xsl:param name="pUnderlyingAsset"/>
    <xsl:param name="pCategory"/>

    <xsl:choose>
      <!-- Stock-Equities -->
      <xsl:when test="$pUnderlyingAsset='S'">
        <xsl:choose>
          <xsl:when test="$pCategory='F'">XMRV Spanish Stock Futures</xsl:when>
          <xsl:when test="$pCategory='O'">XMRV Stock Options</xsl:when>
          <!-- Unlikely -->
          <xsl:otherwise>
            <xsl:text>Default Rule</xsl:text>
            <!-- For debug only
            <xsl:text> - AssetCategory: </xsl:text>
            <xsl:value-of select="$pCategory" />
            -->
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- Indexes -->
      <xsl:when test="$pUnderlyingAsset='I'">
        <xsl:choose>
          <xsl:when test="$pCategory='F'">XMRV Index Futures</xsl:when>
          <xsl:when test="$pCategory='O'">XMRV Index Options</xsl:when>
          <!-- Unlikely -->
          <xsl:otherwise>
            <xsl:text>Default Rule</xsl:text>
            <!-- For debug only
            <xsl:text> - AssetCategory: </xsl:text>
            <xsl:value-of select="$pCategory" />
            -->
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      
      <!-- Stock-Dividend -->
      <xsl:when test="$pUnderlyingAsset='M'">
        <xsl:choose>
          <xsl:when test="$pCategory='F'">XMRV Dividend Futures</xsl:when>
          <!-- Unlikely -->
          <xsl:otherwise>
            <xsl:text>Default Rule</xsl:text>
            <!-- For debug only
            <xsl:text> - AssetCategory: </xsl:text>
            <xsl:value-of select="$pCategory" />
            -->
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- Underlying Asset not managed -->
      <xsl:otherwise>
        <xsl:text>Default Rule</xsl:text>
        <!-- For debug only
        <xsl:text> - UnderlyingAsset: </xsl:text>
        <xsl:value-of select="$pUnderlyingAsset" />
        -->
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>

  <xsl:template name="BuildMEFFDERIVATIVECONTRACTS">
    <xsl:param name="pAssetIdentifierFromFile"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pDerivativeContractIdentifier"/>
    <!--<xsl:param name="pMaturityRuleIdentifier"/>-->

    <xsl:variable name="vFamilyCode">
      <xsl:value-of select="data[@name='FamilyCode']"/>
    </xsl:variable>

    <xsl:variable name="vFamilyMnemonicCode">
      <xsl:value-of select="data[@name='FamilyMnemonicCode']"/>
    </xsl:variable>

    <xsl:variable name="vFirstFamilyMnemonicCode">
      <xsl:value-of select="
                    concat(
                    substring($vFamilyMnemonicCode, 1, 3),
                    '0')"/>
    </xsl:variable>

    <xsl:variable name="vClearingHouseCode">
      <xsl:choose>
        <xsl:when test="data[contains(@name, 'ClearingSegmentCode')]">
          <xsl:value-of select="data[@name='ClearingSegmentCode']"/>
        </xsl:when>
        <xsl:otherwise>
          <!-- WARNING : the  MCONTRACTS.M3 file does not cotain 
        any information about Clearing House Code, we force the C2 value -->
          <xsl:value-of select="'C2'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- not available -->
    <xsl:variable name="vUnderlyingContractSymbol"/>

    <!-- Additional data will be suffixed at xml table building time -->
    <!--<xsl:variable name="vContractDisplayName" select="concat($pExchangeSymbol, ' ',$pContractSymbol,' ', $pCategory)"/>-->
    <xsl:variable name="vContractDisplayName">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:variable name="vInstrumentIdentifier">
      <xsl:call-template name="InstrumentIdentifier">
        <xsl:with-param name="pCategory" select="$pCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vCurrency"/>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vExerciseStyle"/>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vSettlMethod"/>

    <xsl:variable name="vFutValuationMethod">
      <xsl:call-template name="FutValuationMethod">
        <xsl:with-param name="pCategory" select="$pCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vContractFactor"/>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vContractMultiplier"/>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vNominalValue"/>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vUnderlyingGroup"/>

    <xsl:variable name="vAssetCategory">
      <xsl:choose>
        <!-- FL 20171006 [23495] -->
        <!-- 
          To recognize DCs whose underlying is an index, I have not found other rules that FamilyCode contains '20' or '21'
                  20 -> for Mini contracts on IBEX 35 (SSJ: ES0SI0000005)
                  21 -> for IBEX 35 contracts (SSJ: ES0SI0000005)
                  S1 -> for IBEX BANCOS contracts (SSJ: ES0S00000901)
                  S2 -> for IBEX ENERGIA contracts (SSJ: ES0S00000919)
          Ps. ISIN of the underlying IBEX 35: ES0SI0000005 Symbol: FIE, in the file CCONTRACTS.C2 only the contract 21
               is referenced
           => Can not manage it dynamically.
        -->
        <xsl:when test="contains(',20,21,S1,S2,',concat(',',$vFamilyCode,','))">
          <xsl:value-of select="'Index'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'EquityAsset'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Filled at xml table building time -->
    <xsl:variable name="vUnderlyingAsset"/>

    <!--FL 20170914 [23495]-->
    <xsl:variable name="vSQLUnderlyingIdAsset">
      <xsl:choose>
        <xsl:when test="$vAssetCategory = 'Index'">
          <SQL command="select" result="IDASSET" cache="true">
            select asset.IDASSET
            from dbo.ASSET_INDEX asset
            inner join dbo.MARKET m on (m.IDM=asset.IDM_RELATED)
            inner join dbo.MEFFCCONTRGRP mc on (mc.CONTRACTGROUPCODE=@CONTRACTGROUPCODE)
            where (m.ISO10383_ALPHA4=@ISO10383_ALPHA4) and (asset.SYMBOL=mc.UNDERLYINGMNEMONIC)
            <Param name="CONTRACTGROUPCODE" datatype="string">
              <xsl:value-of select="'21'"/>
            </Param>
            <Param name="ISO10383_ALPHA4" datatype="string">
              <xsl:value-of select="'XMRV'"/>
            </Param>
          </SQL>
        </xsl:when>
        <xsl:otherwise>
          <SQL command="select" result="IDASSET" cache="true">
            select asset.IDASSET
            from dbo.ASSET_EQUITY_RDCMK asset
            inner join dbo.MARKET m on (m.IDM=asset.IDM_RELATED)
            inner join dbo.MEFFCCONTRGRP mc on (mc.CONTRACTGROUPCODE=@CONTRACTGROUPCODE)
            where (m.ISO10383_ALPHA4=@ISO10383_ALPHA4) and (asset.SYMBOL=mc.UNDERLYINGMNEMONIC)
            <Param name="CONTRACTGROUPCODE" datatype="string">
              <xsl:value-of select="$vFamilyCode"/>
            </Param>
            <Param name="ISO10383_ALPHA4" datatype="string">
              <xsl:value-of select="'XMRV'"/>
            </Param>
          </SQL>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vAssignmentMethod">
      <xsl:call-template name="AssignmentMethod">
        <xsl:with-param name="pCategory" select="$pCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vExtSQLFilterValues" select="
                  concat(
                  'KEY_OP',',',
                  $vClearingHouseCode,',', 
                  $vFamilyCode,',', 
                  $vFamilyMnemonicCode,',', 
                  $pCategory,',', 
                  $vFirstFamilyMnemonicCode
                  )"/>
    <!-- the unique PTIDENTIFIER value will be added afterwards -->
    <xsl:variable name="vExtSQLFilterNames" select="
                  concat(
                  'MEFF_SPECIAL',',',
                  'CLEARINGHOUSECODE',',',
                  'CONTRACTGROUPCODE',',',
                  'CONTRACTTYPECODE',',',
                  'CATEGORY',',', 
                  'FIRSTCONTRACTTYPECODE')"/>

    <!-- not available -->
    <xsl:variable name ="vContractAttribute"/>

    <!-- FL 20171006 [23495] add column PHYSETTLTAMOUNT -->
    <xsl:variable name="vPhysettltamount">None</xsl:variable>

    <xsl:variable name="vContractGroupSymbol">
      <xsl:value-of select="data[@name='FamilyCode']"/>
    </xsl:variable>

    <!--FL 20170914 [23495]-->
    <xsl:variable name="vSQLMaturityRuleID">
      <xsl:call-template name="SQLIdMaturityRule2">
        <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FL 20171006 [23495] -->
    <!-- Insert into the table EXCHANGELIST the DECCRITION of the DC in function:
            ORIGNEDATA: BMECLEATRING
            EXCHANGEISO10383: XMRV
            CATEGORY: X
            CONTRACT SYMBOL of the DC -->
    <xsl:call-template name ="SQLTableEXCHANGELIST">
      <xsl:with-param name="pContractGroupSymbol" select="$vContractGroupSymbol"/>
      <xsl:with-param name="pExchangeISO10383" select="'XMRV'"/>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
    </xsl:call-template>

    <!--FL 20170914 [23495]-->
    <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pSQLMaturityRuleID">
        <xsl:copy-of select="$vSQLMaturityRuleID"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pUnderlyingContractSymbol" select="$vUnderlyingContractSymbol"/>
      <xsl:with-param name="pDerivativeContractIdentifier" select="$pDerivativeContractIdentifier"/>
      <xsl:with-param name="pContractDisplayName" select="$vContractDisplayName"/>
      <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
      <xsl:with-param name="pCurrency" select="$vCurrency"/>
      <xsl:with-param name="pCategory" select="$pCategory"/>
      <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
      <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
      <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
      <xsl:with-param name="pContractFactor" select="$vContractFactor"/>
      <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
      <xsl:with-param name="pNominalValue" select="$vNominalValue"/>
      <xsl:with-param name="pUnderlyingGroup" select="$vUnderlyingGroup"/>
      <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
      <xsl:with-param name="pSQLUnderlyingIdAsset">
        <xsl:copy-of select="$vSQLUnderlyingIdAsset"/>
      </xsl:with-param>
      <xsl:with-param name="pAssignmentMethod" select="normalize-space($vAssignmentMethod)"/>
      <xsl:with-param name="pContractAttribute" select="$vContractAttribute"/>
      <xsl:with-param name="pStrikeDecLocator"/>
      <xsl:with-param name="pMinPriceIncr"/>
      <xsl:with-param name="pMinPriceIncrAmount"/>
      <xsl:with-param name="pInsertMaturityRule" select="$gFalse"/>
      <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>

      <xsl:with-param name="pExtSQLFilterValues" select="
                    concat($vExtSQLFilterValues,',',$pDerivativeContractIdentifier,',',$vContractDisplayName)"/>
      <xsl:with-param name="pExtSQLFilterNames" select="
                    concat($vExtSQLFilterNames,',','PTIDENTIFIER',',','PTDISPLAYNAME')"/>
    </xsl:call-template>

    <!-- Additional data will be suffixed at xml table building time -->
    <xsl:variable name="vMEFFContractGroupIdSuffix">
      <xsl:call-template name="BuildPartialContractGroupIdentifier">
        <xsl:with-param name="pContractGroupSymbol" select="$vContractGroupSymbol"/>
        <xsl:with-param name="pContractGroupUnderlying"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vContractGroupIdentifier">
      <xsl:call-template name="ContractGroupIdentifier">
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        <xsl:with-param name="pContractSymbol" select="$vMEFFContractGroupIdSuffix"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name ="SQLTableCONTRACTG">
      <xsl:with-param name="pContractGroupIdentifier" select="$vContractGroupIdentifier"/>

      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>

      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>

    </xsl:call-template>


  </xsl:template>

  <xsl:template name="BuildASSETs">
    <xsl:param name="pAssetIdentifierFromFile"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pDerivativeContractIdentifier"/>
    <!--<xsl:param name="pMaturityRuleIdentifier"/>-->

    <xsl:variable name="vMaturityMonthYear">
      <xsl:value-of select="normalize-space(data[@name='ExpiryCode'])"/>
    </xsl:variable>

    <xsl:variable name="vMaturityDate">
      <xsl:value-of select="
                        concat(
                          substring(data[@name='RealExpiryDate'], 1, 4), 
                          substring(data[@name='RealExpiryDate'], 6, 2),
                          substring(data[@name='RealExpiryDate'], 9, 2)
                        )"/>
    </xsl:variable>

    <!-- Not available -->
    <xsl:variable name="vDeliveryDate"/>

    <xsl:variable name="vLastTradingDay">
      <xsl:value-of select="
                                concat(
                                  substring(data[@name='TradingEndsDateTime'], 1, 4), 
                                  substring(data[@name='TradingEndsDateTime'], 6, 2),
                                  substring(data[@name='TradingEndsDateTime'], 9, 2)
                                )"/>
    </xsl:variable>

    <!-- Not available -->
    <xsl:variable name="vUnderlyingAssetSymbol"/>

    <!-- Not available -->
    <xsl:variable name="vUnderlyingContractSymbol"/>

    <xsl:variable name="vFamilyCode">
      <xsl:value-of select="data[@name='FamilyCode']"/>
    </xsl:variable>

    <xsl:variable name="vFamilyMnemonicCode">
      <xsl:value-of select="data[@name='FamilyMnemonicCode']"/>
    </xsl:variable>

    <xsl:variable name="vClearingHouseCode">
      <xsl:choose>
        <xsl:when test="data[contains(@name, 'ClearingSegmentCode')]">
          <xsl:value-of select="data[@name='ClearingSegmentCode']"/>
        </xsl:when>
        <xsl:otherwise>
          <!-- WARNING : the  MCONTRACTS.M3 file does not cotain 
        any information about Clearing House Code, we force the C2 value -->
          <xsl:value-of select="'C2'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vExtSQLFilterValues" select="
                  concat(
                  'KEY_OP',',',
                  $vClearingHouseCode,',', 
                  $vFamilyCode,',', 
                  $vFamilyMnemonicCode,',', 
                  $pCategory
                  )"/>
    
    <!-- the unique PTIDENTIFIER value will be added afterwards -->
    <xsl:variable name="vExtSQLFilterNames" select="
                  concat(
                  'MEFF_SPECIAL',',',
                  'CLEARINGHOUSECODE',',',
                  'CONTRACTGROUPCODE',',',
                  'CONTRACTTYPECODE',',',
                  'CATEGORY')"/>

   
    <!-- Additional data will be suffixed at xml table building time -->
    <xsl:variable name="vDerivativeContractIdentifier">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:call-template name="SQLTableMATURITY_DERIVATIVEATTRIB">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
      <xsl:with-param name="pMaturityDate" select="$vMaturityDate"/>
      <xsl:with-param name="pDeliveryDate" select="$vDeliveryDate"/>
      <xsl:with-param name="pLastTradingDay" select="$vLastTradingDay"/>
      <xsl:with-param name="pUnderlyingAssetSymbol" select="$vUnderlyingAssetSymbol"/>
      <xsl:with-param name="pUnderlyingContractSymbol" select="$vUnderlyingContractSymbol"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
    </xsl:call-template>

    <xsl:variable name="vAssetDisplayName">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <!--Not available-->
    <xsl:variable name="vAssetSymbol">
      <xsl:value-of select="$pAssetIdentifierFromFile"/>
    </xsl:variable>

    <xsl:variable name="vAssetISINCode">
      <xsl:value-of select="normalize-space(data[@name='ISINCode'])"/>
    </xsl:variable>

    <xsl:variable name="vAIICode">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <!--Not available-->
    <xsl:variable name="vAssetMultiplier"/>

    <xsl:variable name="vPutCall">
      <xsl:value-of select="substring($pAssetIdentifierFromFile, 1, 1)"/>
    </xsl:variable>
    
    <xsl:variable name="vAssetPutCall">
      <xsl:choose>
        <xsl:when test="$vPutCall='C'">
          <xsl:value-of select="'1'"/>
        </xsl:when>
        <xsl:when test="$vPutCall='P'">
          <xsl:value-of select="'0'"/>
        </xsl:when>
        <xsl:otherwise>null</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Phil : insert the strikeprice for OPtion only-->
    <xsl:variable name="vAssetStrikePrice">
      <xsl:choose>
        <xsl:when test="$pCategory='O' and data[@name='StrikePrice'] != ''">
          <xsl:value-of select="number(data[@name='StrikePrice'])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gNull"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vAssetFirstQuotationDay">
      <xsl:choose>
        <xsl:when test ="data[contains(@name, 'TradingStartsDateTime')]">
          <xsl:value-of select="
          concat
          (
            substring(data[@name='TradingStartsDateTime'], 1, 4),
            substring(data[@name='TradingStartsDateTime'], 6, 2),
            substring(data[@name='TradingStartsDateTime'], 9, 2)
          )"/>
        </xsl:when>
        <xsl:otherwise/>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="SQLTableASSET_ETD">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
      <xsl:with-param name="pAssetIdentifier" select="$gAutomaticCompute"/>
      <xsl:with-param name="pAssetDisplayName" select="$vAssetDisplayName"/>
      <xsl:with-param name="pAssetSymbol" select="$vAssetSymbol"/>
      <xsl:with-param name="pAssetISINCode" select="$vAssetISINCode"/>
      <xsl:with-param name="pAIICode" select="$vAIICode"/>
      <xsl:with-param name="pAssetMultiplier" select="$vAssetMultiplier"/>
      <xsl:with-param name="pCategory" select="$pCategory"/>
      <xsl:with-param name="pAssetPutCall" select="normalize-space($vAssetPutCall)"/>
      <xsl:with-param name="pAssetStrikePrice" select="$vAssetStrikePrice"/>
      <xsl:with-param name="pAssetFirstQuotationDay" select="normalize-space($vAssetFirstQuotationDay)"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
    </xsl:call-template>

  </xsl:template>

  <!-- Main template  -->
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
      <xsl:apply-templates select="row"/>

    </file>
  </xsl:template>

  <xsl:template match="row">

    <xsl:call-template name="rowStream"/>

  </xsl:template>

  <!-- Specific to each Import -->
  <xsl:template name="rowStream">

    <xsl:choose>

      <!-- Import: BME Repository Contract Types 
            Read File: CCONTRTYP.C2 
            Manage Table: MEFFCCONTRTYP           -->
      <xsl:when test="data[contains(@status, 'success')] and 
                      data[contains(@name, 'FamilyName')] and 
                      data[contains(@name, 'SharesNumber')] and
                      normalize-space(data[@name='FamilyMnemonicCode']) != '0240'">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <xsl:variable name="vCurrency">
            <xsl:value-of select="normalize-space(data[@name='CurrencyIso3ACode'])"/>
          </xsl:variable>

          <!--Phil : CFICode II Chars (P/C) not specified for options-->

          <xsl:variable name="vCFICodeForInsert">
            <xsl:call-template name="GeneralizeCFICodeXOptions">
              <xsl:with-param name="pCFICode" select="normalize-space(data[@name='FamilyCFICode'])"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vCFICode">
            <xsl:value-of select="normalize-space(data[@name='FamilyCFICode'])"/>
          </xsl:variable>

          <xsl:variable name="vCategory">
            <xsl:call-template name="ContractCategory">
              <xsl:with-param name="pCFICode" select="$vCFICode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vExerciseStyle">
            <xsl:call-template name="ExtractExerciseStyle">
              <xsl:with-param name="pCFICode">
                <xsl:value-of select="$vCFICode"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vSettlMethod">
            <xsl:call-template name="ExtractSettlMethod">
              <xsl:with-param name="pCFICode">
                <xsl:value-of select="$vCFICode"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vContractFactor">
            <xsl:value-of select="normalize-space(data[@name='SharesNumber'])"/>
          </xsl:variable>

          <xsl:variable name="vContractMultiplier">
            <xsl:value-of select="$vContractFactor"/>
          </xsl:variable>

          <xsl:variable name="vNominalValue">
            <xsl:value-of select="normalize-space(data[@name='NominalValue'])"/>
          </xsl:variable>

          <xsl:variable name="vUnderlyingAsset">
            <xsl:call-template name="GetUnderlyingAsset">
              <xsl:with-param name="pCFICode" select="$vCFICode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUnderlyingAssetGroup">
            <xsl:call-template name="GetUnderlyingAssetGroup">
              <xsl:with-param name="pCFICode" select="$vCFICode"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vPublication">
            <xsl:call-template name="DateBeautyfier">
              <xsl:with-param name="pDate" select="data[@name='BusinessDate']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vExerciseStyleNoTransCoded">
            <xsl:call-template name="ExtractExerciseStyle">
              <xsl:with-param name="pCFICode">
                <xsl:value-of select="$vCFICode"/>
              </xsl:with-param>
              <xsl:with-param name="NoTransCode" select="true()"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vDCSuffix">
            <xsl:choose>
              <xsl:when test="$vCategory = 'F'">
                <xsl:value-of select="concat($vCategory, $vSettlMethod)"/>
              </xsl:when>
              <xsl:when test="$vCategory = 'O'">
                <xsl:value-of select="concat($vCategory, $vExerciseStyleNoTransCoded, $vSettlMethod)"/>
              </xsl:when>
              <xsl:otherwise/>
            </xsl:choose>
          </xsl:variable>

          <!-- Maturity Rule -->
          <!-- FL 20171006 [23495] New variable -->
          <xsl:variable name="vMaturityRule">
            <xsl:call-template name="GetMEFF_MaturityRule">
              <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
              <xsl:with-param name="pCategory" select="$vCategory"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:call-template name ="SQLTableMEFFCCONTRTYP">

            <xsl:with-param name="pExtSQLFilterValues" select="
                        concat(data[@name='ClearingSegmentCode'],',',data[@name='FamilyCode'],',',data[@name='FamilyMnemonicCode'])"/>
            <xsl:with-param name="pExtSQLFilterNames" select="
                        concat('CLEARINGHOUSECODE',',','CONTRACTGROUPCODE',',','CONTRACTTYPECODE')"/>

            <xsl:with-param name="pClearingHouseCode" select="data[@name='ClearingSegmentCode']"/>
            <xsl:with-param name="pContractGroupCode" select="data[@name='FamilyCode']"/>
            <xsl:with-param name="pContractTypeCode" select="data[@name='FamilyMnemonicCode']"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
            <xsl:with-param name="pPublication" select="$vPublication"/>
            <xsl:with-param name="pContractTypeDescription" select="data[@name='FamilyName']"/>
            <xsl:with-param name="pContractTypeMultiplier" select="$vContractMultiplier"/>
            <xsl:with-param name="pNominalValue" select="$vNominalValue"/>
            <xsl:with-param name="pCalcMethod" select="data[@name='CalcMethod']"/>
            <xsl:with-param name="pCFICode" select="$vCFICodeForInsert"/>
            <xsl:with-param name="pCategory" select="$vCategory"/>
            <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
            <xsl:with-param name="pUnderlyingAssetGroup" select="$vUnderlyingAssetGroup"/>
            <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
            <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
            <xsl:with-param name="pMaturityRule" select="$vMaturityRule"/>

          </xsl:call-template>

        </row>

      </xsl:when>

      <!-- Import: BME Repository Contracts
            Read File: CCONTRGRP.C2
            Manage Table: GCONTRACT -->
      <xsl:when test ="data[contains(@status, 'success')] and 
                      data[contains(@name, 'FamilyType')] and 
                      data[contains(@name, 'UnderlyingMnemonic')]">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <!-- WARNING : we insert just the XMRV data market. The MEFF identifier code for that market is M3 (hardcoded) -->
          <xsl:variable name="vExchangeSymbol">
            <xsl:value-of select="'M3'"/>
          </xsl:variable>

          <xsl:variable name="vClearingHouseCode">
            <xsl:value-of select="data[@name='ClearingSegmentCode']"/>
          </xsl:variable>

          <xsl:variable name="vContractGroupSymbol">
            <xsl:value-of select="data[@name='FamilyCode']"/>
          </xsl:variable>

          <xsl:variable name="vContractGroupUnderlying">
            <xsl:value-of select="data[@name='UnderlyingMnemonic']"/>
          </xsl:variable>

          <xsl:variable name="vContractGroupDescription">
            <xsl:value-of select="data[@name='FamilyType']"/>
          </xsl:variable>

          <xsl:variable name="vMEFFContractGroupIdSuffix">
            <xsl:call-template name="BuildPartialContractGroupIdentifier">
              <xsl:with-param name="pContractGroupSymbol" select="$vContractGroupSymbol"/>
              <xsl:with-param name="pContractGroupUnderlying" select="$vContractGroupUnderlying"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vContractGroupIdentifier">
            <xsl:call-template name="ContractGroupIdentifier">
              <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
              <xsl:with-param name="pContractSymbol" select="$vMEFFContractGroupIdSuffix"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:call-template name="SQLTableGCONTRACT">
            <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
            <xsl:with-param name="pContractGroupIdentifier" select="$vContractGroupIdentifier"/>
            <xsl:with-param name="pContractGroupDisplayName" select="$vContractGroupDescription"/>
            <xsl:with-param name="pContractGroupDescription"/>
            <xsl:with-param name="pContractGroupSymbol" select="$vContractGroupSymbol"/>
          </xsl:call-template>

          <xsl:call-template name="SQLTableGCONTRACTROLE">
            <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
            <xsl:with-param name="pContractGroupIdentifier" select="$vContractGroupIdentifier"/>
            <xsl:with-param name="pContractGroupRoleType" select="'MARGIN'"/>
          </xsl:call-template>

          <xsl:call-template name="SQLTableMEFFCCONTRGRP">

            <xsl:with-param name="pClearingHouseCode" select="$vClearingHouseCode"/>
            <xsl:with-param name="pContractGroupCode" select="$vContractGroupSymbol"/>

            <xsl:with-param name="pExtSQLFilterValues" select="
                        concat($vClearingHouseCode,',', $vContractGroupSymbol)"/>
            <xsl:with-param name="pExtSQLFilterNames" select="
                        concat('CLEARINGHOUSECODE',',','CONTRACTGROUPCODE')"/>

            <xsl:with-param name="pContractGroupUnderlying" select="$vContractGroupUnderlying"/>
          </xsl:call-template>

        </row>

      </xsl:when>

      <!-- Import: BME Repository Asset Types
            Read File: CCONTRACTS.C2
            Manage Table: ASSET_EQUITY, ASSET_EQUITY_RDCMK, ASSET_INDEX -->
      <xsl:when test="data[contains(@status, 'success')] and 
                      data[contains(@name, 'UnderlyingShortId')] and 
                      data[contains(@name, 'UnderlyingMnemonic')] and
                      data[contains(@name, 'ArrayCode')] and
                      normalize-space(data[@name='FamilyMnemonicCode']) = '0240'">
        <!--FL 20171006 [23064] - Manage the creation of 
             Equity (ASSET_EQUITY, ASSET_EQUITY_RDCMK) 
             and index (ASSET_INDEX) 
            FamilyMnemonicCode = 0240 -->
        
        <xsl:variable name="vAssetIdentifierFromFile">
          <xsl:call-template name="AssetIdentifier">
            <xsl:with-param name="pAssetIdentifierFromFile" select="data[@name='AssetIdentifier']"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vCategory">
          <xsl:call-template name="ContractCategoryFromAsset">
            <xsl:with-param name="pAssetIdentifierFromFile" select="$vAssetIdentifierFromFile"/>
          </xsl:call-template>
        </xsl:variable>

        <row>

          <xsl:call-template name="IORowAtt"/>

          <xsl:variable name="vExchangeSymbol">
            <xsl:value-of select="'M3'"/>
          </xsl:variable>

          <xsl:variable name="vContractSymbol">
            <xsl:call-template name="ExtractContractSymbol">
              <xsl:with-param name="pAssetIdentifierFromFile" select="$vAssetIdentifierFromFile"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUnderlyingAssetSymbol">
            <xsl:value-of select="data[@name='UnderlyingMnemonic']"/>
          </xsl:variable>

          <xsl:variable name="vFamilyCode">
            <xsl:value-of select="data[@name='FamilyCode']"/>
          </xsl:variable>

          <xsl:variable name="vFamilyMnemonicCode">
            <xsl:value-of select="data[@name='FamilyMnemonicCode']"/>
          </xsl:variable>

          <xsl:variable name="vIsincode">
            <xsl:value-of select="data[@name='ISINCode']"/>
          </xsl:variable>

          <xsl:variable name="vClearingHouseCode">
            <xsl:choose>
              <xsl:when test="data[contains(@name, 'ClearingSegmentCode')]">
                <xsl:value-of select="data[@name='ClearingSegmentCode']"/>
              </xsl:when>
              <xsl:otherwise>
                <!-- WARNING : the  MCONTRACTS.M3 file does not cotain 
                     any information about Clearing House Code, we force the C2 value -->
                <xsl:value-of select="'C2'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vExtSQLFilterValues" select="
                  concat(
                  'KEY_OP',',',
                  $vClearingHouseCode,',', 
                  $vFamilyCode,',', 
                  $vFamilyMnemonicCode,',', 
                  $vCategory
                  )"/>

          <xsl:variable name="vExtSQLFilterNames" select="
                  concat(
                  'MEFF_SPECIAL',',',
                  'CLEARINGHOUSECODE',',',
                  'CONTRACTGROUPCODE',',',
                  'CONTRACTTYPECODE',',',
                  'CATEGORY')"/>

          <!-- Currency: EUR -->
          <xsl:variable name="vIdc">EUR</xsl:variable>

          <!-- Exchange Equity: XMAD -->
          <xsl:variable name="vISO10383">XMAD</xsl:variable>

          <!-- Exchange Related: XMRV -->
          <xsl:variable name="vIso10383Related">XMRV</xsl:variable>

          <!-- ExchangeSymbolRelated: XMRV -->
          <xsl:variable name="vExchangeSymbolRelated">M3</xsl:variable>

          <!-- AssetTitled -->
          <xsl:variable name="vSQLDescription">
            <SQL command="select" result="DESCRIPTION" cache="true">
              <![CDATA[select @AUTOMATICCOMPUTE || gc.DISPLAYNAME as DESCRIPTION
              from dbo.GCONTRACT gc
              where (gc.GROUPSYMBOL=@FAMILYCODE)
              ]]>
              <Param name="FAMILYCODE" datatype="string">
                <xsl:choose>
                  <!--When the label is more than 37 characters, it is trunked and added '...'  -->
                  <xsl:when test="string-length(normalize-space(data[@name='FamilyCode'])) > 37">
                    <xsl:value-of select="concat(substring(normalize-space(data[@name='FamilyCode']),0,37),'...')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="normalize-space(data[@name='FamilyCode'])"/>
                  </xsl:otherwise>
                </xsl:choose>
              </Param>
              <Param name="AUTOMATICCOMPUTE" datatype="string">
                <xsl:value-of select="$gAutomaticComputeSQL"/>
              </Param>
            </SQL>
          </xsl:variable>

          <!-- FL 20171006 [23495] -->
          <!-- 
          To recognize DCs whose underlying is an index, I have not found other rules that FamilyCode contains '20' or '21'
                  20 -> for Mini contracts on IBEX 35 (SSJ: ES0SI0000005)
                  21 -> for IBEX 35 contracts (SSJ: ES0SI0000005)
                  S1 -> for IBEX BANCOS contracts (SSJ: ES0S00000901)
                  S2 -> for IBEX ENERGIA contracts (SSJ: ES0S00000919)
          Ps. ISIN of the underlying IBEX 35: ES0SI0000005 Symbol: FIE, in the file CCONTRACTS.C2 only the contract 21
               is referenced
           => Can not manage it dynamically.
        -->
          <xsl:choose>
            <xsl:when test="contains(',20,21,S1,S2,',concat(',',$vFamilyCode,','))">
              <xsl:call-template name="SQLTableASSET_INDEX">
                <xsl:with-param name="pSymbol"          select="$vAssetIdentifierFromFile"/>
                <xsl:with-param name="pIsincode"        select="$vIsincode"/>
                <xsl:with-param name="pIdc"             select="$vIdc"/>
                <xsl:with-param name="pIso10383"        select="$vISO10383"/>
                <xsl:with-param name="pIso10383Related" select="$vIso10383Related"/>
                <xsl:with-param name="pSQLdescription">
                  <xsl:copy-of select="$vSQLDescription"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
             <xsl:call-template name="SQLTableASSET_EQUITY">
                <xsl:with-param name="pSymbol"                select="$vAssetIdentifierFromFile"/>
                <xsl:with-param name="pIsincode"              select="$vIsincode"/>
                <xsl:with-param name="pIdc"                   select="$vIdc"/>
                <xsl:with-param name="pIso10383"              select="$vISO10383"/>
                <xsl:with-param name="pExchangeSymbolRelated" select="$vExchangeSymbolRelated"/>
                <xsl:with-param name="pExtSQLFilterValues"    select="$vExtSQLFilterValues"/>
                <xsl:with-param name="pExtSQLFilterNames"     select="$vExtSQLFilterNames"/>
                <xsl:with-param name="pSQLdescription">
                  <xsl:copy-of select="$vSQLDescription"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </row>

      </xsl:when>

      <!-- Import: BME Repository Assets
            Read File: MCONTRACTS.M3
            Manage Table: EXCHANGELIST, DERIVATIVECONTRACT, CONTRACTG -->
      <xsl:when test="data[contains(@status, 'success')] and 
                      data[contains(@name, 'TradingStartsDateTime')] and 
                      data[contains(@name, 'TSBuyingAssetCode')] and
                      data[contains(@name, 'TSSellingAssetCode')] and
                      normalize-space(data[@name='FamilyMnemonicCode']) != '0240'">

        <xsl:variable name="vAssetIdentifierFromFile">
          <xsl:call-template name="AssetIdentifier">
            <xsl:with-param name="pAssetIdentifierFromFile" select="data[@name='AssetIdentifier']"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vCategory">
          <xsl:call-template name="ContractCategoryFromAsset">
            <xsl:with-param name="pAssetIdentifierFromFile" select="$vAssetIdentifierFromFile"/>
          </xsl:call-template>
        </xsl:variable>

        <!-- Instrument filter -->
        <xsl:if test="$vCategory = 'F' or $vCategory = 'O'">

          <row>
            <xsl:call-template name="IORowAtt"/>

            <xsl:variable name="vContractSymbol">
              <xsl:call-template name="ExtractContractSymbol">
                <xsl:with-param name="pAssetIdentifierFromFile" select="$vAssetIdentifierFromFile"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExchangeSymbol">
              <xsl:value-of select="'M3'"/>
            </xsl:variable>

            <!-- Additional data will be suffixed at xml table building time -->
            <xsl:variable name="vDerivativeContractIdentifier">
              <xsl:value-of select="$gAutomaticCompute"/>
            </xsl:variable>

            <xsl:call-template name="BuildMEFFDERIVATIVECONTRACTS">
              <xsl:with-param name="pAssetIdentifierFromFile" select="$vAssetIdentifierFromFile"/>
              <xsl:with-param name="pCategory" select="$vCategory"/>
              <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
              <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
              <xsl:with-param name="pDerivativeContractIdentifier" select="$vDerivativeContractIdentifier"/>
            </xsl:call-template>

          </row>

        </xsl:if>

      </xsl:when>

      <xsl:otherwise/>

    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>
