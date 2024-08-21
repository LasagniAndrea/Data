<!--
=========================================================================================
 Summary : Import Eurex Referential                           
 File    : MarketData_EurexCommon_Import_Map.xsl              
=========================================================================================
 Version : v13.0.0.0                                           
 Date    : 20230726
 Author  : PL
 Comment : [XXXXX] - Use new file ProductList
=========================================================================================  
 Version : v6.0.0.0    
 Date    : 20170420    
 Author  : FL/PLA
 Comment : [23064] - Derivative Contracts: Settled amount behavior for "Physical" delivery
                     Add pPhysettltamount parameter on ovrSQLGetDerivativeContractDefaultValue template
=========================================================================================
FL 20140220 [19648] 
  Manage DERIVATIVECONTRACT.UNDERLYINGGROUP & DERIVATIVECONTRACT.UNDERLYINGASSET
  in template (ovrSQLGetDerivativeContractDefaultValue)
=========================================================================================
FI 20131205 [19275] 
  pContractMultiplierSpecified n'existe plus dans le template SQLTableDERIVATIVECONTRACT 
  Utilisation des paramètres pInsContractMultiplier et pUpdContractMultiplier
=========================================================================================
FI 20131129 [19284] 
  Manage DERIVATIVECONTRACT.FINALSETTLTSIDE
  in template (ovrSQLGetDerivativeContractDefaultValue)
=========================================================================================
FL 20130222
  Simplification de cet import (suppression de tous les imports autres que celui
            des DC) et enrichissement des données.
=========================================================================================
MF 20100127                                           
  Import Eurex Referential File                   
=========================================================================================
MF 20100312
  column DERIVATIVEATTRIB.DERIVATIVEATTRIBUTE is not used anymore
=========================================================================================
MF 20100219
  DERIVATIVECONTRACT.STRIKEDECLOCATOR added         
