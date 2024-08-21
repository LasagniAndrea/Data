<?xml version="1.0" encoding="utf-8"?>
<!-- 
=============================================================================================
 Summary : Import Eurex Referential 
           Mise à jours de la table EUREXPRODUCT
           Création des Equities (Table: ASSET_EQUITY) et des Index (Table: ASSET_INDEX)          
 File    : MarketData_Eurex_EurexProduct_Import_Map.xsl
 =============================================================================================
 Version : v13.0.0.1                                               
 Date    : 20231009
 Author  : PL                                                 
 Comment : [XXXXX] - Misc
 =============================================================================================  
  Version : v13.0.0.0                                               
 Date    : 20230726
 Author  : PL                                                 
 Comment : [XXXXX] - Use new file ProductList
 ============================================================================================= 
 Version : v5.1.6171.0                                               
 Date    : 20161123
 Author  : FL                                                 
 Comment : [20625] - Manage Last Maturity Rule Added in template(GetMaturityRule_EUREX)                     
       - XEUR Index Futures Standard
         for DC Product Group = STOXX® Europe 600 Sector Index Futures
 ============================================================================================= 
 Version : v5.1.5973.0                                               
 Date    : 20160509
 Author  : FL                                                 
 Comment : [22104] - EMIR TR: Valorizzazione dei campi Identifier of Underlying ID e 
            Underlying per contratti con sottostante Bond 
              
           - Manage 'Euro Swap Futures' in Template GetAssetCategory_EUREX 
            (AssetCategory = RateIndex)
   After : 
         pProductType   pIsShareISINExist   AssetCategory retournée
         ================================================================================
         FBND                               Bond
         
     Now : 
         pProductType   pProductGroup         pIsShareISINExist   AssetCategory retournée
         ================================================================================
         FBND           =  Euro Swap Futures                      Bond
         FBND           != Euro Swap Futures                      RateIndex
 =============================================================================================
 Version : v5.1.5935.0                                               
 Date    : 20160401
 Author  : RD/FL                                                 
 Comment : [21994] - Fichier de la chambre incorrect -> Remplacer 'XLSE' par 'XLON'         
 =============================================================================================
 Version : v4.1.0.0                                               
 Date    : 20140513
 Author  : FL                                                 
 Comment : [19934] - Manage Last Maturity Rule Added in template(GetMaturityRule_EUREX)                     
    - XEUR Volatility Futures & Options
         for DC (Product Type = FVOL) or (Product Type = FVOL and Contract Symbol = OVS)
         - FVS - VSTOXX® Futures
		     - OVS - VSTOXX® Options
 ============================================================================================= 
 Version : v4.0.0.0
 Date    : 20140423
 Author  : FL
 Comment : [19880](Import: Task EUREX Repository - Wrong settlement method for Options Euribor and Mid Curve)
 
            Modification in template GetSettltMethod_EUREX on ProducType 'OFIT'
            
   After : 
         pProductType       pCountryCode        SettltMethod 
         ===============================================================
         ............................
         OFIT                                   Cash Settlement
         ............................

   Now : 
         pProductType       pCountryCode        SettltMethod 
         ===============================================================
         ............................
         OFIT                                   Physical
         ............................
 ============================================================================================= 
 Version : v3.8.0.0
 Date    : 20140220
 Author  : FL
 Comment : [19648] (Repository : UnderlyingGroup & UnderlyingAsset)
           - Création des Templates suivants:
                  GetUnderlyingGroup_EUREX
                      (return UnderlyingGroup from an EUREX Derivative Contract)
                  GetUnderlyingAsset_EUREX
                      (return UnderlyingAsset from an EUREX Derivative Contract)
           - Mise à jours des nouvelles colonnes UNDERLYINGGROUP & UNDERLYINGASSET dans la
              table EUREXPRODUCT.
           - Modification du Template GetAssetCategory_EUREX pour gestion du DC ayant pour
              Symbol OCCO => Dow Jones UBS Commodity Options
           - Modification du Template GetAssetCategory_EUREX pour gestion du DC ayant pour
              Product Type FENE et OFEN  => AssetCategory =  Index
 =============================================================================================
 Version : v3.7.0.0
 Date    : 20131219
 Author  : FL
 Comment : [19333-19373] - Wrong ExerciseStyle for DC on ProductType 'OFEN', 'OFIX' 
           in temmplate : GetExerciseStyle_EUREX

    After : pProductType  ExerciseStyle
            ============================================
            OFEN          'American'
            OFIX          'American'
        
    Now :  pProductType  ExerciseStyle
           =============================================
            OFEN          'European'
            OFIX          'European'
 =============================================================================================
 Version : v3.7.0.0
 Date    : 20131219
 Author  : FL
 Comment : [19333-19373] - Wrong FutValuationMethod for DC on ProductType 'OFEN', 'OFIX' 
           in temmplate : GetFutValuationMethod_EUREX

    After : pProductType  FutValuationMethod
            ==================================
            OFEN          'FUT'
            OFIX          'FUT'
        
    Now :   pProductType  FutValuationMethod 
            ====================================
            OFEN          'EQTY'
            OFIX          'EQTY'
 =============================================================================================
 Version : v3.7.0.0                                              
 Date    : 20131022                                          
 Author  : FL
 Comment : [18812] - Manage Last Maturity Rule Added in template(GetMaturityRule_EUREX)
    - XEUR Equity Futures Standard
         for DC Product Group = Single Stock Futures
 =============================================================================================
 Version : v3.7.0.0                                               
 Date    : 20130920                                          
 Author  : FL                                                 
 Comment : [18812] - Manage Last Maturity Rule Added in template(GetMaturityRule_EUREX)                     
    - XEUR Index Futures Standard
         for DC Product Group = Broadbased / Size Indexes
            - FXXE - EURO STOXX® Index Futures
            - FLCE - EURO STOXX® Large Index Futures
            - FMCE - EURO STOXX® Mid Index Futures
            - FSCE - EURO STOXX® Small Index Futures
            - FXXP - STOXX® Europe 600 Index Futures
            - FLCP - STOXX® Europe Large 200 Index Futures
            - FMCP - STOXX® Europe Mid 200 Index Futures
            - FSCP - TOXX® Europe Small 200 Index Futures
 =============================================================================================    
 Version : v3.7.0.0                                               
 Date    : 20130904                                           
 Author  : FL                                                 
 Comment : [18812] - Manage Last Maturity Rule Added in template(GetMaturityRule_EUREX)                     
    - XEUR Index Futures Standard
    - XEUR Volatility Index Futures Standard
 =============================================================================================
 Version : v3.4.0.0                                              
 Date    : 20130620                                          
 Author  : BD & FL                                               
 Comment : [18771] Mise à jour du template "GetMaturityRule_EUREX" 
                    pour gérer les MR des DC sur INDEX & BOND FUTURE&OPTIONS
 =============================================================================================
 Version : v3.4.0.0                                              
 Date    : 20130406                                          
 Author  : BD & FL                                               
 Comment : [18726] Mise à jour du template "GetMaturityRule_EUREX" 
                    pour gérer les MR des DC sur Options Equity 
 =============================================================================================
 Version : v3.2.0.0                                              
 Date    : 20130213                                          
 Author  : BD                                                 
 Comment : Création 
