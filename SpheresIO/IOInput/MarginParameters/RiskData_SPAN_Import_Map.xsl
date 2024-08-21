<?xml version="1.0" encoding="utf-8"?>
<!--
============================================================================================= 
 Summary: Import SPAN Risk Margin parameters
 File   : RiskData_SPAN_Import_Map.xsl
============================================================================================= 
 PM 20230302 [WI292] Add test on values before updating table DERIVATIVECONTRACT for columns
 EXERCISESTYLE, CABINETOPTVALUE, STRIKEDECLOCATOR and STRIKEALIGNCODE to update only for options
============================================================================================= 
 PM 20181016 [24260] The contract group of EuroDollar on CME is overwrited if CME is imported
 from XML file then SGX text file is imported
=============================================================================================  
 FI 20181015 [24198][24251] MONEP : Stock underlying price are missing
=============================================================================================  
 PM 20150707 [21104] Add column IDIMSPAN_H on table IMSPANGRPCTR_H
=============================================================================================  
 Version: v4.2.0.0    Date: 20141007    Author: PL
 Comment: Add a dot ('.') into SymbolSuffix. ('VAL' becomes '.VAL', 'IND' becomes '.IND')
============================================================================================= 
 PM 20140210 [19493] Gestion forcée de fichiers Intra-Day
=============================================================================================  
 FI 20140108 [19460] Les parameters SQL doivent être en majuscule 
=============================================================================================  
 BD 20110608 Importation XML PostMapping de type SPAN
=============================================================================================  
 PM Management of all record types usefull for margin calculation.