=========================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Imports -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes -->
  <xsl:include href="MarketData_Common.xsl"/>

  <!-- PM 20140515 [19970][19259] Template de nouveau utilisé pour la devise du nominal-->
  <!-- BD 20130524 [Ticket TRIM 18677] : Ce template ne doit plus être utilisé pour convertir GBX (British Pence) en GBP ! -->
  <!-- Transcoding template for unknown currencies -->
  <xsl:template name="CurrencyIso3ACode">
    <xsl:param name="pCurrency"/>
    <xsl:choose>
      <xsl:when test="$pCurrency = 'GBX'">
        <xsl:value-of select="'GBP'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pCurrency"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <row>                                       -->
  <!-- ================================================== -->
  <xsl:template name="rowStreamCommon">
    <!-- PL 20230726 A REVOIR Les 2 paramètres sont inutilisés -->
    <!--
    <xsl:param name="pTradingStartsDateTime"/>
    <xsl:param name="pIsSeriesChange" select="false()"/>
    -->
    <!-- PL 20230726 A REVOIR Variable inutilisée -->
    <!--
    <xsl:variable name="vChangeIdentifierCode">
      <xsl:if test="data[contains(@name, 'CIC')]">
        <xsl:value-of select="normalize-space(data[@name='CIC'])"/>
      </xsl:if>

      <xsl:if test="data[contains(@name, 'E00')]">
        <xsl:value-of select="normalize-space(data[@name='E00'])"/>
      </xsl:if>
    </xsl:variable>
    -->
    <!-- ================================================================================ -->
    <!--                              Shared variables                                    -->
    <!-- ================================================================================ -->
    <!-- vExchangeSymbol -->
    <xsl:variable name="vExchangeSymbol">
      <xsl:value-of select="normalize-space(data[@name='MC'])"/>
    </xsl:variable>

    <!-- vContractSymbol : we use the FamilyMnemonicCode node value which contains the Eurex product code -->
    <xsl:variable name="vContractSymbol">
      <xsl:value-of select="normalize-space(data[@name='FMC'])"/>
    </xsl:variable>

    <!-- ContractAttribute -->
    <xsl:variable name ="vContractAttribute">
      <xsl:value-of select="data[@name='VN']"/>
    </xsl:variable>

    <!-- vCategory : When CombinedFamilyType is Call or Put then we choose Option else Future -->
    <xsl:variable name="vCategory">
      <xsl:choose>
        <xsl:when test="normalize-space(data[@name='CFT'])='C' or normalize-space(data[@name='CFT'])='P'">
          <xsl:value-of select="'O'"/>
        </xsl:when>
        <xsl:when test="normalize-space(data[@name='CFT'])='F'">
          <xsl:value-of select="'F'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- vContractFactor -->
    <!-- This variable is filled with the SharesNumber node value, 
           which is the number of shares to be delivered in the event of exercise / assignment  -->
    <xsl:variable name="vContractFactor">
      <xsl:value-of select="normalize-space(data[@name='SN'])"/>
    </xsl:variable>

    <!-- vContractMultiplier : WARNING - Note Importante à lire absolument -->
    <!-- FL 20130221 : [17435] le ContractMultiplier n'est pas mis à jour dans cet import. 
           Il sera mis à jour dans le cadre de l'importation des cotations et paramètre de RISK en repectant la règle suivante :
           (extrait du ticket [17435])
            Le fichier des cotations et paramètre de RISK contient le paramètres Contract Multiplier pour tous les actifs. 
            Cela donne lieu:
              - A la mise à jour du Contract Multiplier sur le référentiel "Contrats Dérivés", pour les contrats dérivés avec "Version du contrat" égale à 0.
              - A la mise à jour du Contract Multiplier sur le référentiel "Actifs", pour les contrats dérivés avec "Version du contrat" égale ou supérieur à 1. -->

    <!-- FL 20130221 : 
         La valorisation des données suivantes est effectuée depuis le template ovrSQLGetDerivativeContractDefaultValue (Override of SQLGetDerivativeContractDefaultValue): 
         - ExerciseStyle, AssetCategory, SettlMethod, FutValuationMethod, IsinCode, Assetcategroy, Idasset_UNL, Idmaturityrule  
         NB: ce template récupère ces données depuis la table EUREXPRODUCT à partir du ContractSymbol du DC. 
    -->

    <!-- vCurrency : it is filled with the ISO Code (unknown curency will be transcoded) -->
    <xsl:variable name="vCurrency">
      <!-- BD 20130524 [Ticket TRIM 18677] : On utilise data[@name='CU'] pour vCurrency -->
      <xsl:value-of select="normalize-space(data[@name='CU'])"/>
      <!--<xsl:call-template name="CurrencyIso3ACode">
        <xsl:with-param name="pCurrency" select="normalize-space(data[@name='CU'])"/>
      </xsl:call-template>-->
    </xsl:variable>

    <!--PM 20140515 [19970][19259] Ajout vNominalCurrency -->
    <xsl:variable name="vNominalCurrency">
      <xsl:call-template name="CurrencyIso3ACode">
        <xsl:with-param name="pCurrency" select="$vCurrency"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- vStrikeDecLocator -->
    <xsl:variable name="vStrikeDecLocator">
      <xsl:value-of select="number(data[@name='SPDN'])"/>
    </xsl:variable>

    <!-- vInstrumentIdentifier : translating vCategory in to a mnemonic code -->
    <xsl:variable name="vInstrumentIdentifier">
      <xsl:call-template name="InstrumentIdentifier">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- vAssignmentMethod -->
    <xsl:variable name="vAssignmentMethod">
      <xsl:call-template name="AssignmentMethod">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vExtSQLFilterValues" select="concat('KEY_OP',',',$vContractAttribute)"/>
    <xsl:variable name="vExtSQLFilterNames" select="concat('EUREX_SPECIAL',',','CONTRACTATTRIBUTEEUREX')"/>

    <!-- ================================================================================ -->
    <!-- Call template SQLTableDERIVATIVECONTRACT                                          -->
    <!-- ================================================================================ -->
    <!-- FI 20131205 [19275] Alimentation des paramètres pInsContractMultiplier et pUpdContractMultiplier
                             Contract Multiplier n'est jamais alimenté par l'import
    -->
    <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
      <xsl:with-param name="pISO10383" select="'XEUR'"/>
      <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
      <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
      <xsl:with-param name="pDerivativeContractIdentifier" select="$gAutomaticCompute"/>
      <xsl:with-param name="pContractDisplayName" select="$gAutomaticCompute"/>
      <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
      <xsl:with-param name="pCurrency" select="$vCurrency"/>
      <xsl:with-param name="pCategory" select="$vCategory"/>
      <xsl:with-param name="pContractFactor" select="$vContractFactor"/>
      <xsl:with-param name="pContractMultiplier"/>
      <xsl:with-param name="pInsContractMultiplier" select="$gFalse"/>
      <xsl:with-param name="pUpdContractMultiplier" select="$gFalse"/>
      <xsl:with-param name="pAssignmentMethod" select="$vAssignmentMethod"/>
      <xsl:with-param name="pContractAttribute" select="$vContractAttribute"/>
      <xsl:with-param name="pStrikeDecLocator" select="$vStrikeDecLocator"/>
      <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$gTrue"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <!--PM 20140515 [19970][19259] Ajout pNominalCurrency pour continuer à prendre la devise du fichier SPAN pour la devise du nominal-->
      <xsl:with-param name="pNominalCurrency" select="$vNominalCurrency"/>
    </xsl:call-template>

  </xsl:template>

  <!-- Override of SQLGetDerivativeContractDefaultValue -->
  <!-- FI 20131121 [19216] add CONTRACTTYPE (CONTRACTTYPE retourne tjs 'STD', la table EUREXPRODUCT ne contient que des contrats standard) -->
  <!-- FI 20131129 [19284] add FINALSETTLTSIDE (FINALSETTLTSIDE 'OfficialSettlement' sur les options sur indice et 'OfficialClose' sur les -->
  <!--                     autres DC option) -->
  <!-- FL 20140220 [19648] add UNDERLYINGGROUP & UNDERLYINGASSET(Retourne tjs les valeurs contenues dans la table EUREXPRODUCT)            -->
  <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
  <xsl:template name="ovrSQLGetDerivativeContractDefaultValue">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:choose>
      <!-- BD 20130226 Gestion de valeur par defaut dans le cas d'un DC inexistant dans la table EUREXPRODUCT -->
      <!-- FL 20130228 Affectation de la valeur par défaut de la colonne FUTVALUATIONMETHOD à 'FUT' -->      
      <xsl:when test="$pResult='IDMATURITYRULE' or
                      $pResult='SETTLTMETHOD' or
                      $pResult='PHYSETTLTAMOUNT' or
                      $pResult='EXERCISESTYLE' or
                      $pResult='FUTVALUATIONMETHOD' or
                      $pResult='ASSETCATEGORY' or
                      $pResult='IDASSET_UNL' or
                      $pResult='ISINCODE' or
                      $pResult='CONTRACTTYPE' or
                      $pResult='FINALSETTLTSIDE'or
                      $pResult='UNDERLYINGGROUP' or $pResult='UNDERLYINGASSET' or
                      $pResult='BLOOMBERG_CODE' or $pResult='REUTERS_CODE'">
        <SQL command="select" result="{$pResult}" cache="true">
          <![CDATA[
          select '0' as LEVELSORT,
          IDMATURITYRULE, SETTLTMETHOD, case when SETTLTMETHOD = 'C' then 'NA' else 'None' end as PHYSETTLTAMOUNT, 
          EXERCISESTYLE, FUTVALUATIONMETHOD, IDASSET_UNL, ASSETCATEGORY, PRODUCT_ISIN as ISINCODE, 'STD' as CONTRACTTYPE, 
          case when CATEGORY = 'O' then case when ASSETCATEGORY='Index' then 'OfficialSettlement' else 'OfficialClose' end else null end as FINALSETTLTSIDE,
          UNDERLYINGGROUP, UNDERLYINGASSET, BLOOMBERG_CODE, REUTERS_CODE
          from dbo.EUREXPRODUCT
          where (CONTRACTSYMBOL=@CONTRACTSYMBOLEXL)
          union all
          select '1' as LEVELSORT, 
          null as IDMATURITYRULE, 'C' as SETTLTMETHOD, 'NA' as PHYSETTLTAMOUNT, 
          '1' as EXERCISESTYLE, 'FUT' as FUTVALUATIONMETHOD, null as IDASSET_UNL, null as ASSETCATEGORY, null as ISINCODE, 'STD' as CONTRACTTYPE, 
          'OfficialClose' as FINALSETTLTSIDE, 
          'F' as UNDERLYINGGROUP, 'FC' as UNDERLYINGASSET, null as BLOOMBERG_CODE, null as REUTERS_CODE
          from DUAL
          order by LEVELSORT
          ]]>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
        </SQL>
      </xsl:when>

      <xsl:when test="$pResult='IDDC_UNL'">
        <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
        <SQL command="select" result="IDDC" cache="true">
          <![CDATA[
          select dc.IDDC
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.EUREXPRODUCT eurprod on (eurprod.CONTRACTSYMBOL=@CONTRACTSYMBOLEXL)
          inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4='XEUR')
          where (dc.ISINCODE= eurprod.UNDERLYING_ISIN) and (dc.CATEGORY='F')
          ]]>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <Param name="DT" datatype="date">
            <xsl:value-of select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
          </Param>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
        </SQL>
      </xsl:when>
      <xsl:otherwise>null</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