=============================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="MarketData_Common_SQL.xsl"/>

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
      <xsl:apply-templates select="pms"/>
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

      <xsl:apply-templates select="row" />
    </file>
  </xsl:template>

  <!-- ===================================================================== -->
  <!-- GetCategory_EUREX - return Category from an EUREX Derivative Contract -->
  <!-- ===================================================================== -->
  <!-- PL 20230724 Deprecated with newness PRODUCT_TYPE_FLAG -->
  <xsl:template name="GetCategory_EUREX_UNUSED">
    <xsl:param name="pProductType"/>

    <!-- Affectation de Category sur l'EUREX sur la base de la matrice suivante :
        
         pProductType                                   Category retournée
         ======================================================================== 
         FBND, FCRD, FENE, FINT,                        'F'
         FINX, FSTK ou FVOL
         
         OFBD, OFEN, OFIT, OFIX,                        'O'
         OINX, OSTK ou OVOL -->

    <xsl:choose>
      <xsl:when test="$pProductType='FBND' or
                      $pProductType='FCRD' or
                      $pProductType='FENE' or
                      $pProductType='FINT' or
                      $pProductType='FINX' or
                      $pProductType='FSTK' or
                      $pProductType='FVOL'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <xsl:when test="$pProductType='OFBD' or
                      $pProductType='OFEN' or
                      $pProductType='OFIT' or
                      $pProductType='OFIX' or
                      $pProductType='OINX' or
                      $pProductType='OVOL' or
                      $pProductType='OSTK'">
        <xsl:value-of select="'O'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ============================================================================= -->
  <!-- GetSettltMethod_EUREX - return SettltMethod from an EUREX Derivative Contract -->
  <!-- ============================================================================= -->
  <!-- PL 20230724 Deprecated with newness PRODUCT_TYPE_FLAG -->
  <xsl:template name="GetSettltMethod_EUREX_UNUSED">
    <xsl:param name="pProductType"/>
    <xsl:param name="pCountryCode"/>

    <!-- Affectation de SettltMethod sur l'EUREX sur la base de la matrice suivante :
     
         pProductType       pCountryCode        SettltMethod retournée
         ===============================================================
         FINX, OINX                             Cash Settlement
         OFIT                                   Physical
         FINT                                   Cash Settlement
         FSTK               != 'ES'             Cash Settlement
         FSTK                = 'ES'             Physical
         OSTK                                   Physical
         FVOL, OVOL                             Cash Settlement
         FBND, OFBD                             Physical
         FCRD                                   Cash Settlement           
         Valeur par défaut                      Cash Settlement -->

    <!-- Valeurs possibles de SettltMethod :
            Cash Settlement : 'C'
            Physical : 'P' -->

    <xsl:choose>
      <!-- pProductType = FINX ou OINX -->
      <xsl:when test="$pProductType='FINX' or
                      $pProductType='OINX'">
        <xsl:value-of select="'C'"/>
      </xsl:when>

      <!-- pProductType = OFIT -->
      <xsl:when test="$pProductType='OFIT'">
        <!-- FL[19880] 20140423 -->
        <!--<xsl:value-of select="'C'"/>-->
        <xsl:value-of select="'P'"/>
      </xsl:when>

      <!-- pProductType = FINT -->
      <xsl:when test="$pProductType='FINT'">
        <xsl:value-of select="'C'"/>
      </xsl:when>

      <!-- pProductType = FSTK -->
      <xsl:when test="$pProductType='FSTK'">
        <xsl:choose>
          <!-- pCountryCode = ES -->
          <xsl:when test="$pCountryCode='ES'">
            <xsl:value-of select="'P'"/>
          </xsl:when>
          <!-- Autres cas... -->
          <xsl:otherwise>
            <xsl:value-of select="'C'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- pProductType = OSTK -->
      <xsl:when test="$pProductType='OSTK'">
        <xsl:value-of select="'P'"/>
      </xsl:when>

      <!-- pProductType = FVOL ou OVOL -->
      <xsl:when test="$pProductType='FVOL' or
                      $pProductType='OVOL'">
        <xsl:value-of select="'C'"/>
      </xsl:when>

      <!-- pProductType = FBND ou OFBD -->
      <xsl:when test="$pProductType='FBND' or
                      $pProductType='OFBD'">
        <xsl:value-of select="'P'"/>
      </xsl:when>

      <!-- pProductType = FCRD -->
      <xsl:when test="$pProductType='FCRD'">
        <xsl:value-of select="'C'"/>
      </xsl:when>

      <!-- Valeur par défaut -->
      <xsl:otherwise>
        <xsl:value-of select="'C'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- =============================================================================== -->
  <!-- GetExerciseStyle_EUREX - return ExerciseStyle from an EUREX Derivative Contract -->
  <!-- =============================================================================== -->
  <!-- PL 20230724 Deprecated with newness PRODUCT_TYPE_FLAG -->
  <xsl:template name="GetExerciseStyle_EUREX_UNUSED">
    <xsl:param name="pProductType"/>
    <xsl:param name="pGroupID"/>
    <xsl:param name="pCountryCode"/>

    <!-- Affectation de ExerciseStyle sur l'EUREX sur la base de la matrice suivante :
        
         pProductType       pGroupID        pCountryCode        ExerciseStyle retournée
         =============================================================================== 
           OSTK                               = 'RU'            European
           
           OSTK             = DE14, CH14  
                              FI14, FR14                        European
                              ou NL14
                              
           OSTK             != DE14, CH14   != 'RU'             American
           OSTK                FI14, FR14   != 'RU'             
                               ou NL14
                               
           OINX                                                 European
           OFBD                                                 American
           OFIT                                                 American
           OVOL                                                 European
           OFEN                                                 European
           OFIX                                                 European
           Valeur par défaut pour une option                    American 
           FBND, FCRD, FENE, 
           FINT, FINX, FSTK                                     null
           ou FVOL                                                                    -->

    <!-- Valeurs possibles de ExerciseStyle :
            European : 0
            American : 1 -->

    <xsl:choose>
      <!-- pProductType = OSTK -->
      <xsl:when test="$pProductType='OSTK'">
        <xsl:choose>
          <!-- pCountryCode = RU  -->
          <xsl:when test="$pCountryCode='RU'">
            <xsl:value-of select="0"/>
          </xsl:when>
          <!-- pGroupID = DE14, CH14, FI14, FR14 ou NL14 -->
          <xsl:when test="$pGroupID='DE14' or
                          $pGroupID='CH14' or
                          $pGroupID='FI14' or
                          $pGroupID='FR14' or
                          $pGroupID='NL14'">
            <xsl:value-of select="0"/>
          </xsl:when>
          <!-- pCountryCode != 'RU' et (pGroupID != DE14, CH14, FI14, FR14 ou NL14) -->
          <xsl:when test="$pCountryCode!='RU' and (
                          $pGroupID='DE14' or
                          $pGroupID='CH14' or
                          $pGroupID='FI14' or
                          $pGroupID='FR14' or
                          $pGroupID='NL14')">
            <xsl:value-of select="1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- pProductType = OINX -->
      <xsl:when test="$pProductType='OINX'">
        <xsl:value-of select="0"/>
      </xsl:when>

      <!-- pProductType = OFBD -->
      <xsl:when test="$pProductType='OFBD'">
        <xsl:value-of select="1"/>
      </xsl:when>

      <!-- pProductType = OFIT -->
      <xsl:when test="$pProductType='OFIT'">
        <xsl:value-of select="1"/>
      </xsl:when>

      <!-- pProductType = OVOL -->
      <xsl:when test="$pProductType='OVOL'">
        <xsl:value-of select="0"/>
      </xsl:when>

      <!-- pProductType = OFEN -->
      <xsl:when test="$pProductType='OFEN'">
        <xsl:value-of select="0"/>
      </xsl:when>

      <!-- pProductType = OFIX -->
      <xsl:when test="$pProductType='OFIX'">
        <xsl:value-of select="0"/>
      </xsl:when>

      <!-- Autres futures -->
      <xsl:when test="$pProductType='FBND' or 
                      $pProductType='FCRD' or 
                      $pProductType='FENE' or
                      $pProductType='FINT' or
                      $pProductType='FINX' or
                      $pProductType='FSTK' or
                      $pProductType='FVOL'">
        <xsl:value-of select="null"/>
      </xsl:when>

      <!-- Autres cas... -->
      <xsl:otherwise>
        <xsl:value-of select="1"/>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!-- ========================================================================================= -->
  <!-- GetFutValuationMethod_EUREX - return FutValuationMethod from an EUREX Derivative Contract -->
  <!-- ========================================================================================= -->
  <xsl:template name="GetFutValuationMethod_EUREX">
    <xsl:param name="pProductType"/>

    <!-- Affectation de FutValuationMethod sur l'EUREX sur la base de la matrice suivante :
     
       pProductType                           FutValuationMethod retournée
      ====================================================================
        OFBD                                  'FUT'
        OFIT                                  'FUT'
        OSTK                                  'EQTY' 
        OINX                                  'EQTY' 
        OVOL                                  'EQTY' 
        OFEN                                  'EQTY'
        OFIX                                  'EQTY'
        FBND                                  'FUT'
        FCRD                                  'FUT'
        FINT                                  'FUT'
        FINX                                  'FUT'
        FSTK                                  'FUT'
        FVOL                                  'FUT'
        Valeur par défaut                     'FUT'   -->

    <xsl:choose>
      <!-- pProductType = OFBD ou OFIT -->
      <xsl:when test="$pProductType='OFBD' or
                      $pProductType='OFIT'">
        <xsl:value-of select="'FUT'"/>
      </xsl:when>

      <!-- pProductType = OSTK, OINX, OVOL, OFEN ou OFIX -->
      <xsl:when test="$pProductType='OSTK' or
                      $pProductType='OINX' or
                      $pProductType='OVOL' or
                      $pProductType='OFEN' or
                      $pProductType='OFIX'">
        <xsl:value-of select="'EQTY'"/>
      </xsl:when>

      <!-- pProductType = FBND, FCRD, FINT, FINX, FSTK ou FVOL -->
      <xsl:when test="$pProductType='FBND' or
                      $pProductType='FCRD' or
                      $pProductType='FINT' or
                      $pProductType='FINX' or
                      $pProductType='FSTK' or
                      $pProductType='FVOL'">
        <xsl:value-of select="'FUT'"/>
      </xsl:when>

      <!-- Autres cas... -->
      <xsl:otherwise>
        <xsl:value-of select="'FUT'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- =============================================================================== -->
  <!-- GetAssetCategory_EUREX - return AssetCategory from an EUREX Derivative Contract -->
  <!-- =============================================================================== -->
  <xsl:template name="GetAssetCategory_EUREX">
    <xsl:param name="pProductID"/>
    <xsl:param name="pProductType"/>
    <xsl:param name="pProductGroup"/>
    <xsl:param name="pIsShareISINExist"/> <!-- PL 20231009 Refactoring, param unused  -->

    <!-- Affectation de AssetCategory sur l'EUREX sur la base de la matrice suivante :
     
       pProductType   pProductGroup         pIsShareISINExist   AssetCategory retournée
      ================================================================================
         FBND         =  Euro Swap Futures                      Bond
         FBND         != Euro Swap Futures                      RateIndex
         FCRD                                                   Commodity
         FINT                                                   RateIndex
         FINX                                                   Index
         FSTK                                                   Equity Asset
         FSTK                               true                Equity Asset
         FVOL                                                   Index
         OVOL                                                   Index
         OFBD                                                   Future
         OFIT                                                   Future
         OINX                                                   Index
         OSTK                                                   Equity Asset
         
         Règle en dur pour ces deux produits (ProductType='OFIX') :
          - ProductID 'OEXD'  -> Future
          - ProductID 'OVS'   -> Index -->

    <xsl:choose>
      <!-- pProductType = FBND -->
      <xsl:when test="$pProductType='FBND'">
        <xsl:choose>
          <xsl:when test="$pProductGroup='Euro Swap Futures'">
            <!-- Euro Swap Futures =>  RateIndex -->
            <xsl:value-of select="'RateIndex'"/>
          </xsl:when>
          <!-- Autres cas... -->
          <xsl:otherwise>
            <xsl:value-of select="'Bond'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- pProductType = FCRD -->
      <xsl:when test="$pProductType='FCRD'">
        <xsl:value-of select="'Commodity'"/>
      </xsl:when>

      <!-- pProductType = FINT -->
      <xsl:when test="$pProductType='FINT'">
        <xsl:value-of select="'RateIndex'"/>
      </xsl:when>

      <!-- pProductType = FINX -->
      <xsl:when test="$pProductType='FINX'">
        <xsl:value-of select="'Index'"/>
      </xsl:when>

      <!-- pProductType = FSTK -->
      <xsl:when test="$pProductType='FSTK'">
        <!-- PL 20231009 Refactoring -->
        <xsl:value-of select="'EquityAsset'"/>
        <!-- 
        <xsl:choose>
          Comment: Share ISIN renseigné 
          <xsl:when test="pIsShareISINExist='true'">
            Comment: Equity Assset sur dividende  
            <xsl:value-of select="'EquityAsset'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'EquityAsset'"/>
          </xsl:otherwise>
        </xsl:choose>
        -->
      </xsl:when>

      <!-- pProductType = FVOL ou OVOL -->
      <xsl:when test="$pProductType='FVOL' or $pProductType='OVOL'">
        <xsl:value-of select="'Index'"/>
      </xsl:when>

      <!-- pProductType = OFBD -->
      <xsl:when test="$pProductType='OFBD'">
        <xsl:value-of select="'Future'"/>
      </xsl:when>

      <!-- pProductType = OFIT -->
      <xsl:when test="$pProductType='OFIT'">
        <xsl:value-of select="'Future'"/>
      </xsl:when>

      <!-- pProductType = OINX -->
      <xsl:when test="$pProductType='OINX'">
        <xsl:value-of select="'Index'"/>
      </xsl:when>

      <!-- pProductType = OSTK -->
      <xsl:when test="$pProductType='OSTK'">
        <xsl:value-of select="'EquityAsset'"/>
      </xsl:when>

      <!-- FL & BD 20130705 : Gestion des deux produits Eurex qui ont comme Product Type OFIX -->
      <!-- pProductType = OFIX -->
      <xsl:when test="$pProductType='OFIX'">
        <xsl:choose>
          <!-- pProductID = OEXD -->
          <xsl:when test="$pProductID = 'OEXD'">
            <xsl:value-of select="'Future'"/>
          </xsl:when>
          <!-- pProductID = OVS -->
          <xsl:when test="$pProductID = 'OVS'">
            <xsl:value-of select="'Index'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- FL 20140220 [19648] Création Template : GetUnderlyingGroup_EUREX                    -->
  <!-- =================================================================================== -->
  <!-- GetUnderlyingGroup_EUREX - return UnderlyingGroup from an EUREX Derivative Contract -->
  <!-- =================================================================================== -->
  <xsl:template name="GetUnderlyingGroup_EUREX">
    <xsl:param name="pProductType"/>

    <!-- Affectation de UnderlyingGroup sur l'EUREX sur la base de la matrice suivante :
     
       pProductType                         AssetCategory retournée
      ================================================================================
         FENE, OFEN                         C (Commodities)
         Autre que FENE                     F (Financial)                                 -->

    <xsl:choose>

      <!-- pProductType = FENE, OFEN -->
      <xsl:when test="$pProductType='FENE' or $pProductType='OFEN'">
        <xsl:value-of select="'C'"/>
      </xsl:when>

      <!-- pProductType autre que FENE -->
      <xsl:otherwise>
        <xsl:value-of select="'F'"/>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!-- FL 20140220 [19648] Création Template : GetUnderlyingAsset_EUREX                    -->
  <!-- =================================================================================== -->
  <!-- GetUnderlyingAsset_EUREX - return UnderlyingAsset from an EUREX Derivative Contract -->
  <!-- =================================================================================== -->
  <xsl:template name="GetUnderlyingAsset_EUREX">
    <xsl:param name="pProductType"/>
    <xsl:param name="pProductID"/>

    <!-- Affectation de UnderlyingGroup sur l'EUREX sur la base de la matrice suivante :
     
       pProductType              ProductID                   AssetCategory retournée
      ==================================================================================================
       FBND, FINT,                                           FD (Interest rate/notional debt securities)
       OFBD, OFIT                                         
       FINX, FCRD, FVOL,                                     FI (Indices)
       OINX, OFIX                
       FSTK, OSTK                                            FS (Stock-Equities)
       FENE, OFEN                                            M  (Other) 
       
       Règle en dur pour les produits ID suivant car pour l'instant ceci ne sont pas dans le fichier
       (Productlist.YYYYMMDD.csv) => Je ne sais pas quel ProductType il auront d'attribué.
       pProductType              ProductID                   AssetCategory retournée
      ==================================================================================================
                                 FCEU, FCEF, FCEP,           FC (Currencies)
                                 FCPU, FCPF, FCUF
                                 OCEU, OCEF, OCEP,
                                 OCPU, OCPF, OCUF                                                        -->

    <xsl:choose>

      <!-- pProductID = FCEU, FCEF, FCEP, FCPU, FCPF, FCUF, OCEU, OCEF, OCEP, OCPU, OCPF, OCUF => FC (Currencies) -->
      <xsl:when test="$pProductID='FCEU' or  $pProductID='FCEF' or $pProductID='FCEP' or
                      $pProductID='FCPU' or  $pProductID='FCPF' or $pProductID='FCUF' or
                      $pProductID='OCEU' or  $pProductID='OCEF' or $pProductID='OCEP' or
                      $pProductID='OCPU' or  $pProductID='OCPF' or $pProductID='OCUF'">
        <xsl:value-of select="'FC'"/>
      </xsl:when>

      <!-- pProductType = FBND, FINT, OFBD, OFIT => FD (Interest rate/notional debt securities) -->
      <xsl:when test="$pProductType='FBND' or $pProductType='FINT' or
                      $pProductType='OFBD' or $pProductType='OFIT'">
        <xsl:value-of select="'FD'"/>
      </xsl:when>

      <!-- pProductType = FINX, FCRD, FVOL, OINX, OFIX => FI (Indices) -->
      <xsl:when test="$pProductType='FINX' or $pProductType='FCRD' or
                      $pProductType='FVOL' or $pProductType='OINX' or
                      $pProductType='OFIX'">
        <xsl:value-of select="'FI'"/>
      </xsl:when>

      <!-- pProductType = FSTK, OSTK  => FS (Stock-Equities)-->
      <xsl:when test="$pProductType='FSTK' or $pProductType='OSTK'">
        <xsl:value-of select="'FS'"/>
      </xsl:when>

      <!-- pProductType = FENE, OFEN => M  (Other) -->
      <xsl:when test="$pProductType='FENE' or $pProductType='OFEN'">
        <xsl:value-of select="'M'"/>
      </xsl:when>

      <!-- otherwise => M  (Other) -->
      <xsl:otherwise>
        <xsl:value-of select="'M'"/>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!-- ======================================================================================== -->
  <!-- GetMaturityRule_EUREX - return MaturityRule Identifier from an EUREX Derivative Contract -->
  <!-- ======================================================================================== -->
  <xsl:template name="GetMaturityRule_EUREX">
    <xsl:param name="pProductType"/>
    <xsl:param name="pProductGroup"/>
    <xsl:param name="pCountryCode"/>
    <xsl:param name="pGroupId"/>
    <xsl:param name="pContractSymbol"/>

    <!-- 
       Affectation des règles d'échéances sur l'EUREX sur la base de la matrice suivante:
  
           pProductType       pProductGroup       pCountryCode        pGroupId        pContractSymbol       MaturityRuleIdentifier
           =======================================================================================================================
           OSTK               SINGLE STOCK OPTIONS      !(IT,GB,FI,IE,DE)                                   XEUR Equity Options Standard
           OSTK               SINGLE STOCK OPTIONS      DE            !DE13                                 XEUR Equity Options German
           OSTK               SINGLE STOCK OPTIONS      IT                                                  XEUR Equity Options Italian
           OSTK               SINGLE STOCK OPTIONS      GB,FI,IE      !FI13                                 XEUR Equity Options British,Finnish,Irish
           OSTK               SINGLE STOCK OPTIONS                    FI13            ends-with(1)          XEUR Equity Options 1st Weekly Finnish
           OSTK               SINGLE STOCK OPTIONS                    FI13            ends-with(2)          XEUR Equity Options 2nd Weekly Finnish
           OSTK               SINGLE STOCK OPTIONS                    FI13            ends-with(4)          XEUR Equity Options 4th Weekly Finnish
           OSTK               SINGLE STOCK OPTIONS                    FI13            ends-with(5)          XEUR Equity Options 5th Weekly Finnish
           OSTK               SINGLE STOCK OPTIONS                    DE13            ends-with(1)          XEUR Equity Options 1st Weekly German
           OSTK               SINGLE STOCK OPTIONS                    DE13            ends-with(2)          XEUR Equity Options 2nd Weekly German
           OSTK               SINGLE STOCK OPTIONS                    DE13            ends-with(4)          XEUR Equity Options 4th Weekly German
           OSTK               SINGLE STOCK OPTIONS                    DE13            ends-with(5)          XEUR Equity Options 5th Weekly German
           FSTK               SINGLE STOCK FUTURES                                                          XEUR Equity Futures Standard
       
