<?xml version="1.0" encoding="utf-8"?>

<!--
/*===============================================================================================================================
/* Name     : RiskData_London_SPAN_Import.xsl
/*===============================================================================================================================
/* Summary  : Import London SPAN Risk Margin parameters
/*===============================================================================================================================
FI 2021129 [XXXXX] Le mode PRICEONLY fonctionne désormais correctement
RD 20170803 [23248] Ajout de (NOLOCK) et Refactoring pour éviter les Deadlock
RD/PM 20170615 [23248] Utilisation du parametre EXCHANGECOMPLEX de la tâche et ajout d'un critère sur les requête UPDATE
PM/FL 20160512 [22165] Limiter la gestion de la base du prix au DC uniquement car le prix des sous-jacents n'était plus alimenté
PM 20160302 [21967] Ajout NYBOT ICE Clear US
PM 20150707 [21104] Add column IDIMSPAN_H on table IMSPANGRPCTR_H
PM/FL 20150603 [20398][21088] Gerer les Generic Contract Type ( 'M'-> Monthly & 'D' -> Daily) comme les Futures 
PM 20140326 [19794] Ajout gestion des cours des sous-jacents Indice
PM 20140326 [19792] Utilisation du Tick Denominator au lieu du Decimal Locator pour l'interprétation du prix du sous-jacent
FL 20140312 [19711] Suite à la gestion du segment de marché LCP - EURONEXT.LIFFE COMMODITY PRODUCT(Exchange Symbol 'X')
la transformation de l'exchange Symbol 'X' en 'L' n'est plus à faire dans le template "Record60"
FI 20140108 [19460] Les parameters SQL doivent être en majuscule
PM 20131218 [19360] Ne pas prendre en compte un prix valant 99999999
PM 20131205 [19303] Utilisation du Tick Denominator pour le calcul du prix des Futures et Options
PM 20130423 Ajout gestion des cours des sous-jacents Equity
PM 20120305 Mapping XSL des données London SPAN
/*===============================================================================================================================
-->

<!--
==================================================================
            Alias used for RDBMS tables
==================================================================
 a => IMSPANASSET_H
 b => IMSPANGRPCOMB_H
 c => IMSPANCONTRACT_H
 d => IMSPANDLVMONTH_H
 e => IMSPANEXCHANGE_H
 g => IMSPANGRPCTR_H
 i => IMSPANINTRASPR_H
 j => IMSPANINTRALEG_H
 k => IMSPANINTERSPR_H
 l => IMSPANINTERLEG_H
 m => IMSPANMATURITY_H
 r => IMSPANARRAY_H
 s => IMSPAN_H
 t => IMSPANTIER_H
 v => IMSPANCURCONV_H
 y => IMSPANCURRENCY_H
==================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Parameters -->
  <xsl:param name="pPassNumber" select="1"/>

  <xsl:variable name ="vAskedBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
  <!--RD/PM 20170615 [23248] Utilisation du parametre EXCHANGECOMPLEX de la tâche -->
  <xsl:variable name ="vExchangeComplex" select="/iotask/parameters/parameter[@id='EXCHANGECOMPLEX']"/>

  <!-- Includes -->
  <xsl:include href=".\RiskData_SPAN_Import_Common.xsl"/>

  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_London_SPAN_Import.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v5.1.0.0</xsl:variable>
  <xsl:variable name ="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>

  <!-- Le paramètre PRICEONLY active/désactive l'import des données de calcul de risque  -->
  <xsl:variable name ="vIsImportPriceOnly" >
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='PRICEONLY']='true'">true</xsl:when>
      <xsl:otherwise>false</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Variable for file identification -->
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>

  <!-- FILE -->
  <xsl:template match="file">
    <file>
      <!-- CommonInput : IOFileAtt -->
      <xsl:call-template name="IOFileAtt"/>
      <!--  -->
      <xsl:call-template name="StartImport"/>
    </file>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- Calcul du prix de clôture pour les fichiers London SPAN            -->
  <xsl:template name="AlignedPrice">
    <!-- PARAMETRES -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pPrice"/>

    <sql cd="select" rt="PRICE" t="dc" cache="true">
      select
      (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end) * @PRICEVALUE as PRICE
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
      and (( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM ) or ((mk.EXCHANGEACRONYM = 'L') and (@EXCHANGEACRONYM = 'O') and (dc.ASSETCATEGORY = 'Index')))
      where ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
      and ( dc.CATEGORY = @CATEGORY )
      and ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BUSINESSDATETIME ) )
      <!-- Param business date -->
      <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
      <!-- Param Exchange Code / Acronym -->
      <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
      <!-- Param Commodity Code -->
      <p n="CONTRACTSYMBOL" v="{$pCommodityCode}"/>
      <!-- Param Category -->
      <p n="CATEGORY" v="{$pCategory}"/>
      <p n="PRICEVALUE" t="dc" v="{number( $pPrice )}"/>
    </sql>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- Initialisation des variables provenant du fichier -->
  <!-- Lancement de l'import -->
  <xsl:template name="StartImport">
    <!-- Asked Business Date & Time -->
    <xsl:variable name="vAskedBusinessDateTime">
      <xsl:value-of select="concat($vAskedBusinessDate, ' 00:00:00')"/>
    </xsl:variable>
    <!-- Business Date & Time -->
    <xsl:variable name="vBusinessDateTime">
      <xsl:value-of select="concat(./r[1]/d[@n='BD']/@v, ' 00:00:00')"/>
    </xsl:variable>
    <!-- Settlement or Intraday -->
    <xsl:variable name="vSettlementorIntraday">
      <xsl:choose>
        <xsl:when test="./r[1]/d[@n='FI']/@v = 'F'">EOD</xsl:when>
        <xsl:otherwise>ITD</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- File Version -->
    <xsl:variable name="vFileVersion" select="./r[1]/d[@n='V']/@v"/>
    <!-- Exchange Complex -->
    <!--PM 20160302 [21967] Passage de l'ExchangeComplex LCH à LCH/ICE-->
    <!--RD/PM 20170615 [23248] Utilisation du parametre EXCHANGECOMPLEX de la tâche -->
    <!--<xsl:variable name="vExchangeComplex" select="'LCH/ICE'"/>-->
    <!-- Ecriture da la première ligne de données dans la table IMSPAN_H -->
    <xsl:if test="($pPassNumber = 1) and ($vIsImportPriceOnly='false')">
      <xsl:call-template name="IMSPAN_H">
        <xsl:with-param name="pFileFormat" select="$vFileVersion"/>
        <xsl:with-param name="pBusinessDateTime" select="$vBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$vSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$vExchangeComplex"/>
        <xsl:with-param name="pAskedBusinessDateTime" select="$vAskedBusinessDateTime"/>
        <xsl:with-param name="pImportedFileName" select="$vImportedFileName"/>
        <xsl:with-param name="pImportedFileFolder" select="$vImportedFileFolder"/>
        <xsl:with-param name="pIOInputName" select="$vIOInputName"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Gestion des lignes suivantes -->
    <xsl:apply-templates select="./r">
      <xsl:with-param name="pFileVersion" select="$vFileVersion"/>
      <xsl:with-param name="pBusinessDateTime" select="$vBusinessDateTime"/>
      <xsl:with-param name="pSettlementorIntraday" select="$vSettlementorIntraday"/>
      <xsl:with-param name="pExchangeComplex" select="$vExchangeComplex"/>
    </xsl:apply-templates>
  </xsl:template>

  <!-- ================== TEMPLATE PRINCIPALE R (ROW) =================== -->
  <xsl:template match="r">
    <!-- PARAMETRES -->
    <xsl:param name="pFileVersion"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. de la série -->
    <xsl:param name="pExchangeCode"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedContractCode"/>
    <xsl:param name="pContractCode"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pExpiryDate"/>
    <xsl:param name="pDecimalLocator"/>
    <!-- PM 20131205 [19303] Ajout Tick Denominator -->
    <xsl:param name="pTickDenominator"/>

    <!-- PM 20130329 Ajout gestion de $vIsImportPriceOnly dans ce template -->
    <!-- Traitement en fonction du type d'enregistrement RID -->
    <xsl:choose>
      <!--RID = 12 (Currency Details)-->
      <xsl:when test="(./d[@n='RID']/@v = '12') and ($vIsImportPriceOnly='false')">
        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANCURRENCY_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 13 (Currency Conversion Details)-->
      <xsl:when test="./d[@n='RID']/@v = '13' and ($vIsImportPriceOnly='false')">
        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANCURCONV_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 14 (Intercontract Spread Details)-->
      <xsl:when test="./d[@n='RID']/@v = '14' and ($vIsImportPriceOnly='false')">
        <!-- PM 20131112 [19165][19144] Ajout vSpreadPriority -->
        <!-- Attention !!! Offset en fonction du format du fichier -->
        <!--<xsl:variable name="vSpreadPriority">
              <xsl:choose>
                <xsl:when test="$pFileVersion = 3">
                  <xsl:value-of select="number(./d[@n='SP']/@v) + 100000"/>
                </xsl:when>
                <xsl:when test="$pFileVersion = 5">
                  <xsl:value-of select="number(./d[@n='SP']/@v) + 200000"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="number(./d[@n='SP']/@v)"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>-->

        <!--PM 20140317 [19669] L'Offset n'est plus en fonction du format du fichier, mais en fonction de l'Echange Code du 1er contrat du spread-->
        <!--PM 20160302 [21967] Ajout offset pour NYBOT ICE Clear US-->
        <xsl:variable name="vFirstEchangeCode" select="normalize-space(./d[@n='EC1']/@v)" />
        <xsl:variable name="vSpreadPriority">
          <xsl:choose>
            <xsl:when test="$vFirstEchangeCode = 'M'">
              <!--LME-->
              <xsl:value-of select="number(./d[@n='SP']/@v) + 100000"/>
            </xsl:when>
            <xsl:when test="$vFirstEchangeCode = 'I'">
              <!--IPE (ICE)-->
              <xsl:value-of select="number(./d[@n='SP']/@v) + 200000"/>
            </xsl:when>
            <xsl:when test="$vFirstEchangeCode = 'N'">
              <!--NYBOT ICE Clear US-->
              <xsl:value-of select="number(./d[@n='SP']/@v) + 300000"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(./d[@n='SP']/@v)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANINTERSPR_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <!-- PM 20131112 [19165][19144] Ajout pSpreadPriority -->
            <xsl:with-param name="pSpreadPriority" select="$vSpreadPriority"/>
          </xsl:call-template>
        </r>
        <r uc="true">
          <xsl:call-template name="RowAttributes">
            <xsl:with-param name="pInfo" select="leg"/>
          </xsl:call-template>
          <xsl:call-template name="IMSPANINTERLEG_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <!-- PM 20131112 [19165][19144] Ajout pSpreadPriority -->
            <xsl:with-param name="pSpreadPriority" select="$vSpreadPriority"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 16 (Margin Group Description)-->
      <xsl:when test="./d[@n='RID']/@v = '16' and ($vIsImportPriceOnly='false')">
        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANGRPCOMB_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 20 (Exchange Details)-->
      <xsl:when test="./d[@n='RID']/@v = '20'">
        <xsl:if  test="($vIsImportPriceOnly='false')">
          <r uc="true">
            <xsl:call-template name="RowAttributes"/>
            <xsl:call-template name="IMSPANEXCHANGE_H">
              <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            </xsl:call-template>
          </r>
        </xsl:if>
        <!-- Gestion des noeuds enfant suivants -->
        <xsl:apply-templates select="./r">
          <xsl:with-param name="pFileVersion" select="$pFileVersion"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeCode" select="./d[@n='EC']/@v"/>
          <xsl:with-param name="pExchangeAcronym" select="./d[@n='EA']/@v"/>
        </xsl:apply-templates>
        <!-- Mise à jour de IMSPANINTERLEG_H -->
        <xsl:if  test="($vIsImportPriceOnly='false')">
          <r uc="true">
            <xsl:call-template name="RowAttributes"/>
            <xsl:call-template name="Upd_IMSPANINTERLEG_H">
              <xsl:with-param name="pFileVersion" select="$pFileVersion"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeCode" select="./d[@n='EC']/@v"/>
              <xsl:with-param name="pExchangeAcronym" select="./d[@n='EA']/@v"/>
            </xsl:call-template>
          </r>
        </xsl:if>
      </xsl:when>

      <!--RID = 30 (Combined Contract Details)-->
      <xsl:when test="./d[@n='RID']/@v = '30'">
        <xsl:if  test="($vIsImportPriceOnly='false')">
          <r uc="true">
            <xsl:call-template name="RowAttributes"/>
            <xsl:call-template name="IMSPANGRPCTR_H">
              <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
            </xsl:call-template>
          </r>
        </xsl:if>
        <!-- Gestion des noeuds enfant suivants -->
        <xsl:apply-templates select="./r">
          <xsl:with-param name="pFileVersion" select="$pFileVersion"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pCombinedContractCode" select="./d[@n='CC']/@v"/>
        </xsl:apply-templates>
      </xsl:when>

      <!--RID = 31 (Month Tier Details)-->
      <xsl:when test="./d[@n='RID']/@v = '31' and ($vIsImportPriceOnly='false')">
        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANTIER_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 32 (Leg Spread Details)-->
      <xsl:when test="./d[@n='RID']/@v = '32' and ($vIsImportPriceOnly='false')">
        <xsl:call-template name="RowINTRASPREAD">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
        </xsl:call-template>
      </xsl:when>

      <!--RID = 33 (Prompt Date Charge Details)-->
      <xsl:when test="./d[@n='RID']/@v = '33'  and ($vIsImportPriceOnly='false')">
        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANDLVMONTH_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 34 (Intercontract Tier Details)-->
      <xsl:when test="./d[@n='RID']/@v = '34'  and ($vIsImportPriceOnly='false')">
        <r uc="true">
          <xsl:call-template name="RowAttributes"/>
          <xsl:call-template name="IMSPANTIER_H">
            <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          </xsl:call-template>
        </r>
      </xsl:when>

      <!--RID = 35 (Strategy Spread Details)-->
      <xsl:when test="./d[@n='RID']/@v = '35'  and ($vIsImportPriceOnly='false')">
        <xsl:call-template name="RowINTRASPREAD">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
        </xsl:call-template>
      </xsl:when>

      <!--RID = 40 (Contract Details)-->
      <xsl:when test="./d[@n='RID']/@v = '40'">
        <xsl:variable name="vDecimalLocator">
          <xsl:choose>
            <xsl:when test="(normalize-space(./d[@n='DL']/@v='')) or (normalize-space(./d[@n='DL']/@v)='0')">3</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="normalize-space(./d[@n='DL']/@v)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <!-- PM 20131205 [19303] Ajout Tick Denominator -->
        <xsl:variable name="vTickDenominator">
          <xsl:choose>
            <xsl:when test="number(./d[@n='TD']/@v) = 0">1</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(./d[@n='TD']/@v)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:if  test="($vIsImportPriceOnly='false')">
          <r uc="true">
            <xsl:call-template name="RowAttributes"/>
            <xsl:call-template name="IMSPANCONTRACT_H">
              <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
              <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
              <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
            </xsl:call-template>
          </r>
        </xsl:if>
        <!-- Gestion des noeuds enfant suivants -->
        <xsl:apply-templates select="./r">
          <xsl:with-param name="pFileVersion" select="$pFileVersion"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          <xsl:with-param name="pContractCode" select="./d[@n='C']/@v"/>
          <xsl:with-param name="pContractType" select="./d[@n='CT']/@v"/>
          <xsl:with-param name="pDecimalLocator" select="$vDecimalLocator"/>
          <!-- PM 20131205 [19303] Ajout Tick Denominator -->
          <xsl:with-param name="pTickDenominator" select="$vTickDenominator"/>
        </xsl:apply-templates>
      </xsl:when>

      <!--RID = 50 (Contract Expiry Details)-->
      <xsl:when test="./d[@n='RID']/@v = '50'">
        <!-- Supression des '00' à la fin de l'échéance des échéances mensuelles -->
        <xsl:variable name="vExpiryDate">
          <xsl:call-template name="LondonExpiryDate">
            <xsl:with-param name="pExpiryDate" select="./d[@n='ED']/@v"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if  test="($vIsImportPriceOnly='false')">
          <r uc="true">
            <xsl:call-template name="RowAttributes"/>
            <xsl:call-template name="IMSPANMATURITY_H">
              <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
              <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
              <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
              <xsl:with-param name="pContractCode" select="$pContractCode"/>
              <xsl:with-param name="pContractType" select="$pContractType"/>
              <xsl:with-param name="pExpiryDate" select="$vExpiryDate"/>
            </xsl:call-template>
          </r>
        </xsl:if>
        <!-- Gestion des noeuds enfant suivants -->
        <xsl:apply-templates select="./r">
          <xsl:with-param name="pFileVersion" select="$pFileVersion"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          <xsl:with-param name="pContractCode" select="$pContractCode"/>
          <xsl:with-param name="pContractType" select="$pContractType"/>
          <xsl:with-param name="pExpiryDate" select="$vExpiryDate"/>
          <xsl:with-param name="pDecimalLocator" select="$pDecimalLocator"/>
          <!-- PM 20131205 [19303] Ajout Tick Denominator -->
          <xsl:with-param name="pTickDenominator" select="$pTickDenominator"/>
        </xsl:apply-templates>
      </xsl:when>

      <!--RID = 60 (Series Details: Risk Array Record)-->
      <xsl:when test="./d[@n='RID']/@v = '60'">
        <xsl:call-template name="Record60">
          <xsl:with-param name="pFileFormat" select="$pFileVersion"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          <xsl:with-param name="pContractCode" select="$pContractCode"/>
          <xsl:with-param name="pContractType" select="$pContractType"/>
          <xsl:with-param name="pExpiryDate" select="$pExpiryDate"/>
          <xsl:with-param name="pDecimalLocator" select="$pDecimalLocator"/>
          <!-- PM 20131205 [19303] Ajout Tick Denominator -->
          <xsl:with-param name="pTickDenominator" select="$pTickDenominator"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- ============== IMSPANCURRENCY_H ================================== -->
  <!-- ============== (Type "12" London) ================================ -->
  <xsl:template name="IMSPANCURRENCY_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <tbl n="IMSPANCURRENCY_H" a="IU">
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_IDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <!-- Currency -->
      <c n="IDC" dk="true" v="{./d[@n='ISOCod']/@v}"/>
      <!-- Currency Description -->
      <c n="DESCRIPTION" dku="true" v="{./d[@n='Desc']/@v}"/>
      <!-- Currency Exponent -->
      <c n="EXPONENT" dku="true" t="i">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="normalize-space(./d[@n='CE']/@v)=''">0</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="./d[@n='CE']/@v"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ============== IMSPANCURCONV_H ================================== -->
  <!-- ============== (Type "13" London) ================================ -->
  <xsl:template name="IMSPANCURCONV_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <tbl n="IMSPANCURCONV_H" a="IU">
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_IDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <!-- Contract Currency -->
      <c n="IDC_CONTRACT" dk="true" v="{./d[@n='SCur']/@v}"/>
      <!-- Margin Currency -->
      <c n="IDC_MARGIN" dk="true" v="{./d[@n='PBCur']/@v}"/>
      <!-- Currency Description -->
      <c n="VALUE" dku="true" t="dc" v="{./d[@n='FXR']/@v}"/>
      <!-- Currency Description -->
      <c n="SHIFTUP" dku="true" t="dc" v="{./d[@n='PFXU']/@v}"/>
      <!-- Currency Description -->
      <c n="SHIFTDOWN" dku="true" t="dc" v="{./d[@n='PFXD']/@v}"/>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ============== Update IMSPANINTERLEG_H =========================== -->
  <!-- ============== (Type "14" Liffe) ================================= -->
  <xsl:template name="Upd_IMSPANINTERLEG_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du marché -->
    <xsl:param name="pExchangeCode"/>
    <xsl:param name="pExchangeAcronym"/>

    <!-- Astuce !!! -->
    <!-- Ici seul importe la mise à jour de la donnée IDIMSPANGRPCTR_H de la table IMSPANINTERLEG_H -->
    <!-- La mise à jour de la table IMSPAN_H n'est utilisée que parcequ'il faut une action sur -->
    <!-- un enregistrement d'une table avec des colonnes pour pouvoir executer une requête de mise à jour global de donnée -->
    <tbl n="IMSPAN_H" a="U">
      <!-- Mise à jour de l'Id du groupe de contrat de la table IMSPANINTERLEG_H -->
      <!-- pour tous les exchanges de l'exchange complex -->
      <!-- RD 20170803 [23248] Ajout de (NOLOCK) et Refactoring pour éviter les Deadlock-->
      <c n="IDIMSPANGRPCTR_H">
        <sql cd="update" rt="">
          update IMSPANINTERLEG_H
          set IDIMSPANGRPCTR_H = (select g.IDIMSPANGRPCTR_H
          from dbo.IMSPANGRPCTR_H g (NOLOCK)
          inner join dbo.IMSPANEXCHANGE_H e (NOLOCK) on (e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H)
          inner join dbo.IMSPANINTERSPR_H k (NOLOCK) on (k.IDIMSPAN_H = e.IDIMSPAN_H)<!--<xsl:value-of select="$vSpace"/>-->
          <!-- Vérification de IDIMSPAN_H -->
          <!--<xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>-->
          where (e.IDIMSPAN_H = @IDIMSPAN_H)
          and (IMSPANINTERLEG_H.EXCHANGEACRONYM = e.EXCHANGESYMBOL)
          and (IMSPANINTERLEG_H.COMBCOMCODE = g.COMBCOMCODE)
          and (IMSPANINTERLEG_H.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H))
          <!--RD/PM 20170615 [23248] Ajouter critère IDIMSPANGRPCTR_H is null -->
          where (IDIMSPANGRPCTR_H is null)
          and exists (select g.IDIMSPANGRPCTR_H
          from dbo.IMSPANGRPCTR_H g (NOLOCK)
          inner join dbo.IMSPANEXCHANGE_H e (NOLOCK) on (e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H)
          inner join dbo.IMSPANINTERSPR_H k (NOLOCK) on (k.IDIMSPAN_H = e.IDIMSPAN_H)<!--<xsl:value-of select="$vSpace"/>-->
          <!-- Vérification de IDIMSPAN_H -->
          <!--<xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>-->
          where (e.IDIMSPAN_H = @IDIMSPAN_H)
          and (IMSPANINTERLEG_H.EXCHANGEACRONYM = e.EXCHANGESYMBOL)
          and (IMSPANINTERLEG_H.COMBCOMCODE = g.COMBCOMCODE)
          and (IMSPANINTERLEG_H.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H))
          <!--<xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>-->
          <p n="IDIMSPAN_H">
            <sql cd="select" rt="IDIMSPAN_H">
              select s.IDIMSPAN_H
              from dbo.IMSPAN_H s (NOLOCK)
              where ( s.DTBUSINESSTIME = @BUSINESSDATETIME )
              and ( s.SETTLEMENTSESSION = @SETTLEMENTORINTRADAY)
              and ( s.EXCHANGECOMPLEX = @EXCHANGECOMPLEX )
              <xsl:call-template name="paramInnerIDIMSPAN_H">
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </sql>
          </p>
        </sql>
        <ctls>
          <ctl a="RejectColumn" rt="true" v="true">
          </ctl>
        </ctls>
      </c>
      <!-- Mise à jour de l'Id du tier du leg la table IMSPANINTERLEG_H -->
      <!-- pour tous les exchanges de l'exchange complex -->
      <c n="IDIMSPANTIER_H">
        <!-- RD 20170803 [23248] Ajout de (NOLOCK) et Refactoring pour éviter les Deadlock-->
        <sql cd="update" rt="">
          update IMSPANINTERLEG_H
          set IDIMSPANTIER_H = (select t.IDIMSPANTIER_H
          from dbo.IMSPANTIER_H t (NOLOCK)
          inner join dbo.IMSPANGRPCTR_H g (NOLOCK) on ( g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H )
          inner join dbo.IMSPANEXCHANGE_H e (NOLOCK) on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )
          inner join dbo.IMSPANINTERSPR_H k (NOLOCK) on (k.IDIMSPAN_H = e.IDIMSPAN_H)<!--<xsl:value-of select="$vSpace"/>-->
          <!-- Vérification de IDIMSPAN_H -->
          <!--<xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>-->
          where (e.IDIMSPAN_H = @IDIMSPAN_H)
          and (t.SPREADTYPE = 'L')
          and (IMSPANINTERLEG_H.EXCHANGEACRONYM = e.EXCHANGESYMBOL)
          and (IMSPANINTERLEG_H.COMBCOMCODE = g.COMBCOMCODE)
          and (IMSPANINTERLEG_H.TIERNUMBER = t.TIERNUMBER)
          and (IMSPANINTERLEG_H.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H))
          <!--RD/PM 20170615 [23248] Ajouter critère IDIMSPANGRPCTR_H is null -->
          where (IDIMSPANTIER_H is null)
          and exists (select t.IDIMSPANTIER_H
          from dbo.IMSPANTIER_H t (NOLOCK)
          inner join dbo.IMSPANGRPCTR_H g (NOLOCK) on ( g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H )
          inner join dbo.IMSPANEXCHANGE_H e (NOLOCK) on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )
          inner join dbo.IMSPANINTERSPR_H k (NOLOCK) on (k.IDIMSPAN_H = e.IDIMSPAN_H)<!--<xsl:value-of select="$vSpace"/>-->
          <!-- Vérification de IDIMSPAN_H -->
          <!--<xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>-->
          where (e.IDIMSPAN_H = @IDIMSPAN_H)
          and (t.SPREADTYPE = 'L')
          and (IMSPANINTERLEG_H.EXCHANGEACRONYM = e.EXCHANGESYMBOL)
          and (IMSPANINTERLEG_H.COMBCOMCODE = g.COMBCOMCODE)
          and (IMSPANINTERLEG_H.TIERNUMBER = t.TIERNUMBER)
          and (IMSPANINTERLEG_H.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H))
          <!--<xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>-->
          <p n="IDIMSPAN_H">
            <sql cd="select" rt="IDIMSPAN_H">
              select s.IDIMSPAN_H
              from dbo.IMSPAN_H s (NOLOCK)
              where ( s.DTBUSINESSTIME = @BUSINESSDATETIME )
              and ( s.SETTLEMENTSESSION = @SETTLEMENTORINTRADAY)
              and ( s.EXCHANGECOMPLEX = @EXCHANGECOMPLEX )
              <xsl:call-template name="paramInnerIDIMSPAN_H">
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </sql>
          </p>
        </sql>
        <ctls>
          <ctl a="RejectColumn" rt="true" v="true">
          </ctl>
        </ctls>
      </c>
      <!-- Business Date Time -->
      <c n="DTBUSINESSTIME" dk="true" t="dt" v="{$pBusinessDateTime}"/>
      <!-- Exchange Code / Acronym -->
      <c n="EXCHANGECOMPLEX" dk="true" v="{$pExchangeComplex}"/>
      <!-- Settlement or Intraday -->
      <c n="SETTLEMENTSESSION" dk="true" v="{$pSettlementorIntraday}"/>
    </tbl>
  </xsl:template>
  <!-- ============== IMSPANGRPCTR_H ==================================== -->
  <!-- ============== (Type "30" London) ================================ -->
  <xsl:template name="IMSPANGRPCTR_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du groupe de contrat -->
    <xsl:param name="pExchangeCode"/>


    <tbl n="IMSPANGRPCTR_H" a="IU">
      <!--PM 20150707 [21104] Add column IDIMSPAN_H-->
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_IDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <!-- Exchange -->
      <c n="IDIMSPANEXCHANGE_H" dk="true" t="i">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDIMSPANEXCHANGE_H" cache="true">
          select e.IDIMSPANEXCHANGE_H
          from dbo.IMSPANEXCHANGE_H e<xsl:value-of select="$vSpace"/>
          <!--Verif IDIMSPAN_H-->
          <xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>
          and ( e.EXCHANGESYMBOL = @EXCHANGECODE )
          <!--Param. IDIMSPAN_H-->
          <xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
          <p n="EXCHANGECODE" v="{$pExchangeCode}"/>
        </sql>
      </c>
      <!-- Combined Commodity Code -->
      <c n="COMBCOMCODE" dk="true" v="{./d[@n='CC']/@v}">
        <ctls>
          <ctl a="RejectRow" rt="true">
            <sl fn="IsEmpty()"/>
            <li st="REJECT" ex="true">
              <code>SYS</code>
              <number>2001</number>
              <d1>
                <xsl:value-of select="$vXSLFileNameRiskDataSPANCommon"/>
              </d1>
              <d2>
                <xsl:value-of select="$vXSLVersionRiskDataSPANCommon"/>
              </d2>
            </li>
          </ctl>
        </ctls>
      </c>

      <!-- Combined Group -->
      <c n="IDIMSPANGRPCOMB_H" dku="true" t="i">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDIMSPANGRPCOMB_H" cache="true">
          select b.IDIMSPANGRPCOMB_H
          from dbo.IMSPANGRPCOMB_H b <xsl:value-of select="$vSpace"/>
          <!--Verif IDIMSPAN_H-->
          <xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'b'"/>
          </xsl:call-template>
          and ( b.COMBINEDGROUPCODE = @COMBINEDGROUPCODE ) <xsl:value-of select="$vSpace"/>
          <xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
          <p n="COMBINEDGROUPCODE" v="{./d[@n='MG']/@v}"/>
        </sql>
      </c>
      <!-- Limit Option Value -->
      <c n="ISOPTIONVALUELIMIT" dku="true" t="b" v="0"/>
      <!-- Short Option Minimum Charge Rate -->
      <c n="SOMCHARGERATE" dku="true" t="dc" v="{./d[@n='SOM']/@v}"/>
      <!-- Intracommodity Spread Charge Method Code -->
      <c n="INTRASPREADMETHOD" dku="true" v="{./d[@n='IASM']/@v}"/>
      <!-- Strategy Spread Charge Method Code -->
      <c n="STRATEGYSPREADMETH" dku="true">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="normalize-space(./d[@n='SSM']/@v)=''">1</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="./d[@n='SSM']/@v"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <!-- Delivery (Spot) Charge Method Code -->
      <c n="DELIVERYCHARGEMETH" dku="true" v="{./d[@n='DM']/@v}"/>
      <!-- Margin Currency Code -->
      <c n="IDC" dku="true">
        <xsl:attribute name="v">
          <xsl:call-template name="ValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='PBCur']/@v"/>
          </xsl:call-template>
        </xsl:attribute>
      </c>
      <!-- ================== -->
      <!-- Valeurs par défaut -->
      <!-- ================== -->
      <!-- Initial to Maintenance Ratio _ Member Accounts -->
      <c n="MEMBERITOMRATIO" dku="true" t="dc" v="1"/>
      <!-- Initial to Maintenance Ratio _ Hedger Accounts -->
      <c n="HEDGERITOMRATIO" dku="true" t="dc" v="1"/>
      <!-- Initial to Maintenance Ratio _ Speculator Accounts -->
      <c n="SPECULATITOMRATIO" dku="true" t="dc" v="1"/>
      <!-- Members -->
      <c n="MEMBERADJFACTOR" dku="true" t="dc" v="1"/>
      <!-- Hedgers-->
      <c n="HEDGERADJFACTOR" dku="true" t="dc" v="1"/>
      <!-- Speculators -->
      <c n="SPECULATADJFACTOR" dku="true" t="dc" v="1"/>
      <!-- Short Option Minimum Calculation Method -->
      <c n="SOMMETHOD" dku="true" v="2"/>
      <!-- Intercommodity Spreading Method Code -->
      <c n="INTERSPREADMETHOD" dku="true" v="01"/>
      <!-- Weighted Futures Price Risk Calculation Method -->
      <!--PM 20151127 [21571][21605] Ne pas alimenter WEIGHTEDRISKMETHOD -->
      <!--<c n="WEIGHTEDRISKMETHOD" dku="true" v="1"/>-->
      <!-- Risk Exponent -->
      <!-- PM 20131120 [19220] Mettre 0 car sinon vaut null -->
      <c n="RISKEXPONENT" dku="true" t="i" v="0"/>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANCONTRACT_H ================================== -->
  <!-- ============== (Type "40" London) ================================ -->
  <xsl:template  name="IMSPANCONTRACT_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du contrat -->
    <xsl:param name="pExchangeCode"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedContractCode"/>

    <xsl:variable name="vContractSymbol" select="./d[@n='C']/@v"/>

    <!-- PM/FL 20150603[20398][21088] Gerer les Daily et Monthly comme les Futures -->
    <xsl:variable name="vCategory" >
      <xsl:choose>
        <xsl:when test="./d[@n='CT']/@v ='D'">F</xsl:when>
        <xsl:when test="./d[@n='CT']/@v ='F'">F</xsl:when>
        <xsl:when test="./d[@n='CT']/@v ='M'">F</xsl:when>
        <xsl:when test="./d[@n='CT']/@v ='O'">O</xsl:when>
        <xsl:otherwise></xsl:otherwise>
        <!-- Si ni Option ni Future Alors vide -->
      </xsl:choose>
    </xsl:variable>


    <tbl n="IMSPANCONTRACT_H" a="IU">
      <!-- ID du Combined Commodity pour le lien indirect avec IMSPAN_H -->
      <c n="IDIMSPANEXCHANGE_H" dk="true" t="i">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDIMSPANEXCHANGE_H" cache="true">
          select e.IDIMSPANEXCHANGE_H
          from dbo.IMSPANEXCHANGE_H e<xsl:value-of select="$vSpace"/>
          <!-- Vérification de IDIMSPAN_H -->
          <xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>
          and ( e.EXCHANGESYMBOL = @EXCHANGESYMBOL )
          <xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
          <!-- Param Exchange Code / Acronym -->
          <p n="EXCHANGESYMBOL" v="{$pExchangeCode}"/>
        </sql>
      </c>
      <!-- IDIMSPANGRPCTR_H -->
      <c n="IDIMSPANGRPCTR_H" dku="true" t="i">
        <xsl:call-template name="SQL_IDIMSPANGRPCTR_H">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pCombinedCommodityCode" select="$pCombinedContractCode"/>
        </xsl:call-template>
      </c>
      <!-- Commodity (Product) Code -->
      <c n="CONTRACTSYMBOL" dk="true" v="{$vContractSymbol}"/>
      <!-- Contract Type -> Colonne CONTRACTTYPE (S, E et P) -->
      <c n="CONTRACTTYPE" dk="true">
        <xsl:attribute name="v">
          <xsl:call-template name="ValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='CT']/@v"/>
          </xsl:call-template>
        </xsl:attribute>
      </c>
      <c n="PRODUCTNAME" dku="true" v="{./d[@n='Desc']/@v}"/>
      <c n="IDC_PRICE" dku="true" v="{./d[@n='Cur']/@v}"/>
      <c n="CONTRACTMULTIPLIER" dku="true" t="dc" v="{number(./d[@n='TD']/@v) * number(./d[@n='TV']/@v)}"/>
      <c n="MINPRICEINCR" dku="true" t="dc">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="number(./d[@n='TD']/@v) != 0">
              <xsl:value-of select="number(./d[@n='MPF']/@v) div number(./d[@n='TD']/@v)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(./d[@n='MPF']/@v)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <c n="MINPRICEINCRAMOUNT" dku="true" t="dc" v="{number(./d[@n='MPF']/@v) * number(./d[@n='TV']/@v)}"/>
      <c n="TICKVALUE" dku="true" t="dc" v="{number(./d[@n='TV']/@v)}"/>
      <c n="DELTADEN" dku="true" t="dc" v="{./d[@n='DD']/@v}"/>
      <c n="PRICEDECLOCATOR" dku="true" t="i" v="{./d[@n='DL']/@v}"/>
      <c n="STRIKEDEN" dku="true" t="i" v="{./d[@n='SD']/@v}"/>
      <c n="SCANNINGRANGE" dku="true" t="i" v="{./d[@n='SR']/@v}"/>
      <c n="SETTLTMETHOD" dku="true" v="{./d[@n='SM']/@v}"/>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANMATURITY_H ================================== -->
  <!-- ============== (Type "50" London) ================================ -->
  <xsl:template name="IMSPANMATURITY_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. de l'échéance -->
    <xsl:param name="pExchangeCode"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedContractCode"/>
    <xsl:param name="pContractCode"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pExpiryDate"/>

    <tbl n="IMSPANMATURITY_H" a="IU">
      <!-- IDIMSPANCONTRACT_H -->
      <xsl:call-template name="Col_IDIMSPANCONTRACT_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
        <xsl:with-param name="pCommodityCode" select="$pContractCode"/>
        <xsl:with-param name="pContractType" select="$pContractType"/>
      </xsl:call-template>
      <c n="FUTMMY" dk="true" v="{$pExpiryDate}"/>
      <c n="DISCOUNTFACTOR" dku="true" t="dc" v="{./d[@n='DF']/@v}"/>
      <c n="VOLATILITYUP" dku="true" t="dc" v="{./d[@n='VSU']/@v}"/>
      <c n="VOLATILITYDOWN" dku="true" t="dc" v="{./d[@n='VSD']/@v}"/>
      <c n="DELTASCALINGFACTOR" dku="true" t="dc" v="1"/>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>

  </xsl:template>
  <!-- ================================================================== -->
  <!-- Information sur l'asset pour les record Type 60 -->
  <xsl:template name="Record60">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. de la série -->
    <xsl:param name="pExchangeCode"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedContractCode"/>
    <xsl:param name="pContractCode"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pExpiryDate"/>
    <xsl:param name="pDecimalLocator"/>
    <!-- PM 20131205 [19303] Ajout Tick Denominator -->
    <xsl:param name="pTickDenominator"/>

    <!-- Transco de l'ExchangeSymbol pour la table MARKET -->
    <!-- FL 20140312[19711] Suite à la gestion du segment de marché LCP - EURONEXT.LIFFE COMMODITY PRODUCT(Exchange Symbol 'X')
          la transformation de l'exchange Symbol 'X' en 'L' n'est plus à faire, Il n’y a donc plus aucune transcodification à faire  -->
    <xsl:variable name ="vMarketExchangeSymbol" >
      <!--<xsl:choose>-->
      <!-- PM 20130329 Modification suite à création d'un Market Segment Equities -->
      <!--<xsl:when test="$pExchangeCode='O'">-->
      <!--LTOM = LIFFE-->
      <!--<xsl:value-of select="'L'"/>
            </xsl:when>-->
      <!--<xsl:when test="$pExchangeCode='X'">-->
      <!--LCP/NFP = LIFFE-->
      <!--<xsl:value-of select="'L'"/>
        </xsl:when>-->
      <!--<xsl:otherwise>-->
      <xsl:value-of select="$pExchangeCode"/>
      <!--</xsl:otherwise>
      </xsl:choose>-->
    </xsl:variable>

    <!-- Put / Call -->
    <xsl:variable name="vPutCall">
      <xsl:call-template name="CallOrPutOrNull">
        <xsl:with-param name="pValue" select="./d[@n='CT']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- Option Strike Price -->
    <xsl:variable name="vStrikePrice">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='SP']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- Category : F for Future ; O for Option  -->
    <xsl:variable name="vCategory">
      <xsl:choose>
        <xsl:when test="$vPutCall = 0 or $vPutCall = 1">O</xsl:when>
        <xsl:otherwise>F</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Echeance  -->
    <xsl:variable name ="vMaturityMonthYear" >
      <xsl:value-of select="$pExpiryDate"/>
    </xsl:variable>
    <!-- Asset Category  -->
    <xsl:variable name ="vAssetCategory" >
      <xsl:choose>
        <!-- PM/FL 20150603[20398][21088] Gerer les Daily et Monthly comme les Futures -->
        <!-- Dérivé Listé : Future et Option -->
        <xsl:when test="$pContractType = 'D' or $pContractType = 'F' or $pContractType = 'M' or $pContractType = 'O' ">
          <xsl:value-of select="'Future'"/>
        </xsl:when>
        <!-- Cash -->
        <xsl:when test="$pContractType = 'Z' ">
          <xsl:value-of select="'Unmanaged'"/>
        </xsl:when>
        <!-- Underlying -->
        <xsl:when test="$pContractType = 'U' ">
          <xsl:value-of select="'Unmanaged'"/>
        </xsl:when>
        <!-- Equity -->
        <xsl:when test="$pContractType = 'S' ">
          <xsl:value-of select="'EquityAsset'"/>
        </xsl:when>
        <!-- Index -->
        <!-- PM 20140326 [19794] Ajout gestion des Assets Indice -->
        <xsl:when test="$pContractType = 'I' ">
          <xsl:value-of select="'Index'"/>
        </xsl:when>
        <!-- Bond -->
        <!-- Commodity -->
        <!-- FxRateAsset -->
        <!-- RateIndex -->
        <xsl:otherwise>
          <xsl:value-of select="'Unmanaged'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Contract Multiplier (et Factor)  -->
    <xsl:variable name="vContractMultiplier" select="./d[@n='LS']/@v"/>

    <r uc="true">
      <xsl:call-template name="RowAttributes"/>

      <pms>
        <pm n="ASSETCATEGORY" v="{$vAssetCategory}"/>
        <pm n="CATEGORY" v="{$vCategory}"/>
        <pm n="IDASSET">
          <xsl:choose>
            <xsl:when test="$vAssetCategory='Future'">
              <xsl:call-template name="SQL_ASSET_ETD">
                <xsl:with-param name="pResultColumn" select="'IDASSET'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pExchangeAcronym" select="$vMarketExchangeSymbol"/>
                <xsl:with-param name="pContractSymbol" select="$pContractCode"/>
                <xsl:with-param name="pCategory" select="$vCategory"/>
                <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
                <xsl:with-param name="pPutCall" select="$vPutCall"/>
                <xsl:with-param name="pStrikePrice" select="$vStrikePrice"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="$vAssetCategory='EquityAsset'">
              <!-- PM 20131224 [19404][19402] : Ajout de la jointure ASSET_EQUITY_RDCMK en fonction de SYMBOL et IDM_RELATED -->
              <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
              <sql cd="select" rt="IDASSET" cache="true">
                select distinct aeq.IDASSET, aeq.IDC
                from dbo.ASSET_EQUITY aeq
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aeq.IDASSET) and (dc.ASSETCATEGORY = 'EquityAsset')
                inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
                inner join dbo.ASSET_EQUITY_RDCMK aeqrdc on (aeqrdc.IDASSET = aeq.IDASSET)
                where (aeqrdc.SYMBOL = @CONTRACTSYMBOL)
                and (aeqrdc.IDM_RELATED = dc.IDM)
                and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM)
                and ((aeq.DTDISABLED is null) or (aeq.DTDISABLED > @BUSINESSDATETIME))
                <!--and ( ( aeq.DTENABLED is null ) or ( aeq.DTENABLED &lt;= @BusinessDateTime ) )-->
                and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME))
                <!--and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
                and ((mk.DTDISABLED is null) or (mk.DTDISABLED > @BUSINESSDATETIME))
                <!--and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )-->
                <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
                <p n="CONTRACTSYMBOL" v="{$pContractCode}"/>
                <p n="EXCHANGEACRONYM" v="{$vMarketExchangeSymbol}"/>
              </sql>
            </xsl:when>
            <xsl:when test="$vAssetCategory='Index'">
              <!-- PM 20140326 [19794] Ajout gestion des Assets Indice -->
              <sql cd="select" rt="IDASSET" cache="true">
                select distinct aid.IDASSET, aid.IDC
                from dbo.ASSET_INDEX aid
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aid.IDASSET) and (dc.ASSETCATEGORY = 'Index')
                inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
                where ((aid.DTDISABLED is null) or (aid.DTDISABLED > @BUSINESSDATETIME))
                <!--									   and ( ( aid.DTENABLED is null ) or ( aid.DTENABLED &lt;= @BusinessDateTime ) )-->
                and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME))
                <!--										and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
                and ((mk.DTDISABLED is null) or (mk.DTDISABLED > @BUSINESSDATETIME))
                <!--										and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )-->
                and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM)
                and (aid.SYMBOL = @CONTRACTSYMBOL)
                <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
                <p n="CONTRACTSYMBOL" v="{$pContractCode}"/>
                <p n="EXCHANGEACRONYM" v="{$vMarketExchangeSymbol}"/>
              </sql>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="null"/>
            </xsl:otherwise>
          </xsl:choose>
        </pm>
        <pm n="SETTLEMENTPRICE">
          <xsl:choose>
            <!-- PM 20131218 [19360] Ne pas prendre en compte un cours valant 99999999 -->
            <xsl:when test="number( ./d[@n='Pr']/@v ) = 99999999">
              <xsl:attribute name="v">
                <xsl:value-of select="'null'"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <!--PM 20140326 [19792] Utilisation du Tick Denominator au lieu du Decimal Locator pour l'interprétation du prix-->
                <!--PM 20140326 [19794] Ajout gestion des Assets Indice et démocratisation de l'utilisation du pTickDenominator-->
                <xsl:when test="number($pTickDenominator) != 0">
                  <!--RD 20160307 [21976] Ajout gestion base du prix (INSTRUMENTDEN)-->
                  <!--<xsl:attribute name="v">
                    <xsl:value-of select="number( ./d[@n='Pr']/@v ) div number($pTickDenominator)"/>
                  </xsl:attribute>-->
                  <!--PM/FL 20160512 [22165] Limiter la gestion de la base du prix au DC uniquement car le prix des sous-jacents n'était plus alimenté -->
                  <xsl:choose>
                    <xsl:when test="$vAssetCategory='Future'">
                      <xsl:variable name="vPrice" select="number( ./d[@n='Pr']/@v ) div number($pTickDenominator)"/>
                      <xsl:call-template name="AlignedPrice">
                        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                        <xsl:with-param name="pExchangeAcronym" select="$vMarketExchangeSymbol"/>
                        <xsl:with-param name="pCommodityCode" select="$pContractCode"/>
                        <xsl:with-param name="pCategory" select="$vCategory"/>
                        <xsl:with-param name="pPrice" select="$vPrice"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:attribute name="v">
                        <xsl:value-of select="number( ./d[@n='Pr']/@v ) div number($pTickDenominator)"/>
                      </xsl:attribute>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="v">
                    <xsl:value-of select="'null'"/>
                  </xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </pm>
        <pm n="STRIKEPRICE">
          <xsl:choose>
            <xsl:when test="$vAssetCategory='Future'">
              <xsl:call-template name="SQL_ASSET_ETD">
                <xsl:with-param name="pResultColumn" select="'STRIKEPRICE'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pExchangeAcronym" select="$vMarketExchangeSymbol"/>
                <xsl:with-param name="pContractSymbol" select="$pContractCode"/>
                <xsl:with-param name="pCategory" select="$vCategory"/>
                <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
                <xsl:with-param name="pPutCall" select="$vPutCall"/>
                <xsl:with-param name="pStrikePrice" select="$vStrikePrice"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="v">
                <xsl:value-of select="'null'"/>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </pm>
        <pm n="DELTA" v="{./d[@n='D']/@v}"/>
      </pms>

      <xsl:if test="$vIsImportPriceOnly='false'">
        <xsl:call-template name="IMSPANARRAY_H">
          <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeCode" select="$pExchangeCode"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
          <xsl:with-param name="pContractCode" select="$pContractCode"/>
          <xsl:with-param name="pContractType" select="$pContractType"/>
          <xsl:with-param name="pExpiryDate" select="$pExpiryDate"/>
          <xsl:with-param name="pPutCall" select="$vPutCall"/>
        </xsl:call-template>
      </xsl:if>

      <!-- PM 20131218 [19360] Ne rien faire si le cours vaut 'null' -->
      <xsl:if test="'parameters.SETTLEMENTPRICE' != 'null'">
        <xsl:choose>
          <xsl:when test="$vAssetCategory='Future'">
            <xsl:call-template name="QUOTE_XXX_H">
              <xsl:with-param name="pQuoteTable" select="'QUOTE_ETD_H'"/>
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeAcronym" select="$vMarketExchangeSymbol"/>
              <xsl:with-param name="pCommodityCode" select="$pContractCode"/>
              <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
              <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
              <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
              <xsl:with-param name="pDelta" select="'parameters.DELTA'"/>
              <xsl:with-param name="pVolatility" select="''"/>
              <xsl:with-param name="pTimeToExpiration" select="''"/>
            </xsl:call-template>

            <!-- PM 20131202 [19275] Ajout CONTRACTMULTIPLIER et FACTOR -->
            <!-- PM 20131202 [19321] Mise à jour uniquement si > à 1 -->
            <xsl:if test="$vContractMultiplier &gt; 1">

              <!-- PM 20131202 [19275] Mise à jour du CONTRACTMULTIPLIER et du FACTOR de l'échéance ouverte si différent de celui du contrat -->
              <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
              <xsl:call-template name="Upd_DERIVATIVEATTRIB">
                <xsl:with-param name="pBusinessDate" select="$pBusinessDateTime"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
                <xsl:with-param name="pFactor" select="$vContractMultiplier"/>
              </xsl:call-template>

              <!-- PM 20131202 [19275] Mise à jour du CONTRACTMULTIPLIER et du FACTOR de l'Asset si différent de celui du contrat -->
              <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
              <xsl:call-template name="Upd_ASSET_ETD">
                <xsl:with-param name="pBusinessDate" select="$pBusinessDateTime"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
                <xsl:with-param name="pFactor" select="$vContractMultiplier"/>
              </xsl:call-template>

            </xsl:if>

          </xsl:when>
          <xsl:when test="$vAssetCategory='EquityAsset'">
            <xsl:call-template name="QUOTE_XXX_H">
              <xsl:with-param name="pQuoteTable" select="'QUOTE_EQUITY_H'"/>
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeAcronym" select="$vMarketExchangeSymbol"/>
              <xsl:with-param name="pCommodityCode" select="$pContractCode"/>
              <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
              <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
              <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$vAssetCategory='Index'">
            <!-- PM 20140326 [19794] Ajout gestion des Assets Indice -->
            <xsl:call-template name="QUOTE_XXX_H">
              <xsl:with-param name="pQuoteTable" select="'QUOTE_INDEX_H'"/>
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pExchangeAcronym" select="$vMarketExchangeSymbol"/>
              <xsl:with-param name="pCommodityCode" select="$pContractCode"/>
              <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
              <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
              <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:if>

    </r>
  </xsl:template>
  <!-- ============== IMSPANARRAY_H ===================================== -->
  <!-- ============== (Type "60" London) ================================ -->
  <xsl:template name="IMSPANARRAY_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. de la série -->
    <xsl:param name="pExchangeCode"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedContractCode"/>
    <xsl:param name="pContractCode"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pExpiryDate"/>
    <xsl:param name="pPutCall"/>

    <tbl n="IMSPANARRAY_H" a="IU">
      <!-- IDIMSPANCONTRACT_H -->
      <xsl:call-template name="Col_IDIMSPANCONTRACT_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
        <xsl:with-param name="pCommodityCode" select="$pContractCode"/>
        <xsl:with-param name="pContractType" select="$pContractType"/>
      </xsl:call-template>
      <!-- Identifiant Spheres de l'asset -->
      <c n="IDASSET" dk="true" t="i" v="parameters.IDASSET">
        <ctls>
          <ctl a="RejectRow" rt="true">
            <sl fn="IsNull()"/>
          </ctl>
        </ctls>
      </c>
      <!-- Categorie d'asset -->
      <c n="ASSETCATEGORY" dku="true" v="parameters.ASSETCATEGORY"/>
      <!-- Futures Contract Month (+ Day or Week Code) -->
      <c n="FUTMMY" dk="true" v="{$pExpiryDate}"/>
      <!-- PUT / CALL -->
      <c n="PUTCALL" dk="true" v="{$pPutCall}"/>
      <!-- Option Strike Price -->
      <c n="STRIKEPRICE" dk="true" t="dc" v="parameters.STRIKEPRICE"/>
      <!-- Composite Delta -->
      <c n="COMPOSITEDELTA" dku="true" t="dc" v="parameters.DELTA"/>
      <!-- Lot Size -->
      <c n="LOTSIZE" dku="true" t="dc" v="{./d[@n='LS']/@v}"/>
      <!-- Settlement Price -->
      <c n="PRICE" dku="true" t="dc" v="parameters.SETTLEMENTPRICE"/>
      <!-- risk value -->
      <xsl:for-each select="./d[(substring(@n,1,1)='A') and (number(substring-after(@n,'A')) = substring-after(@n,'A'))]">
        <!-- Variable Scenario -->
        <xsl:variable name="Scenario">
          <xsl:value-of select="substring-after(@n,'A')"/>
        </xsl:variable>
        <xsl:variable name="Value" select="number(translate(concat(../d[@n=concat('S',$Scenario)]/@v, @v),'+',' '))"/>
        <c dku="true" t="dc" n="{concat('RISKVALUE',$Scenario)}" v="{$Value}"/>
      </xsl:for-each>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
</xsl:stylesheet>
