<?xml version="1.0" encoding="utf-8"?>
<!-- 
===============================================================================================
 Summary  : XSL commun à l'importation du fichier Excel bclear         
 File     : MarketData_ICE-EUCommon_ICE-EUProduct_Import_Map.xsl
===============================================================================================
  Version : v4.2.5388
  Date    : 20141002
  Author  : PL
  Comment : [19669] New MIC 
    Before
        XLIF - EURONEXT.LIFFE                   (ISO: XLIF, Symbol du marché: L) for INDEX 
        EQY  - EURONEXT.LIFFE EQUITY            (ISO: XLIF, Symbol du marché: O) for EQUITY 
        LCP  - EURONEXT.LIFFE COMMODITY PRODUCT (ISO: XLIF, Symbol du marché: X) for COMMODITY
    Now
        IFLL - ICE FUTURES EUROPE - FINANCIAL PRODUCTS DIVISION    (ISO: IFLL, Symbol du marché: L) for FINANCIAL PRODUCTS 
        IFLO - ICE FUTURES EUROPE - EQUITY PRODUCTS DIVISION       (ISO: IFLO, Symbol du marché: O) for EQUITY PRODUCTS 
        IFLX - ICE FUTURES EUROPE - AGRICULTURAL PRODUCTS DIVISION (ISO: IFLX, Symbol du marché: X) for AGRICULTURAL PRODUCTS 
  =============================================================================================
  Version : v4.1.0.0
  Date    : 20140512
  Author  : PM
  Comment : [19970][19259] - Manage data Currency in table ICEEUPRODUCT in template
            "rowStreamCommon_ICE-EU"
  =============================================================================================
  Version : v3.8.0.0
  Date    : 20140305
  Author  : FL
  Comment : [19648] - Manage data UnderlyingGroup & UnderlyingAsset in table ICEEUPRODUCT
            in template (rowStreamCommon_ICE-EU)
  =============================================================================================
  Version : v3.7.5108
  Date    : 20131226
  Author  : FL
  Comment : [19404]
  - Manage market (Wrong Market for EQUITIES)
    Before
        XLIF - EURONEXT.LIFFE (ISO : XLIF, Symbol du marché: L) for INDEX and EQUITY 
    Now
        XLIF - EURONEXT.LIFFE (ISO : XLIF, Symbol du marché: L) for INDEX 
        EQY - EURONEXT.LIFFE EQUITY (ISO : XLIF, Symbol du marché: O) for EQUITY 
  =============================================================================================
  Version : v3.7.5072
  Date    : 20131120                                          
  Author  : FI                                                 
  Comment : [19216] - Manage Flex Contract
  =============================================================================================
  Version : v3.7.0.0
  Date    : 20131022
  Author  : FL
  Comment : [18812] - Manage Last Maturity Rule Added in template(GetMaturityRule_BCLEAR)
  - XLIF Equity Options Standard
  =============================================================================================
  Version : v3.7.0.0
  Date    : 20130923
  Author  : FL
  Comment : [18812] - Manage Last Maturity Rule Added in template(GetMaturityRule_BCLEAR)
  - XLIF I F
  =============================================================================================
  Version : v3.7.0.0
  Date    : 20130904
  Author  : FL
  Comment : [18812] - Manage Last Maturity Rule Added in template(GetMaturityRule_BCLEAR)
  - XLIF Bond Gilt Futures
  =============================================================================================
  Version : v3.5.0.0
  Date    : 20132806
  Author  : BD & FL                                               
  Comment : [18785] Mise à jour du template "GetMaturityRule_BCLEAR" 
                    pour gérer la MR du DC "Z F"
  =============================================================================================
  Version  : v3.2.0.0                                              
  Date     : 20130314                                          
  File     : BD & FL                                                 
  Comment  : Création