Deprecated OINX                                                                       ODX1, OES1            XEUR Index Options 1st Weekly
Deprecated OINX                                                                       ODX2, OES2            XEUR Index Options 2nd Weekly
Deprecated OINX                                                                       ODX4, OES4            XEUR Index Options 4th Weekly
Deprecated OINX                                                                       ODX5, OES5            XEUR Index Options 5th Weekly
          
           OINX                                                                       ODAX, OESX            XEUR Index Options Standard
           OINX               INDEX OPTIONS                                                                 XEUR Index Options Standard
           OINX                                                                       OSLI, OSLI, OSMM      XEUR Index Options SMI, SLI Derivative Contracts 
       
           FINX               INDEX FUTURES                                                                 XEUR Index Futures Standard
                            
           FBND                                                                                             XEUR Bond Futures
           OFBD                                                                                             XEUR Bond Options
           FVOL                                              							                                  XEUR Volatility Futures & Options
           OFIX                                                                       OVS                   XEUR Volatility Futures & Options
              
       Valeur par défaut: Règle d'échéance intitulée "Default Rule" 
       
       '!(x,y)' signifie différent de x et de y
       'ends-with(x)' signifie se terminant par x 
    -->

    <!-- Choix de l'Identifier de la Maturity Rule en fonction des paramètres -->
    <xsl:variable name="vMaturityRuleIdentifier">

      <!-- pProductType -->
      <xsl:choose>
        <!-- pProductType = 'OSTK' -->
        <xsl:when test="$pProductType = 'OSTK'">
          <!-- pProductGroup -->
          <xsl:choose>
            <!-- pProductGroup = 'SINGLE STOCK OPTIONS' (anciennement 'Equity Options') -->
            <xsl:when test="$pProductGroup = 'SINGLE STOCK OPTIONS'">
              <!-- pGroupId -->
              <xsl:choose>
                <!-- pGroupId = 'FI13' (PL 20230724 Ne semble plus exister) -->
                <xsl:when test="$pGroupId = 'FI13'">
                  <xsl:choose>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '1'">XEUR Equity Options 1st Weekly Finnish</xsl:when>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '2'">XEUR Equity Options 2nd Weekly Finnish</xsl:when>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '4'">XEUR Equity Options 4th Weekly Finnish</xsl:when>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '5'">XEUR Equity Options 5th Weekly Finnish</xsl:when>
                  </xsl:choose>
                </xsl:when>
                <!-- pGroupId = 'DE13' (PL 20230724 Ne semble plus exister) -->
                <xsl:when test="$pGroupId = 'DE13'">
                  <xsl:choose>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '1'">XEUR Equity Options 1st Weekly German</xsl:when>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '2'">XEUR Equity Options 2nd Weekly German</xsl:when>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '4'">XEUR Equity Options 4th Weekly German</xsl:when>
                    <xsl:when test="substring($pContractSymbol, string-length($pContractSymbol)) = '5'">XEUR Equity Options 5th Weekly German</xsl:when>
                  </xsl:choose>
                </xsl:when>

                <!-- Autres pGroupId -->
                <xsl:otherwise>
                  <!-- pCountryCode -->
                  <xsl:choose>
                    <!-- pCountryCode != (IT,GB,FI,IE,DE) -->
                    <xsl:when test="$pCountryCode != 'IT' and
                                    $pCountryCode != 'GB' and
                                    $pCountryCode != 'FI' and
                                    $pCountryCode != 'IE' and
                                    $pCountryCode != 'DE'">XEUR Equity Options Standard</xsl:when>
                    <!-- pCountryCode = 'DE' -->
                    <xsl:when test="$pGroupId != 'DE13' and
                                    $pCountryCode = 'DE'">XEUR Equity Options German</xsl:when>
                    <!-- pCountryCode = 'IT' -->
                    <xsl:when test="$pCountryCode = 'IT'">XEUR Equity Options Italian</xsl:when>
                    <!-- pCountryCode = (GB,FI,IE) -->
                    <xsl:when test="$pGroupId != 'FI13' and
                                    ($pCountryCode = 'GB' or
                                     $pCountryCode = 'FI' or
                                     $pCountryCode = 'IE')">XEUR Equity Options British,Finnish,Irish</xsl:when>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>

        <!-- pProductType = 'FSTK' -->
        <xsl:when test="$pProductType = 'FSTK'">
          <!-- pProductGroup -->
          <xsl:choose>
            <!-- pProductGroup = 'SINGLE STOCK FUTURES' (anciennement 'Single Stock Futures') -->
            <xsl:when test="$pProductGroup = 'SINGLE STOCK FUTURES'">XEUR Equity Futures Standard</xsl:when>
          </xsl:choose>
        </xsl:when>

        <!-- pProductType = 'FINX' -->
        <xsl:when test="$pProductType = 'FINX'">
          <!-- pProductGroup -->
          <xsl:choose>
            <!-- pProductGroup = 'INDEX FUTURES' (anciennement'DAX®','Blue Chip','EURO STOXX® Sector Index Futures','SMI®','Broadbased / Size Indexes','STOXX® Europe 600 Sector Index Futures') -->
            <xsl:when test="$pProductGroup = 'INDEX FUTURES'">XEUR Index Futures Standard</xsl:when>
          </xsl:choose>
        </xsl:when>

        <!-- pProductType = 'FBND' -->
        <xsl:when test="$pProductType = 'FBND'">XEUR Bond Futures</xsl:when>

        <!-- pProductType = 'FVOL' -->
        <xsl:when test="$pProductType = 'FVOL'">XEUR Volatility Futures &amp; Options</xsl:when>

        <!-- pProductType = 'OFBD' -->
        <xsl:when test="$pProductType = 'OFBD'">XEUR Bond Options</xsl:when>

        <!-- pProductType = 'OINX' -->
        <xsl:when test="$pProductType = 'OINX'">
          <xsl:choose>
            <!-- 
            <xsl:when test="$pContractSymbol = 'ODX1' or
                            $pContractSymbol = 'OES1'">XEUR Index Options 1st Weekly</xsl:when>
            <xsl:when test="$pContractSymbol = 'ODX2' or
                            $pContractSymbol = 'OES2'">XEUR Index Options 2nd Weekly</xsl:when>
            <xsl:when test="$pContractSymbol = 'ODX4' or
                            $pContractSymbol = 'OES4'">XEUR Index Options 4th Weekly</xsl:when>
            <xsl:when test="$pContractSymbol = 'ODX5' or
                            $pContractSymbol = 'OES5'">XEUR Index Options 5th Weekly</xsl:when>
            -->
            <!-- pContractSymbol = (ODAX, OESX) -->
            <!-- Inclus maintenant dans pProductGroup = 'INDEX OPTIONS' 
            <xsl:when test="$pContractSymbol = 'ODAX' or
                            $pContractSymbol = 'OESX'">XEUR Index Options Standard</xsl:when>
            -->
            
            <!-- pProductGroup = ('SMI®', 'SLI Swiss Leader Index®') -->
            <!--
            <xsl:when test="$pProductGroup = 'SMI®' or 
                            $pProductGroup = 'SLI Swiss Leader Index®'">XEUR Index Options SMI, SLI Contracts</xsl:when>
            -->
            <xsl:when test="$pContractSymbol = 'OSLI' or 
                            $pContractSymbol = 'OSLI' or 
                            $pContractSymbol = 'OSMM'">XEUR Index Options SMI, SLI Contracts</xsl:when>

            <!-- pProductGroup = 'INDEX OPTIONS' (anciennement 'EURO STOXX® Sector Index Options') -->
            <xsl:when test="$pProductGroup = 'INDEX OPTIONS'">XEUR Index Options Standard</xsl:when>
          </xsl:choose>
        </xsl:when>

        <!-- pProductType = 'OFIX' -->
        <xsl:when test="$pProductType = 'OFIX'">
          <xsl:choose>
            <!-- pContractSymbol = (OVS) -->
            <xsl:when test="$pContractSymbol = 'OVS'">XEUR Volatility Futures &amp; Options</xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    
    <!-- EG 20130717 Pamramètre SQL en majuscule -->
    <SQL command="select" result="IDMATURITYRULE">
      select IDMATURITYRULE
      from dbo.MATURITYRULE
      WHERE IDENTIFIER=@PMATURITYRULEIDENTIFIER
      <Param name="PMATURITYRULEIDENTIFIER" datatype="string">
        <xsl:choose>
          <xsl:when test="string-length($vMaturityRuleIdentifier) > 0">
            <xsl:value-of select="$vMaturityRuleIdentifier"/>
          </xsl:when>
          <xsl:otherwise>Default Rule</xsl:otherwise>
        </xsl:choose>
      </Param>
    </SQL>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <row>                                       -->
  <!-- ================================================== -->
  <xsl:template match="row">
    <row>

      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>

      <!-- - Variables - -->
      <!-- Contract Symbol / Product ID -->
      <xsl:variable name="vProductID">
        <xsl:value-of select="data[@name='PID']"/>
      </xsl:variable>
      <!-- Product Type -->
      <xsl:variable name="vProductType">
        <xsl:value-of select="data[@name='PType']"/>
      </xsl:variable>
      <!-- Product Type Flag -->
      <xsl:variable name="vProductTypeFlag">
        <xsl:value-of select="data[@name='PTypeFlag']"/>
      </xsl:variable>      
      <!-- Product Name (=Description) -->
      <xsl:variable name="vProductName">
        <xsl:value-of select="data[@name='Desc']"/>
      </xsl:variable>
      <!-- Product Group -->
      <xsl:variable name="vProductGroup">
        <xsl:value-of select="data[@name='PGroup']"/>
      </xsl:variable>
      <!-- IDC -->
      <xsl:variable name="vIDC">
        <xsl:value-of select="data[@name='IDC']"/>
      </xsl:variable>
      <!-- Group ID -->
      <xsl:variable name="vGroupID">
        <xsl:value-of select="data[@name='Group']"/>
      </xsl:variable>
      <!-- ID Country / Country Code -->
      <xsl:variable name="vCountryCode">
        <xsl:value-of select="data[@name='Country']"/>
      </xsl:variable>
      <!-- Underlying Isin -->
      <xsl:variable name="vUnderlyingISIN">
        <xsl:value-of select="data[@name='UISIN']"/>
      </xsl:variable>
      <!-- Underlying BBG -->
      <xsl:variable name="vUnderlyingBBG">
        <xsl:value-of select="data[@name='BBGUCode']"/>
      </xsl:variable>
      <!-- Underlying RIC -->
      <xsl:variable name="vUnderlyingRIC">
        <xsl:value-of select="data[@name='RICUCode']"/>
      </xsl:variable>
      <!-- Share ISIN renseigné ou non -->
      <xsl:variable name="vIsShareISINExist">
        <xsl:choose>
          <xsl:when test="string-length(data[@name='SISIN'])>0">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Underlying ISIN renseigné ou non -->
      <xsl:variable name="vIsUnderlyingISINExist">
        <xsl:choose>
          <xsl:when test="string-length(data[@name='UISIN'])>0">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Product Name pour ASSET_EQUITY et ASSET_INDEX -->
      <!-- Arrangement du Product Name -->
      <xsl:variable name="vAssetProductName">
        <xsl:choose>
          <xsl:when test="contains($vProductName,'Futures')">
            <xsl:value-of select="normalize-space(substring-before($vProductName,'Futures'))"/>
          </xsl:when>
          <xsl:when test="contains($vProductName,'Options')">
            <xsl:value-of select="normalize-space(substring-before($vProductName,'Options'))"/>
          </xsl:when>
          <xsl:when test="contains($vProductName,'[')">
            <xsl:value-of select="normalize-space(substring-before($vProductName,'['))"/>
          </xsl:when>
          <xsl:when test="contains($vProductName,'(')">
            <xsl:value-of select="normalize-space(substring-before($vProductName,'('))"/>
          </xsl:when>
          <xsl:when test="contains($vProductName,',') and contains($vProductName,'Weekly')">
            <xsl:value-of select="normalize-space(substring-before($vProductName,','))"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="normalize-space($vProductName)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- BD 20130429 On utilise le code ISO pour le marché du sous-jacents -->
      <!-- ISO10383 -->

      <!-- RD/FL 20160401 [21994] Fichier de la chambre incorrect -> Remplacer 'XLSE' par 'XLON' -->
      <!--<xsl:variable name="vISO10383">
        <xsl:choose>
          <xsl:when test="string-length(data[@name='IDM']) > 0">
            <xsl:value-of select="data[@name='IDM']"/>
          </xsl:when>
          <xsl:otherwise>XEUR</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>-->
      <xsl:variable name="vIDM" select="data[@name='IDM']"/>
      <xsl:variable name="vISO10383">
        <xsl:choose>
          <xsl:when test="string-length($vIDM) > 0">
            <xsl:choose>
              <xsl:when test="$vIDM='XLSE'">
                <xsl:value-of select="'XLON'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vIDM"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>XEUR</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vAssetCategory">
        <xsl:call-template name="GetAssetCategory_EUREX">
          <xsl:with-param name="pProductID" select="$vProductID"/>
          <xsl:with-param name="pProductType" select="$vProductType"/>
          <xsl:with-param name="pProductGroup" select="$vProductGroup"/>
          <xsl:with-param name="pIsShareISINExist" select="$vIsShareISINExist"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- PL 20231009 variable unused  -->
      <!--
      <xsl:variable name="pAssetDisplayName">
        <xsl:choose>
          Comment: Quand le libellé fait plus de 37 car., on le tronc et rajoute '...'  
          <xsl:when test="string-length($vAssetProductName) > 37">
            <xsl:value-of select="concat($vUnderlyingISIN,' ',substring($vAssetProductName,0,37),'... - ',$vISO10383)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat($vUnderlyingISIN,' ',$vAssetProductName,' - ',$vISO10383)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      -->

      <!-- Création des EQUITY -->
      <xsl:if test="$vIsUnderlyingISINExist='true' and $vAssetCategory='EquityAsset'">
        <!-- Insert dans la table ASSET_EQUITY uniquement dans le cas d'un Product Type = OSTK ou FSTK et UnderlyingISIN de renseigné  -->
        <xsl:call-template name="SQLTableASSET_EQUITY">
          <xsl:with-param name="pIsincode" select="$vUnderlyingISIN"/>
          <xsl:with-param name="pIdc" select="$vIDC"/>
          <xsl:with-param name="pRICCode" select="$vUnderlyingRIC"/>
          <xsl:with-param name="pBBGCode" select="$vUnderlyingBBG"/>
          <xsl:with-param name="pIso10383" select="$vISO10383"/>
          <xsl:with-param name="pIso10383Related" select="'XEUR'"/>
          <xsl:with-param name="pAssetTitled" select="$vAssetProductName"/>
        </xsl:call-template>
      </xsl:if>

      <!-- Création des INDEX -->
      <xsl:if test="$vIsUnderlyingISINExist='true' and $vAssetCategory='Index'">
        <!-- Insert dans la table ASSET_INDEX uniquement dans le cas d'un Product Type = FINX, OINX, FVOL ou OVOL et UnderlyingISIN de renseigné  -->
        <xsl:call-template name="SQLTableASSET_INDEX">
          <xsl:with-param name="pIsincode" select="$vUnderlyingISIN"/>
          <xsl:with-param name="pIdc" select="$vIDC"/>
          <xsl:with-param name="pRICCode" select="$vUnderlyingRIC"/>
          <xsl:with-param name="pBBGCode" select="$vUnderlyingBBG"/>
          <xsl:with-param name="pIso10383" select="$vISO10383"/>
          <xsl:with-param name="pIso10383Related" select="'XEUR'"/>
          <xsl:with-param name="pAssetTitled" select="$vAssetProductName"/>
        </xsl:call-template>
      </xsl:if>

      <!-- Insert/Update dans la Table EUREXPRODUCT -->
      <table name="EUREXPRODUCT" action="IU">
        <!-- CONTRACTSYMBOL -->
        <column name="CONTRACTSYMBOL" datakey="true">
          <xsl:value-of select="$vProductID"/>
          <controls>
            <control action="RejectRow" return="true" >
              <SpheresLib function="IsNull()" />
              <logInfo status="INFO" isexception="false">
                <message>Contract Symbol (Product ID) missing.</message>
              </logInfo>
            </control>
          </controls>
        </column>

        <!-- PRODUCTTYPE -->
        <column name="PRODUCTTYPE" datakeyupd="true">
          <xsl:value-of select="$vProductType"/>
        </column>

        <!-- PRODUCT_TYPE_FLAG (Newness 2023) -->
        <column name="PRODUCT_TYPE_FLAG" datakeyupd="true">
          <xsl:value-of select="$vProductTypeFlag"/>
          <controls>
            <control action="RejectRow" return="true" >
              <SpheresLib function="IsNull()" />
              <logInfo status="INFO" isexception="false">
                <message>Category (Product Type Flag) missing.</message>
              </logInfo>
            </control>
          </controls>
        </column>

        <!-- DESCRIPTION -->
        <column name="DESCRIPTION" datakeyupd="true">
          <xsl:value-of select="$vProductName"/>
        </column>
        
        <!-- PL 20230724 Column PRODUCTGROUP déplacée ici. Elle se trouvait étonnamment au dessus du tag <table> et était donc non traitée pour mise à jour de la table EUREXPRODUCT. -->
        <!-- PRODUCTGROUP -->
        <column name="PRODUCTGROUP" datakeyupd="true">
          <xsl:value-of select="$vProductGroup"/>
        </column>

        <!-- IDC -->
        <column name="IDC" datakeyupd="true">
          <xsl:value-of select="$vIDC"/>
        </column>

        <!-- PRODUCT_ISIN -->
        <!-- FL 20130221 : Si ISIN du DC == ISIN du sous-jacent -> PRODUCT_ISIN = null -->
        <column name="PRODUCT_ISIN" datakeyupd="true">
          <xsl:choose>
            <xsl:when test="data[@name='UISIN'] = data[@name='PISIN']">null</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="data[@name='PISIN']"/>
            </xsl:otherwise>
          </xsl:choose>
        </column>

        <!-- UNDERLYING_ISIN -->
        <column name="UNDERLYING_ISIN" datakeyupd="true">
          <xsl:value-of select="$vUnderlyingISIN"/>
        </column>

        <!-- SHARE_ISIN -->
        <column name="SHARE_ISIN" datakeyupd="true">
          <xsl:value-of select="data[@name='SISIN']"/>
        </column>

        <!-- BLOOMBERG_CODE (Newness 2023) -->
        <column name="BLOOMBERG_CODE" datakeyupd="true">
          <xsl:value-of select="data[@name='BBGCode']"/>
        </column>
        <column name="BLOOMBERG_UL_CODE" datakeyupd="true">
          <xsl:value-of select="data[@name='BBGUCode']"/>
        </column>
        <!-- REUTERS_CODE (Newness 2023) -->
        <column name="REUTERS_CODE" datakeyupd="true">
          <xsl:value-of select="data[@name='RICCode']"/>
        </column>
        <column name="REUTERS_UL_CODE" datakeyupd="true">
          <xsl:value-of select="data[@name='RICUCode']"/>
        </column>

        <!-- IDCOUNTRY -->
        <column name="IDCOUNTRY" datakeyupd="true">
          <xsl:value-of select="$vCountryCode"/>
        </column>

        <!-- GROUPID -->
        <column name="GROUPID" datakeyupd="true">
          <xsl:value-of select="$vGroupID"/>
        </column>

        <!-- CASHMARKETID -->
        <column name="CASHMARKETID" datakeyupd="true">
          <!-- RD/FL 20160401 [21994] Fichier de la chambre incorrect -> Remplacer 'XLSE' par 'XLON' -->
          <!--<xsl:value-of select="data[@name='IDM']"/>-->
          <xsl:choose>
            <xsl:when test="$vIDM='XLSE'">
              <xsl:value-of select="'XLON'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$vIDM"/>
            </xsl:otherwise>
          </xsl:choose>
        </column>

        <!-- SECTOR -->
        <column name="SECTOR" datakeyupd="true">
          <xsl:value-of select="data[@name='Sector']"/>
        </column>

        <!-- CATEGORY -->
        <column name="CATEGORY" datakeyupd="true">
          <!--
          <xsl:call-template name="GetCategory_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
          </xsl:call-template>
          -->
          <xsl:value-of select="$vProductTypeFlag"/>
        </column>

        <!-- SETTLTMETHOD -->
        <column name="SETTLTMETHOD" datakeyupd="true">
          <!--
          <xsl:call-template name="GetSettltMethod_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
            <xsl:with-param name="pCountryCode" select="$vCountryCode"/>
          </xsl:call-template>
          -->
          <xsl:value-of select="data[@name='StlType']"/>
        </column>

        <!-- EXERCISESTYLE -->
        <column name="EXERCISESTYLE" datakeyupd="true">
          <!--
          <xsl:call-template name="GetExerciseStyle_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
            <xsl:with-param name="pGroupID" select="$vGroupID"/>
            <xsl:with-param name="pCountryCode" select="$vCountryCode"/>
          </xsl:call-template>
          -->
          <xsl:choose>
            <xsl:when test="$vProductTypeFlag='O'">
              <xsl:choose>
                <xsl:when test="data[@name='ExeStyle']='E'">
                  <!-- European: 0 dans Spheres et E dans fichier -->
                  <xsl:value-of select="0"/>
                </xsl:when>
                <xsl:otherwise>
                  <!-- American: 1 dans Spheres et A dans fichier -->
                  <xsl:value-of select="1"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </column>

        <!-- FUTVALUATIONMETHOD -->
        <column name="FUTVALUATIONMETHOD" datakeyupd="true">
          <xsl:call-template name="GetFutValuationMethod_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
          </xsl:call-template>
        </column>

        <!-- ASSETCATEGORY -->
        <column name="ASSETCATEGORY" datakeyupd="true">
          <xsl:value-of select="$vAssetCategory"/>
        </column>

        <!-- FL 20140220 [19648] UNDERLYINGGROUP -->
        <column name="UNDERLYINGGROUP" datakeyupd="true">
          <xsl:call-template name="GetUnderlyingGroup_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
          </xsl:call-template>
        </column>

        <!-- FL 20140220 [19648] UNDERLYINGASSET -->
        <column name="UNDERLYINGASSET" datakeyupd="true">
          <xsl:call-template name="GetUnderlyingAsset_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
            <xsl:with-param name="pProductID" select="$vProductID"/>
          </xsl:call-template>
        </column>

        <!-- IDDC_UNL -->
        <!--<column name="IDDC_UNL" datakeyupd="true">
        </column>-->

        <!-- IDASSET_UNL -->
        <column name="IDASSET_UNL" datakeyupd="true" datatype="integer">
          <xsl:if test="$vAssetCategory = 'EquityAsset'">
            <SQL command="select" result="IDASSET" cache="true">
              select asset.IDASSET
              from dbo.ASSET_EQUITY asset
              inner join dbo.MARKET m on (m.IDM=asset.IDM) and (m.ISO10383_ALPHA4=@UNDERLYINGMARKET)
              where asset.ISINCODE=@ISINCODE
              <Param name="UNDERLYINGMARKET" datatype="string">
                <xsl:value-of select="$vISO10383"/>
              </Param>
              <Param name="ISINCODE" datatype="string">
                <xsl:value-of select="$vUnderlyingISIN"/>
              </Param>
            </SQL>
          </xsl:if>
          <xsl:if test="$vAssetCategory = 'Index'">
            <SQL command="select" result="IDASSET" cache="true">
              select asset.IDASSET
              from dbo.ASSET_INDEX asset
              inner join dbo.MARKET m on (m.IDM=asset.IDM_RELATED) and (m.ISO10383_ALPHA4='XEUR')
              where asset.ISINCODE=@ISINCODE
              <Param name="ISINCODE" datatype="string">
                <xsl:value-of select="$vUnderlyingISIN"/>
              </Param>
            </SQL>
          </xsl:if>
        </column>

        <!-- IDMATURITYRULE -->
        <column name="IDMATURITYRULE" datakeyupd="true" datatype="integer">
          <xsl:call-template name="GetMaturityRule_EUREX">
            <xsl:with-param name="pProductType" select="$vProductType"/>
            <xsl:with-param name="pProductGroup" select="$vProductGroup"/>
            <xsl:with-param name="pCountryCode" select="$vCountryCode"/>
            <xsl:with-param name="pGroupId" select="$vGroupID"/>
            <xsl:with-param name="pContractSymbol" select="$vProductID"/>
          </xsl:call-template>
        </column>

				<column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="parent::file/@name"/>
        </column>
        
				<column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
					<SpheresLib function="GetUTCDateTimeSys()" />
					<controls>
						<control action="RejectColumn" return="true" >
							<SpheresLib function="IsInsert()" />
              <logInfo status="NONE"/>
						</control>			
					</controls>
				</column>
				<column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
					<SpheresLib function="GetUserId()" />
					<controls>
						<control action="RejectColumn" return="true" >
							<SpheresLib function="IsInsert()" />
              <logInfo status="NONE"/>
						</control>			
					</controls>
				</column>
				<column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
					<SpheresLib function="GetUTCDateTimeSys()" />
					<controls>
						<control action="RejectColumn" return="true" >
							<SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
						</control>			
					</controls>
				</column>
				<column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
					<SpheresLib function="GetUserId()" />
					<controls>
						<control action="RejectColumn" return="true" >
							<SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
						</control>			
					</controls>
				</column>
      </table>

    </row>

  </xsl:template>

</xsl:stylesheet>