<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <!--
============================================================================================================================================================  
 Summary: Import Clearnet SA Referential 
 File   : MarketData_Clearnet_SA_Import_Map.xsl
============================================================================================================================================================  
 Version: v8.1.0.0    Date: 20201104    Author: FL
  Comment: [25563] - Import: Task LCH.CLEARNET-SA - REPOSITORY: Manage New Market (XODB) an New MR 
  
    New Market (308 - XODB (OSLO - Norvège)) Added in template(GetISO10383_Clearnet)
  
    New Maturity Rule Added in template(GetMaturityRuleIdentifier_Clearnet)
       XODB Index Futures Standard
       XODB Index Options Standard
       
============================================================================================================================================================  
Version: v7.0.0.0    Date: 20180405    Author: FL
  Comment: [23662] - Import: Task LCH.CLEARNET-SA - REPOSITORY: Manage New MR
    New Maturity Rule Added in template(GetMaturityRuleIdentifier_Clearnet):
      XMAT Bond Futures Standard, XMAT OSM, XMAT OSO, XMAT RSM, XMAT RSO, XMAT UAN
      XMON Dividend Futures Standard, XMON Weekly Index Futures Week 1, XMON Weekly Index Futures Week 2, XMON Weekly Index Futures Week 4,
      XMON Weekly Index Futures Week 5, XMON Equity Futures Standard, XMON Weekly Equity Options Week 1, XMON Weekly Equity Options Week 2,
      XMON Weekly Equity Options Week 4, XMON Weekly Equity Options Week 5, XMON Equity Options Standard, XMON Weekly Index Options Week 1,
      XMON Weekly Index Options Week 2, XMON Weekly Index Options Week 4, XMON Weekly Index Options Week 5, XMON Index Options Standard,
      XAMS Bond Futures Standard, XAMS Dividend Futures Standard, XAMS Dividend Futures US, XAMS Fx Futures Standard, XAMS Weekly Index Futures Week 1,
      XAMS Weekly Index Futures Week 2, XAMS Weekly Index Futures Week 4, XAMS Weekly Index Futures Week 5, XAMS Index Futures Standard,
      XAMS Weekly Index Options Week 1, XAMS Weekly Index Options Week 2, XAMS Weekly Index Options Week 4, XAMS Weekly Index Options Week 5,
      XAMS Weekly Equity Options Week 1, XAMS Weekly Equity Options Week 2, XAMS Weekly Equity Options Week 4, XAMS Weekly Equity Options Week 5,
      XAMS Equity Options Standard, XAMS Fx Options Standard, XAMS Weekly Index Options Week 1, XAMS Weekly Index Options Week 2,
      XAMS Weekly Index Options Week 4, XAMS Weekly Index Options Week 5, XAMS Daily Index Options Day 1, XAMS Daily Index Options Day 2,
      XAMS Daily Index Options Day 3, XAMS Daily Index Options Day 4, XAMS Daily Index Options Day 5, XAMS Daily Index Options Day 6,
      XAMS Daily Index Options Day 7, XAMS Daily Index Options Day 8, XAMS Daily Index Options Day 9, XAMS Daily Index Options Day 10,
      XAMS Daily Index Options Day 11, XAMS Daily Index Options Day 12, XAMS Daily Index Options Day 13, XAMS Daily Index Options Day 14,
      XAMS Daily Index Options Day 15, XAMS Daily Index Options Day 17, XAMS Daily Index Options Day 18, XAMS Daily Index Options Day 19,
      XAMS Daily Index Options Day 20, XAMS Daily Index Options Day 21, XAMS Daily Index Options Day 22, XAMS Daily Index Options Day 23,
      XAMS Daily Index Options Day 24, XAMS Daily Index Options Day 25, XAMS Daily Index Options Day 26, XAMS Daily Index Options Day 27,
      XAMS Daily Index Options Day 28, XAMS Daily Index Options Day 29, XAMS Daily Index Options Day 30, XAMS Daily Index Options Day 31,
      XAMS Index Options Standard, XBRU Bond Futures Standard, XBRU Dividend Futures Standard, XBRU Index Futures Standard,
      XBRU Weekly Equity Options Week 1, XBRU Weekly Equity Options Week 2, XBRU Weekly Equity Options Week 4, XBRU Weekly Equity Options Week 5,
      XBRU Equity Options Standard, XBRU Index Options Standard, XLIS Bond Futures Standard, XLIS Dividend Futures Standard, XLIS Equity Futures Standard,
      XLIS Index Futures Standard.
    Modified template (GetSettlMethod_Clearnet) to manage Stock Future abd Dividend Future.
     
    Modified template (GetAssetCategory_Clearnet) to manage Stock Future abd Dividend Future.
============================================================================================================================================================  
  Version: v6.0.0.0    Date: 20170420    Author: FL/PLA
  Comment: [23064] - Derivative Contracts: Settled amount behavior for "Physical" delivery
  Add pPhysettltamount parameter on SQLTableDERIVATIVECONTRACT template
============================================================================================================================================================  
 Version: v4.2.0.0    Date: 20141007    Author: PL
 Comment: Add a dot ('.') into SymbolSuffix. ('VAL' becomes '.VAL', 'IND' becomes '.IND')
============================================================================================================================================================  
RD/Pony 20141107 [20475]
  Pour certains Contrats Future du Monep (XMON), le ULI (Long Id for the underlying instrument) n'est pas renseigné dans 
  le fichier source. Il sert pour la recherche de l'asset sous-jacent (DERIVATIVECONTRACT.IDASSET_UNL) 
============================================================================================================================================================  
FL 20140327 [18889]
  Sur le référentiel Indice pas de SymbolSuffix pour BX1 et PX1
============================================================================================================================================================  
FL 20140327 [19648] 
  Correction du template GetUnderlyingAsset_Clearnet, GetAssetCategory_Clearnet et GetSettlMethod_Clearnet 
============================================================================================================================================================  
FI 20140108 [19460] 
  Les parameters SQL doivent être en majuscule
============================================================================================================================================================  
FI 20131205 [19275] .
  Alimentation de CONTRACTMULTIPLIER et FACTOR lors de l'insertion d'un nouveau contrat uniquement
============================================================================================================================================================  
FI 20131129 [19284] 
  Alimentation de DERIVATIVECONTRACT.FINALSETTLTSIDE