===============================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <!-- ================================================== -->
  <!--        import(s)                                   -->
  <!-- ================================================== -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ============================================== -->
  <!--              GetISO10383_Liffe                 -->
  <!-- ============================================== -->
  <xsl:template name="GetISO10383_Liffe">
    <xsl:param name="pRelevantStockExchange"/>

    <!-- Affectation du code ISO10383 du marché en fonction de Relevant Stock Exchange :
              RelevantStockExchange           ISO10383
              ========================================
              Borsa Italiana	                XMIL
              Budapest Stock Exchange	        XBUD
              Copenhagen Stock Exchange	      XCSE
              Deutsche Boerse	                XFRA
              Deutsche Börse (Xetra)	        XETR
              Euronext Amsterdam	            XAMS
              Euronext Brussels	              XBRU
              Euronext Lisbon	                XLIS
              Euronext Paris	                XPAR
              Helsinki Stock Exchange	        XHEX
              Irish Stock Exchange	          XDUB
              London Stock Exchange	          XLON
              LSE (International Order Book)	XLON 
              Madrid Stock Exchange	          XMDS
              NASDAQ	                        XNAS
              NYSE	                          XNLI
              Oslo Stock Exchange	            XOSL
              Prague Stock Exchange	          XPRA
              SIX Swiss Exchange	            XSWX
              Stockholm Stock Exchange	      XSTO
              Vienna Stock Exchange	          XVIE
              Warsaw Stock Exchange	          XWAR
                Par défaut                    XLON -->

    <xsl:choose>
      <xsl:when test="$pRelevantStockExchange = 'Borsa Italiana'">XMIL</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Budapest Stock Exchange'">XBUD</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Copenhagen Stock Exchange'">XCSE</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Deutsche Boerse'">XETR</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Deutsche Börse (Xetra)'">XETR</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Euronext Amsterdam'">XAMS</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Euronext Brussels'">XBRU</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Euronext Lisbon'">XLIS</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Euronext Paris'">XPAR</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Helsinki Stock Exchange'">XHEL</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Irish Stock Exchange'">XDUB</xsl:when>
      <!--<xsl:when test="$pRelevantStockExchange = 'London Stock Exchange'">XLON</xsl:when>-->
      <!--<xsl:when test="$pRelevantStockExchange = 'LSE (International Order Book)'">XLON</xsl:when>-->
      <xsl:when test="$pRelevantStockExchange = 'Madrid Stock Exchange'">XMAD</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'NASDAQ'">XNAS</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'NYSE'">XNYS</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Oslo Stock Exchange'">XOSL</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Prague Stock Exchange'">XPRA</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'SIX Swiss Exchange'">XSWX</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Stockholm Stock Exchange'">XSTO</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Vienna Stock Exchange'">XVIE</xsl:when>
      <xsl:when test="$pRelevantStockExchange = 'Warsaw Stock Exchange'">XWAR</xsl:when>
      <!-- PL 20141002 [19669] New default value: XLON -->
      <!-- <xsl:otherwise>XLIF</xsl:otherwise> -->
      <xsl:otherwise>XLON</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ============================================== -->
  <!--              rowStreamCommon_ICE-EU            -->
  <!-- ============================================== -->
  <xsl:template name="rowStreamCommon_ICE-EU">
    <xsl:param name="pSheetName"/>

    <!-- ========================= -->
    <!-- CREATION DES SOUS JACENTS -->
    <!-- ========================= -->
    <!-- Si l'ISIN n'est pas disponible (='n/a'), on utilise le Symbol 
              pour créer l'IDENTIFIER, le DISPLAYNAME et la DESCRIPTION -->

    <!-- isISINExist -->
    <xsl:variable name="isISINExist">
      <xsl:choose>
        <!-- ISIN != 'n/a' -> true -->
        <xsl:when test="data[@name='ISIN'] != 'n/a'">true</xsl:when>
        <!-- ISIN = 'n/a' -> false -->
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Nom du sous-jacent (Company Name) -->
    <xsl:variable name="vUnderlyingName">
      <xsl:choose>
        <xsl:when test="contains(data[@name='AssetName'],'*')">
          <xsl:value-of select="normalize-space(substring-before(data[@name='AssetName'],'*'))"/>
        </xsl:when>
        <xsl:when test="contains(data[@name='AssetName'],'(')">
          <xsl:value-of select="normalize-space(substring-before(data[@name='AssetName'],'('))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space(data[@name='AssetName'])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Symbol du sous-jacent -->
    <xsl:variable name="vSymbol">
      <!-- BD 20130705 Ne pas valoriser vSymbol lors que LogicalCode contient '*' -->
      <xsl:if test="not(contains(data[@name='LogicalCode'],'*'))">
        <xsl:value-of select="data[@name='LogicalCode']"/>
      </xsl:if>
      <!-- FL 20140327 valoriser vSymbol avec UKX lorqu'il contient 'UKX/Z*'" -->
      <xsl:if test="contains(data[@name='LogicalCode'],'UKX/Z*')">UKX</xsl:if>
    </xsl:variable>

    <!-- ISIN -->
    <xsl:variable name="vISINCode">
      <xsl:choose>
        <xsl:when test="$isISINExist = 'true'">
          <xsl:value-of select="data[@name='ISIN']"/>
        </xsl:when>
        <xsl:otherwise>null</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Details du sous-jacent (utilisé pour l'IDENTIFIER, DESCRIPTION et DISPLAYNAME) -->
    <xsl:variable name="vAssetDetails">
      <xsl:choose>
        <!-- Si l'ISIN est dispo ... -->
        <xsl:when test="$isISINExist = 'true'">
          <!-- ... on l'utilise -->
          <xsl:value-of select="data[@name='ISIN']"/>
        </xsl:when>
        <!-- Sinon ... -->
        <xsl:otherwise>
          <!-- ... on prend le Symbol (Logical Code) -->
          <xsl:value-of select="$vSymbol"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Currency du sous-jacent -->
    <xsl:variable name="vCurrency">
      <!-- BD 20130524 [Ticket TRIM 18677] : On utilise directement data[@name='Currency'], sans convertir GBX (Bristish Pence) en GBP ! -->
      <xsl:value-of select="data[@name='Currency']"/>
    </xsl:variable>

    <!-- Code Bloomberg -->
    <xsl:variable name="vBBGCode">
      <xsl:value-of select="data[@name='BBGCode']"/>
    </xsl:variable>

    <!-- Code Reuters Instrument -->
    <xsl:variable name="vRICCode">
      <xsl:value-of select="data[@name='RICCode']"/>
    </xsl:variable>

    <!-- ISO10383 -->
    <xsl:variable name="vISO10383">
      <xsl:call-template name="GetISO10383_Liffe">
        <xsl:with-param name="pRelevantStockExchange" select="normalize-space(data[@name='StockExchange'])"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <!-- Feuilles "Individual Equity Futures" et "Individual Equity Options" -->
      <xsl:when test="contains($pSheetName,'Equity')">
        <!-- PL 20141002 [19669] -->
        <!-- Tous les DC de catégorie "Equity" de ICE-EU sont rattachés au marché IFLO (ISO: "IFLO", ExchangeSymbol: "O") -->
        <xsl:variable name="vExchangeSymbolRelated" select="'O'"/>

        <!-- Création des EQUITIES -->
        <xsl:call-template name="SQLTableASSET_EQUITY">
          <xsl:with-param name="pIsincode" select="$vISINCode"/>
          <xsl:with-param name="pIdc" select="$vCurrency"/>
          <xsl:with-param name="pSymbol" select="$vSymbol"/>
          <xsl:with-param name="pBBGCode" select="$vBBGCode"/>
          <xsl:with-param name="pRICCode" select="$vRICCode"/>
          <xsl:with-param name="pIso10383" select="$vISO10383"/>
          <xsl:with-param name="pExchangeSymbolRelated" select="$vExchangeSymbolRelated"/>
          <xsl:with-param name="pAssetTitled" select="$vUnderlyingName"/>
        </xsl:call-template>
      </xsl:when>

      <!-- Feuille "Index Contracts" -->
      <xsl:when test="contains($pSheetName,'Index')">
        <!-- PL 20141002 [19669] -->
        <!-- Tous les DC de catégorie "Index" de ICE-EU sont rattachés au marché IFLL (ISO: "IFLL", ExchangeSymbol: "L") -->
        <xsl:variable name="vExchangeSymbolRelated" select="'L'"/>
        
        <!-- Création des INDICES -->
        <xsl:call-template name="SQLTableASSET_INDEX">
          <xsl:with-param name="pIsincode" select="$vISINCode"/>
          <xsl:with-param name="pIdc" select="$vCurrency"/>
          <xsl:with-param name="pSymbol" select="$vSymbol"/>
          <xsl:with-param name="pBBGCode" select="$vBBGCode"/>
          <xsl:with-param name="pRICCode" select="$vRICCode"/>
          <xsl:with-param name="pIso10383" select="$vISO10383"/>
          <xsl:with-param name="pExchangeSymbolRelated" select="$vExchangeSymbolRelated"/>
          <xsl:with-param name="pAssetTitled" select="$vUnderlyingName"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

    <!-- ================== -->
    <!--    ICEEUPRODUCT    -->
    <!-- ================== -->

    <!-- - Affectation de Category, de ExerciseStyle, de SettltMethod et de AssetCategory
                en fonction du nom de la colonne du fichier Excel BCLEAR où le Contract Code est disponible (!='n/a').
             - Pour chaque Contract Code de renseigné (!='n/a'), on génère un insert dans la table ICEEUPRODUCT
                en appelant le GenerateInsertICEEUPRODUCT avec le paramètre pOrigineData qui vaut 'BCLEAR' -->

    <xsl:choose>

      <!-- Feuille "Individual Equity Futures" -->
      <!--  Nom colonne           Category        ExerciseStyle       SettltMethod        AssetCategory    UnderlyingGroup  UnderlyingAsset
            ===============================================================================================================================
            CashCode              F               1 (American)        C (Cash)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            PhysCode              F               1 (American)        P (Phys)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            LiffeCode1            F               1 (American)        C (Cash)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            LiffeCode2            F               1 (American)        C (Cash)            EquityAsset      F (Financial)    FS (Stock-Equiries) -->
      <!-- FI 20131120 [19216] param pContractType -->
      <!-- FI 20131120 [19216] add test string-length()>0 -->
      <!-- FL 20140304 [19648] add PUnderlyingGroup & PUnderlyingAsset -->
      <!-- PM 20140512 [19970][19259] add pCurrency -->
      <xsl:when test="contains($pSheetName,'EquityFutures')">
        <xsl:if test="(data[@name='CashCode'] != 'n/a') and (string-length(data[@name='CashCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='CashCode']"/>
            <xsl:with-param name="pCategory" select="'F'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='PhysCode'] != 'n/a') and (string-length(data[@name='PhysCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='PhysCode']"/>
            <xsl:with-param name="pCategory" select="'F'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'P'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='LiffeCode1'] != 'n/a') and (string-length(data[@name='LiffeCode1'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='LiffeCode1']"/>
            <xsl:with-param name="pCategory" select="'F'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'STD'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='LiffeCode2'] != 'n/a') and (string-length(data[@name='LiffeCode2'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='LiffeCode2']"/>
            <xsl:with-param name="pCategory" select="'F'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'STD'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>

      <!-- Feuille "Individual Equity Options" -->
      <!--  Nom colonne           Category        ExerciseStyle       SettltMethod        AssetCategory    UnderlyingGroup  UnderlyingAsset
            ===============================================================================================================================
            AmCashCode            O               1 (American)        C (Cash)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            AmPhysCode            O               1 (American)        P (Phys)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            EuCashCode            O               0 (European)        C (Cash)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            EuPhysCode            O               0 (European)        P (Phys)            EquityAsset      F (Financial)    FS (Stock-Equiries)
            LiffeCode             O               1 (American)        P (Phys)            EquityAsset      F (Financial)    FS (Stock-Equiries) -->
      <!-- FI 20131120 [19216]  param pContractType -->
      <!-- FI 20131120 [19216]  add test string-length()>0 -->
      <!-- FL 20140304 [19648] add PUnderlyingGroup & PUnderlyingAsset -->
      <!-- PM 20140512 [19970][19259] add pCurrency -->
      <xsl:when test="contains($pSheetName,'EquityOptions')">
        <xsl:if test="(data[@name='AmCashCode'] != 'n/a') and (string-length(data[@name='AmCashCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='AmCashCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='AmPhysCode'] != 'n/a') and (string-length(data[@name='AmPhysCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='AmPhysCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'P'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='EuCashCode'] != 'n/a') and (string-length(data[@name='EuCashCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='EuCashCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'0'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='EuPhysCode'] != 'n/a') and (string-length(data[@name='EuPhysCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='EuPhysCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'0'"/>
            <xsl:with-param name="pSettltMethod" select="'P'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='LiffeCode'] != 'n/a') and (string-length(data[@name='LiffeCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='LiffeCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'P'"/>
            <xsl:with-param name="pAssetCategory" select="'EquityAsset'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'STD'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FS'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>

      <!-- Feuille "Index Contracts" -->
      <!--  Nom colonne           Category        ExerciseStyle       SettltMethod        AssetCategory    UnderlyingGroup  UnderlyingAsset
            ================================================================================================================================
            AmCode                O               1 (American)        C (Cash)            Index            F (Financial)    FI (Inidces)
            EuCode                O               0 (European)        C (Cash)            Index            F (Financial)    FI (Inidces)
            FutureCode            F               null                C (Cash)            Index            F (Financial)    FI (Inidces)
            LondonCode            O               1 (American)        C (Cash)            Index            F (Financial)    FI (Inidces) -->
      <!-- FI 20131120 [19216]  param pContractType -->
      <!-- FI 20131120 [19216]  add test string-length()>0-->
      <!-- FL 20140304 [19648] add PUnderlyingGroup & PUnderlyingAsset -->
      <!-- PM 20140512 [19970][19259] add pCurrency -->
      <xsl:when test="contains($pSheetName,'Index')">
        <xsl:if test="(data[@name='AmCode'] != 'n/a') and (string-length(data[@name='AmCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='AmCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'Index'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FI'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='EuCode'] != 'n/a') and (string-length(data[@name='EuCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='EuCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'0'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'Index'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'FLEX'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FI'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='FutureCode'] != 'n/a') and (string-length(data[@name='FutureCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='FutureCode']"/>
            <xsl:with-param name="pCategory" select="'F'"/>
            <xsl:with-param name="pExerciseStyle" select="'null'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'Index'"/>
            <!-- Pour les contrats Futures de la feuille "Index Contracts", 
                      on renseigne en plus le ContractMultiplier, le TickSize et le TickValue spécifiques aux Contrats Futures -->
            <xsl:with-param name="pContractMultiplier" select="data[@name='FutContractSize']"/>
            <xsl:with-param name="pTickSize" select="data[@name='FutTickSize']"/>
            <xsl:with-param name="pTickValue" select="data[@name='FutTickValue']"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'STD'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FI'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="(data[@name='LondonCode'] != 'n/a') and (string-length(data[@name='LondonCode'])>0)">
          <xsl:call-template name="GenerateInsertICEEUPRODUCT">
            <xsl:with-param name="pOrigineData" select="'BCLEAR'"/>
            <xsl:with-param name="pContractSymbol" select="data[@name='LondonCode']"/>
            <xsl:with-param name="pCategory" select="'O'"/>
            <xsl:with-param name="pExerciseStyle" select="'1'"/>
            <xsl:with-param name="pSettltMethod" select="'C'"/>
            <xsl:with-param name="pAssetCategory" select="'Index'"/>
            <xsl:with-param name="pISO10383" select="$vISO10383"/>
            <xsl:with-param name="pContractType" select="'STD'"/>
            <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
            <xsl:with-param name="PUnderlyingAsset" select="'FI'"/>
            <xsl:with-param name="pCurrency" select="$vCurrency"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>

    </xsl:choose>
  </xsl:template>

  <!-- ================================================== -->
  <!--                  GenerateInsertICE                 -->
  <!-- ================================================== -->
  <!-- Template permettant d'insérer des données dans la table ICE-EU -->
  <!-- FI 20131120 [19216] add CONTRACTTYPE column-->
  <!-- FL 20140304 [19648] add PUnderlyingGroup & PUnderlyingAsset column -->
  <!-- PM 20140512 [19970][19259] add parameter pCurrency and column IDC -->
  <xsl:template name="GenerateInsertICEEUPRODUCT">
    <xsl:param name="pOrigineData"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExerciseStyle"/>
    <xsl:param name="pSettltMethod"/>
    <xsl:param name="pAssetCategory"/>
    <xsl:param name="pContractMultiplier" select="'null'"/>
    <xsl:param name="pTickSize" select="'null'"/>
    <xsl:param name="pTickValue" select="'null'"/>
    <xsl:param name="pISO10383"/>
    <xsl:param name="pContractType" select="'null'"/>
    <xsl:param name="PUnderlyingGroup"/>
    <xsl:param name="PUnderlyingAsset"/>
    <xsl:param name="pCurrency" select="'null'"/>

    <!-- Remarques sur pOrigineData : Origine du fichier importé.
          Valeurs possibles :
            - 'BCLEAR' - Dans le cas ou les données provienne du fichier Excel provenant de 'BCLEAR'
            - 'SPAN'   - Dans le cas ou les données provienne du SPAN  -->

    <!-- Remarques sur ContractMultiplier, TickSize et TickValue
            Les données ContractMultiplier, TickSize et TickValue sont récupérées en respectant les conditions suivantes :
            - Le fichier exploité pour cette importation est un fichier BCLEAR (pOrigineData='BCLEAR')
            - Les données contenues dans le fichier BCLEAR sont présentes (!='n/a')
            - Le paramètre correspondant à la donnée (pContractMultiplier, pTickSize et pTickValue) ne sont pas renseignés (=null)
        Lorsque l'une des condition n'est pas respectée, on récupère le contenu du paramètre du template -->
    <xsl:variable name="vContractMultiplier">
      <xsl:choose>
        <xsl:when test="$pContractMultiplier='null' and $pOrigineData='BCLEAR' and data[@name='ContractSize']!='n/a'">
          <xsl:value-of select="data[@name='ContractSize']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pContractMultiplier"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vTickSize">
      <xsl:choose>
        <xsl:when test="$pTickSize='null' and $pOrigineData='BCLEAR' and data[@name='TickSize']!='n/a'">
          <xsl:value-of select="data[@name='TickSize']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pTickSize"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vTickValue">
      <xsl:choose>
        <xsl:when test="$pTickValue='null' and $pOrigineData='BCLEAR' and data[@name='TickValue']!='n/a'">
          <xsl:value-of select="data[@name='TickValue']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pTickValue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <table name="ICEEUPRODUCT" action="IU">

      <!-- ORIGINEDATA -->
      <column name="ORIGINEDATA" datakey="true">
        <xsl:value-of select="$pOrigineData"/>
      </column>

      <!-- CONTRACTSYMBOL -->
      <column name="CONTRACTSYMBOL" datakey="true">
        <xsl:value-of select="$pContractSymbol"/>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="IsNull()" />
            <logInfo status="INFO" isexception="false">
              <message>Contract Symbol (Product ID) not found.</message>
            </logInfo>
          </control>
        </controls>
      </column>

      <!-- CATEGORY -->
      <column name="CATEGORY" datakey="true">
        <xsl:value-of select="$pCategory"/>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="IsNull()" />
            <logInfo status="INFO" isexception="false">
              <message>Category is null.</message>
            </logInfo>
          </control>
        </controls>
      </column>

      <!-- SETTLTMETHOD -->
      <column name="SETTLTMETHOD" datakeyupd="true">
        <xsl:value-of select="$pSettltMethod"/>
        <controls>
          <!-- Si SETTLTMETHOD n'est pas renseigné, on lui applique la valeur par défaut : 'C' -->
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()" />
          </control>
        </controls>
        <default>C</default>
      </column>

      <!-- EXERCISESTYLE (Uniquement quand pCategory = 'O') -->
      <xsl:if test="$pCategory = 'O'">
        <!-- EXERCISESTYLE -->
        <column name="EXERCISESTYLE" datakeyupd="true">
          <xsl:value-of select="$pExerciseStyle"/>
          <controls>
            <!-- Si EXERCISESTYLE n'est pas renseigné, on lui applique la valeur par défaut : '1' -->
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()" />
            </control>
          </controls>
          <default>1</default>
        </column>
      </xsl:if>

      <!-- CONTRACTMULTIPLIER -->
      <column name="CONTRACTMULTIPLIER" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$vContractMultiplier"/>
        <controls>
          <!-- Si CONTRACTMULTIPLIER n'est pas renseigné, on rejète la ligne -->
          <control action="RejectRow" return="true" >
            <SpheresLib function="IsNull()" />
            <logInfo status="INFO" isexception="false">
              <message>Contract Multiplier is null.</message>
            </logInfo>
          </control>
        </controls>
      </column>

      <!-- FACTOR -->
      <column name="FACTOR" datakeyupd="true" datatype="decimal">
        <xsl:choose>
          <!-- Quand ContractCategory Option et AssetCategory Future : FACTOR = 1 -->
          <xsl:when test="$pCategory='O' and $pAssetCategory='Future'">1</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$vContractMultiplier"/>
          </xsl:otherwise>
        </xsl:choose>
      </column>

      <!-- MINPRICEINCR -->
      <column name="MINPRICEINCR" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$vTickSize"/>
      </column>

      <!-- MINPRICEINCRAMOUNT -->
      <column name="MINPRICEINCRAMOUNT" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$vTickValue"/>
      </column>

      <!-- ASSETCATEGORY -->
      <column name="ASSETCATEGORY" datakeyupd="true">
        <xsl:value-of select="$pAssetCategory"/>
      </column>

      <xsl:if test="$pOrigineData='BCLEAR'">
        <!-- IDASSET_UNL -->
        <column name="IDASSET_UNL" datakeyupd="true" datatype="integer">
          <SQL command="select" result="IDASSET" cache="true">
            <![CDATA[
            select ASSET.IDASSET
            from dbo.ASSET_EQUITY asset
            inner join dbo.MARKET m on (m.IDM=asset.IDM) and (m.ISO10383_ALPHA4=@UNDERLYINGMARKET)
            where asset.ISINCODE=@ISINCODE
            union all
            select asset.IDASSET
            from dbo.ASSET_INDEX asset
            inner join dbo.MARKET m on (m.IDM=asset.IDM) and (m.ISO10383_ALPHA4=@UNDERLYINGMARKET)
            where asset.SYMBOL=@LOGICALCODE
            ]]>
            <Param name="UNDERLYINGMARKET" datatype="string">
              <xsl:value-of select="$pISO10383"/>
            </Param>
            <Param name="ISINCODE" datatype="string">
              <xsl:value-of select="data[@name='ISIN']"/>
            </Param>
            <Param name="LOGICALCODE" datatype="string">
              <xsl:value-of select="data[@name='LogicalCode']"/>
            </Param>
          </SQL>
        </column>
      </xsl:if>

      <!-- IDMATURITYRULE -->
      <column name="IDMATURITYRULE" datakeyupd="true" datatype="integer">
        <xsl:call-template name="GetMaturityRule_BCLEAR">
          <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
          <xsl:with-param name="pCategory" select="$pCategory"/>
          <xsl:with-param name="pContractType" select="$pContractType"/>
        </xsl:call-template>
      </column>

      <!-- CONTRACTTYPE -->
      <column name="CONTRACTTYPE" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pContractType"/>
      </column>

      <!-- UNDERLYINGGROUP -->
      <column name="UNDERLYINGGROUP" datakeyupd="true">
        <xsl:value-of select="$PUnderlyingGroup"/>
      </column>

      <!-- UNDERLYINGASSET -->
      <column name="UNDERLYINGASSET" datakeyupd="true">
        <xsl:value-of select="$PUnderlyingAsset"/>
      </column>

      <!-- IDC_PRICE -->
      <column name="IDC_PRICE" datakeyupd="true">
        <xsl:value-of select="$pCurrency"/>
      </column>

    </table>

  </xsl:template>

  <!-- ================================================== -->
  <!--        GetMaturityRule_BCLEAR                       -->
  <!-- ================================================== -->
  <xsl:template name="GetMaturityRule_BCLEAR">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractType" select ="'STD'"/>

    <!-- PL 20141002 [19669] New MR identifiers
       Affectation des règles d'échéances sur ICE-EU à l'aide de la matrice suivante (à partir du fichier Excel BCLEAR)
       ==========================================================================
       ContractType ContractSymbol Category    Spheres MaturityRule
       ==========================================================================
       FLEX                                    IFEU Flexible Derivative Contracts
       STD          Z              F           IFLL Index Futures Standard
       STD          G,H,R          F           IFLL Bond Gilt Futures
       STD          I              F           IFLL I F
       STD          BRW            O           IFLO Equity Options Standard
       ==========================================================================              
       NB: Si aucune des conditions ci-dessus n'est vérifiée, on considère la MaturityRule par défaut: "Default Rule"
    -->

    <xsl:variable name="vMaturityRuleIdentifier">
      <xsl:choose>
        <xsl:when test ="$pContractType = 'FLEX'">
          <xsl:value-of select="'IFEU Flexible Derivative Contracts'"/>
        </xsl:when>
        <xsl:otherwise>

          <!-- En fonction de la Category -->
          <xsl:choose>
            
            <!-- Category = F -->
            <xsl:when test="$pCategory = 'F'">
              <xsl:choose>
                <!-- ContractSymbol = 'Z' -->
                <xsl:when test="$pContractSymbol = 'Z'">IFLL Index Futures Standard</xsl:when>
                <!-- ContractSymbol = 'G, H, R' -->
                <xsl:when test="$pContractSymbol = 'G' or 
                                $pContractSymbol = 'H' or 
                                $pContractSymbol = 'R'">IFLL Bond Gilt Futures</xsl:when>
                <!-- ContractSymbol = 'I' THREE MONTH EURO (EURIBOR) FUTURES  -->
                <xsl:when test="$pContractSymbol = 'I'">IFLL I F</xsl:when>
              </xsl:choose>
            </xsl:when>
            
            <!-- Category = O -->
            <xsl:when test="$pCategory = 'O'">
              <xsl:choose>
                <!-- ContractSymbol = 'BRW' -->
                <xsl:when test="$pContractSymbol = 'BRW'">IFLO Equity Options Standard</xsl:when>
              </xsl:choose>
            </xsl:when>
            
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <SQL command="select" result="IDMATURITYRULE">
      <![CDATA[
      select IDMATURITYRULE
      from dbo.MATURITYRULE
      where IDENTIFIER=@PMATURITYRULEIDENTIFIER
      ]]>
      <Param name="PMATURITYRULEIDENTIFIER" datatype="string">
        <xsl:choose>
          <xsl:when test="string-length($vMaturityRuleIdentifier)>0">
            <xsl:value-of select="$vMaturityRuleIdentifier"/>
          </xsl:when>
          <xsl:otherwise>Default Rule</xsl:otherwise>
        </xsl:choose>
      </Param>
    </SQL>
  </xsl:template>

</xsl:stylesheet>