=============================================================================================  
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
==================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Parameters -->
  <xsl:param name="pPassNumber" select="1"/>

  <!-- Includes -->
  <xsl:include href=".\RiskData_SPAN_Import_Common.xsl"/>
  

  <!-- Variable for version identification -->
  <xsl:variable name="vXSLFileName">RiskData_SPAN_Import_Map.xsl</xsl:variable>
  <xsl:variable name="vXSLVersion">v4.2.0.0</xsl:variable>
  <xsl:variable name ="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>

  <!-- Le paramètre PRICEONLY active/désactive l'import des données de calcul de risque  -->
  <xsl:variable name ="vIsImportPriceOnly" >
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='PRICEONLY']='true'">true</xsl:when>
      <xsl:otherwise>false</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- PM 20130612 vIsImportFullFile is Deprecated-->
  <!--xsl:variable name ="vIsImportFullFile" >
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='FULLFILE']='true'">true</xsl:when>
      <xsl:otherwise>false</xsl:otherwise>
    </xsl:choose>
  </xsl:variable-->
  
  <xsl:variable name ="vAskedBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>

  <!--PM 20140212 [19493] Ajout variable pour forcer l'Intra-Day-->
  <xsl:variable name ="vForcedTime" select="/iotask/parameters/parameter[@id='TIME']"/>
  <xsl:variable name ="vForcedIntraDay">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='FORCEDINTRADAY']='true'">true</xsl:when>
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
  <!-- Calcul du prix de clôture "Aligné" pour les fichiers SPAN (autres que Paris Expanded) -->
  <xsl:template name="AlignedPrice">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <!-- others parameters -->
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pPrice"/>
    <xsl:param name="pPriceDecLocator"/>
    <!-- Pour le fichier UP (Expanded Paris) -->

    <xsl:choose>
      <xsl:when test="$pFileFormat = 'UP'">
        <!-- Mise en forme du Closing Price avec le decimal locator du fichier -->
        <xsl:call-template name="AttribDecLoc">
          <xsl:with-param name="pValue" select="$pPrice"/>
          <xsl:with-param name="pDecLoc" select="$pPriceDecLocator"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="number( $pPrice ) = 0">
            <xsl:attribute name="v">
              <!-- Prix à Zéro -->
              <xsl:value-of select="0"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <!-- Decodage du prix-->
            <xsl:choose>
              <xsl:when test="( $pCategory = 'O' ) and ( substring( $pPrice, string-length( $pPrice ) - 6, 3 ) = '999' )">
                <!-- Le prix est égal au Cabinet Option Value -->
                <xsl:choose>
                  <xsl:when test="number($pPrice) = 9999999">
                    <!-- Cabinet Option Value fixe -->
                    <xsl:call-template name="SQL_DERIVATIVECONTRACT">
                      <xsl:with-param name="pResultColumn" select="'CABINETOPT'"/>
                      <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                      <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
                      <xsl:with-param name="pContractSymbol" select="$pCommodityCode"/>
                      <xsl:with-param name="pCategory" select="$pCategory"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- PM 20131219 [19375] Ajout paramètres pPrice pour le calcul du Cabinet Price Variable en base 10000 et plus -->
                    <!-- Le prix est sous la forme '999????' : Variable Cabinet Option Value -->
                    <!-- Cabinet Option Value variable -->
                    <xsl:call-template name="SQL_DERIVATIVECONTRACT">
                      <xsl:with-param name="pResultColumn" select="'VARIABLECABINETOPT'"/>
                      <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                      <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
                      <xsl:with-param name="pContractSymbol" select="$pCommodityCode"/>
                      <xsl:with-param name="pCategory" select="$pCategory"/>
                      <xsl:with-param name="pPrice" select="number( substring( $pPrice, string-length( $pPrice ) - 3, 4 ) ) div 100"/>
                    </xsl:call-template>
                    <!--<xsl:attribute name="v">
                      <xsl:value-of select="number( substring( $pPrice, string-length( $pPrice ) - 3, 4 ) ) div 100"/>
                    </xsl:attribute>-->
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <!-- 3 derniers chiffres du prix -->
                <xsl:variable name="vLast3Digit" select="substring( $pPrice, string-length( $pPrice ) - 2, 3 )"/>
                <!-- RD 20130417 : Ne pas utiliser le code alignement (PRICEALIGNCODE) pour les DC en base 100 (Modification enlevé par FL le 20130507) -->
                <!-- FL 20130507 : Rajout dans le select de (case when dc.INSTRUMENTDEN = 10000 then 100 else 1 end) 
                     pour resoudre Pb XI: CORN Future/Option on XCBT ou le prix et le strike sont exprimés en Déciamles 
                    (Gestion de la base 10000) -->
                <!-- PM 20131022 : (case when dc.INSTRUMENTDEN = 10000 then 100 else 1 end) => (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end)-->
                <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
                <!-- RD 20160621 [22271] Utiliser un "cast" explicite pour le résultat de la fonction "floor" -->
                <sql cd="select" rt="PRICE" t="dc" cache="true">
                  select (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end) * cast(floor(case when (dc.PRICEALIGNCODE is null) then case when dc.PRICEDECLOCATOR is null then @PRICEVALUE else @PRICEVALUE / power(10, dc.PRICEDECLOCATOR) end
                  else case when (dc.PRICEALIGNCODE = '4') and (dc.PRICEDECLOCATOR = 3) then (@PRICEVALUE / power(10, 4))
                  /* '0' => Base 100 : Avec conversion du dernier chiffre decimal */
                  else case when (dc.PRICEALIGNCODE = '0') then floor(@PRICEVALUE / power(10, dc.PRICEDECLOCATOR))
                  +    case when (@LASTDIGIT = 1) then ((@LAST2DIGIT / 100.0) + 0.00125)
                  else case when (@LASTDIGIT = 2) then ((@LAST2DIGIT / 100.0) + 0.00250)
                  else case when (@LASTDIGIT = 3) then ((@LAST2DIGIT / 100.0) + 0.00375)
                  else case when (@LASTDIGIT = 5) then ((@LAST2DIGIT / 100.0) + 0.00500)
                  else case when (@LASTDIGIT = 6) then ((@LAST2DIGIT / 100.0) + 0.00625)
                  else case when (@LASTDIGIT = 7) then ((@LAST2DIGIT / 100.0) + 0.00750)
                  else case when (@LASTDIGIT = 8) then ((@LAST2DIGIT / 100.0) + 0.00875)
                  else ( @LAST3DIGIT / 1000.0 ) end end end end end end end
                  else case when (dc.PRICEALIGNCODE = '7') then floor(@PRICEVALUE / power(10, dc.PRICEDECLOCATOR)) + (@LAST2DIGIT * 0.0015630) - (floor(@LAST2DIGIT / 2.0) / 100000.0)
                  else case when (dc.PRICEALIGNCODE = '9') then floor(@PRICEVALUE / power(10, dc.PRICEDECLOCATOR)) + (@LAST2DIGIT * 0.003125)
                  else case when (dc.PRICEALIGNCODE = 'B') then floor(@PRICEVALUE / power(10, dc.PRICEDECLOCATOR)) + (@LAST2DIGIT * 0.003126) - (floor(@LAST3DIGIT * 0.1) / 100000.0)
                  /* 'C' => Base 32 : Pas de conversion + Ajout 0.0005 si dernier chiffre = 2 ou 7 */
                  else case when (dc.PRICEALIGNCODE = 'C') then (floor(@PRICEVALUE / power(10, dc.PRICEDECLOCATOR)))
                  +    case when (@LASTDIGIT = 2) or (@LASTDIGIT = 7) then ((@LAST3DIGIT / 1000.0) + 0.0005)
                  else (@LAST3DIGIT / 1000.0) end
                  /* 'K' => Base 64 : Pas de conversion */
                  else case when dc.PRICEALIGNCODE = 'K' then (floor(@PRICEVALUE / power(10, dc.PRICEDECLOCATOR)) + (@LAST3DIGIT / 1000.0))
                  else @PRICEVALUE / power( 10, dc.PRICEDECLOCATOR ) end end end end end end end
                  end * 10000000) as decimal(26,9)) / 10000000.0 PRICE
                  from dbo.DERIVATIVECONTRACT dc
                  inner join dbo.MARKET mk on ( mk.IDM = dc.IDM ) and ( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM )
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
                  <!-- 3 derniers chiffres du prix -->
                  <p n="LAST3DIGIT" t="dc" v="{number( $vLast3Digit )}"/>
                  <!-- Dernier chiffre du prix -->
                  <p n="LASTDIGIT" t="dc" v="{number( substring( $vLast3Digit, 3, 1 ) )}"/>
                  <!-- 2 premier chiffres des 3 derniers chiffres du prix -->
                  <p n="LAST2DIGIT" t="dc" v="{number( substring( $vLast3Digit, 1, 2 ) )}"/>
                </sql>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- PM 20140116 [19456] Nouveau template -->
  <!-- SQL de lecture de la table DERIVATIVECONTRACT pour une Option sur Future -->
  <xsl:template name="SQL_DERIVATIVECONTRACT_OPT_UNL_FUT">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>

    <sql cd="select" cache="true">
      <xsl:attribute name="rt">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      select da.IDASSET,
      case when dc.FINALSETTLTTIME is null then convert(varchar,@BUSINESSDATETIME,120) else substring(convert(varchar,@BUSINESSDATETIME,120),1,11) || dc.FINALSETTLTTIME || ':00' end as FINALSETTLTTIME
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
      inner join dbo.MATURITY m on (m.IDMATURITY = da.IDMATURITY)
      inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
      where (dc.CATEGORY = 'O')
      and (dc.ASSETCATEGORY = 'Future')
      and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL)
      and (m.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)
      and ( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM )
      and ( ( mk.DTDISABLED is null ) or ( mk.DTDISABLED > @BUSINESSDATETIME ) )
      <!--and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )-->
      and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME))
      <!--and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
      and ((da.DTDISABLED is null) or (da.DTDISABLED > @BUSINESSDATETIME))
      <!--and ( ( da.DTENABLED is null ) or ( da.DTENABLED &lt;= @BusinessDateTime ) )-->
      <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
      <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
      <p n="CONTRACTSYMBOL" v="{$pContractSymbol}"/>
      <p n="MATURITYMONTHYEAR" v="{$pMaturityMonthYear}"/>
    </sql>
  </xsl:template>
  <!-- ================================================================== -->

  <!-- Initialisation des variables provenant du fichier -->
  <!-- Lancement de l'import -->
  <xsl:template name="StartImport">
    <!--PM 20140212 [19493] Variable pour l'heure des données-->
    <xsl:variable name="vBusinessTime">
      <xsl:choose>
        <xsl:when test="$vForcedTime and normalize-space($vForcedTime)!=''">
          <xsl:value-of select="$vForcedTime"/>
        </xsl:when>
        <xsl:otherwise>
          <!--RD 20180612 [23770] Ne plus valoriser l'heure à partir de la donnée date système du fichier, issue du record 0 (BT - Business Time)-->
          <xsl:value-of select="'00:00:00'"/>
          <!--<xsl:choose>
            <xsl:when test="./r[1]/d[@n='BT']/@v and normalize-space(./r[1]/d[@n='BT']/@v)!=''">
              <xsl:value-of select="./r[1]/d[@n='BT']/@v"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'00:00:00'"/>
            </xsl:otherwise>
          </xsl:choose>-->
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Asked Business Date & Time -->
    <!--PM 20140212 [19493] Utilisation de la variable $vBusinessTim-->
    <xsl:variable name="vAskedBusinessDateTime" select="concat($vAskedBusinessDate, ' ', $vBusinessTime)"/>
      <!--<xsl:choose>
        <xsl:when test="./r[1]/d[@n='BT']/@v and normalize-space(./r[1]/d[@n='BT']/@v)!=''">
          <xsl:value-of select="concat($vAskedBusinessDate, ' ', ./r[1]/d[@n='BT']/@v)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat($vAskedBusinessDate, ' 00:00:00')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>-->
    <!-- Business Date & Time -->
    <!--PM 20140212 [19493] Utilisation de la variable $vBusinessTim-->
    <xsl:variable name="vBusinessDateTime" select="concat(./r[1]/d[@n='BD']/@v, ' ', $vBusinessTime)"/>
    <xsl:variable name="vBusinessDate" select="./r[1]/d[@n='BD']/@v"/>
    <!--<xsl:choose>
        <xsl:when test="./r[1]/d[@n='BT']/@v and normalize-space(./r[1]/d[@n='BT']/@v)!=''">
          <xsl:value-of select="concat(./r[1]/d[@n='BD']/@v, ' ', ./r[1]/d[@n='BT']/@v)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat(./r[1]/d[@n='BD']/@v, ' 00:00:00')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>-->
    <!-- Settlement or Intraday -->
    <!--PM 20140212 [19493] Prise en compte de la variable $vForcedIntraDay-->
    <xsl:variable name="vSettlementorIntraday">
      <xsl:choose>
        <xsl:when test="(./r[1]/d[@n='SeoIn']/@v = 'I') or ($vForcedIntraDay = 'true')">ITD</xsl:when>
        <xsl:otherwise>EOD</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- File Format -->
    <!-- U => Standard Unpacked -->
    <!-- P => Standard Packed (Non utilisé) -->
    <!-- U2 => Expanded Unpacked -->
    <!-- UP => Expanded Paris -->
    <xsl:variable name="vFileFormat" select="./r[1]/d[@n='FF']/@v"/>
    <!-- Exchange Complex -->
    <!-- Exchange Complex for RID = 0 et Exchange Code for RID = 1 -->
    <xsl:variable name="vExchangeComplex" select="./r[1]/d[@n='EC']/@v"/>
    <!-- Ecriture da la première ligne de données dans la table IMSPAN_H -->
    <xsl:if test="($pPassNumber = 1) and ($vIsImportPriceOnly='false')">
      <xsl:call-template name="IMSPAN_H">
        <xsl:with-param name="pFileFormat" select="$vFileFormat"/>
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
      <xsl:with-param name="pFileFormat" select="$vFileFormat"/>
      <xsl:with-param name="pBusinessDateTime" select="$vBusinessDateTime"/>
      <xsl:with-param name="pSettlementorIntraday" select="$vSettlementorIntraday"/>
      <xsl:with-param name="pExchangeComplex" select="$vExchangeComplex"/>
      <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
    </xsl:apply-templates>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ================== TEMPLATE PRINCIPALE R (ROW) =================== -->
  <xsl:template match="r">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
    <xsl:param name="pBusinessDate"/>
    
    <!--Traitement en fonction du type d'enregistrement RID-->
    <xsl:choose>
      <xsl:when test="$vIsImportPriceOnly='false'">
        <xsl:choose>
          <!--RID = 1-->
          <xsl:when test="./d[@n='RID']/@v = '1'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANEXCHANGE_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
            <!-- Pour la gestion de niveaux par Exchange (RID=1) aus sein d'un même fichier -->
            <xsl:if test="./r">
              <xsl:apply-templates select="r">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:apply-templates>
            </xsl:if>
          </xsl:when>
          <!--RID = 2-->
          <xsl:when test="./d[@n='RID']/@v = '2'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANGRPCTR_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANCONTRACT_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
          </xsl:when>
          <!--RID = 3-->
          <xsl:when test="./d[@n='RID']/@v = '3'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANGRPCTR_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
              <xsl:call-template name="IMSPANTIER_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pCombinedContractCode" select="./d[@n='CC']/@v"/>
              </xsl:call-template>
            </r>
          </xsl:when>
          <!--RID = 4-->
          <xsl:when test="./d[@n='RID']/@v = '4'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANGRPCTR_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
            <!-- Si la methode de calcul de la Charge de Livraison est 10 qui nécessite une table de paramètrage -->
            <xsl:if test="./d[@n='DM']/@v='10'">
              <r uc="true">
                <xsl:call-template name="RowAttributes"/>
                <xsl:call-template name="IMSPANDLVMONTH_H">
                  <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                  <xsl:with-param name="pCombinedContractCode" select="./d[@n='CC']/@v"/>
                </xsl:call-template>
              </r>
            </xsl:if>
          </xsl:when>
          <!--RID = 5-->
          <xsl:when test="./d[@n='RID']/@v = '5'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANGRPCOMB_H">
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANGRPCTR_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
          </xsl:when>
          <!--RID = 6-->
          <xsl:when test="./d[@n='RID']/@v = '6'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANGRPCOMB_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
              <xsl:call-template name="IMSPANINTERSPR_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <!-- PM 20131112 [19165][19144] Ajout pSpreadPriority -->
                <xsl:with-param name="pSpreadPriority" select="./d[@n='SP']/@v"/>
              </xsl:call-template>
            </r>
            <r uc="true">
              <xsl:call-template name="RowAttributes">
                <xsl:with-param name="pInfo" select="leg"/>
              </xsl:call-template>
              <xsl:call-template name="IMSPANINTERLEG_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <!-- PM 20131112 [19165][19144] Ajout pSpreadPriority -->
                <xsl:with-param name="pSpreadPriority" select="./d[@n='SP']/@v"/>
              </xsl:call-template>
            </r>
          </xsl:when>
          <!--RID = "81" à "84"-->
          <xsl:when test="substring(./d[@n='RID']/@v, 1, 1) = '8'">
            <xsl:call-template name="Record8">
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            </xsl:call-template>
          </xsl:when>
          <!--RID = B-->
          <xsl:when test="./d[@n='RID']/@v = 'B'">
            <!-- PM 20140116 [19456] Déplacement de la gestion du record B dans un nouveau template -->
            <xsl:call-template name="RecordB">
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            </xsl:call-template>
          </xsl:when>
          <!--RID = C ou E-->
          <xsl:when test="(./d[@n='RID']/@v = 'C') or (./d[@n='RID']/@v = 'E')">
            <xsl:call-template name="RowINTRASPREAD">
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pCombinedContractCode" select="./d[@n='CC']/@v"/>
            </xsl:call-template>
          </xsl:when>
          <!--RID = P-->
          <xsl:when test="./d[@n='RID']/@v = 'P'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="P_IMSPANCONTRACT_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
          </xsl:when>
          <!--RID = S-->
          <xsl:when test="./d[@n='RID']/@v = 'S' and normalize-space(./d[@n='TN1']/@v)!=''">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="IMSPANTIER_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pCombinedContractCode" select="./d[@n='CC']/@v"/>
              </xsl:call-template>
            </r>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$vIsImportPriceOnly='true'">
        <xsl:choose>
          <!--RID = "81" à "84"-->
          <xsl:when test="substring(./d[@n='RID']/@v, 1, 1) = '8'">
            <xsl:call-template name="Record8">
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            </xsl:call-template>
          </xsl:when>
          <!-- FI 20220105 [XXXXX] call template RecordB -->
          <!--RID = B-->
          <xsl:when test="./d[@n='RID']/@v = 'B'">
            <xsl:call-template name="RecordB">
              <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            </xsl:call-template>
          </xsl:when>
          <!--RID = P-->
          <xsl:when test="./d[@n='RID']/@v = 'P'">
            <r uc="true">
              <xsl:call-template name="RowAttributes"/>
              <xsl:call-template name="P_IMSPANCONTRACT_H">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
              </xsl:call-template>
            </r>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- ============== IMSPANGRPCTR_H ================================= -->
  <!-- ============== (Type "2", "3", "4", "5", "S" S, E, P) ======== -->
  <xsl:template name="IMSPANGRPCTR_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <!-- Si Typde d'enregistrement : 2, 3 ou 4 -->
    <xsl:if test="./d[@n='RID']/@v != '5'">
      <tbl n="IMSPANGRPCTR_H" a="IU">
        <!--PM 20150707 [21104] Add column IDIMSPAN_H-->
        <!-- IDIMSPAN_H -->
        <xsl:call-template name="Col_IDIMSPAN_H">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        </xsl:call-template>
        <!-- Exchange Acronym -->
        <c n="IDIMSPANEXCHANGE_H" dk="true" t="i">
          <xsl:choose>
            <!-- TYPE 2 -->
            <xsl:when test="./d[@n='RID']/@v ='2'">
              <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
              <sql cd="select" rt="IDIMSPANEXCHANGE_H" cache="true">
                select e.IDIMSPANEXCHANGE_H
                from dbo.IMSPANEXCHANGE_H e<xsl:value-of select="$vSpace"/>
                <!--Verif IDIMSPAN_H-->
                <xsl:call-template name="innerIDIMSPAN_H">
                  <xsl:with-param name="pTable" select="'e'"/>
                </xsl:call-template>
                and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )<xsl:value-of select="$vSpace"/>
                <!--Param. IDIMSPAN_H-->
                <xsl:call-template name="paramInnerIDIMSPAN_H">
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                </xsl:call-template>
                <xsl:choose>
                  <xsl:when test="$pFileFormat!='U'">
                    <p n="EXCHANGEACRONYM" v="{./d[@n='EA']/@v}"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <p n="EXCHANGEACRONYM" v="{$pExchangeComplex}"/>
                  </xsl:otherwise>
                </xsl:choose>
              </sql>
            </xsl:when>
            <xsl:otherwise>
              <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
              <sql cd="select" rt="IDIMSPANEXCHANGE_H" cache="true">
                select g.IDIMSPANEXCHANGE_H
                from dbo.IMSPANGRPCTR_H g
                inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )<xsl:value-of select="$vSpace"/>
                <xsl:call-template name="innerIDIMSPAN_H">
                  <xsl:with-param name="pTable" select="'e'"/>
                </xsl:call-template>
                and ( g.COMBCOMCODE = @COMBINEDCOMMODITY )
                <!--Param. IDIMSPAN_H-->
                <xsl:call-template name="paramInnerIDIMSPAN_H">
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                </xsl:call-template>
                <p n="COMBINEDCOMMODITY" v="{./d[@n='CC']/@v}"/>
              </sql>
            </xsl:otherwise>
          </xsl:choose>
        </c>
        <!-- Combined Commodity Code -->
        <c n="COMBCOMCODE" dk="true" v="{./d[@n='CC']/@v}">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsEmpty()"/>
              <li st="REJECT" ex="true">
                <c>SYS</c>
                <n>2001</n>
                <d1>
                  <xsl:value-of select="$vXSLFileName"/>
                </d1>
                <d2>
                  <xsl:value-of select="$vXSLVersion"/>
                </d2>
              </li>
            </ctl>
          </ctls>
        </c>
        <xsl:choose>
          <!-- TYPE 2 -->
          <xsl:when test="./d[@n='RID']/@v ='2'">
            <!-- Limit Option Value -->
            <c n="ISOPTIONVALUELIMIT" dku="true" t="b">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="./d[@n='LOV']/@v = 'Y'">1</xsl:when>
                  <xsl:otherwise>0</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
            <!-- Risk Exponent -->
            <!-- PM 20131120 [19220] Mettre 0 si absent ou vide -->
            <!--<c n="RISKEXPONENT" dku="true" t="i" v="{./d[@n='RE']/@v}">
                <ctls>
                <ctl a="RejectColumn" rt="true" lt="None">
                  <sl fn="IsEmpty()"/>
                </ctl>
              </ctls>-->
            <c n="RISKEXPONENT" dku="true" t="i">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="normalize-space(./d[@n='RE']/@v) = ''">0</xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="./d[@n='RE']/@v"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
            <!-- Option Margin Style -->
            <c n="FUTVALUATIONMETHOD" dku="true">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="./d[@n='OM']/@v = 'F'">FUT</xsl:when>
                  <xsl:otherwise>EQTY</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
            <!-- Combination Margining Method Flag -->
            <c n="COMBMARGININGMTH" dku="true" v="{./d[@n='CMM']/@v}">
              <ctls>
                <ctl a="RejectColumn" rt="true" lt="None">
                  <sl fn="IsEmpty()"/>
                </ctl>
              </ctls>
            </c>
            <!--EXPANDED ET PARIS EXPANDED-->
            <xsl:if test="$pFileFormat!='U'">
              <!-- Performance Bond Currency ISO Code -->
              <c n="IDC_PB" dku="true" v="{./d[@n='PBCur']/@v}">
                <ctls>
                  <ctl a="RejectColumn" rt="true" lt="None">
                    <sl fn="IsEmpty()"/>
                  </ctl>
                </ctls>
              </c>
              <!-- Calculation Algorithm Code (PARIS EXPANDED) -->
              <xsl:if test="$pFileFormat='UP'">
                <c n="ALGORITHMCODE" dku="true" v="{./d[@n='CA']/@v}"/>
              </xsl:if>
            </xsl:if>
            <!-- ================== -->
            <!-- Valeurs par défaut -->
            <!-- ================== -->
            <!-- Intracommodity Spread Charge Method Code -->
            <c n="INTRASPREADMETHOD" dku="true" v="01"/>
            <!-- Delivery (Spot) Charge Method Code -->
            <c n="DELIVERYCHARGEMETH" dku="true" v="01"/>
            <!-- Members -->
            <c n="MEMBERADJFACTOR" dku="true" t="dc" v="1"/>
            <!-- Hedgers-->
            <c n="HEDGERADJFACTOR" dku="true" t="dc" v="1"/>
            <!-- Speculators -->
            <c n="SPECULATADJFACTOR" dku="true" t="dc" v="1"/>
            <!-- Short Option Minimum Calculation Method -->
            <c n="SOMMETHOD" dku="true" v="2"/>
            <!-- InfoTable_IU -->
            <!-- Intercommodity Spreading Method Code -->
            <c n="INTERSPREADMETHOD" dku="true" v="01"/>
            <!-- RD/PM 20130502 [18623] Initialiser la méthode à null -->
            <!-- Weighted Futures Price Risk Calculation Method -->
            <c n="WEIGHTEDRISKMETHOD" dku="true" v="null"/>
            <!-- Margin Currency Code -->
            <c n="IDC" dku="true">
              <xsl:attribute name="v">
                <xsl:call-template name="ValueOrNull">
                  <xsl:with-param name="pValue" select="./d[@n='PBCur']/@v"/>
                </xsl:call-template>
              </xsl:attribute>
            </c>
            <xsl:call-template name="InfoTable_IU"/>
          </xsl:when>
          <!--TYPE 3-->
          <xsl:when test="./d[@n='RID']/@v ='3'">
            <!-- Intracommodity Spread Charge Method Code -->
            <xsl:if test="normalize-space(./d[@n='IASM']/@v)!=''">
              <c n="INTRASPREADMETHOD" dku="true" v="{./d[@n='IASM']/@v}"/>
            </xsl:if>
            <!-- Initial to Maintenance Ratio _ Member Accounts -->
            <c n="MEMBERITOMRATIO" dku="true" t="dc">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="normalize-space(./d[@n='MR']/@v)!=''">
                    <xsl:choose>
                      <xsl:when test="$pFileFormat = 'UP'">
                        <xsl:call-template name="SetDecLoc">
                          <xsl:with-param name="pValue" select="./d[@n='MR']/@v"/>
                          <xsl:with-param name="pDecLoc" select="./d[@n='MRD']/@v"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="number(./d[@n='MR']/@v) div 1000"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>1</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
            <!-- Initial to Maintenance Ratio _ Hedger Accounts -->
            <c n="HEDGERITOMRATIO" dku="true" t="dc">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="normalize-space(./d[@n='HR']/@v)!=''">
                    <xsl:choose>
                      <xsl:when test="$pFileFormat = 'UP'">
                        <xsl:call-template name="SetDecLoc">
                          <xsl:with-param name="pValue" select="./d[@n='HR']/@v"/>
                          <xsl:with-param name="pDecLoc" select="./d[@n='HRD']/@v"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="number(./d[@n='HR']/@v) div 1000"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>1</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
            <!-- Initial to Maintenance Ratio _ Speculator Accounts -->
            <c n="SPECULATITOMRATIO" dku="true" t="dc">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="normalize-space(./d[@n='SR']/@v)!=''">
                    <xsl:choose>
                      <xsl:when test="$pFileFormat = 'UP'">
                        <xsl:call-template name="SetDecLoc">
                          <xsl:with-param name="pValue" select="./d[@n='SR']/@v"/>
                          <xsl:with-param name="pDecLoc" select="./d[@n='SRD']/@v"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="number(./d[@n='SR']/@v) div 1000"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>1</xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
          </xsl:when>
          <!--TYPE 4-->
          <xsl:when test="./d[@n='RID']/@v ='4'">
            <!-- Delivery (Spot) Charge Method Code -->
            <xsl:if test="normalize-space(./d[@n='DM']/@v)!=''">
              <c n="DELIVERYCHARGEMETH" dku="true" v="{./d[@n='DM']/@v}"/>
            </xsl:if>
            <!-- Short Option Minimum Calculation Method -->
            <xsl:if test="normalize-space(./d[@n='SOMM']/@v)!=''">
              <c n="SOMMETHOD" dku="true" v="{./d[@n='SOMM']/@v}"/>
            </xsl:if>
            <!-- Number of contract months in delivery -->
            <c n="NBOFDELIVERYMONTH" dku="true" t="i" v="{./d[@n='NM']/@v}">
              <ctls>
                <ctl a="ApplyDefault" rt="true" lt="None">
                  <sl fn="IsEmpty()"/>
                </ctl>
              </ctls>
              <df v="0"/>
            </c>
            <xsl:choose>
              <xsl:when test="$pFileFormat='UP'">
                <!-- Risk Maintenance Performance Bond Adjustment Factor -->
                <!-- Members -->
                <c n="MEMBERADJFACTOR" dku="true" t="dc">
                  <xsl:call-template name="AttribDecLoc">
                    <xsl:with-param name="pValue" select="./d[@n='MF']/@v"/>
                    <xsl:with-param name="pDecLoc" select="./d[@n='MFD']/@v"/>
                    <xsl:with-param name="pDefault" select="1"/>
                  </xsl:call-template>
                </c>
                <!-- Hedgers-->
                <c n="HEDGERADJFACTOR" dku="true" t="dc">
                  <xsl:call-template name="AttribDecLoc">
                    <xsl:with-param name="pValue" select="./d[@n='HF']/@v"/>
                    <xsl:with-param name="pDecLoc" select="./d[@n='HFD']/@v"/>
                    <xsl:with-param name="pDefault" select="1"/>
                  </xsl:call-template>
                </c>
                <!-- Speculators -->
                <c n="SPECULATADJFACTOR" dku="true" t="dc">
                  <xsl:call-template name="AttribDecLoc">
                    <xsl:with-param name="pValue" select="./d[@n='SF']/@v"/>
                    <xsl:with-param name="pDecLoc" select="./d[@n='SFD']/@v"/>
                    <xsl:with-param name="pDefault" select="1"/>
                  </xsl:call-template>
                </c>
                <!-- Short Option Minimum Charge Rate -->
                <c n="SOMCHARGERATE" dku="true" t="dc">
                  <xsl:call-template name="AttribDecLoc">
                    <xsl:with-param name="pValue" select="./d[@n='SOM']/@v"/>
                    <xsl:with-param name="pDecLoc" select="./d[@n='SOMD']/@v"/>
                  </xsl:call-template>
                </c>
              </xsl:when>
              <xsl:otherwise>
                <!-- Risk Maintenance Performance Bond Adjustment Factor -->
                <!-- Members -->
                <c n="MEMBERADJFACTOR" dku="true" t="dc">
                  <xsl:attribute name="v">
                    <xsl:choose>
                      <xsl:when test="normalize-space(./d[@n='MF']/@v)!=''">
                        <xsl:value-of select="number(./d[@n='MF']/@v) div 100"/>
                      </xsl:when>
                      <xsl:otherwise>1</xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                </c>
                <!-- Hedgers-->
                <c n="HEDGERADJFACTOR" dku="true" t="dc">
                  <xsl:attribute name="v">
                    <xsl:choose>
                      <xsl:when test="normalize-space(./d[@n='HF']/@v)!=''">
                        <xsl:value-of select="number(./d[@n='HF']/@v) div 100"/>
                      </xsl:when>
                      <xsl:otherwise>1</xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                </c>
                <!-- Speculators -->
                <c n="SPECULATADJFACTOR" dku="true" t="dc">
                  <xsl:attribute name="v">
                    <xsl:choose>
                      <xsl:when test="normalize-space(./d[@n='SF']/@v)!=''">
                        <xsl:value-of select="number(./d[@n='SF']/@v) div 100"/>
                      </xsl:when>
                      <xsl:otherwise>1</xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                </c>
                <!-- Short Option Minimum Charge Rate -->
                <c n="SOMCHARGERATE" dku="true" t="dc" v="{./d[@n='SOM']/@v}"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!--TYPE S-->
          <xsl:when test="./d[@n='RID']/@v ='S'">
            <!-- Intercommodity Spreading Method Code -->
            <xsl:if test="normalize-space(./d[@n='SIRSM']/@v)!=''">
              <c n="INTERSPREADMETHOD" dku="true" v="{./d[@n='SIRSM']/@v}"/>
            </xsl:if>
            <!-- Weighted Futures Price Risk Calculation Method -->
            <xsl:if test="normalize-space(./d[@n='WFPMF']/@v)!=''">
              <c n="WEIGHTEDRISKMETHOD" dku="true" v="{./d[@n='WFPMF']/@v}"/>
            </xsl:if>
          </xsl:when>
        </xsl:choose>
      </tbl>
    </xsl:if>
    <!-- Si Typde d'enregistrement : 5 -->
    <xsl:if test="./d[@n='RID']/@v ='5'">
      <pms>
        <pm n="R5_IDIMSPANGRPCOMB_H" >
          <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
          <sql cd="select" rt="IDIMSPANGRPCOMB_H">
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
            <p n="COMBINEDGROUPCODE" v="{./d[@n='CG']/@v}"/>
          </sql>
        </pm>
      </pms>

      <xsl:for-each select="./d[(substring(@n,1,2)='CC') and (number(substring-after(@n,'CC')) = substring-after(@n,'CC'))]">
        <!-- Si le Contrat est renseigné -->
        <xsl:if test="normalize-space(@v)!=''">
          <tbl n="IMSPANGRPCTR_H" a="U">
            <!-- PM 20181016 [24260] Ajout IDIMSPAN_H en datakey -->
            <!-- IDIMSPAN_H -->
            <xsl:call-template name="Col_IDIMSPAN_H">
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            </xsl:call-template>
            <!-- IDIMSPANEXCHANGE_H -->
            <c n="IDIMSPANEXCHANGE_H" dk="true" t="i">
              <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
              <sql cd="select" rt="IDIMSPANEXCHANGE_H" cache="true">
                select e.IDIMSPANEXCHANGE_H
                from dbo.IMSPANEXCHANGE_H e
                inner join dbo.IMSPANGRPCTR_H g on (g.IDIMSPANEXCHANGE_H = e.IDIMSPANEXCHANGE_H)<xsl:value-of select="$vSpace"/>
                <!--Verif IDIMSPAN_H-->
                <xsl:call-template name="innerIDIMSPAN_H">
                  <xsl:with-param name="pTable" select="'e'"/>
                </xsl:call-template>
                and (g.COMBCOMCODE = @COMBINEDCOMMODITY)<xsl:value-of select="$vSpace"/>
                <xsl:call-template name="paramInnerIDIMSPAN_H">
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                </xsl:call-template>
                <p n="COMBINEDCOMMODITY" v="{@v}"/>
              </sql>
            </c>
            <!-- COMBCOMCODE -->
            <c n="COMBCOMCODE" dk="true" v="{@v}"/>
            <!-- IDIMSPANGRPCOMB_H -->
            <c n="IDIMSPANGRPCOMB_H" dku="true" t="i" v="parameters.R5_IDIMSPANGRPCOMB_H"/>
          </tbl>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANCONTRACT_H (Type "P" S, E, P) =============== -->
  <xsl:template  name="P_IMSPANCONTRACT_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <xsl:variable name="vExchangeAcronym" select="./d[@n='EA']/@v"/>
    <xsl:variable name="vContractSymbol" select="./d[@n='CP']/@v"/>
    <xsl:variable name="vCategory" >
      <xsl:choose>
        <xsl:when test="./d[@n='PT']/@v ='FUT'">F</xsl:when>
        <xsl:when test="./d[@n='PT']/@v ='OOF'">O</xsl:when>
        <xsl:when test="./d[@n='PT']/@v ='OOP'">O</xsl:when>
        <xsl:when test="./d[@n='PT']/@v ='OOC'">O</xsl:when>
        <xsl:when test="./d[@n='PT']/@v ='OOS'">O</xsl:when>
        <xsl:otherwise></xsl:otherwise>
        <!-- Si ni Option ni Future Alors vide -->
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vContractMultiplier">
      <xsl:call-template name="TenExponent">
        <xsl:with-param name="pValue" select="number(./d[@n='M']/@v) div 10000000"/>
        <xsl:with-param name="pExp" select="./d[@n='ME']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- PM 20131202 [19275] Ajout Factor -->
    <xsl:variable name="vFactor">
      <xsl:choose>
        <xsl:when test="./d[@n='PT']/@v ='OOF'">1</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="round($vContractMultiplier)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vFutValuationMethod"  select="./d[@n='VM']/@v"/>
    <xsl:variable name="vIDC_Price" select="./d[@n='PCur']/@v"/>
    <xsl:variable name="vPriceQuotationMethod">
      <xsl:choose>
        <xsl:when test="./d[@n='PQM']/@v='IDX'">INX</xsl:when>
        <xsl:when test="./d[@n='PQM']/@v='INT'">INT</xsl:when>
        <xsl:when test="./d[@n='PQM']/@v='STD'">STD</xsl:when>
        <xsl:otherwise>STD</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vExerciseStyle">
      <xsl:choose>
        <xsl:when test="./d[@n='ES']/@v='EURO'">0</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vCabinetOptionValue">
      <xsl:call-template name="NumDiv">
        <xsl:with-param name="pValue" select="./d[@n='COV']/@v"/>
        <xsl:with-param name="pDiv" select="100"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vPriceDecLocator">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='PD']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vStrikeDecLocator">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='SD']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vPriceAlignCode">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='PAC']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vStrikeAlignCode">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='SAC']/@v"/>
      </xsl:call-template>
    </xsl:variable>

    <pms>
      <pm n="IDDC">
        <xsl:if test="$vCategory='F' or $vCategory='O'">
          <xsl:call-template name="SQL_DERIVATIVECONTRACT">
            <xsl:with-param name="pResultColumn" select="'IDDC'"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
            <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
            <xsl:with-param name="pCategory" select="$vCategory"/>
          </xsl:call-template>
        </xsl:if>
      </pm>
      <pm n="IDC_PRICE">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDC" cache="true">
          select IDC from dbo.CURRENCY where IDC = @IDC_PRICE
          <p n="IDC_PRICE" v="{$vIDC_Price}"/>
        </sql>
      </pm>
    </pms>

    <xsl:if test="$vIsImportPriceOnly='false'">
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
            and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )
            <xsl:call-template name="paramInnerIDIMSPAN_H">
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
              <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            </xsl:call-template>
            <!-- Param Exchange Code / Acronym -->
            <p n="EXCHANGEACRONYM" v="{$vExchangeAcronym}"/>
          </sql>
        </c>
        <!-- Commodity (Product) Code -->
        <c n="CONTRACTSYMBOL" dk="true" v="{$vContractSymbol}"/>
        <!-- Contract Type -> Colonne CONTRACTTYPE (S, E et P) -->
        <c n="CONTRACTTYPE" dk="true">
          <xsl:attribute name="v">
            <xsl:call-template name="ValueOrNull">
              <xsl:with-param name="pValue" select="./d[@n='PT']/@v"/>
            </xsl:call-template>
          </xsl:attribute>
        </c>
        <c n="CONTRACTMULTIPLIER" dku="true" t="dc" v="{$vContractMultiplier}"/>
        <c n="PRODUCTNAME" dku="true" v="{./d[@n='PN']/@v}"/>
        <c n="PRODUCTLONGNAME" dku="true" v="{./d[@n='PLN']/@v}"/>
        <c n="PRICEDECLOCATOR" dku="true" t="i" v="{$vPriceDecLocator}"/>
        <c n="STRIKEDECLOCATOR" dku="true" t="i" v="{$vStrikeDecLocator}"/>
        <c n="PRICEALIGNCODE" dku="true" v="{$vPriceAlignCode}"/>
        <c n="STRIKEALIGNCODE" dku="true" v="{$vStrikeAlignCode}"/>
        <c n="CABINETOPTVALUE" dku="true" t="dc" v="{$vCabinetOptionValue}"/>
        <c n="IDC_PRICE" dku="true" v="parameters.IDC_PRICE"/>
        <c n="PRICEQUOTEMETHOD" dku="true" v="{$vPriceQuotationMethod}"/>
        <c n="EXERCISESTYLE" dku="true" v="{$vExerciseStyle}"/>
        <c n="FUTVALUATIONMETHOD" dku="true" v="{$vFutValuationMethod}"/>
        <xsl:if test="normalize-space(./d[@n='VSROM']/@v)!=''">
          <c n="VOLATSCANQUOTEMETH" dku="true" v="{./d[@n='VSROM']/@v}"/>
        </xsl:if>
        <xsl:if test="normalize-space(./d[@n='PSRQM']/@v)!=''">
          <c n="PRICESCANQUOTEMETH" dku="true" v="{./d[@n='PSRQM']/@v}"/>
        </xsl:if>
        <xsl:if test="normalize-space(./d[@n='PSRVT']/@v)!=''">
          <c n="PRICESCANVALTYPE" dku="true" v="{./d[@n='PSRVT']/@v}"/>
        </xsl:if>
        <xsl:if test="normalize-space(./d[@n='QPC']/@v)!=''">
          <c n="PRODUCTPERCONTRACT" dku="true" t="i" v="{./d[@n='QPC']/@v}"/>
        </xsl:if>
        <!-- PM 20130805 Gestion tick variable -->
        <xsl:choose>
          <xsl:when test="normalize-space(./d[@n='VTOF']/@v)='1'">
            <c n="ISOPTVARIABLETICK" dku="true" t="b" v="1"/>
          </xsl:when>
          <xsl:otherwise>
            <c n="ISOPTVARIABLETICK" dku="true" t="b" v="0"/>
          </xsl:otherwise>
        </xsl:choose>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </xsl:if>

    <xsl:if test="$vCategory='F' or $vCategory='O'">
      <!-- PM 20131202 [19275] Ajout Factor -->
      <xsl:call-template name="Upd_DERIVATIVECONTRACT">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
        <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
        <xsl:with-param name="pPriceQuotationMethod" select="$vPriceQuotationMethod"/>
        <xsl:with-param name="pExerciseStyle" select="$vExerciseStyle"/>
        <xsl:with-param name="pCabinetOptionValue" select="$vCabinetOptionValue"/>
        <xsl:with-param name="pPriceDecLocator" select="$vPriceDecLocator"/>
        <xsl:with-param name="pStrikeDecLocator" select="$vStrikeDecLocator"/>
        <xsl:with-param name="pPriceAlignCode" select="$vPriceAlignCode"/>
        <xsl:with-param name="pStrikeAlignCode" select="$vStrikeAlignCode"/>
        <xsl:with-param name="pFactor" select="$vFactor"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANCONTRACT_H (Type "2" S, E, P) =============== -->
  <xsl:template name="IMSPANCONTRACT_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <xsl:variable name="ExchangeAcronym">
      <xsl:choose>
        <xsl:when test="$pFileFormat='U2' or $pFileFormat='UP'">
          <xsl:value-of select="./d[@n='EA']/@v"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pExchangeComplex"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <pms>
      <pm n="IDIMSPANEXCHANGE_H">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDIMSPANEXCHANGE_H" cache="true">
          select e.IDIMSPANEXCHANGE_H
          from dbo.IMSPANEXCHANGE_H e<xsl:value-of select="$vSpace"/>
          <!-- Vérification de IDIMSPAN_H -->
          <xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>
          and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )
          <xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
          <!-- Param Exchange Code / Acronym -->
          <p n="EXCHANGEACRONYM" v="{$ExchangeAcronym}"/>
        </sql>
      </pm>
      <pm n="R2_IDIMSPANGRPCTR_H">
        <xsl:call-template name="SQL_IDIMSPANGRPCTR_H">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeAcronym" select="$ExchangeAcronym"/>
          <xsl:with-param name="pCombinedCommodityCode" select="./d[@n='CC']/@v"/>
        </xsl:call-template>
      </pm>
    </pms>

    <!-- Pour chaque Commodity (Product) Code -->
    <xsl:for-each select="./d[(substring(@n,1,2)='CP') and (number(substring-after(@n,'CP')) = substring-after(@n,'CP'))]">
      <!-- Si le Contrat est renseigné -->
      <xsl:if test="normalize-space(@v)!=''">
        <!-- Variable Level pour concat -->
        <xsl:variable name="Level">
          <xsl:value-of select="substring-after(@n,'CP')"/>
        </xsl:variable>
        <tbl n="IMSPANCONTRACT_H" a="IU">
          <!-- IDIMSPANEXCHANGE_H -->
          <c n="IDIMSPANEXCHANGE_H" dk="true" t="i" v="parameters.IDIMSPANEXCHANGE_H">
            <ctls>
              <ctl a="RejectRow" rt="true">
                <sl fn="IsNull()"/>
                <li st="REJECT" ex="true">
                  <c>SYS</c>
                  <n>2001</n>
                  <d1>
                    <xsl:value-of select="$vXSLFileName"/>
                  </d1>
                  <d2>
                    <xsl:value-of select="$vXSLVersion"/>
                  </d2>
                </li>
              </ctl>
            </ctls>
          </c>
          <!-- IDIMSPANGRPCTR_H -->
          <c n="IDIMSPANGRPCTR_H" dku="true" t="i" v="parameters.R2_IDIMSPANGRPCTR_H"/>
          <!-- Commodity (Product) Code -->
          <c n="CONTRACTSYMBOL" dk="true" v="{@v}"/>
          <!-- Contract Type -> CATEGORY (S, E et P) -->
          <c n="CATEGORY" dku="true">
            <xsl:attribute name="v">
              <xsl:choose>
                <xsl:when test="../d[@n=concat('CT',$Level)]/@v = 'FUT' or normalize-space(../d[@n=concat('CT',$Level)]/@v) = ''">F</xsl:when>
                <xsl:when test="../d[@n=concat('CT',$Level)]/@v = 'P' or ../d[@n=concat('CT',$Level)]/@v = 'C'
								or ../d[@n=concat('CT',$Level)]/@v = 'OOF' or ../d[@n=concat('CT',$Level)]/@v = 'OOP'
								or ../d[@n=concat('CT',$Level)]/@v = 'OOC' or ../d[@n=concat('CT',$Level)]/@v = 'OOS'">O</xsl:when>
                <xsl:otherwise>null</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
          </c>
          <!-- Contract Type -> Colonne PUTCALL (S) -->
          <xsl:if test="$pFileFormat='U'">
            <c n="PUTCALL" dku="true">
              <xsl:attribute name="v">
                <xsl:call-template name="CallOrPutOrNull">
                  <xsl:with-param name="pValue" select="../d[@n=concat('CT',$Level)]/@v"/>
                </xsl:call-template>
              </xsl:attribute>
            </c>
          </xsl:if>
          <!-- Contract Type -> Colonne CONTRACTTYPE (S, E et P) -->
          <c n="CONTRACTTYPE" dk="true">
            <xsl:attribute name="v">
              <xsl:call-template name="ValueOrNull">
                <xsl:with-param name="pValue" select="../d[@n=concat('CT',$Level)]/@v"/>
              </xsl:call-template>
            </xsl:attribute>
          </c>
          <!-- Contract Value Factor & Decimal Locator -->
          <xsl:if test="($pFileFormat='UP') and number(./d[@n=concat('M',$Level)]/@v)!=0">
            <c n="CONTRACTMULTIPLIER" dku="true" t="dc">
              <xsl:call-template name="AttribDecLoc">
                <xsl:with-param name="pValue" select="../d[@n=concat('M',$Level)]/@v"/>
                <xsl:with-param name="pDecLoc" select="../d[@n=concat('M',$Level,'D')]/@v"/>
              </xsl:call-template>
            </c>
          </xsl:if>
          <!-- Risk Array Value Decimal Locator (EXPANDED) -->
          <xsl:if test="$pFileFormat='U2'">
            <c n="RISKVALDECLOCATOR" dku="true" t="i" v="{../d[@n=concat('VD',$Level)]/@v}"/>
          </xsl:if>
          <!-- InfoTable_IU -->
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- Information sur les échéances : record Type "B" -->
  <!-- PM 20140116 [19456] Nouveau template pour regrouper la gestion du record "B"-->
  <xsl:template name="RecordB">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <!-- ExchangeAcronym -->
    <xsl:variable name="vExchangeAcronym" select="./d[@n='Ex']/@v"/>
    <!-- Commodity Code -->
    <xsl:variable name="vCommodityCode" select="./d[@n='C']/@v"/>
    <!-- Underlying Commodity Code -->
    <xsl:variable name="vUnderlyingCommodityCode" select="./d[@n='UC']/@v"/>
    <!-- Contract Type / Product Type Code -->
    <xsl:variable name="vContractType">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='CT']/@v"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- Futures Contract Month (+ Day or Week Code) -->
    <xsl:variable name="vFutMMY">
      <xsl:choose>
        <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
          <xsl:value-of select="concat( '20', ./d[@n='FM']/@v, ./d[@n='FDW']/@v )"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat( ./d[@n='FM']/@v, ./d[@n='FDW']/@v )"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Option Contract Month (+ Day or Week Code) -->
    <xsl:variable name="vOptMMY">
      <xsl:choose>
        <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
          <xsl:choose>
            <xsl:when test="normalize-space(concat( ./d[@n='OM']/@v, ./d[@n='ODW']/@v ))=''">null</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat( '20', ./d[@n='OM']/@v, ./d[@n='ODW']/@v )"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="ValueOrNull">
            <xsl:with-param name="pValue" select="concat( ./d[@n='OM']/@v, ./d[@n='ODW']/@v )"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <r uc="true">
      <xsl:call-template name="RowAttributes"/>
      <!-- FI 20220105 [XXXXX] add test="($vIsImportPriceOnly='false') -->
      <xsl:if test="($vIsImportPriceOnly='false')">
        <xsl:call-template name="IMSPANMATURITY_H">
          <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
          <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
          <xsl:with-param name="pContractType" select="$vContractType"/>
          <xsl:with-param name="pFutMMY" select="$vFutMMY"/>
          <xsl:with-param name="pOptMMY" select="$vOptMMY"/>
        </xsl:call-template>
      </xsl:if>

      <!-- PM 20140116 [19456] Gestion DSP et EDSP pour les options le jour d'échéance -->
      <xsl:if test="($pFileFormat = 'U2') and ((./d[@n='ORPF']/@v = 'S') or (./d[@n='ORPF']/@v = 'Y'))">

        <xsl:variable name="vBusinessDate" select="substring($pBusinessDateTime,1,10)"/>

        <!--Vérification qu'il s'agit du jour d'échéance-->
        <xsl:if test="$vBusinessDate = ./d[@n='ED']/@v">

          <!-- FI 20190228 [24559] 
            Pour le contrat 21 O à l'échéance, on recoit la valeur Y et le prix associé n'est pas le prix de clôture de l'asset Future
            Il est donc décider de mettre systématiquement le prix en OfficialSettlement-->
          <!--<xsl:variable name="vQuoteSide">
            <xsl:choose>
              <xsl:when test="(./d[@n='ORPF']/@v = 'S')">
                <xsl:value-of select="'OfficialSettlement'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'OfficialClose'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>-->
          <xsl:variable name="vQuoteSide" select ="'OfficialSettlement'"/>
          

          <!--Pour les OOF uniquement pour le moment-->
          <xsl:choose>
            <xsl:when test="$vContractType='OOF'">

              <pms>
                <!--IDASSET du Future sous-jacent-->
                <pm n="IDASSET">
                  <xsl:call-template name="SQL_DERIVATIVECONTRACT_OPT_UNL_FUT">
                    <xsl:with-param name="pResultColumn" select="'IDASSET'"/>
                    <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                    <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                    <xsl:with-param name="pContractSymbol" select="$vCommodityCode"/>
                    <xsl:with-param name="pMaturityMonthYear" select="$vOptMMY"/>
                  </xsl:call-template>
                </pm>
                <!--Cours DSP ou EDSP du DC-->
                <pm n="SETTLEMENTPRICE">
                  <xsl:call-template name="AlignedPrice">
                    <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                    <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                    <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                    <xsl:with-param name="pCommodityCode" select="$vUnderlyingCommodityCode"/>
                    <xsl:with-param name="pCategory" select="'F'"/>
                    <xsl:with-param name="pPrice" select="./d[@n='ORP']/@v"/>
                    <xsl:with-param name="pPriceDecLocator" select="'null'"/>
                  </xsl:call-template>
                </pm>
                <!--Heure du cours DSP ou EDSP du DC-->
                <pm n="SETTLEMENTTIME">
                  <xsl:call-template name="SQL_DERIVATIVECONTRACT_OPT_UNL_FUT">
                    <xsl:with-param name="pResultColumn" select="'FINALSETTLTTIME'"/>
                    <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                    <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                    <xsl:with-param name="pContractSymbol" select="$vCommodityCode"/>
                    <xsl:with-param name="pMaturityMonthYear" select="$vOptMMY"/>
                  </xsl:call-template>
                </pm>
              </pms>

              <!--Insertion du cours DSP ou EDSP-->
              <xsl:call-template name="QUOTE_XXX_H">
                <xsl:with-param name="pQuoteTable" select="'QUOTE_ETD_H'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                <xsl:with-param name="pCommodityCode" select="$vUnderlyingCommodityCode"/>
                <xsl:with-param name="pCategory" select="'F'"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
                <xsl:with-param name="pDelta" select="'null'"/>
                <xsl:with-param name="pVolatility" select="'null'"/>
                <xsl:with-param name="pTimeToExpiration" select="'null'"/>
                <xsl:with-param name="pQuoteSide" select="$vQuoteSide"/>
                <xsl:with-param name="pTime" select="'parameters.SETTLEMENTTIME'"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:if>

      <!-- BD 20130625 : Appel de Upd_UNDERLYINGSYMBOL dans le cas du Paris Expanded Format (UP)  -->
      <xsl:if test="$pFileFormat = 'UP'">
        <xsl:call-template name="Upd_UNDERLYINGSYMBOL">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pUnderlyingCommodityCode" select="$vUnderlyingCommodityCode"/>
          <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
          <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
        </xsl:call-template>
      </xsl:if>
    </r>

  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANMATURITY_H (Type "B" S, E, P) =============== -->
  <!-- PM 20140116 [19456] Ajout paramètres  pExchangeAcronym, pCommodityCode, pContractType, pFutMMY, pOptMMY -->
  <xsl:template name="IMSPANMATURITY_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. de l'échéance -->
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pFutMMY"/>
    <xsl:param name="pOptMMY"/>

    <!-- Deprecated -->
    <!-- Contract Type / Product Type Code -->
    <!--<xsl:variable name="ContractType">
      <xsl:call-template name="ValueOrNull">
        <xsl:with-param name="pValue" select="./d[@n='CT']/@v"/>
      </xsl:call-template>
    </xsl:variable>-->
    <!-- Option Maturity Month -->
    <!--<xsl:variable name="OptMMY">
      <xsl:choose>
        <xsl:when test="$pFileFormat='U'">
          <xsl:choose>
            <xsl:when test="normalize-space(concat( ./d[@n='OM']/@v, ./d[@n='ODW']/@v ))=''">null</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat( '20', ./d[@n='OM']/@v, ./d[@n='ODW']/@v )"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="ValueOrNull">
            <xsl:with-param name="pValue" select="concat( ./d[@n='OM']/@v, ./d[@n='ODW']/@v )"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>-->

    <tbl n="IMSPANMATURITY_H" a="IU">
      <!-- IDIMSPANCONTRACT_H -->
      <xsl:call-template name="Col_IDIMSPANCONTRACT_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
        <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
        <xsl:with-param name="pContractType" select="$pContractType"/>
      </xsl:call-template>
      <!--<xsl:choose>
        <xsl:when test="$pFileFormat='U'">
          <c n="FUTMMY" dk="true" v="{concat( '20', ./d[@n='FM']/@v, ./d[@n='FDW']/@v )}"/>
        </xsl:when>
        <xsl:otherwise>
          <c n="FUTMMY" dk="true" v="{concat( ./d[@n='FM']/@v, ./d[@n='FDW']/@v )}"/>
        </xsl:otherwise>
      </xsl:choose>-->
      <c n="FUTMMY" dk="true" v="{$pFutMMY}"/>
      <c n="OPTMMY" dk="true" v="{$pOptMMY}"/>
      <xsl:choose>
        <xsl:when test="$pFileFormat='UP'">
          <c n="BASEVOLATILITY" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='BV']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='BVD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="VOLATSCANRANGE" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='VSR']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='VSRD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="FUTPRICESCANRANGE" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='FPSR']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='FPSRD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="EXTRMMOVEMULT" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='EMM']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='EMMD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="EXTRMMOVECOVFRACT" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='EMCF']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='EMCFD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="INTERESTRATE" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='IR']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='IRD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="TIMETOEXPIRATION" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='TE']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='TED']/@v"/>
            </xsl:call-template>
          </c>
          <c n="LOOKAHEADTIME" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='LT']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='LTD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="DELTASCALINGFACTOR" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='DSF']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='DSFD']/@v"/>
            </xsl:call-template>
          </c>
          <c n="DIVIDENDYIELD" dku="true" t="dc">
            <xsl:call-template name="AttribDecLoc">
              <xsl:with-param name="pValue" select="./d[@n='DY']/@v"/>
              <xsl:with-param name="pDecLoc" select="./d[@n='DYD']/@v"/>
            </xsl:call-template>
          </c>
        </xsl:when>
        <xsl:otherwise>
          <c n="BASEVOLATILITY" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='BV']/@v"/>
              <xsl:with-param name="pDiv" select="1000000"/>
            </xsl:call-template>
          </c>
          <c n="VOLATSCANRANGE" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='VSR']/@v"/>
              <xsl:with-param name="pDiv" select="1000000"/>
            </xsl:call-template>
          </c>
          <c n="FUTPRICESCANRANGE" dku="true" t="dc" v="{./d[@n='FPSR']/@v}"/>
          <c n="EXTRMMOVEMULT" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='EMM']/@v"/>
              <xsl:with-param name="pDiv" select="1000"/>
            </xsl:call-template>
          </c>
          <c n="EXTRMMOVECOVFRACT" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='EMCF']/@v"/>
              <xsl:with-param name="pDiv" select="10000"/>
            </xsl:call-template>
          </c>
          <c n="INTERESTRATE" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='IR']/@v"/>
              <xsl:with-param name="pDiv" select="10000"/>
            </xsl:call-template>
          </c>
          <c n="TIMETOEXPIRATION" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='TE']/@v"/>
              <xsl:with-param name="pDiv" select="1000000"/>
            </xsl:call-template>
          </c>
          <c n="LOOKAHEADTIME" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='LT']/@v"/>
              <xsl:with-param name="pDiv" select="1000000"/>
            </xsl:call-template>
          </c>
          <c n="DELTASCALINGFACTOR" dku="true" t="dc">
            <xsl:call-template name="AttribNumDiv">
              <xsl:with-param name="pValue" select="./d[@n='DSF']/@v"/>
              <xsl:with-param name="pDiv" select="10000"/>
            </xsl:call-template>
          </c>
          <xsl:if test="$pFileFormat='U2'">
            <c n="DIVIDENDYIELD" dku="true" t="dc">
              <xsl:call-template name="AttribNumDiv">
                <xsl:with-param name="pValue" select="./d[@n='DY']/@v"/>
                <xsl:with-param name="pDiv" select="1000000"/>
              </xsl:call-template>
            </c>
            <c n="OPTREFERENCEPRICE" dku="true" t="dc">
              <xsl:attribute name="v">
                <xsl:call-template name="ValueOrNull">
                  <xsl:with-param name="pValue" select="./d[@n='OPR']/@v"/>
                </xsl:call-template>
              </xsl:attribute>
            </c>
            <c n="VALUEFACTOR" dku="true" t="dc">
              <xsl:attribute name="v">
                <xsl:choose>
                  <xsl:when test="normalize-space(./d[@n='VF']/@v)=''">null</xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="SetDecLoc">
                      <xsl:with-param name="pValue" select="./d[@n='VF']/@v div 10000000"/>
                      <xsl:with-param name="pDecLoc" select="./d[@n='VFD']/@v"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </c>
            <c n="DISCOUNTFACTOR" dku="true" t="dc">
              <xsl:call-template name="AttribNumDiv">
                <xsl:with-param name="pValue" select="./d[@n='DF']/@v"/>
                <xsl:with-param name="pDiv" select="10000000000"/>
              </xsl:call-template>
            </c>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <c n="MATURITYDATE" dku="true" t="d" v="{./d[@n='ED']/@v}">
        <ctls>
          <ctl a="RejectRow" rt="false">
            <sl fn="IsDate()" f="yyyy-MM-dd"/>
            <li st="none" ex="false">
              <xsl:attribute name="msg">
                MaturityDate: Invalid Date  (<xsl:value-of select="./d[@n='ED']/@v"/>)
              </xsl:attribute>
            </li>
          </ctl>
        </ctls>
      </c>
      <c n="UNLCONTRACTSYMBOL" dku="true">
        <xsl:attribute name="v">
          <xsl:call-template name="ValueOrNull">
            <xsl:with-param name="pValue" select="./d[@n='UC']/@v"/>
          </xsl:call-template>
        </xsl:attribute>
      </c>
      <c n="PRICINGMODEL" dku="true" v="{./d[@n='PM']/@v}">
        <ctls>
          <ctl a="RejectColumn" rt="true" lt="None">
            <sl fn="IsEmpty()"/>
          </ctl>
        </ctls>
      </c>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>

  </xsl:template>
  <!-- ================================================================== -->
  <!-- Information sur l'asset pour les record Type 8 -->
  <xsl:template name="Record8">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
    <xsl:param name="pBusinessDate"/>

    <xsl:variable name="vRID">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:variable>

    <!-- Variable indiquant s'il s'agit d'un record comportant le prix de clôture -->
    <xsl:variable name="vIsPriceRecord">
      <xsl:choose>
        <xsl:when test="(($pFileFormat='P') and ($vRID='81'))
							   or (($pFileFormat='U') and ($vRID='82'))
							   or (($pFileFormat='U2') and ($vRID='81') and (normalize-space(./d[@n='Pr']/@v)!=''))
							   or (($pFileFormat='U2') and (($vRID='82') or ($vRID='84')) and (normalize-space(./d[@n='Pr']/@v)!=''))
							   or (($pFileFormat='UP') and ($vRID='83'))">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- S'il s'agit d'un record de prix ou si on intégre toutes les données -->
    <xsl:if test="($vIsImportPriceOnly='false') or ($vIsPriceRecord='true')">
      <!-- ExchangeAcronym -->
      <xsl:variable name="vExchangeAcronym" select="./d[@n='Ex']/@v"/>
      <!-- Commodity Code -->
      <xsl:variable name="vCommodityCode" select="./d[@n='C']/@v"/>
      <!-- Contract Type / Product Type Code -->
      <xsl:variable name="vContractType">
        <xsl:call-template name="ValueOrNull">
          <xsl:with-param name="pValue" select="./d[@n='CT']/@v"/>
        </xsl:call-template>
      </xsl:variable>
      <!-- Futures Contract Month (+ Day or Week Code) -->
      <xsl:variable name="vFutMMY">
        <xsl:choose>
          <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
            <xsl:value-of select="concat( '20', ./d[@n='FM']/@v, ./d[@n='FDW']/@v )"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat( ./d[@n='FM']/@v, ./d[@n='FDW']/@v )"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Option Contract Month (+ Day or Week Code) -->
      <xsl:variable name="vOptMMY">
        <xsl:choose>
          <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
            <xsl:choose>
              <xsl:when test="normalize-space(concat( ./d[@n='OM']/@v, ./d[@n='ODW']/@v ))=''">null</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="concat( '20', ./d[@n='OM']/@v, ./d[@n='ODW']/@v )"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ValueOrNull">
              <xsl:with-param name="pValue" select="concat( ./d[@n='OM']/@v, ./d[@n='ODW']/@v )"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Put / Call -->
      <xsl:variable name="vPutCall">
        <xsl:choose>
          <!-- Quand Fichier Standard -->
          <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
            <xsl:call-template name="CallOrPutOrNull">
              <xsl:with-param name="pValue" select="./d[@n='CT']/@v"/>
            </xsl:call-template>
          </xsl:when>
          <!-- Quand Fichier Expanded ou Paris Expanded -->
          <xsl:otherwise>
            <xsl:call-template name="CallOrPutOrNull">
              <xsl:with-param name="pValue" select="./d[@n='CP']/@v"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Option Strike Price (+Decimal Locator) -->
      <xsl:variable name="vStrikePrice">
        <xsl:choose>
          <xsl:when test="normalize-space(./d[@n='SP']/@v) = ''">null</xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <!-- Quand Fichier Paris Expanded -->
              <xsl:when test="$pFileFormat='UP' and d[@n='RID']/@v != '84'">
                <xsl:call-template name="SetDecLoc">
                  <xsl:with-param name="pValue" select="./d[@n='SP']/@v"/>
                  <xsl:with-param name="pDecLoc" select="./d[@n='SPD']/@v"/>
                </xsl:call-template>
              </xsl:when>
              <!-- Quand Fichier Standard ou Expanded -->
              <xsl:otherwise>
                <xsl:value-of select="d[@n='SP']/@v"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
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
        <xsl:choose>
          <xsl:when test="$vCategory = 'O'">
            <xsl:value-of select="$vOptMMY"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$vFutMMY"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Asset Category -->
      <xsl:variable name ="vAssetCategory" >
        <xsl:choose>
          <!-- Dérivé Listé : Future et Option -->
          <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')
								 or	($vContractType='FUT') or ($vContractType='FWD') or ($vContractType='CMB')
								 or ($vContractType='OOC') or ($vContractType='OOF')
								 or ($vContractType='OOP') or ($vContractType='OOS')">
            <xsl:value-of select="'Future'"/>
          </xsl:when>
          <!-- Equity -->
          <xsl:when test="($vContractType='STOCK') and ($pFileFormat='UP')">
            <xsl:value-of select="'EquityAsset'"/>
          </xsl:when>
          <!-- Indice -->
          <xsl:when test="($vContractType='PHY') and ($pFileFormat='UP')">
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
      <!-- PM 20130917 [18889] Ajout variables vContractSymbol & vSymbolSuffix -->
      <!-- Contract Symbol -->
      <xsl:variable name ="vContractSymbol" >
        <xsl:choose>
          <xsl:when test="normalize-space(substring-before($vCommodityCode, '.'))=''">
            <xsl:value-of select="$vCommodityCode"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="substring-before($vCommodityCode, '.')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- Symbol Suffix -->
      <xsl:variable name ="vSymbolSuffix" >
        <xsl:call-template name="SymbolSuffixOrNull">
          <xsl:with-param name="pFullSymbol" select="$vCommodityCode"/>
        
          <xsl:with-param name="pSeparator" select="'.'"/>
        </xsl:call-template>
      </xsl:variable>
      
      <r uc="true">
        <xsl:call-template name="RowAttributes"/>

        <pms>
          <pm n="ASSETCATEGORY" v="{$vAssetCategory}"/>
          <pm n="CATEGORY" v="{$vCategory}"/>
          <xsl:choose>
            <xsl:when test="$vAssetCategory='Future'">
              <!-- FI 20220106 [XXXXX] Call GetParameterIDAssetETD -->
              <xsl:call-template name="GetParameterIDAssetETD">
                <xsl:with-param name="pElementName" select="'pm'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                <xsl:with-param name="pCategory" select="$vCategory"/>
                <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
                <xsl:with-param name="pPutCall" select="$vPutCall"/>
                <xsl:with-param name="pStrikePrice" select="$vStrikePrice"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <pm n="IDASSET">
                <xsl:choose>
                  <xsl:when test="$vAssetCategory='EquityAsset'">
                    <!-- BD 20130625 : Rajout de la jointure ASSET_EQUITY_RDCMK avec SYMBOL, SYMBOLSUFFIX et IDM_RELATED -->
                    <!-- PM 20130916 [18889] Gestion asset sans SYMBOLSUFFIX -->
                    <sql cd="select" rt="IDASSET" cache="true">
                      select distinct aeq.IDASSET, aeq.IDC
                      from dbo.ASSET_EQUITY aeq
                      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aeq.IDASSET) and (dc.ASSETCATEGORY = 'EquityAsset')
                      inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
                      inner join dbo.ASSET_EQUITY_RDCMK aeqrdc on (aeqrdc.IDASSET = aeq.IDASSET)
                      and (aeqrdc.SYMBOL = @CONTRACTSYMBOL)
                      and (aeqrdc.IDM_RELATED = dc.IDM)
                      and ( (aeqrdc.SYMBOLSUFFIX = @SYMBOLSUFFIX ) or ((aeqrdc.SYMBOLSUFFIX is null) and (@SYMBOLSUFFIX is null)) )
                      where ((aeq.DTDISABLED is null) or (aeq.DTDISABLED > @BUSINESSDATETIME))
                      and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME))
                      and ((mk.DTDISABLED is null) or (mk.DTDISABLED > @BUSINESSDATETIME))
                      and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM)
                      <!-- Ancienne Requête : -->
                      <!--select distinct aeq.IDASSET, aeq.IDC
                  from dbo.ASSET_EQUITY aeq
                  inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aeq.IDASSET) and (dc.ASSETCATEGORY = 'EquityAsset')
                  inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
                  where ( ( aeq.DTDISABLED is null ) or ( aeq.DTDISABLED > @BusinessDateTime ) )
                  --><!--									   and ( ( aeq.DTENABLED is null ) or ( aeq.DTENABLED &lt;= @BusinessDateTime ) )--><!--
                  and ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BusinessDateTime ) )
                  --><!--										and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )--><!--
                  and ( ( mk.DTDISABLED is null ) or ( mk.DTDISABLED > @BusinessDateTime ) )
                  --><!--										and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )--><!--
                  and ( aeq.SYMBOL = @ContractSymbol )
                  and ( aeq.SYMBOLSUFFIX = @SymbolSuffix )
                  and ( mk.EXCHANGEACRONYM = @ExchangeAcronym )-->
                      <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
                      <p n="CONTRACTSYMBOL" v="{$vContractSymbol}"/>
                      <p n="SYMBOLSUFFIX" v="{$vSymbolSuffix}"/>
                      <p n="EXCHANGEACRONYM" v="{$vExchangeAcronym}"/>
                    </sql>
                  </xsl:when>
                  <xsl:when test="$vAssetCategory='Index'">
                    <!-- PM 20130916 [18889] Gestion asset sans SYMBOLSUFFIX -->
                    <sql cd="select" rt="IDASSET" cache="true">
                      select distinct aid.IDASSET, aid.IDC
                      from dbo.ASSET_INDEX aid
                      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aid.IDASSET) and (dc.ASSETCATEGORY = 'Index')
                      inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
                      where ((aid.DTDISABLED is null) or (aid.DTDISABLED > @BUSINESSDATETIME))
                      and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME))
                      and ((mk.DTDISABLED is null) or (mk.DTDISABLED > @BUSINESSDATETIME))
                      and (mk.EXCHANGEACRONYM = @EXCHANGEACRONYM)
                      and (aid.SYMBOL = @CONTRACTSYMBOL)
                      and ( (aid.SYMBOLSUFFIX = @SYMBOLSUFFIX) or ((aid.SYMBOLSUFFIX is null) and (@SYMBOLSUFFIX is null)) )
                      <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
                      <p n="CONTRACTSYMBOL" v="{$vContractSymbol}"/>
                      <p n="SYMBOLSUFFIX" v="{$vSymbolSuffix}"/>
                      <p n="EXCHANGEACRONYM" v="{$vExchangeAcronym}"/>
                    </sql>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="null"/>
                  </xsl:otherwise>
                </xsl:choose>
              </pm>
            </xsl:otherwise>
          </xsl:choose>
          
          
          <xsl:if test="$vIsPriceRecord='true'">
            <pm n="SETTLEMENTPRICE">
              <xsl:call-template name="AlignedPrice">
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                <xsl:with-param name="pCategory" select="$vCategory"/>
                <xsl:with-param name="pPrice" select="./d[@n='Pr']/@v"/>
                <xsl:with-param name="pPriceDecLocator" select="./d[@n='PrD']/@v"/>
              </xsl:call-template>
            </pm>
          </xsl:if>
          <pm n="STRIKEPRICE">
            <xsl:choose>
              <xsl:when test="$vAssetCategory='Future'">
                <xsl:choose>
                  <xsl:when test="$pFileFormat = 'UP'">
                    <xsl:attribute name="v">
                      <xsl:value-of select="$vStrikePrice"/>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="SQL_ASSET_ETD">
                      <xsl:with-param name="pResultColumn" select="'STRIKEPRICE'"/>
                      <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                      <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                      <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                      <xsl:with-param name="pContractSymbol" select="$vCommodityCode"/>
                      <xsl:with-param name="pCategory" select="$vCategory"/>
                      <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
                      <xsl:with-param name="pPutCall" select="$vPutCall"/>
                      <xsl:with-param name="pStrikePrice" select="$vStrikePrice"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="v">
                  <xsl:value-of select="'null'"/>
                </xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
          </pm>
          <pm n="DELTA">
            <xsl:choose>
              <!-- Fichier Standard et RID = 82 (ou 81)-->
              <xsl:when test="(($pFileFormat='U') and ($vRID='82')) or (($pFileFormat='P') and ($vRID='81'))">
                <xsl:attribute name="v">
                  <xsl:value-of select="number(translate(concat(./d[@n='SD']/@v, ./d[@n='D']/@v),'+',' ')) div 100"/>
                </xsl:attribute>
              </xsl:when>
              <!-- Fichier Expanded et RID = 82 ou 84 -->
              <xsl:when test="(($pFileFormat='U2') and (($vRID='82') or ($vRID='84')))">
                <xsl:attribute name="v">
                  <xsl:value-of select="number(translate(concat(./d[@n='SD']/@v, ./d[@n='D']/@v),'+',' ')) div 10000"/>
                </xsl:attribute>
              </xsl:when>
              <!-- Quand Fichier Paris Expanded et RID = 83 -->
              <xsl:when test="($pFileFormat='UP') and ($vRID='83')">
                <xsl:call-template name="AttribDecLoc">
                  <xsl:with-param name="pValue" select="concat(./d[@n='SD']/@v, ./d[@n='D']/@v)"/>
                  <xsl:with-param name="pDecLoc" select="./d[@n='DD']/@v"/>
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
          </pm>
          <pm n="VOLATILITY">
            <xsl:choose>
              <xsl:when test="(($pFileFormat='U') and ($vRID='82')) or (($pFileFormat='P') and ($vRID='81'))">
                <xsl:call-template name="AttribNumDiv">
                  <xsl:with-param name="pValue" select="./d[@n='V']/@v"/>
                  <xsl:with-param name="pDiv" select="10000"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="(($pFileFormat='U2') and (($vRID='82') or ($vRID='84')))">
                <xsl:call-template name="AttribNumDiv">
                  <xsl:with-param name="pValue" select="./d[@n='V']/@v"/>
                  <xsl:with-param name="pDiv" select="1000000"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="($pFileFormat='UP') and ($vRID='83')">
                <xsl:call-template name="AttribDecLoc">
                  <xsl:with-param name="pValue" select="./d[@n='V']/@v"/>
                  <xsl:with-param name="pDecLoc" select="./d[@n='VD']/@v"/>
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
          </pm>
          <pm n="TIMETOEXPIRATION">
            <xsl:choose>
              <xsl:when test="$vIsImportPriceOnly='false'">
                <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
                <sql cd="select" rt="TIMETOEXPIRATION" t="dc" cache="true">
                  select m.TIMETOEXPIRATION from dbo.IMSPANMATURITY_H m
                  inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANCONTRACT_H = m.IDIMSPANCONTRACT_H )
                  inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H ) and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )<xsl:value-of select="$vSpace"/>
                  <!-- Vérification de IDIMSPAN_H -->
                  <xsl:call-template name="innerIDIMSPAN_H">
                    <xsl:with-param name="pTable" select="'e'"/>
                  </xsl:call-template>
                  and ( c.CONTRACTSYMBOL = @COMMODITYCODE )
                  and ( ( c.CONTRACTTYPE = @CONTRACTTYPE ) or ( ( c.CONTRACTTYPE is null ) and ( @CONTRACTTYPE is null ) ) )
                  and ( m.FUTMMY = @FUTMMY )
                  and ( m.OPTMMY = @OPTMMY )
                  <xsl:call-template name="paramInnerIDIMSPAN_H">
                    <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                    <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                    <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                  </xsl:call-template>
                  <!-- Param Exchange Code / Acronym -->
                  <p n="EXCHANGEACRONYM" v="{$vExchangeAcronym}"/>
                  <!-- Param Commodity Code -->
                  <p n="COMMODITYCODE" v="{$vCommodityCode}"/>
                  <!-- Param Contract Type / Product Type Code -->
                  <p n="CONTRACTTYPE" v="{$vContractType}"/>
                  <p n="FUTMMY" v="{$vFutMMY}"/>
                  <p n="OPTMMY" v="{$vOptMMY}"/>
                </sql>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="v">
                  <xsl:value-of select="'null'"/>
                </xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
          </pm>
          <pm n="IDASSET_UNL_EDSP">
            <!-- PM 20130916 [18889] Recherche IDASSET sous jacent le jour de l'échéance -->
            <xsl:choose>
              <xsl:when test="($pFileFormat='UP') and ($vRID='83') and ($vAssetCategory='Future') and ($vCategory='O')">
                <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
                <sql cd="select" rt="IDASSET_UNL" cache="true">
                  select dc.IDASSET_UNL
                  from dbo.ASSET_ETD a
                  inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                  inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY 
                  inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC
                  where a.IDASSET= @IDASSET and isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) = @BUSINESSDATETIME
                  <xsl:call-template name="GetParameterIDAssetETD">
                    <xsl:with-param name="pElementName" select="'p'"/>
                    <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                    <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                    <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                    <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                    <xsl:with-param name="pCategory" select="$vCategory"/>
                    <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
                    <xsl:with-param name="pPutCall" select="$vPutCall"/>
                    <xsl:with-param name="pStrikePrice" select="$vStrikePrice"/>
                  </xsl:call-template>
                  <p n="BUSINESSDATETIME"  t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
                </sql>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="null"/>
              </xsl:otherwise>
            </xsl:choose>
          </pm>
          <pm n="ISIMPORTASSET">
            <!-- FI 20220106 [XXXXX] ASSET ETD OPTION AT MATURITY => price not imported -->
            <xsl:choose>
              <xsl:when test="($pFileFormat='UP') and ($vAssetCategory='Future') and ($vCategory='O')">
                <sql cd="select" rt="ISIMPORTASSET" cache="true">
                  select case when isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) &lt;= @BUSINESSDATETIME  then 0 else 1 end ISIMPORTASSET
                  from dbo.ASSET_ETD a
                  inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                  inner join dbo.MATURITY ma on ma.IDMATURITY = da.IDMATURITY
                  inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC
                  where a.IDASSET= @IDASSET
                  <xsl:call-template name="GetParameterIDAssetETD">
                    <xsl:with-param name="pElementName" select="'p'"/>
                    <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                    <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                    <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                    <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                    <xsl:with-param name="pCategory" select="$vCategory"/>
                    <xsl:with-param name="pMaturityMonthYear" select="$vMaturityMonthYear"/>
                    <xsl:with-param name="pPutCall" select="$vPutCall"/>
                    <xsl:with-param name="pStrikePrice" select="$vStrikePrice"/>
                  </xsl:call-template>
                  <p n="BUSINESSDATETIME"  t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
                </sql>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="1"/>
              </xsl:otherwise>
            </xsl:choose>
          </pm>
        </pms>

        <xsl:if test="$vIsImportPriceOnly='false'">
          <xsl:call-template name="IMSPANARRAY_H">
            <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
            <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
            <xsl:with-param name="pContractType" select="$vContractType"/>
            <xsl:with-param name="pFutMMY" select="$vFutMMY"/>
            <xsl:with-param name="pOptMMY" select="$vOptMMY"/>
            <xsl:with-param name="pPutCall" select="$vPutCall"/>
            <xsl:with-param name="pIsPriceRecord" select="$vIsPriceRecord"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:if test="$vIsPriceRecord='true'">
          <xsl:choose>
            <xsl:when test="$vAssetCategory='Future'">
              <!--PM 20140120 [19504] Ajout paramètre pIsImportPrice-->
              <xsl:call-template name="QUOTE_XXX_H">
                <xsl:with-param name="pQuoteTable" select="'QUOTE_ETD_H'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                <xsl:with-param name="pCategory" select="'parameters.CATEGORY' "/>
                <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
                <xsl:with-param name="pDelta" select="'parameters.DELTA'"/>
                <xsl:with-param name="pVolatility" select="'parameters.VOLATILITY'"/>
                <xsl:with-param name="pTimeToExpiration" select="'parameters.TIMETOEXPIRATION'"/>
                <xsl:with-param name="pQuoteSide" select="'OfficialClose'"/>
                <xsl:with-param name="pIsImportPrice" select="'parameters.ISIMPORTASSET'"/>
              </xsl:call-template>
              <!-- PM 20130916 [18889] Insertion cours EDSP du sous jacent le jour de l'échéance pour fichier Expanded Paris -->
              <xsl:if test="($pFileFormat='UP') and ($vRID='83')">

                <!-- PM 20131202 [19275] Ajout CONTRACTMULTIPLIER et FACTOR -->
                <xsl:variable name="vContractMultiplier">
                  <xsl:call-template name="SetDecLoc">
                    <xsl:with-param name="pValue" select="./d[@n='CVF']/@v"/>
                    <xsl:with-param name="pDecLoc" select="./d[@n='CVFD']/@v"/>
                    <xsl:with-param name="pDefault" select="0"/>
                  </xsl:call-template>
                </xsl:variable>

                <xsl:if test="$vContractMultiplier != 0">
                  
                  <xsl:variable name="vFactor">
                    <xsl:choose>
                      <xsl:when test="$vContractType='OOF'">1</xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="round($vContractMultiplier)"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>

                  <!-- PM 20131202 [19275] Mise à jour du CONTRACTMULTIPLIER et du FACTOR de l'échéance ouverte si différent de celui du contrat -->
                  <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
                  <xsl:call-template name="Upd_DERIVATIVEATTRIB">
                    <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
                    <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                    <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
                    <xsl:with-param name="pFactor" select="$vFactor"/>
                  </xsl:call-template>

                  <!-- PM 20131202 [19275] Mise à jour du CONTRACTMULTIPLIER et du FACTOR de l'Asset si différent de celui du contrat -->
                  <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
                  <xsl:call-template name="Upd_ASSET_ETD">
                    <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
                    <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                    <xsl:with-param name="pContractMultiplier" select="$vContractMultiplier"/>
                    <xsl:with-param name="pFactor" select="$vFactor"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:choose>
                  <xsl:when test="$vContractType='OOP'">
                    <xsl:call-template name="QUOTE_XXX_H">
                      <xsl:with-param name="pQuoteTable" select="'QUOTE_INDEX_H'"/>
                      <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                      <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                      <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                      <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                      <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                      <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                      <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
                      <xsl:with-param name="pIdAsset" select="'parameters.IDASSET_UNL_EDSP'"/>
                      <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
                      <xsl:with-param name="pQuoteSide" select="'OfficialSettlement'"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$vContractType='OOS'">
                    <xsl:call-template name="QUOTE_XXX_H">
                      <xsl:with-param name="pQuoteTable" select="'QUOTE_EQUITY_H'"/>
                      <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                      <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                      <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                      <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                      <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                      <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                      <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
                      <xsl:with-param name="pIdAsset" select="'parameters.IDASSET_UNL_EDSP'"/>
                      <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
                      <xsl:with-param name="pQuoteSide" select="'OfficialSettlement'"/>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:if>
            </xsl:when>
            <xsl:when test="$vAssetCategory='EquityAsset'">
              <xsl:call-template name="QUOTE_XXX_H">
                <xsl:with-param name="pQuoteTable" select="'QUOTE_EQUITY_H'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="$vAssetCategory='Index'">
              <xsl:call-template name="QUOTE_XXX_H">
                <xsl:with-param name="pQuoteTable" select="'QUOTE_INDEX_H'"/>
                <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pExchangeAcronym" select="$vExchangeAcronym"/>
                <xsl:with-param name="pCommodityCode" select="$vCommodityCode"/>
                <xsl:with-param name="pCategory" select="'parameters.CATEGORY'"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IDASSET'"/>
                <xsl:with-param name="pSettlementPrice" select="'parameters.SETTLEMENTPRICE'"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </r>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANARRAY_H (Type "81" à "84" S, E, P) ====== -->
  <xsl:template name="IMSPANARRAY_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. de la série -->
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pContractType" select="'null'"/>
    <xsl:param name="pFutMMY"/>
    <xsl:param name="pOptMMY"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pIsPriceRecord"/>

    <tbl n="IMSPANARRAY_H" a="IU">
      <!--PM 20140120 [19504] Contrôle pour importer ou non certains asset (Par exemple: ne pas importer les assets échues des fichiers UP)-->
      <c n ="ISIMPORTASSET" t="i" v="parameters.ISIMPORTASSET">
        <ctls>
          <ctl a="RejectRow" rt="0"/>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <!-- IDIMSPANCONTRACT_H -->
      <xsl:call-template name="Col_IDIMSPANCONTRACT_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
        <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
        <xsl:with-param name="pContractType" select="$pContractType"/>
      </xsl:call-template>
      <!-- Si Fichier Expanded ou Paris Expanded ou Standard avec RID = 81 -->
      <!-- Underlying Commodity (Product) Code -->
      <xsl:if test="(($pFileFormat!='U') or (($pFileFormat='U') and (./d[@n='RID']/@v ='81')))">
        <c n="UNLCONTRACTSYMBOL" dku="true">
          <xsl:attribute name="v">
            <xsl:call-template name="ValueOrNull">
              <xsl:with-param name="pValue" select="./d[@n='UC']/@v"/>
            </xsl:call-template>
          </xsl:attribute>
        </c>
      </xsl:if>
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
      <c n="FUTMMY" dk="true" v="{$pFutMMY}"/>
      <!-- Option Contract Month (+ Day or Week Code) -->
      <c n="OPTMMY" dk="true" v="{$pOptMMY}"/>
      <!-- PUT / CALL -->
      <c n="PUTCALL" dk="true" v="{$pPutCall}"/>
      <!-- Option Strike Price -->
      <c n="STRIKEPRICE" dk="true" t="dc" v="parameters.STRIKEPRICE"/>
      <!-- Si Fichier Standard et RID = 81 -->
      <!-- Cycle Indicator & Expiration Day of Month -->
      <xsl:if test="($pFileFormat='U') and ./d[@n='RID']/@v ='81'">
        <!-- Cycle Indicator  -->
        <c n="CYCLEINDICATOR" dku="true">
          <xsl:attribute name="v">
            <xsl:call-template name="ValueOrNull">
              <xsl:with-param name="pValue" select="./d[@n='CI']/@v"/>
            </xsl:call-template>
          </xsl:attribute>
        </c>
        <!-- Expiration Day of Month -->
        <xsl:if test="./d[@n='EDoM']">
          <c n="MATURITYDAY" dku="true" t="i">
            <xsl:attribute name="v">
              <xsl:call-template name="ValueOrNull">
                <xsl:with-param name="pValue" select="./d[@n='EDoM']/@v"/>
              </xsl:call-template>
            </xsl:attribute>
          </c>
        </xsl:if>
      </xsl:if>
      <!-- Composite Delta -->
      <c n="COMPOSITEDELTA" dku="true" t="dc" v="parameters.DELTA"/>
      <!-- Current Delta -->
      <xsl:if test="($pFileFormat='U2') and (./d[@n='RID']/@v ='82' or ./d[@n='RID']/@v ='84') and normalize-space(./d[@n='CD']/@v)!=''">
        <c n="CURRENTDELTA" dku="true" t="dc" v="{number(translate(concat(./d[@n='SCD']/@v, ./d[@n='CD']/@v),'+',' ')) div 10000}"/>
      </xsl:if>
      <!-- Implied Volatility -->
      <c n="VOLATILITY" dku="true" t="dc" v="parameters.VOLATILITY"/>
      <!-- Contract Value Factor (Multiplier) (+Decimal Locator) -->
      <xsl:if test="$pFileFormat='UP' and d[@n='RID']/@v ='83'">
        <c n="CONTRACTMULTIPLIER" dku="true" t="dc">
          <xsl:call-template name="AttribDecLoc">
            <xsl:with-param name="pValue" select="./d[@n='CVF']/@v"/>
            <xsl:with-param name="pDecLoc" select="./d[@n='CVFD']/@v"/>
          </xsl:call-template>
        </c>
      </xsl:if>
      <!-- Settlement Price -->
      <xsl:if test="$pIsPriceRecord='true'">
        <c n="PRICE" dku="true" t="dc" v="parameters.SETTLEMENTPRICE"/>
      </xsl:if>
      <!-- PM 20130805 Gestion tick variable -->
      <xsl:if test="$pFileFormat='U2'">
        <xsl:variable name="vSCVFT" select="concat(./d[@n=SCVF]/@v,./d[@n='CVF']/@v)"/>
        <xsl:variable name="vSSVFT" select="concat(./d[@n=SSVF]/@v,./d[@n='SVF']/@v)"/>
        <xsl:variable name="vContractValueFactor">
          <xsl:choose>
            <xsl:when test="normalize-space(translate($vSCVFT,'+-',' '))=''">null</xsl:when>
            <xsl:otherwise>
              <xsl:variable name="vSCVFTDec">
                <xsl:call-template name="NumDiv">
                  <xsl:with-param name="pValue" select="$vSCVFT"/>
                  <xsl:with-param name="pDiv" select="10000000"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:call-template name="TenExponent">
                <xsl:with-param name="pValue" select="$vSCVFTDec"/>
                <xsl:with-param name="pExp" select="./d[@n='CVFE']/@v"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vStrikeValueFactor">
          <xsl:choose>
            <xsl:when test="normalize-space(translate($vSSVFT,'+-',' '))=''">null</xsl:when>
            <xsl:otherwise>
              <xsl:variable name="vSSVFTDec">
                <xsl:call-template name="NumDiv">
                  <xsl:with-param name="pValue" select="$vSSVFT"/>
                  <xsl:with-param name="pDiv" select="10000000"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:call-template name="TenExponent">
                <xsl:with-param name="pValue" select="$vSSVFTDec"/>
                <xsl:with-param name="pExp" select="./d[@n='SVFE']/@v"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <!-- Contract-specific Contract Value Factor (for variable tick options only) -->
        <c n="CONTRACTVALUEFACTOR" dku="true" t="dc" v="{$vContractValueFactor}"/>
        <!-- Contract-specific Strike Value Factor (for variable tick options only) -->
        <c n="STRIKEVALUEFACTOR" dku="true" t="dc" v="{$vStrikeValueFactor}"/>
      </xsl:if>
      <!-- risk value -->
      <xsl:for-each select="./d[(substring(@n,1,1)='A') and (number(substring-after(@n,'A')) = substring-after(@n,'A'))]">
        <!-- Variable Scenario -->
        <xsl:variable name="Scenario">
          <xsl:value-of select="substring-after(@n,'A')"/>
        </xsl:variable>
        <xsl:variable name="Value" select="number(translate(concat(../d[@n=concat('S',$Scenario)]/@v, @v),'+',' '))"/>
        <c dku="true" t="dc" n="{concat('RISKVALUE',$Scenario)}">
          <xsl:choose>
            <!-- Quand Fichier Standard ou Expanded -->
            <xsl:when test="$pFileFormat!='UP'">
              <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
              <sql cd="select" rt="RISKVALUE" cache="true">
                select @VALUE * power( 10, g.RISKEXPONENT ) / case when c.RISKVALDECLOCATOR is null then 1 else power( 10, c.RISKVALDECLOCATOR ) end as RISKVALUE
                from dbo.IMSPANGRPCTR_H g
                inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANGRPCTR_H = g.IDIMSPANGRPCTR_H )
                inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )<xsl:value-of select="$vSpace"/>
                <!-- Vérification de IDIMSPAN_H -->
                <xsl:call-template name="innerIDIMSPAN_H">
                  <xsl:with-param name="pTable" select="'e'"/>
                </xsl:call-template>
                and ( c.CONTRACTSYMBOL = @COMMODITYCODE )
                and ( ( c.CONTRACTTYPE = @CONTRACTTYPE ) or ( ( c.CONTRACTTYPE is null ) and ( @CONTRACTTYPE is null ) ) )
                <xsl:call-template name="paramInnerIDIMSPAN_H">
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                </xsl:call-template>
                <!-- Param Commodity Code -->
                <p n="COMMODITYCODE" v="{$pCommodityCode}"/>
                <!-- Param Contract Type / Product Type Code -->
                <p n="CONTRACTTYPE" v="{$pContractType}"/>
                <!-- Param Futures Contract Month (+ Day or Week Code) -->
                <p n="VALUE" t="i" v="{$Value}"/>
              </sql>
              <!-- Quand Fichier Expanded Paris -->
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="AttribDecLoc">
                <xsl:with-param name="pValue" select="$Value"/>
                <xsl:with-param name="pDecLoc" select="../d[@n='AD']/@v"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </c>
      </xsl:for-each>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- =  Mise à jour de la table DERIVATIVECONTRACT ==================== -->
  <xsl:template name="Upd_DERIVATIVECONTRACT">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pContractMultiplier"/>
    <xsl:param name="pFutValuationMethod"/>
    <xsl:param name="pPriceQuotationMethod"/>
    <xsl:param name="pExerciseStyle"/>
    <xsl:param name="pCabinetOptionValue"/>
    <xsl:param name="pPriceDecLocator"/>
    <xsl:param name="pStrikeDecLocator"/>
    <xsl:param name="pPriceAlignCode"/>
    <xsl:param name="pStrikeAlignCode"/>
    <xsl:param name="pFactor"/>

    <!-- Mise à jours de la table DERIVATIVECONTRACT -->
    <tbl n="DERIVATIVECONTRACT" a="U">
      <c n="IDDC" dk="true" t="i" v="parameters.IDDC">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="IsNull()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <c n="ISAUTOSETTING" t="b">
        <xsl:call-template name="SQL_DERIVATIVECONTRACT">
          <xsl:with-param name="pResultColumn" select="'ISAUTOSETTING'"/>
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
          <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
          <xsl:with-param name="pCategory" select="$pCategory"/>
        </xsl:call-template>
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="IsNull()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <!-- PM 20130808 [18876] Variable Tick Value : MAJ si $pContractMultiplier != 0-->
      <xsl:if test="(normalize-space($pContractMultiplier)!='') and (number($pContractMultiplier) = $pContractMultiplier) and (number($pContractMultiplier) != 0)">
        <!-- RD 20220419 [25997] Ajout QuotationHandlingMQueue-->
        <mq n="QuotationHandlingMQueue" a="U">
          <p n="TIME" t="d" f="yyyy-MM-dd" v="{$pBusinessDateTime}"/>
          <p n="IDDC" t="i" v="parameters.IDDC"/>
          <p n="IsCashFlowsVal" t="b" v="true"/>
        </mq>        
        <c n="CONTRACTMULTIPLIER" dku="true" t="dc" v="{$pContractMultiplier}"/>
      </xsl:if>
      <!-- PM 20131202 [19275] Ajout Factor -->
      <xsl:if test="(normalize-space($pFactor)!='') and (number($pFactor) = $pFactor) and (number($pFactor) != 0)">
        <c n="FACTOR" dku="true" t="i" v="{$pFactor}"/>
      </xsl:if>
      <xsl:if test="normalize-space($pFutValuationMethod)!=''">
        <c n="FUTVALUATIONMETHOD" dku="true" v="{$pFutValuationMethod}"/>
      </xsl:if>
      <xsl:if test="normalize-space(parameters.IDC_PRICE)!=''">
        <c n="IDC_PRICE" dku="true" v="parameters.IDC_PRICE"/>
      </xsl:if>
      <!-- PM 20130808 [18876] Variable Tick Value : MAJ si $pContractMultiplier != 0-->
      <xsl:if test="(normalize-space($pPriceQuotationMethod)!='') and (number($pContractMultiplier) != 0)">
        <c n="PRICEQUOTEMETHOD" dku="true" v="{$pPriceQuotationMethod}"/>
      </xsl:if>
      <xsl:if test="(normalize-space($pExerciseStyle)!='') and ($pCategory = 'O')">
        <c n="EXERCISESTYLE" dku="true" v="{$pExerciseStyle}"/>
      </xsl:if>
      <xsl:if test="(normalize-space($pCabinetOptionValue)!='') and (number($pCabinetOptionValue) = $pCabinetOptionValue) and ($pCategory = 'O')">
        <c n="CABINETOPTVALUE" dku="true" t="dc" v="{$pCabinetOptionValue}"/>
      </xsl:if>
      <xsl:if test="(normalize-space($pPriceDecLocator)!='') and (number($pPriceDecLocator) = $pPriceDecLocator)">
        <c n="PRICEDECLOCATOR" dku="true" t="i" v="{$pPriceDecLocator}"/>
      </xsl:if>
      <xsl:if test="(normalize-space($pStrikeDecLocator)!='') and (number($pStrikeDecLocator) = $pStrikeDecLocator) and ($pCategory = 'O')">
        <c n="STRIKEDECLOCATOR" dku="true" t="i" v="{$pStrikeDecLocator}"/>
      </xsl:if>
      <xsl:if test="normalize-space($pPriceAlignCode)!=''">
        <c n="PRICEALIGNCODE" dku="true" v="{$pPriceAlignCode}"/>
      </xsl:if>
      <xsl:if test="(normalize-space($pStrikeAlignCode)!='') and ($pCategory = 'O')">
        <c n="STRIKEALIGNCODE" dku="true" v="{$pStrikeAlignCode}"/>
      </xsl:if>
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ========== Mise à jour du SYMBOL des ACTIFS SOUS-JACENTS ========= -->
  <!-- ======================= BD 20130625 ============================== -->
  <!-- Dans le cas des sous-jacents provenant de Clearnet SA, ce sont les RiskDatas qui mettent à jours les symboles
        car la donnée présente dans le fichier utilisé pour le Repository est erronnée. -->
  <xsl:template name="Upd_UNDERLYINGSYMBOL">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pUnderlyingCommodityCode"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pExchangeAcronym"/>

    <xsl:variable name="vAssetCategory">
      <xsl:choose>
        <xsl:when test="substring-after($pUnderlyingCommodityCode,'.')='VAL'">EquityAsset</xsl:when>
        <xsl:when test="substring-after($pUnderlyingCommodityCode,'.')='IND'">Index</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vSymbolSuffix">
      <xsl:call-template name="SymbolSuffixOrNull">
        <xsl:with-param name="pFullSymbol" select="$pUnderlyingCommodityCode"/>
        <xsl:with-param name="pSeparator" select="'.'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <!-- EquityAsset -> Update de ASSET_EQUITY_RDCMK.SYMBOL -->
      <xsl:when test="$vAssetCategory = 'EquityAsset'">
        <tbl n="ASSET_EQUITY_RDCMK" a="U">
          <c n="IDASSET" dk="true" t="i">
            <xsl:call-template name="SQL_UpdUNDERLYINGSYMBOL">
              <xsl:with-param name="pResult" select="'IDASSET'"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
              <!--<xsl:with-param name="pUnderlyingCommodityCode" select="$pUnderlyingCommodityCode"/>-->
              <xsl:with-param name="pSymbolSuffix" select="$vSymbolSuffix"/>
              <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
              <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
            </xsl:call-template>
          </c>
          <c n="IDM_RELATED" dk="true" t="i">
            <xsl:call-template name="SQL_UpdUNDERLYINGSYMBOL">
              <xsl:with-param name="pResult" select="'IDM'"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
              <!--<xsl:with-param name="pUnderlyingCommodityCode" select="$pUnderlyingCommodityCode"/>-->
              <xsl:with-param name="pSymbolSuffix" select="$vSymbolSuffix"/>
              <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
              <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
            </xsl:call-template>
          </c>
          <c n="SYMBOL" dku="true" v="{substring-before($pUnderlyingCommodityCode, '.')}" />
          <!-- FI  20220104 [XXXXX] Ajout Call InfoTable_IU template --> 
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:when>

      <!-- Index -> Update de ASSET_INDEX.SYMBOL -->
      <xsl:when test="$vAssetCategory = 'Index'">
        <tbl n="ASSET_INDEX" a="U">
          <c n="IDASSET" dk="true" t="i">
            <xsl:call-template name="SQL_UpdUNDERLYINGSYMBOL">
              <xsl:with-param name="pResult" select="'IDASSET'"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
              <!--<xsl:with-param name="pUnderlyingCommodityCode" select="$pUnderlyingCommodityCode"/>-->
              <xsl:with-param name="pSymbolSuffix" select="$vSymbolSuffix"/>
              <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
              <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
            </xsl:call-template>
          </c>
          <c n="IDM_RELATED" dk="true" t="i">
            <xsl:call-template name="SQL_UpdUNDERLYINGSYMBOL">
              <xsl:with-param name="pResult" select="'IDM_RELATED'"/>
              <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
              <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
              <!--<xsl:with-param name="pUnderlyingCommodityCode" select="$pUnderlyingCommodityCode"/>-->
              <xsl:with-param name="pSymbolSuffix" select="$vSymbolSuffix"/>
              <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
              <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
            </xsl:call-template>
          </c>
          <c n="SYMBOL" dku="true" v="{substring-before($pUnderlyingCommodityCode, '.')}" />
          <!-- FI  20220104 [XXXXX] Ajout Call InfoTable_IU template -->   
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="SQL_UpdUNDERLYINGSYMBOL">
    <xsl:param name="pResult"/>
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pAssetCategory"/>
    <!-- PL 20141007 Replace pUnderlyingCommodityCode by pSymbolSuffix -->
    <!--<xsl:param name="pUnderlyingCommodityCode"/>-->
    <xsl:param name="pSymbolSuffix"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pExchangeAcronym"/>

    <xsl:choose>
      <xsl:when test="$pAssetCategory = 'EquityAsset'">
        <sql cd="select" rt="{$pResult}" cache="true">
          select aeq.IDASSET, dc.IDM
          from dbo.ASSET_EQUITY aeq
          inner join dbo.ASSET_EQUITY_RDCMK aeqrdc on (aeqrdc.IDASSET = aeq.IDASSET) and (aeqrdc.SYMBOLSUFFIX = @SYMBOLSUFFIX)
          inner join dbo.MARKET m on (m.IDM = aeqrdc.IDM_RELATED) and (m.EXCHANGEACRONYM = @EXCHANGEACRONYM)
          inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aeq.IDASSET) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.ASSETCATEGORY = @ASSETCATEGORY)
          where ( (aeq.DTDISABLED is null) or (aeq.DTDISABLED > @BUSINESSDATETIME) )
            and ( (aeqrdc.DTDISABLED is null) or (aeqrdc.DTDISABLED > @BUSINESSDATETIME) )
            and ( (m.DTDISABLED is null) or (m.DTDISABLED > @BUSINESSDATETIME) )
            and ( (dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME) )
          <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
          <p n="ASSETCATEGORY" v="{$pAssetCategory}"/>
          <p n="CONTRACTSYMBOL" v="{$pCommodityCode}"/>
          <!--<p n="SYMBOLSUFFIX" v="{substring-after($pUnderlyingCommodityCode,'.')}"/>-->
          <p n="SYMBOLSUFFIX" v="{$pSymbolSuffix}"/>
          <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
        </sql>
      </xsl:when>

      <xsl:when test="$pAssetCategory = 'Index'">
        <sql cd="select" rt="{$pResult}" cache="true">
          select aid.IDASSET, dc.IDM
          from dbo.ASSET_INDEX aid
          inner join dbo.MARKET m on (m.IDM = aid.IDM_RELATED) and (m.EXCHANGEACRONYM = @EXCHANGEACRONYM)
          inner join dbo.DERIVATIVECONTRACT dc on (dc.IDASSET_UNL = aid.IDASSET) and (dc.CONTRACTSYMBOL = @CONTRACTSYMBOL) and (dc.ASSETCATEGORY = @ASSETCATEGORY)
          where (aid.SYMBOLSUFFIX = @SYMBOLSUFFIX)
            and ( (aid.DTDISABLED is null) or (aid.DTDISABLED > @BUSINESSDATETIME) )
            and ( (m.DTDISABLED is null) or (m.DTDISABLED > @BUSINESSDATETIME) )
            and ( (dc.DTDISABLED is null) or (dc.DTDISABLED > @BUSINESSDATETIME) )
          <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
          <p n="ASSETCATEGORY" v="{$pAssetCategory}"/>
          <p n="CONTRACTSYMBOL" v="{$pCommodityCode}"/>
          <!--<p n="SYMBOLSUFFIX" v="{substring-after($pUnderlyingCommodityCode,'.')}"/>-->
          <p n="SYMBOLSUFFIX" v="{$pSymbolSuffix}"/>
          <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
        </sql>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <!-- FI 20220106 [XXXXX] Add Template GetParameterIDAssetETD -->
  <xsl:template name="GetParameterIDAssetETD">
    <xsl:param name="pElementName"/>
    <xsl:param name="pFileFormat"/>
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pStrikePrice"/>

    <xsl:element name="{$pElementName}">
      <xsl:attribute name="n">IDASSET</xsl:attribute>
      <xsl:choose>
        <!-- Prendre directement l'IDASSET du fichier enrichie lorsqu'il est présent -->
        <xsl:when test="(./d[@n='IDAsset']/@v)">
          <xsl:attribute name="v">
            <xsl:value-of select="./d[@n='IDAsset']/@v"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="SQL_ASSET_ETD">
            <xsl:with-param name="pResultColumn" select="'IDASSET'"/>
            <xsl:with-param name="pFileFormat" select="$pFileFormat"/>
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
            <xsl:with-param name="pContractSymbol" select="$pCommodityCode"/>
            <xsl:with-param name="pCategory" select="$pCategory"/>
            <xsl:with-param name="pMaturityMonthYear" select="$pMaturityMonthYear"/>
            <xsl:with-param name="pPutCall" select="$pPutCall"/>
            <xsl:with-param name="pStrikePrice" select="$pStrikePrice"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>
  
  
</xsl:stylesheet>