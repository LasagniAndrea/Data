<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- 
   ==================================================================================================================
   Summary      : Importation des DC de la chambre de compensation OCC.                    
                  Les DC sont importés sur le marché de Symbole XUSS. 
                  Ce dernier est un marché virtuel destiné à regrouper tous les DC compensés par la chambre OCC,
                  et ce quelque soit le marché où a eu lieu la négociation (CBOE, ISE, OneChigago…), 
                  information fréquemment indisponible dans les systèmes amonts.
                  Par ailleurs, de ce marché unique de la chambre offre implicitement la fongibilité.   
  ===================================================================================================================
   Version: v8.1.0.7754    Date: 20210324    Author: FL
   Comment: [25539] Problema esercizio vix a scadenza (21/10/2020 Forced to 22/10/2020) - 
                     wrongly exercised by using the closing price of 20/10/2020
    Alimentation de DERIVATIVECONTRACT.FINALSETTLTPRICE qui est toujours égale sur cette chambre à 'ExpiryDate'
  ===================================================================================================================  
  Version: v6.0.0.0    Date: 20170420    Author: FL/PLA
  Comment: [23064] - Derivative Contracts: Settled amount behavior for "Physical" delivery
  Add pPhysettltamount parameter on SQLTableDERIVATIVECONTRACT template
  ===================================================================================================================
  FL 20140514[19933] (Repository : UnderlyingGroup & UnderlyingAsset)
    Alimentation de DERIVATIVECONTRACT.UNDERLYINGGROUP qui est toujours égale sur cette chambre à 'F' -> (Financial)
     
    Alimentation de DERIVATIVECONTRACT.UNDERLYINGASSET qui est égale sur cette chambre à :
     FS (Stock-Equities) si AssetCategory = 'EquityAsset'
     FI (Indices) si AssetCategory = 'Index'

  FL 20140327 [19796]
    Alimentation de DERIVATIVECONTRACT.FINALSETTLTPRICE qui est toujours égale sur cette chambre à 'LastTradingDay'

  FI 20140108 [19460]
    Les parameters SQL doivent être en majuscule
     
  FI 20131129 [19284]
     Alimentation de DERIVATIVECONTRACT.FINALSETTLTSIDE
     
  FL 20131107 [19141]
    Contratto VIX O, [18812] - MANAGE MATURITY RULES - New MATURITY RULES
      Maturity Rule Added in template(GetMaturityRuleIdentifier_Occ):
        - XUSS Index Options Monthly Volatility Index

  FL  20130905 [18404]
    Regola di scadenza per le opzioni OCC, [18812] - MANAGE MATURITY RULES - New MATURITY RULES
      Maturity Rule Added in template(GetMaturityRuleIdentifier_Occ) :
        - XUSS Equity Options Monthly
        - XUSS Equity Options Monthly & Weekly
        - XUSS Equity Options Monthly, Quaterly & Weekly
        - XUSS Index Options Monthly
        - XUSS Index Options Monthly & Weekly
        - XUSS Index Options Monthly, Quaterly & Weekly
   
   BD 20130531 
    Gestion des DC "Mini Options" du CBOE : AMZN7, AAPL7, GLD7, SPY7
  ===================================================================================================================
 -->

  <!-- ================================================== -->
  <!--        import(s)                                   -->
  <!-- ================================================== -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="MarketData_Common.xsl"/>

  <!-- ================================================== -->
  <!--        <iotask>                                    -->
  <!-- ================================================== -->
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
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <parameters>                                -->
  <!-- ================================================== -->
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

  <!-- ================================================== -->
  <!--        <iotaskdet>                                 -->
  <!-- ================================================== -->
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

  <!-- ================================================== -->
  <!--        <ioinput>                                   -->
  <!-- ================================================== -->
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

  <!-- ================================================== -->
  <!--        <file>                                      -->
  <!-- ================================================== -->
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
      <xsl:apply-templates select="row"/>
    </file>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <row>                                       -->
  <!-- ================================================== -->
  <xsl:template match="row">

    <row>
      <xsl:call-template name="rowStream"/>
    </row>
  </xsl:template>

  <!-- ============================================== -->
  <!--                 rowStream                      -->
  <!-- ============================================== -->
  <xsl:template name="rowStream">

    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <!-- RECUPERATION DES VARIABLES -->
    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->

    <!-- Exchange Symbol, Code ISO -->
    <xsl:variable name="vExchangeSymbol">XUSS</xsl:variable>
    <xsl:variable name="vISO10383">XUSS</xsl:variable>

    <!-- Contract Identifier -->
    <xsl:variable name="vDerivativeContractIdentifier">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <!-- Contract Symbol -->
    <xsl:variable name="vContractSymbol">
      <xsl:value-of select="normalize-space(data[@name='Symbol'])"/>
    </xsl:variable>

    <!-- Company Name -->
    <xsl:variable name="vCompanyName">
      <xsl:value-of select="normalize-space(data[@name='Name'])"/>
    </xsl:variable>

    <!-- Category : Option -->
    <xsl:variable name="vCategory">O</xsl:variable>

    <!-- FutValuationMethod : EQTY -->
    <xsl:variable name="vFutValuationMethod">EQTY</xsl:variable>

    <!-- Currency : : USD -->
    <xsl:variable name="vCurrency">USD</xsl:variable>

    <!-- InstrumentIdentifier -->
    <xsl:variable name="vInstrumentIdentifier">
      <xsl:call-template name="InstrumentIdentifier">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- AssetCategory -->
    <!-- Appel du template "GetAssetCategory_Occ" qui determine l'AssetCategory en fonction du Symbol du DC -->
    <xsl:variable name="vAssetCategory">
      <xsl:call-template name="GetAssetCategory_Occ">
        <xsl:with-param name="pSymbol" select="$vContractSymbol"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Maturity Rule Identifier -->
    <xsl:variable name="vMaturityRuleIdentifier">
      <xsl:call-template name="GetMaturityRuleIdentifier_Occ">
        <xsl:with-param name="pSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
        <xsl:with-param name="pProductType1" select="data[@name='ProductType1']"/>
        <xsl:with-param name="pProductType2" select="data[@name='ProductType2']"/>
        <xsl:with-param name="pProductType3" select="data[@name='ProductType3']"/>
        <xsl:with-param name="pProductType4" select="data[@name='ProductType4']"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- SettltMethod -->
    <xsl:variable name="vSettlMethod">
      <!-- En fonction de l'AssetCategory -->
      <xsl:choose>
        <!-- EquityAsset -> P (Phys) -->
        <xsl:when test="$vAssetCategory='EquityAsset'">P</xsl:when>
        <!-- Index -> C (Cash) -->
        <xsl:when test="$vAssetCategory='Index'">C</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- ExerciseStyle -->
    <xsl:variable name="vExerciseStyle">
      <!-- Appel du template "GetExerciseStyle_Occ" qui determine l'ExerciseStyle en fonction de l'AssetCategory et du Symbol du DC -->
      <xsl:call-template name="GetExerciseStyle_Occ">
        <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
        <xsl:with-param name="pSymbol" select="$vContractSymbol"/>
      </xsl:call-template>
    </xsl:variable>

    <!--FL 20140514 [19933]  -->
    <!-- UnderlyingAsset     -->
    <xsl:variable name="vUnderlyingAsset">
      <!-- En fonction de l'AssetCategory -->
      <xsl:choose>
        <!-- EquityAsset -> FS (Stock-Equities) -->
        <xsl:when test="$vAssetCategory='EquityAsset'">FS</xsl:when>
        <!-- Index -> FI (Indices) -->
        <xsl:when test="$vAssetCategory='Index'">FI</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- FL/PLA 20170419 [23064] add column PHYSETTLTAMOUNT -->
    <xsl:variable name="vPhysettltamount">
      <xsl:choose>
        <xsl:when test="($vSettlMethod = 'C')">NA</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <!-- ~~~~~~~~~~~~~~~~~ -->
    <!-- GENERATION DU SQL -->
    <!-- ~~~~~~~~~~~~~~~~~ -->

    <xsl:variable name="vAssetSymbol">
      <xsl:value-of select="normalize-space(data[@name='Symbol'])"/>
    </xsl:variable>
    <!-- En fonction de l'AssetCategory -->
    <xsl:choose>
      <xsl:when test="$vAssetCategory='EquityAsset'">
        <!-- Création des Equities -->
        <xsl:call-template name="SQLTableASSET_EQUITY">
          <xsl:with-param name="pIdc" select="$vCurrency"/>
          <xsl:with-param name="pSymbol" select="$vAssetSymbol"/>
          <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
          <xsl:with-param name="pExchangeSymbolRelated" select="$vExchangeSymbol"/>
          <xsl:with-param name="pAssetTitled" select="$vCompanyName"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$vAssetCategory='Index'">
        <!-- Création des Index -->
        <xsl:call-template name="SQLTableASSET_INDEX">
          <xsl:with-param name="pIdc" select="$vCurrency"/>
          <xsl:with-param name="pSymbol" select="$vAssetSymbol"/>
          <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
          <xsl:with-param name="pExchangeSymbolRelated" select="$vExchangeSymbol"/>
          <xsl:with-param name="pAssetTitled" select="$vCompanyName"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

    <!-- CREATION DES CONTRATS DERIVES -->
    <!-- Contract Description -->
    <xsl:variable name="vDerivativeContractDescription">
      <xsl:call-template name="SetDCDescription_Occ">
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pCompanyName" select="$vCompanyName"/>
        <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
        <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
        <xsl:with-param name="pIsMini" select="$gFalse"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <xsl:variable name="vExtSQLFilterNames"  select="concat ('ISO10383_OCC',',','ASSETSYMBOL_OCC')"/>
    <xsl:variable name="vExtSQLFilterValues" select="concat ($vISO10383,',',$vAssetSymbol)"/>
    <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
      <xsl:with-param name="pISO10383" select="vISO10383"/>
      <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
      <xsl:with-param name="pMaturityRuleIdentifier" select="$vMaturityRuleIdentifier"/>
      <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
      <xsl:with-param name="pDerivativeContractIdentifier" select="$gAutomaticCompute"/>
      <xsl:with-param name="pContractDisplayName" select="$gAutomaticCompute"/>
      <xsl:with-param name="pDescription" select="$vDerivativeContractDescription"/>
      <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
      <xsl:with-param name="pCurrency" select="$vCurrency"/>
      <xsl:with-param name="pCategory" select="$vCategory"/>
      <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
      <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
      <xsl:with-param name="pPhysettltamount" select="$vPhysettltamount"/>
      <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
      <xsl:with-param name="pContractFactor" select="'100'"/>
      <xsl:with-param name="pContractMultiplier" select="'100'"/>
      <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
      <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$gTrue"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <!-- FI 20140327 [19796] FINALSETTLTPRICE toujours égale sur cette chambre à 'LastTradingDay' -->
      <!-- Fl 20140327 [25539] FINALSETTLTPRICE toujours égale sur cette chambre à 'ExpiryDate' -->
      <xsl:with-param name="pFinalSettltPrice" select="'ExpiryDate'"/>
      <!-- FL 20140514 [19933] UNDERLYINGGROUP toujours égale sur cette chambre à F -> (Financial)  -->
      <xsl:with-param name="pUnderlyingGroup" select="'F'"/>
      <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
           
    </xsl:call-template>
    <!-- CBOE MINIS OPTIONS (www.cboe.com/micro/mini) -->
    <xsl:if test="($vContractSymbol = 'AMZN') or
                  ($vContractSymbol = 'AAPL') or
                  ($vContractSymbol = 'GOOG') or
                  ($vContractSymbol = 'GLD')  or
                  ($vContractSymbol = 'SPY')">
      <xsl:variable name="vMinisDerivativeContractDescription">
        <xsl:call-template name="SetDCDescription_Occ">
          <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
          <xsl:with-param name="pCategory" select="$vCategory"/>
          <xsl:with-param name="pCompanyName" select="$vCompanyName"/>
          <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
          <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
          <xsl:with-param name="pIsMini" select="$gTrue"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
        <xsl:with-param name="pISO10383" select="vISO10383"/>
        <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
        <xsl:with-param name="pMaturityRuleIdentifier" select="$vMaturityRuleIdentifier"/>
        <xsl:with-param name="pContractSymbol" select="concat($vContractSymbol,'7')"/>
        <xsl:with-param name="pDerivativeContractIdentifier" select="$gAutomaticCompute"/>
        <xsl:with-param name="pContractDisplayName" select="$gAutomaticCompute"/>
        <xsl:with-param name="pDescription" select="$vMinisDerivativeContractDescription"/>
        <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
        <xsl:with-param name="pCurrency" select="$vCurrency"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
        <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
        <xsl:with-param name="pPhysettltamount" select="$vPhysettltamount"/>
        <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
        <xsl:with-param name="pContractFactor" select="'10'"/>
        <xsl:with-param name="pContractMultiplier" select="'10'"/>
        <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
        <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$gTrue"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
        <!-- FI 20140327 [19796] FINALSETTLTPRICE toujours égale sur cette chambre à 'LastTradingDay' -->
        <!-- Fl 20140327 [25539] FINALSETTLTPRICE toujours égale sur cette chambre à 'ExpiryDate' -->
        <xsl:with-param name="pFinalSettltPrice" select="'ExpiryDate'"/>
        <!-- FL 20140514 [19933] UNDERLYINGGROUP toujours égale sur cette chambre à F -> (Financial) -->
        <xsl:with-param name="pUnderlyingGroup" select="'F'"/>
        <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
        
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <!-- ============================================== -->
  <!--        GetMaturityRuleIdentifier_Occ           -->
  <!-- ============================================== -->
  <xsl:template name="GetMaturityRuleIdentifier_Occ">
    <xsl:param name="pSymbol"/>
    <xsl:param name="pAssetCategory"/>
    <xsl:param name="pProductType1"/>
    <xsl:param name="pProductType2"/>
    <xsl:param name="pProductType3"/>
    <xsl:param name="pProductType4"/>

    <!-- Valeurs possibles des Product Types : 'B'=Binaries; 'E'=ETFs ou HOLDRs; 'L'=LEAPS; 'N'=ETNs; 'Q'=Quarterlys; 'W'=Weeklys; ''=null;
    
        AssetCategory   Symbol   ProductType1   ProductType2   ProductType3   ProductType4   MaturityRyleIdentifier
        ===============================================================================================================================
        'EquityAsset'     Condition : Si aucun des paramètres ProductType1, ProductType2,    XUSS Equity Options Monthly
                            ProductType3, ProductType4 contient 'W' et 'Q')  
       
        'EquityAsset'     Condition : Si un des paramètres ProductType1, ProductType2,       XUSS Equity Options Monthly & Weekly
                            ProductType3, ProductType4 contient 'W')  
       
        'EquityAsset'     Condition : Si un des paramètres ProductType1, ProductType2,       XUSS Equity Options Monthly, Quaterly & Weekly
                            ProductType3, ProductType4 contient 'W' et 'Q' )  
                           
        'Index'           Condition : Si Symbol not in ('DJX','NDX','RUT','OEX',             XUSS Index Options Monthly
                            'XSP','XEO','SPX','VIX','GVZ','OVX','VXEEM','VXEWZ')                 
                            
        'Index'           Condition : Si Symbol in ('VIX','GVZ','OVX','VXEEM','VXEWZ')       XUSS Index Options Monthly Volatility Index 
                           
        'Index'           Condition : Si Symbol in ('DJX','NDX','RUT','OEX')                 XUSS Index Options Monthly & Weekly
      
        'Index'           Condition : Si Symbol in ('XSP','XEO','SPX')                       XUSS Index Options Monthly, Quaterly & Weekly
                      
        VALEUR PAR DEFAUT : Default Rule                                                                           -->

    <xsl:choose>

      <!--AssetCategory = 'EquityAsset'-->
      <xsl:when test="$pAssetCategory='EquityAsset'">

        <xsl:choose>
          <!-- MaturityRyleIdentifier - XUSS Equity Options Monthly, Quaterly & Weekly -->
          <xsl:when test=" (     ($pProductType1  = 'W' or  $pProductType2  = 'W' or  $pProductType3  = 'W' or  $pProductType4 = 'W') 
                             and ($pProductType1  = 'Q' or  $pProductType2  = 'Q' or  $pProductType3  = 'Q' or  $pProductType4 = 'Q') )">XUSS Equity Options Monthly, Quaterly &amp; Weekly</xsl:when>

          <!-- MaturityRyleIdentifier - XUSS Equity Options Monthly & Weekly -->
          <xsl:when test=" (     ($pProductType1  = 'W' or  $pProductType2  = 'W' or  $pProductType3  = 'W' or  $pProductType4 = 'W') )">XUSS Equity Options Monthly &amp; Weekly</xsl:when>

          <!-- MaturityRyleIdentifier - XUSS Equity Options Monthly -->
          <xsl:otherwise>XUSS Equity Options Monthly</xsl:otherwise>
        </xsl:choose>

      </xsl:when>

      <!--AssetCategory = 'Index'-->
      <xsl:when test="$pAssetCategory='Index'">

        <xsl:choose>
          <!-- MaturityRyleIdentifier - XUSS Index Options Monthly & Weekly -->
          <xsl:when test="$pSymbol = 'DJX' or $pSymbol = 'NDX' or $pSymbol = 'RUT' or $pSymbol = 'OEX'">XUSS Index Options Monthly &amp; Weekly</xsl:when>

          <!-- MaturityRyleIdentifier - XUSS Index Options Monthly, Quaterly & Weekly -->
          <xsl:when test="$pSymbol = 'XSP' or $pSymbol = 'XEO' or $pSymbol = 'SPX'">XUSS Index Options Monthly, Quaterly &amp; Weekly</xsl:when>

          <!-- MaturityRyleIdentifier - XUSS Index Options Monthly Volatility Index -->
          <xsl:when test="$pSymbol = 'VIX' or $pSymbol = 'GVZ' or $pSymbol = 'OVX' or $pSymbol = 'VXEEM' or $pSymbol = 'VXEWZ'">XUSS Index Options Monthly Volatility Index</xsl:when>

          <!-- MaturityRyleIdentifier - XUSS Index Options Monthly -->
          <xsl:otherwise>XUSS Index Options Monthly</xsl:otherwise>
        </xsl:choose>

      </xsl:when>

      <xsl:otherwise>Default Rule</xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <!-- ============================================== -->
  <!--          GetAssetCategory_Occ                  -->
  <!-- ============================================== -->
  <xsl:template name="GetAssetCategory_Occ">
    <xsl:param name="pSymbol"/>
    <!--  Pour déterminer la catégorie du DC, je ne peux le faire que par le symbol du DC en identifiant ceux qui sont de catégorie ‘Index’ 
           les autres sont de catégorie ‘EquityAsset’.
          Pour avoir la liste des symboles des DC de catégorie ‘Index’, je vais sur le site suivants http://www.theocc.com/webapps/delo-search
          en faisant une restriction sur Product Type = IU(Index Underlying) et Exchange = CBOE 
          (FL 20140814 [19933] mise à jours de la liste des DC de catégorie ‘Index’) -->

    <xsl:choose>
      <!-- Quand le DC à un Symbol faisant partie de la liste des DC de catégorie ‘Index’ -->
      <xsl:when test="$pSymbol = 'BACD'  or $pSymbol = 'BSZ'   or $pSymbol = 'BVZ'  or   
                      $pSymbol = 'CITD'  or $pSymbol = 'DIVD'  or $pSymbol = 'DJX'  or   
                      $pSymbol = 'GSSD'  or $pSymbol = 'GVZ'   or $pSymbol = 'JPMD' or  
                      $pSymbol = 'MNX'   or $pSymbol = 'MSTD'  or $pSymbol = 'NDX'  or   
                      $pSymbol = 'OEX'   or $pSymbol = 'OVX'   or $pSymbol = 'RMN'  or   
                      $pSymbol = 'RUT'   or $pSymbol = 'RUTQ'  or $pSymbol = 'RVX'  or 
                      $pSymbol = 'SPX'   or $pSymbol = 'SPXPM' or $pSymbol = 'SPXQ' or 
                      $pSymbol = 'SPXW'  or $pSymbol = 'SRO'   or $pSymbol = 'VIX'  or 
                      $pSymbol = 'VXEEM' or $pSymbol = 'VXEWZ' or $pSymbol = 'VXST' or
                      $pSymbol = 'XEO'   or $pSymbol = 'XSP'   or $pSymbol = 'XSPAM'">Index</xsl:when>

      <!-- Sinon -> AssetCategory = 'EquityAsset' -->
      <xsl:otherwise>EquityAsset</xsl:otherwise>
    </xsl:choose>

  </xsl:template>
 
  <!-- ============================================== -->
  <!--          GetExerciseStyle_Occ                  -->
  <!-- ============================================== -->
  <xsl:template name="GetExerciseStyle_Occ">
    <xsl:param name="pAssetCategory"/>
    <xsl:param name="pSymbol"/>

    <!-- Affectation de ExerciseStyle en fonction du Symbol du DC :
          (uniquement pour les DC dont l'AssetCategory est Index)
             Symbol               ExerciseStyle
             =================================
             OEX                  1
             BSZ, BVZ, DJX        0
             GVZ, MNX, NDX        0
             RUT, SPX, SPXPM      0
             VIX, VXEEM, VXEWZ    0
             XEO, XSP, DIVD       0
             OVX, SRO, SPXQ       0
             SPXW                 0             
              Valeur par défaut : 0
             
         1 : American
         0 : European -->

    <xsl:choose>
      <!-- AssetCategory = EquityAsset -> 1 (American) -->
      <xsl:when test="$pAssetCategory='EquityAsset'">1</xsl:when>
      <!-- AssetCategory = Index -->
      <xsl:when test="$pAssetCategory='Index'">
        <!-- En fonction du Symbol du DC -->
        <xsl:choose>
          <xsl:when test="$pSymbol = 'OEX'">1</xsl:when>
          <xsl:when test="$pSymbol = 'BSZ' or $pSymbol = 'BVZ'    or $pSymbol = 'DJX'     or
                          $pSymbol = 'GVZ' or $pSymbol = 'MNX'    or $pSymbol = 'NDX'     or
                          $pSymbol = 'RUT' or $pSymbol = 'SPX'    or $pSymbol = 'SPXPM'   or
                          $pSymbol = 'VIX' or $pSymbol = 'VXEEM'  or $pSymbol = 'VXEWZ'   or
                          $pSymbol = 'XEO' or $pSymbol = 'XSP'    or $pSymbol = 'DIVD'    or
                          $pSymbol = 'OVX' or $pSymbol = 'SRO'    or $pSymbol = 'SPXQ'    or
                          $pSymbol = 'SPXW'">0</xsl:when>
          <!-- Valeur par défaut des Index : 0 (European) -->
          <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ============================================== -->
  <!--           SetDCDescription_Occ                 -->
  <!-- ============================================== -->
  <xsl:template name="SetDCDescription_Occ">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pCompanyName"/>
    <xsl:param name="pSettlMethod"/>
    <xsl:param name="pExerciseStyle"/>
    <xsl:param name="pIsMini"/>

    <xsl:variable name="FutOrOpt">
      <xsl:choose>
        <xsl:when test="$pCategory='F'">Fut</xsl:when>
        <xsl:when test="$pCategory='O'">Opt</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="CashOrPhys">
      <xsl:choose>
        <xsl:when test="$pSettlMethod='C'">Cash</xsl:when>
        <xsl:when test="$pSettlMethod='P'">Phys</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="AmOrEu">
      <xsl:choose>
        <xsl:when test="$pExerciseStyle='1'">Am</xsl:when>
        <xsl:when test="$pExerciseStyle='0'">Eu</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- BD & FL 20130531 : S'il s'agit d'un DC Mini, on rajoute "Mini" à sa DESCRIPTION -->
    <xsl:choose>
      <xsl:when test="$pIsMini = $gTrue">
        <xsl:value-of select="concat(
                            $pContractSymbol,' ',
                            $FutOrOpt,' ',
                            '(',concat($pCompanyName,' Mini'),') ',
                            $CashOrPhys,' ',
                            $AmOrEu
                          )"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat(
                            $pContractSymbol,' ',
                            $FutOrOpt,' ',
                            '(',$pCompanyName,') ',
                            $CashOrPhys,' ',
                            $AmOrEu
                          )"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- ================================================== -->
  <!--        ovrSQLGetDerivativeContractDefaultValue     -->
  <!-- ================================================== -->
  <!-- FI 20131121 [19216] add CONTRACTTYPE -->
  <!-- FI 20131129 [19284] add FINALSETTLTSIDE -->
  <xsl:template name="ovrSQLGetDerivativeContractDefaultValue">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:choose>

      <xsl:when test="$pResult='IDASSET_UNL'">
        <SQL command="select" result="IDASSET" cache="true">
          <![CDATA[
          select ASSET.IDASSET
          from dbo.ASSET_EQUITY asset
          inner join dbo.ASSET_EQUITY_RDCMK ardcm on (ardcm.IDASSET=asset.IDASSET)
          inner join dbo.MARKET m on (m.IDM=ardcm.IDM_RELATED) and (m.ISO10383_ALPHA4='XUSS')
          where asset.SYMBOL=@ASSETSYMBOL_OCC
          union all
          select ASSET.IDASSET
          from dbo.ASSET_INDEX asset
          inner join dbo.MARKET m on (m.IDM=asset.IDM) and (m.ISO10383_ALPHA4='XUSS')
          where asset.SYMBOL=@ASSETSYMBOL_OCC
          ]]>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
        </SQL>
      </xsl:when>

      <xsl:when test="$pResult='CONTRACTTYPE'">
        <xsl:value-of select ="'STD'"/>
      </xsl:when>

      <xsl:when test="$pResult='FINALSETTLTSIDE'">
        <SQL command="select" result="FINALSETTLTSIDE" cache="true">
          <![CDATA[
          select case when @CATEGORYEXL='O' then 'OfficialClose' else null end as FINALSETTLTSIDE
          from dual
          ]]>
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