============================================================================================================================================================  
FL 20130923 [18812]
  Manage Last Maturity Rule Added in template(GetMaturityRuleIdentifier_Clearnet)
  XMON Equity Options Standard
============================================================================================================================================================  
BD 20130709 [18812] 
  MANAGE MATURITY RULES - New MATURITY RULES 
============================================================================================================================================================  
BD/FL   20130311
  Simplification de cet import (suppression de tous les imports autres que celui des DC et enrichissement des données).
  Création des Equities, des Index des DC
============================================================================================================================================================  
PM/MF/RD 20100126 
  Create File 
============================================================================================================================================================  
-->

  <!-- import -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- include -->
  <xsl:include href="MarketData_Common.xsl"/>

  <!-- Main template -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- Specific template -->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>
      <xsl:apply-templates select="row"/>
    </file>
  </xsl:template>

  <!-- row -->
  <xsl:template match="row">
    <row useCache="false">
      <xsl:call-template name="IORowAtt"/>
      <xsl:call-template name="rowStream"/>
    </row>
  </xsl:template>

  <!-- ============================================== -->
  <!--              GetISO10383_Clearnet              -->
  <!-- ============================================== -->
  <xsl:template name="GetISO10383_Clearnet">
    <xsl:param name="pExchangeSymbol"/>

    <!-- Affectation du code ISO10383 du marché en fonction de ExchangeSymbol :
             ExchangeSymbol       ISO10383
             =============================
             274                  XMON
             276                  XMAT
             280                  XBRU
             281                  XAMS
             291                  XLIS 
             308                  XODB 
    -->

    <xsl:choose>
      <xsl:when test="$pExchangeSymbol = '274'">XMON</xsl:when>
      <xsl:when test="$pExchangeSymbol = '276'">XMAT</xsl:when>
      <xsl:when test="$pExchangeSymbol = '280'">XBRU</xsl:when>
      <xsl:when test="$pExchangeSymbol = '281'">XAMS</xsl:when>
      <xsl:when test="$pExchangeSymbol = '291'">XLIS</xsl:when>
      <xsl:when test="$pExchangeSymbol = '308'">XODB</xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ============================================== -->
  <!--        GetFutValuationMethod_Clearnet          -->
  <!-- ============================================== -->
  <xsl:template name="GetFutValuationMethod_Clearnet">
    <xsl:param name="pCategory"/>

    <!-- Affectation de FutValuationMethod en fonction de Category :
             Category       FutValuationMethod
             =================================
             F              FUT
             O              EQTY 
    -->

    <xsl:choose>
      <xsl:when test="$pCategory = 'F'">FUT</xsl:when>
      <xsl:when test="$pCategory = 'O'">EQTY</xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ============================================== -->
  <!--          GetExerciseStyle_Clearnet             -->
  <!-- ============================================== -->
  <xsl:template name="GetExerciseStyle_Clearnet">
    <xsl:param name="pExecutionType"/>

    <!-- Affectation de ExerciseStyle en fonction de ExecutionType :
             ExecutionType       ExerciseStyle
             =================================
             A                   1
             E                   0 
    -->

    <xsl:choose>
      <xsl:when test="$pExecutionType = 'A'">1</xsl:when>
      <xsl:when test="$pExecutionType = 'E'">0</xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ======================================================================================================== -->
  <!--            GetSettlMethod_Clearnet                                                                       -->
  <!-- ========================================================================))))))========================== -->
  <xsl:template name="GetSettlMethod_Clearnet">
    <xsl:param name="pUnderlyingType"/>
    <xsl:param name="pContractSymbol"/>
    <!-- FL 20140327 [19648] -->
    <!-- FL 20180405 [23662] -->
    <!-- Affectation de SettlMethod en fonction de UnderlyingType + ContractSymbol(Cf. Cas Particulier) :
             UnderlyingType       ContractSymbol           SettlMethod
             =======================================================================
             SE                                            P
             IN                                            C
             CO                                            P
             CU                                            C
             FI                   Finish by 6              C (Stock Future)
             FI                   Finish by 8              C (Dividend Future)
             FI                   B8O,I8O,I8X,K80,A80,D80  C (Dividend Future)
             FI                                            P 
             Défaut                                        C 
    -->

    <xsl:choose>
      <xsl:when test="$pUnderlyingType = 'SE'">P</xsl:when>
      <xsl:when test="$pUnderlyingType = 'IN'">C</xsl:when>
      <xsl:when test="$pUnderlyingType = 'CO'">P</xsl:when>
      <xsl:when test="$pUnderlyingType = 'CU'">C</xsl:when>
      <xsl:when test="$pUnderlyingType = 'FI'">
        <xsl:choose>
          <xsl:when test="substring($pContractSymbol,string-length($pContractSymbol))='6'">C</xsl:when>
          <xsl:when test="substring($pContractSymbol,string-length($pContractSymbol))='8'">C</xsl:when>
          <xsl:when test="($pContractSymbol='B8O' or
                         $pContractSymbol='I8O' or
                         $pContractSymbol='I8X' or
                         $pContractSymbol='K8O' or
                         $pContractSymbol='A8O' or
                         $pContractSymbol='D8O')">C</xsl:when>
          <xsl:otherwise>P</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>C</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ==================================================================================================== -->
  <!--           GetAssetCategory_Clearnet                                                                  -->
  <!-- ==================================================================================================== -->
  <xsl:template name="GetAssetCategory_Clearnet">
    <xsl:param name="pUnderlyingType"/>
    <xsl:param name="pContractSymbol"/>

    <!-- FL 20140327 [19648] -->
    <!-- FL 20180405 [23662] -->
    <!-- Affectation de AssetCategory en fonction de UnderlyingType + ContractSymbol(Cf. Cas Particulier)
             UnderlyingType       ContractSymbol           AssetCategory
             =======================================================================
             SE                                            EquityAsset
             IN                                            Index
             CO                                            Commodity
             CU                                            FxRateAsset
             FI                   Finish by 6              EquityAsset (Stock Future)
             FI                   Finish by 8              EquityAsset (Dividend Future)
             FI                   B8O,I8O,I8X,K80,A80,D80  EquityAsset (Dividend Future)
             FI                                            Bond 
    -->

    <xsl:choose>
      <xsl:when test="$pUnderlyingType = 'SE'">EquityAsset</xsl:when>
      <xsl:when test="$pUnderlyingType = 'IN'">Index</xsl:when>
      <xsl:when test="$pUnderlyingType = 'CO'">Commodity</xsl:when>
      <xsl:when test="$pUnderlyingType = 'CU'">FxRateAsset</xsl:when>
      <xsl:when test="$pUnderlyingType = 'FI'">
        <xsl:choose>
          <xsl:when test="substring($pContractSymbol,string-length($pContractSymbol))='6'">EquityAsset</xsl:when>
          <xsl:when test="substring($pContractSymbol,string-length($pContractSymbol))='8'">EquityAsset</xsl:when>
          <xsl:when test="($pContractSymbol='B8O' or
                         $pContractSymbol='I8O' or
                         $pContractSymbol='I8X' or
                         $pContractSymbol='K8O' or
                         $pContractSymbol='A8O' or
                         $pContractSymbol='D8O')">EquityAsset</xsl:when>
          <xsl:otherwise>Bond</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ============================================================================================== -->
  <!--       GetMaturityRuleIdentifier_Clearnet                                                       -->
  <!-- ============================================================================================== -->
  <xsl:template name="GetMaturityRuleIdentifier_Clearnet">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pAssetCategory"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pContractDisplayName"/>
    <xsl:param name="pCurrency"/>
    <!--<xsl:param name="pExerciseStyle"/>-->
    <!--<xsl:param name="pGroup"/>-->
    <!--<xsl:param name="pProduct"/>-->
    <!-- FL 20180405 [23662] -->
    <!-- Affectation des règles d'échéances pour Clearnet :
           ISO10383   Category   AssetCategory   ContractSymbol   ContractDisplayName   Currency   MaturityRyleIdentifier
           ===============================================================================================================================
           XMAT       F          Bond                                                              XMAT Bond Futures Standard
           XMAT       F          Commodity       EBM                                               XMAT EBM
           XMAT       F          Commodity       ECO                                               XMAT ECO
           XMAT       F          Commodity       EMA                                               XMAT EMA
           XMAT       F          Commodity       EOB                                               XMAT EOB
           XMAT       F          Commodity       RSM                                               XMAT RSM
           XMAT       F          Commodity       RSO                                               XMAT RSO
           XMAT       F          Commodity       UAN                                               XMAT UAN
           XMAT       O          Commodity       OBM                                               XMAT OBM
           XMAT       O          Commodity       OCO                                               XMAT OCO
           XMAT       O          Commodity       OMA                                               XMAT OMA
           XMAT       O          Commodity       OOB                                               XMAT OOB
           XMAT       O          Commodity       OSM                                               XMAT OSM
           XMAT       O          Commodity       OSO                                               XMAT OSO
           XMON       F          EquityAsset     Finish by 8                                       XMON Dividend Futures Standard
           XMON       F          EquityAsset                                                       XMON Equity Futures Standard
           XMON		    F		       Index		       Begin by 1       Contains Week 	                 XMON Weekly Index Futures Week 1
           XMON		    F		       Index		       Begin by 2       Contains Week		                 XMON Weekly Index Futures Week 2
           XMON		    F		       Index		       Begin by 4       Contains Week	      	           XMON Weekly Index Futures Week 4              
           XMON		    F		       Index		       Begin by 5       Contains Week	                   XMON Weekly Index Futures Week 5
           XMON       F          Index           FCE,FEF,FEO,MFC,XFC,EPE,EPR                       XMON Index Futures Standard
           XMON		    O		       EquityAsset	   Begin by 1       Contains Week	                   XMON Weekly Equity Options Week 1
           XMON		    O		       EquityAsset	   Begin by 2       Contains Week	                   XMON Weekly Equity Options Week 2
           XMON		    O		       EquityAsset	   Begin by 4       Contains Week	                   XMON Weekly Equity Options Week 4
           XMON		    O		       EquityAsset	   Begin by 5       Contains Week	                   XMON Weekly Equity Options Week 5
           XMON       O          EquityAsset                                                       XMON Equity Options Standard
           XMON		    O		       Index		       Begin by 1       Contains Week                    XMON Weekly Index Options Week 1
           XMON		    O		       Index		       Begin by 2       Contains Week         	         XMON Weekly Index Options Week 2
           XMON		    O		       Index		       Begin by 4       Contains Week	                   XMON Weekly Index Options Week 4
           XMON		    O		       Index		       Begin by 5       Contains Week         	         XMON Weekly Index Options Week 5
           XMON		    O		       Index		                           	                             XMON Index Options Standard
           XAMS       F          Bond                                                              XAMS Bond Futures Standard
           XAMS       F          Commodity                                                         (*) Default Rule
           XAMS       F          EquityAsset     Finish by 8                            USD        XAMS Dividend Futures US
           XAMS       F          EquityAsset     Finish by 8                                       XAMS Dividend Futures Standard
           XAMS       F          EquityAsset     B8O,I80,I8X,K8O                                   XAMS Dividend Futures Standard
           XAMS       F          EquityAsset                                                       XAMS Equity Futures Standard
           XAMS       F          FxRateAsset                                                       XAMS Fx Futures Standard
           XAMS		    F		       Index		       Begin by 1       Contains Week                    XAMS Weekly Index Futures Week 1
           XAMS		    F		       Index		       Begin by 2       Contains Week	                   XAMS Weekly Index Futures Week 2
           XAMS		    F		       Index		       Begin by 4       Contains Week         	         XAMS Weekly Index Futures Week 4
           XAMS		    F		       Index		       Begin by 5       Contains Week	                   XAMS Weekly Index Futures Week 5
           XAMS       F          Index                                                             XAMS Index Futures Standard
           XAMS       O          Bond                                                              XAMS Bond Options Standard
           XAMS		    O		       EquityAsset	   Begin by 1       Contains Week	                   XAMS Weekly Equity Options Week 1
           XAMS		    O		       EquityAsset	   Begin by 2       Contains Week                    XAMS Weekly Equity Options Week 2
           XAMS		    O		       EquityAsset	   Begin by 4       Contains Week	                   XAMS Weekly Equity Options Week 4
           XAMS		    O		       EquityAsset	   Begin by 5       Contains Week         	         XAMS Weekly Equity Options Week 5
           XAMS       O          EquityAsset                                                       XAMS Equity Options Standard
           XAMS       O          FxRateAsset                                                       XAMS Fx Options Standard
           XAMS		    O		       Index		       Finish by 1      Contains Week        	           XAMS Weekly Index Options Week 1
           XAMS		    O		       Index		       Finish by 2      Contains Week	      	           XAMS Weekly Index Options Week 2
           XAMS		    O		       Index		       Finish by 4      Contains Week	      	           XAMS Weekly Index Options Week 4
           XAMS		    O		       Index		       Finish by 5      Contains Week	      	           XAMS Weekly Index Options Week 5
           XAMS		    O		       Index		       Finish by 1      Contains Daily         	         XAMS Daily Index Options Day 1
           ......................................................................................................................................
           ......................................................................................................................................
           XAMS		    O		       Index		       Finish by 11     Contains Daily         	         XAMS Daily Index Options Day 11
           ......................................................................................................................................
           ......................................................................................................................................
           XAMS		    O		       Index		       Finish by 21     Contains Daily         	         XAMS Daily Index Options Day 21
           ......................................................................................................................................
           ......................................................................................................................................
           XAMS		    O		       Index		       Finish by 31     Contains Daily         	         XAMS Daily Index Options Day 31
           XAMS       O          Index                                                             XAMS Index Options Standard
           XBRU       F          Bond                                                              XBRU Bond Futures Standard
           XBRU       F          EquityAsset     Finish by 8                                       XBRU Dividend Futures Standard
           XBRU       F          EquityAsset     A8O,D80                                           XBRU Dividend Futures Standard
           XBRU       F          EquityAsset                                                       XBRU Equity Futures Standard
           XBRU       F          Index                                                             XBRU Index Futures Standard
           XBRU		    O		       EquityAsset	   Begin by 1       Contains Week	     	             XBRU Weekly Equity Options Week 1
           XBRU		    O		       EquityAsset	   Begin by 2       Contains Week	                   XBRU Weekly Equity Options Week 2
           XBRU		    O		       EquityAsset	   Begin by 4       Contains Week	                   XBRU Weekly Equity Options Week 4
           XBRU		    O		       EquityAsset	   Begin by 5       Contains Week         	         XBRU Weekly Equity Options Week 5
           XBRU       O          EquityAsset                                                       XBRU Equity Options Standard
           XBRU       O          Index                                                             XBRU Index Options Standard
           XLIS       F          Bond                                                              XLIS Bond Futures Standard
           XLIS       F          EquityAsset     Finish by 8                                       XLIS Dividend Futures Standard
           XLIS       F          EquityAsset                                                       XLIS Equity Futures Standard
           XLIS       F          Index                                                             XLIS Index Futures Standard
           XODB       F          Index                                                             XODB Index Futures Standard
           XODB       O          Index                                                             XODB Index Options Standard
           Valeur par défaut                                                                       Default Rule      
           
           (*) un seul DC sur ce marché FAP (Potato Futures) sur lequel on a aucune spécification de plus de DC n'est pas encore dans
                le fichier SPAN il est uniquement dans le fichier EPPUBLICXXLCDDXXXXXX.lst
    -->

    <xsl:choose>

      <xsl:when test="$pISO10383='XMAT'">
        <xsl:choose>
          <!-- Category = F -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory='Bond'">XMAT Bond Futures Standard</xsl:when>
              <xsl:when test="$pContractSymbol='EBM'">XMAT EBM</xsl:when>
              <xsl:when test="$pContractSymbol='ECO'">XMAT ECO</xsl:when>
              <xsl:when test="$pContractSymbol='EMA'">XMAT EMA</xsl:when>
              <xsl:when test="$pContractSymbol='EOB'">XMAT EOB</xsl:when>
              <xsl:when test="$pContractSymbol='RSM'">XMAT RSM</xsl:when>
              <xsl:when test="$pContractSymbol='RSO'">XMAT RSO</xsl:when>
              <xsl:when test="$pContractSymbol='UAN'">XMAT UAN</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- Category = O  -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <xsl:when test="$pContractSymbol='OBM'">XMAT OBM</xsl:when>
              <xsl:when test="$pContractSymbol='OCO'">XMAT OCO</xsl:when>
              <xsl:when test="$pContractSymbol='OMA'">XMAT OMA</xsl:when>
              <xsl:when test="$pContractSymbol='OOB'">XMAT OOB</xsl:when>
              <xsl:when test="$pContractSymbol='OSM'">XMAT OSM</xsl:when>
              <xsl:when test="$pContractSymbol='OSO'">XMAT OSO</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="$pISO10383='XMON'">
        <xsl:choose>
          <!-- Category = F -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction du Contract Symbol -->
            <xsl:choose>
              <xsl:when test="$pContractSymbol = 'FCE' or $pContractSymbol = 'FEF' or $pContractSymbol = 'FEO' or
                              $pContractSymbol = 'MFC' or $pContractSymbol = 'XFC' or $pContractSymbol = 'EPE' or $pContractSymbol = 'EPR'">XMON Index Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='1'">XMON Weekly Index Futures Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='2'">XMON Weekly Index Futures Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='4'">XMON Weekly Index Futures Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='5'">XMON Weekly Index Futures Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and substring($pContractSymbol,string-length($pContractSymbol))='8'">XMON Dividend Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XMON Equity Futures Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- Category = O  -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='1'">XMON Weekly Equity Options Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='2'">XMON Weekly Equity Options Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='4'">XMON Weekly Equity Options Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='5'">XMON Weekly Equity Options Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XMON Equity Options Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='1'">XMON Weekly Index Options Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='2'">XMON Weekly Index Options Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='4'">XMON Weekly Index Options Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='5'">XMON Weekly Index Options Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index'">XMON Index Options Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="$pISO10383='XAMS'">
        <xsl:choose>
          <!-- Category = F -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'Bond'">XAMS Bond Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'FxRateAsset'">XAMS Fx Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='1'">XAMS Weekly Index Futures Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='2'">XAMS Weekly Index Futures Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='4'">XAMS Weekly Index Futures Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='5'">XAMS Weekly Index Futures Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index'">XAMS Index Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and substring($pContractSymbol,string-length($pContractSymbol))='8' and $pCurrency='USD'">XAMS Dividend Futures US</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and substring($pContractSymbol,string-length($pContractSymbol))='8'">XAMS Dividend Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and ($pContractSymbol='B8O' or $pContractSymbol='I8O' or $pContractSymbol='I8X' or $pContractSymbol='K8O')">XAMS Dividend Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XAMS Equity Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Commodity'">Default Rule</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- Category = O  -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'Bond'">XAMS Bond Options Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='1'">XAMS Weekly Equity Options Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='2'">XAMS Weekly Equity Options Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='4'">XAMS Weekly Equity Options Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='5'">XAMS Weekly Equity Options Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XAMS Equity Options Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'FxRateAsset'">XAMS Fx Options Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week')  and substring($pContractSymbol,string-length($pContractSymbol))='1'">XAMS Weekly Index Options Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week')  and substring($pContractSymbol,string-length($pContractSymbol))='2'">XAMS Weekly Index Options Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week')  and substring($pContractSymbol,string-length($pContractSymbol))='4'">XAMS Weekly Index Options Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Week')  and substring($pContractSymbol,string-length($pContractSymbol))='5'">XAMS Weekly Index Options Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='31'">XAMS Daily Index Options Day 31</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='30'">XAMS Daily Index Options Day 30</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='29'">XAMS Daily Index Options Day 29</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='28'">XAMS Daily Index Options Day 28</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='27'">XAMS Daily Index Options Day 27</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='26'">XAMS Daily Index Options Day 26</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='25'">XAMS Daily Index Options Day 25</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='24'">XAMS Daily Index Options Day 24</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='23'">XAMS Daily Index Options Day 23</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='22'">XAMS Daily Index Options Day 22</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='21'">XAMS Daily Index Options Day 21</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='20'">XAMS Daily Index Options Day 20</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='19'">XAMS Daily Index Options Day 19</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='18'">XAMS Daily Index Options Day 18</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='17'">XAMS Daily Index Options Day 17</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='16'">XAMS Daily Index Options Day 16</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='15'">XAMS Daily Index Options Day 15</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='14'">XAMS Daily Index Options Day 14</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='13'">XAMS Daily Index Options Day 13</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='12'">XAMS Daily Index Options Day 12</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='11'">XAMS Daily Index Options Day 11</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol)- 1)='10'">XAMS Daily Index Options Day 10</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='9'">XAMS Daily Index Options Day 9</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='8'">XAMS Daily Index Options Day 8</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='7'">XAMS Daily Index Options Day 7</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='6'">XAMS Daily Index Options Day 6</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='5'">XAMS Daily Index Options Day 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='4'">XAMS Daily Index Options Day 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='3'">XAMS Daily Index Options Day 3</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='2'">XAMS Daily Index Options Day 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index' and contains($pContractDisplayName,'Daily') and substring($pContractSymbol,string-length($pContractSymbol))='1'">XAMS Daily Index Options Day 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index'">XAMS Index Options Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
        
      </xsl:when>

      <xsl:when test="$pISO10383='XBRU'">
        <xsl:choose>
          <!-- Category = F -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'Bond'">XBRU Bond Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index'">XBRU Index Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and substring($pContractSymbol,string-length($pContractSymbol))='8'">XBRU Dividend Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and ($pContractSymbol='A8O' or $pContractSymbol='D8O')">XBRU Dividend Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XBRU Equity Futures Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- Category = O  -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='1'">XBRU Weekly Equity Options Week 1</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='2'">XBRU Weekly Equity Options Week 2</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='4'">XBRU Weekly Equity Options Week 4</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and contains($pContractDisplayName,'Week') and substring($pContractSymbol,1,1)='5'">XBRU Weekly Equity Options Week 5</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XBRU Equity Options Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index'">XBRU Index Options Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="$pISO10383='XLIS'">
        <xsl:choose>
          <!-- Category = F -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'Bond'">XLIS Bond Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset' and substring($pContractSymbol,string-length($pContractSymbol))='8'">XLIS Dividend Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'EquityAsset'">XLIS Equity Futures Standard</xsl:when>
              <xsl:when test="$pAssetCategory = 'Index'">XLIS Index Futures Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- Category = O  -->
          <xsl:when test="$pCategory = 'O'">
            Default Rule
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      
      <xsl:when test="$pISO10383='XODB'">
        <xsl:choose>
          <!-- Category = F -->
          <xsl:when test="$pCategory = 'F'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'Index'">XODB Index Futures Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- Category = O  -->
          <xsl:when test="$pCategory = 'O'">
            <!-- En fonction de Asset Category -->
            <xsl:choose>
              <xsl:when test="$pAssetCategory = 'Index'">XODB Index Options Standard</xsl:when>
              <xsl:otherwise>Default Rule</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>

      <xsl:otherwise>Default Rule</xsl:otherwise>

    </xsl:choose>
  </xsl:template>


  <!-- =========================================================================== -->
  <!--           GetUnderlyingAsset_Clearnet                                       -->
  <!-- =========================================================================== -->
  <xsl:template name="GetUnderlyingAsset_Clearnet">
    <xsl:param name="pAssetCategory"/>
    <xsl:param name="pClearingSegment"/>

    <!-- FL 20140327 [19648]: Erreur sur UnderlyingAsset sur  Commodity -->
    <!-- Affectation de UnderlyingAsset en fonction de AssetCategory :
             AssetCategory    ClearingSegment      UnderlyingAsset
             ====================================================
             EquityAsset                           FS
             Index                                 FI
             FxRateAsset                           FC
             Bond                                  FD
             Commodity         M                   CA
             Future            M                   CA
             Commodity         A,F                 FT
             Future            A,F                 FT
    -->

    <xsl:choose>
      <xsl:when test="$pAssetCategory='EquityAsset'">FS</xsl:when>
      <xsl:when test="$pAssetCategory='Index'">FI</xsl:when>
      <xsl:when test="$pAssetCategory='FxRateAsset'">FC</xsl:when>
      <xsl:when test="$pAssetCategory='Bond'">FD</xsl:when>
      <!--FL 20140327 [19648]  -->
      <!--<xsl:when test="$pAssetCategory='Commodity'">FT</xsl:when>-->
      <xsl:when test="$pAssetCategory='Commodity'">
        <xsl:choose>
          <xsl:when test="$pClearingSegment='M'">CA</xsl:when>
          <xsl:otherwise>FT</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pAssetCategory='Future'">
        <xsl:choose>
          <xsl:when test="$pClearingSegment='M'">CA</xsl:when>
          <xsl:otherwise>FT</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ======================================================================= -->
  <!--           GetUnderlyingGroup_Clearnet                                   -->
  <!-- ======================================================================= -->
  <xsl:template name="GetUnderlyingGroup_Clearnet">
    <xsl:param name="pClearingSegment"/>

    <!-- Affectation de UnderlyingGroup en fonction de ClearingSegment :
             ClearingSegment                  UnderlyingGroup
             ================================================
             A                                F
             F                                F
             M                                C
    -->

    <xsl:choose>
      <xsl:when test="$pClearingSegment='A'">F</xsl:when>
      <xsl:when test="$pClearingSegment='F'">F</xsl:when>
      <xsl:when test="$pClearingSegment='M'">C</xsl:when>
    </xsl:choose>
  </xsl:template>



  <!-- ============================================== -->
  <!--                 rowStream                      -->
  <!-- ============================================== -->
  <xsl:template name="rowStream">

    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <!-- Récupération des variables -->
    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->

    <xsl:variable name="vExchangeSymbol">
      <xsl:value-of select="normalize-space(data[@name='MC'])"/>
    </xsl:variable>

    <xsl:variable name="vISO10383">
      <xsl:call-template name="GetISO10383_Clearnet">
        <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vContractSymbol">
      <xsl:value-of select="normalize-space(data[@name='FMC'])"/>
    </xsl:variable>

    <xsl:variable name="vCategory">
      <xsl:value-of select="normalize-space(data[@name='FT'])"/>
    </xsl:variable>

    <xsl:variable name="vFutValuationMethod">
      <xsl:call-template name="GetFutValuationMethod_Clearnet">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vContractMultiplier">
      <xsl:value-of select="normalize-space(data[@name='VC'])"/>
    </xsl:variable>

    <xsl:variable name="vUnderlyingIsinCode">
      <!-- Dans le cas du XMAT, le "UnderlyingIsinCode n'est pas réellement un code ISN (ex: "EUFR09637687")
              Voir FL pour plus d'infos -->
      <xsl:choose>
        <xsl:when test="$vISO10383!='XMAT'">
          <!-- RD/Pony 20141107 [20475]
          Pour certains Contrats Future du Monep (XMON), le ULI (Long Id for the underlying instrument) n'est pas renseigné dans le fichier source.
          Il sert pour la recherche de l'asset sous-jacent (DERIVATIVECONTRACT.IDASSET_UNL) -->
          <xsl:variable name="vULI" select="normalize-space(data[@name='ULI'])"/>
          <xsl:choose>
            <xsl:when test="string-length($vULI) >0">
              <xsl:value-of select="$vULI"/>
            </xsl:when>
            <xsl:otherwise>null</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>null</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vCurrency">
      <xsl:value-of select="normalize-space(data[@name='CU'])"/>
    </xsl:variable>

    <xsl:variable name="vExerciseStyle">
      <xsl:call-template name="GetExerciseStyle_Clearnet">
        <xsl:with-param name="pExecutionType" select="normalize-space(data[@name='ET'])" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vContractDisplayName">
      <xsl:value-of select="normalize-space(data[@name='FN'])"/>
    </xsl:variable>

    <xsl:variable name="vNominalValue">
      <!-- Affectation de NominalValue en fonction de ExchangeSymbol :
             ExchangeSymbol       NominalValue
             =================================
             276                  data[@name='NV']
             E                    data[@name='VC'] 
      -->
      <xsl:choose>
        <!-- ExchangeSymbol=276 -> XMAT -->
        <xsl:when test="$vExchangeSymbol='276'">
          <xsl:value-of select="normalize-space(data[@name='NV'])"/>
        </xsl:when>
        <!-- Autres ExchangeSymbol... -->
        <xsl:otherwise>
          <xsl:value-of select="normalize-space(data[@name='VC'])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vSettlMethod">
      <!-- NYSE Liffe a décidé de lister sur le MONEP 15 contrats d’options (dont le code contrat se termine par un '7') 
            sur actions de style européen en Cash Settlement (C) -->
      <xsl:choose>
        <xsl:when test="$vISO10383='XMON' and $vCategory='O' and substring($vContractSymbol,3)='7'">C</xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="GetSettlMethod_Clearnet">
            <xsl:with-param name="pUnderlyingType" select="normalize-space(data[@name='UT'])"/>
            <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vAssetCategory">
      <xsl:choose>
        <xsl:when test="$vCategory='O' and normalize-space(data[@name='UT'])='CO'">Future</xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="GetAssetCategory_Clearnet">
            <xsl:with-param name="pUnderlyingType" select="normalize-space(data[@name='UT'])"/>
            <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vContractFactor">
      <xsl:choose>
        <!-- FL&BD 20130311: Import sur toutes les options sur futures Contract Size (Column: FACTOR) doit êtres égale à 1 -->
        <xsl:when test="$vCategory='O' and $vAssetCategory='Future'">1</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vContractMultiplier"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vMaturityRuleIdentifier">
      <xsl:call-template name="GetMaturityRuleIdentifier_Clearnet">
        <xsl:with-param name="pISO10383" select="$vISO10383"/>
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
        <xsl:with-param name="pContractDisplayName" select="$vContractDisplayName"/>
        <xsl:with-param name="pCurrency" select="$vCurrency"/>
        <!--<xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>-->
        <!--<xsl:with-param name="pGroup" select="$???"/>-->
        <!--<xsl:with-param name="pProduct" select="$???"/>-->
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vDerivativeContractIdentifier">
      <xsl:value-of select="$gAutomaticCompute"/>
    </xsl:variable>

    <xsl:variable name="vUnderlyingGroup">
      <xsl:call-template name="GetUnderlyingGroup_Clearnet">
        <xsl:with-param name="pClearingSegment" select="normalize-space(data[@name='CSC'])"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vUnderlyingAsset">
      <xsl:call-template name="GetUnderlyingAsset_Clearnet">
        <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
        <xsl:with-param name="pClearingSegment" select="normalize-space(data[@name='CSC'])"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vInstrumentIdentifier">
      <xsl:call-template name="InstrumentIdentifier">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vAssignmentMethod">
      <xsl:call-template name="AssignmentMethod">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>


    <!-- ~~~~~~~~~~~~~~~~~ -->
    <!-- Génération du SQL -->
    <!-- ~~~~~~~~~~~~~~~~~ -->

    <!-- Création des ASSET -->

    <!-- Arrangement du Product Name -->
    <xsl:variable name="vAssetProductName">
      <xsl:choose>
        <xsl:when test="contains($vContractDisplayName,'-')">
          <!-- "AEX Weekly - 2nd Friday" -> "AEX Weekly" -->
          <xsl:value-of select="normalize-space(substring-before($vContractDisplayName,'-'))"/>
        </xsl:when>
        <xsl:when test="contains($vContractDisplayName,'(')">
          <!-- "Publicis Groupe (100)" -> "Publicis Groupe" -->
          <xsl:value-of select="normalize-space(substring-before($vContractDisplayName,'('))"/>
        </xsl:when>
        <xsl:when test="contains($vContractDisplayName,'European')">
          <!-- "France Telecom European" -> "France Telecom" -->
          <xsl:value-of select="normalize-space(substring-before($vContractDisplayName,'European'))"/>
        </xsl:when>
        <xsl:when test="contains($vContractDisplayName,'American')">
          <!-- "BinckBank N.V. American Style Stock" -> "BinckBank N.V." -->
          <xsl:value-of select="normalize-space(substring-before($vContractDisplayName,'American'))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space($vContractDisplayName)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vUnderlyingMarket">
      <xsl:choose>
        <!-- Si marché du dérivé XMON ou XMAT, marché du sous-jacent : XPAR
              BD 20130513 : Cas spécial pour Air France (FR0000031122) qui est référencé sur XAMS alors qu'en réalité il s'agit de XPAR -->
        <xsl:when test="$vISO10383='XMON' or $vISO10383='XMAT' or $vUnderlyingIsinCode='FR0000031122'">XPAR</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vISO10383"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$vAssetCategory = 'Index'">
        <!-- Création INDEX -->
        <xsl:variable name="vSymbolSuffix">
          <xsl:choose>
            <!-- FL 20140327 [18889]: Sur le référentiel Indice pas de SymbolSuffix pour BX1(BEL 20 IND) et PX1 -->
            <xsl:when test="contains($vAssetProductName,'BEL 20')"/>
            <xsl:when test="contains($vAssetProductName,'CAC 40')"/>
            <!--<xsl:otherwise>IND</xsl:otherwise>-->
            <xsl:otherwise>.IND</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:call-template name="SQLTableASSET_INDEX">
          <xsl:with-param name="pIsincode" select="$vUnderlyingIsinCode"/>
          <xsl:with-param name="pIdc" select="$vCurrency"/>
          <xsl:with-param name="pSymbolSuffix" select="$vSymbolSuffix"/>
          <xsl:with-param name="pIso10383" select="$vUnderlyingMarket"/>
          <xsl:with-param name="pIso10383Related" select="$vISO10383"/>
          <xsl:with-param name="pAssetTitled" select="$vAssetProductName"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test="$vAssetCategory = 'EquityAsset'">
        <!-- Création EQUITY -->
        <xsl:call-template name="SQLTableASSET_EQUITY">
          <xsl:with-param name="pIsincode" select="$vUnderlyingIsinCode"/>
          <xsl:with-param name="pIdc" select="$vCurrency"/>
          <!--<xsl:with-param name="pSymbolSuffix" select="'VAL'"/>-->
          <xsl:with-param name="pSymbolSuffix" select="'.VAL'"/>
          <xsl:with-param name="pIso10383" select="$vUnderlyingMarket"/>
          <xsl:with-param name="pIso10383Related" select="$vISO10383"/>
          <xsl:with-param name="pAssetTitled" select="$vAssetProductName"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

    <!-- Création des DERIVATIVECONTRACT
            DESCRIPTION : "'ContractSymbol' Opt/Fut ('ContractDisplayName') Phys/Cash Am/Eu-->
    <xsl:variable name="vDCDescription">
      <xsl:call-template name="SetDCDescription_Clearnet">
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pContractDisplayName" select="$vContractDisplayName"/>
        <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
        <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vMnemonicCode">
      <xsl:choose>
        <xsl:when test="string-length(normalize-space(data[@name='UM']))>0">
          <xsl:value-of select="normalize-space(data[@name='UM'])"/>
        </xsl:when>
        <xsl:otherwise>null</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- FI 20131129 [19284] add ASSETCATEGORY_Clearnet -->
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <xsl:variable name="vExtSQLFilterNames" select="concat('ISO10383_CLEARNET',',','UNDERLYINGISIN_CLEARNET',',','MNEMONICCODE_CLEARNET',',','UNDERLYINGMARKET_CLEARNET',',','ASSETCATEGORY_CLEARNET')"/>
    <xsl:variable name="vExtSQLFilterValues" select="concat($vISO10383,',',$vUnderlyingIsinCode,',',$vMnemonicCode,',',$vUnderlyingMarket,',',$vAssetCategory)"/>

    <xsl:variable name ="vContractMultiplierSpecified">
      <xsl:choose>
        <xsl:when test ="$vContractMultiplier=0">
          <xsl:value-of select="$gFalse"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$gTrue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vContractFactorSpecified">
      <xsl:choose>
        <xsl:when test ="$vContractFactor=0">
          <xsl:value-of select="$gFalse"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$gTrue"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
    <xsl:variable name="vPhysettltamount">
      <xsl:choose>
        <xsl:when test="($vSettlMethod = 'C')">NA</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$vAssetCategory = 'Commodity'">
        <!-- Quand AssetCategory = 'Commodity', appel de SQLTableDERIVATIVECONTRACT avec le param pUnitOfMeasureQty -->
        <!-- FI 20131205 [19275] Alimentation des paramètres pInsContractFactor, pUpdContractFactor , pInsContractMultiplier, pUpdContractMultiplier -->
        <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
          <xsl:with-param name="pISO10383" select="$vISO10383"/>
          <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
          <xsl:with-param name="pMaturityRuleIdentifier" select="$vMaturityRuleIdentifier"/>
          <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
          <xsl:with-param name="pDerivativeContractIdentifier" select="$gAutomaticCompute"/>
          <xsl:with-param name="pContractDisplayName" select="$gAutomaticCompute"/>
          <xsl:with-param name="pDescription" select="$vDCDescription"/>
          <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
          <xsl:with-param name="pCurrency" select="$vCurrency"/>
          <xsl:with-param name="pCategory" select="$vCategory"/>
          <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
          <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
          <xsl:with-param name="pPhysettltamount" select="$vPhysettltamount"/>
          <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
          <xsl:with-param name="pContractFactor" select="$vContractFactor"/>
          <xsl:with-param name="pInsContractFactor" select="$vContractFactorSpecified"/>
          <xsl:with-param name="pUpdContractFactor" select="false()"/>
          <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
          <xsl:with-param name="pInsContractMultiplier" select="$vContractMultiplierSpecified"/>
          <xsl:with-param name="pUpdContractMultiplier" select="false()"/>
          <xsl:with-param name="pUnitOfMeasureQty" select="$vNominalValue"/>
          <xsl:with-param name="pUnitOfMeasure" select="'t'"/>
          <xsl:with-param name="pUnderlyingGroup" select="$vUnderlyingGroup"/>
          <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
          <xsl:with-param name="pAssignmentMethod" select="$vAssignmentMethod"/>
          <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
          <xsl:with-param name="pUnderlyingIsinCode" select="$vUnderlyingIsinCode"/>
          <!-- BD 20130513 pDerivativeContractIsAutoSetting=gTrue -->
          <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$gTrue"/>
          <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
          <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>


        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!-- Autres cas, appel de SQLTableDERIVATIVECONTRACT avec le param pNominalValue -->
        <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
          <xsl:with-param name="pISO10383" select="$vISO10383"/>
          <xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>
          <xsl:with-param name="pMaturityRuleIdentifier" select="$vMaturityRuleIdentifier"/>
          <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
          <xsl:with-param name="pDerivativeContractIdentifier" select="$gAutomaticCompute"/>
          <xsl:with-param name="pContractDisplayName" select="$gAutomaticCompute"/>
          <xsl:with-param name="pDescription" select="$vDCDescription"/>
          <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
          <xsl:with-param name="pCurrency" select="$vCurrency"/>
          <xsl:with-param name="pCategory" select="$vCategory"/>
          <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
          <xsl:with-param name="pSettlMethod" select="$vSettlMethod"/>
          <xsl:with-param name="pPhysettltamount" select="$vPhysettltamount"/>
          <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
          <xsl:with-param name="pContractFactor" select="$vContractFactor"/>
          <xsl:with-param name="pInsContractFactor" select="$vContractFactorSpecified"/>
          <xsl:with-param name="pUpdContractFactor" select="false()"/>
          <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
          <xsl:with-param name="pInsContractMultiplier" select="$vContractMultiplierSpecified"/>
          <xsl:with-param name="pUpdContractMultiplier" select="false()"/>
          <xsl:with-param name="pNominalValue" select="$vNominalValue"/>
          <xsl:with-param name="pUnderlyingGroup" select="$vUnderlyingGroup"/>
          <xsl:with-param name="pUnderlyingAsset" select="$vUnderlyingAsset"/>
          <xsl:with-param name="pAssignmentMethod" select="$vAssignmentMethod"/>
          <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
          <xsl:with-param name="pUnderlyingIsinCode" select="$vUnderlyingIsinCode"/>
          <!-- BD 20130513 pDerivativeContractIsAutoSetting=gTrue -->
          <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$gTrue"/>
          <xsl:with-param name="pExtSQLFilterNames" select="$vExtSQLFilterNames"/>
          <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>



  <!-- ============================================== -->
  <!--           SetDCDescription_Clearnet            -->
  <!-- ============================================== -->
  <xsl:template name="SetDCDescription_Clearnet">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractDisplayName"/>
    <xsl:param name="pSettlMethod"/>
    <xsl:param name="pExerciseStyle"/>

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

    <xsl:value-of select="concat(
                            $pContractSymbol,' ',
                            $FutOrOpt,' ',
                            '(',$pContractDisplayName,') ',
                            $CashOrPhys,' ',
                            $AmOrEu
                          )"/>
  </xsl:template>



  <!-- ============================================== -->
  <!--    ovrSQLGetDerivativeContractDefaultValue     -->
  <!-- ============================================== -->
  <!-- FI 20131121 [19216] add CONTRACTTYPE -->
  <!-- FI 20131129 [19284] add FINALSETTLTSIDE -->
  <xsl:template name="ovrSQLGetDerivativeContractDefaultValue">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:choose>
      <xsl:when test="$pResult='IDASSET_UNL'">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <SQL command="select" result="IDASSET" cache="true">
          <![CDATA[
          select ASSET.IDASSET
          from dbo.ASSET_EQUITY asset
          inner join dbo.MARKET m on (m.IDM=asset.IDM) and (m.ISO10383_ALPHA4=@UNDERLYINGMARKET_CLEARNET)
          where asset.ISINCODE=@UNDERLYINGISIN_CLEARNET
          union all
          select asset.IDASSET
          from dbo.ASSET_INDEX asset
          inner join dbo.MARKET m on (m.IDM=asset.IDM) and (m.ISO10383_ALPHA4=@UNDERLYINGMARKET_CLEARNET)
          where asset.ISINCODE=@UNDERLYINGISIN_CLEARNET
          ]]>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
        </SQL>
      </xsl:when>

      <xsl:when test="$pResult='IDDC_UNL'">
        <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <SQL command="select" result="IDDC" cache="true">
          <![CDATA[
          select dc.IDDC
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOLEXL) and (m.ISO10383_ALPHA4=@ISO10383_CLEARNET)
          where dc.CONTRACTSYMBOL=@MNEMONICCODE_CLEARNET and dc.CATEGORY='F'
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

      <xsl:when test="$pResult='CONTRACTTYPE'">
        <xsl:value-of select ="'STD'"/>
      </xsl:when>

      <xsl:when test="$pResult='FINALSETTLTSIDE'">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <SQL command="select" result="FINALSETTLTSIDE" cache="true">
          <![CDATA[
          select  case when @CATEGORYEXL = 'O' then 
                        case when @ASSETCATEGORY_CLEARNET in ('Index','EquityAsset') then 
                            'OfficialSettlement' 
                        else 
                            'OfficialClose' end
                  else 
                    null end as FINALSETTLTSIDE
          from DUAL
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