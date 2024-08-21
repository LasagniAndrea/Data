<?xml version="1.0" encoding="utf-8"?>
<!--
=============================================================================================
 Summary : Common SPAN Risk Margin parameters templates
 File    : RiskData_SPAN_Import_Common.xsl
=============================================================================================
FI 20200901 [25468] use of GetUTCDateTimeSys
RD 20170803 [23248] Ajout de (NOLOCK) et Refactoring pour éviter les Deadlock
PM 20160811 [22390][22388] Ajout d'une convertion de base pour le 'option cabinet price'
PL 20141007 New template: SymbolSuffixOrNull
PM 20140702 [20163][20157] Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY
PM 20140528 [19992] Gestion plus dynamique de la conversion des strikes
FI 20140108 [19460] Les parameters SQL doivent être en majuscule
PM 20131219 [19375] Gestion du calcul du Cabinet Price en base 10000 et plus
EG 20130726 Add Control (Column: IDIMSPANINTRASPR_H) on template "IMSPANINTRALEG_H"
PM 20120305 Templates commun aux XML PostMapping de type SPAN
=============================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>

  <xsl:variable name="vXSLVersionRiskDataSPANCommon">v6.0.0.0</xsl:variable>
  <xsl:variable name="vXSLFileNameRiskDataSPANCommon">RiskData_SPAN_Import_Common.xsl</xsl:variable>

  <!-- Variable empéchant les sauts de ligne dans les requêtes -->
  <xsl:variable name="vSpace"> </xsl:variable>

  <!-- ================================================================== -->
  <!-- Lire et Ajouter la Colonne IDIMSPAN_H -->
  <xsl:template name="Col_IDIMSPAN_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <c n="IDIMSPAN_H" dk="true" t="i">
      <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
      <sql cd="select" rt="IDIMSPAN_H" cache="true">
        select s.IDIMSPAN_H
        from dbo.IMSPAN_H s
        where ( s.DTBUSINESSTIME = @BUSINESSDATETIME )
        and ( s.SETTLEMENTSESSION = @SETTLEMENTORINTRADAY)
        and ( s.EXCHANGECOMPLEX = @EXCHANGECOMPLEX )
        <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
        <p n="SETTLEMENTORINTRADAY" v="{$pSettlementorIntraday}"/>
        <p n="EXCHANGECOMPLEX" v="{$pExchangeComplex}"/>
      </sql>
      <ctls>
        <ctl a="RejectRow" rt="true">
          <sl fn="IsNull()"/>
          <li st="REJECT" ex="true">
            <code>SYS</code>
            <number>2006</number>
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
  </xsl:template>
  <!-- SQL de lecture de la Colonne IDIMSPANGRPCTR_H -->
  <xsl:template name="SQL_IDIMSPANGRPCTR_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedCommodityCode"/>
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <sql cd="select" rt="IDIMSPANGRPCTR_H"  cache="true">
      select g.IDIMSPANGRPCTR_H
      from dbo.IMSPANGRPCTR_H g
      inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )
      <!-- RD 20130328 [18535] Bug sous SqlServer / si @ExchangeAcronym est vide alors il n'est pas considéré comme IS NULL-->
      <xsl:if test="string-length($pExchangeAcronym) > 0">and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )</xsl:if>
      <!--and ( ( e.EXCHANGEACRONYM = @ExchangeAcronym ) or ( @ExchangeAcronym is null ) )<xsl:value-of select="$vSpace"/>-->
      <!-- Vérification de IDIMSPAN_H -->
      <xsl:call-template name="innerIDIMSPAN_H">
        <xsl:with-param name="pTable" select="'e'"/>
      </xsl:call-template>
      and ( g.COMBCOMCODE = @COMBINEDCOMMODITYCODE ) <xsl:value-of select="$vSpace"/>
      <xsl:call-template name="paramInnerIDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <xsl:if test="string-length($pExchangeAcronym) > 0">
        <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
      </xsl:if>
      <!--<p n="ExchangeAcronym" v="{$pExchangeAcronym}"/>-->
      <p n="COMBINEDCOMMODITYCODE" v="{$pCombinedCommodityCode}"/>
    </sql>
  </xsl:template>
  <!-- Lire et Ajouter la Colonne IDIMSPANGRPCTR_H -->
  <xsl:template name="Col_IDIMSPANGRPCTR_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCombinedCommodityCode"/>

    <c n="IDIMSPANGRPCTR_H" dk="true" t="i">
      <xsl:call-template name="SQL_IDIMSPANGRPCTR_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
        <xsl:with-param name="pCombinedCommodityCode" select="$pCombinedCommodityCode"/>
      </xsl:call-template>
      <ctls>
        <ctl a="RejectRow" rt="true">
          <sl fn="IsNull()"/>
          <li st="REJECT" ex="true">
            <xsl:attribute name="msg">
              &lt;b&gt;Combined Commodity Not Found.&lt;/b&gt;
              &lt;b&gt;Cause:&lt;/b&gt; The Combined Commodity &lt;b&gt;<xsl:value-of select="$pCombinedCommodityCode"/>&lt;/b&gt; is not found for Business Date &lt;b&gt;<xsl:value-of select="$pBusinessDateTime"/>&lt;/b&gt; ( &lt;b&gt;<xsl:value-of select="$pSettlementorIntraday"/>&lt;/b&gt; ) on Clearing Organization  &lt;b&gt;<xsl:value-of select="$pExchangeComplex"/>&lt;/b&gt;.
              <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vXSLFileNameRiskDataSPANCommon"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vXSLVersionRiskDataSPANCommon"/><xsl:text>&#xa;</xsl:text>
            </xsl:attribute>
            <!--
				  <c>SYS</c>
				  <n>2006</n>
				  <d1>
					<xsl:value-of select="$vXSLFileName"/>
				  </d1>
				  <d2>
					<xsl:value-of select="$vXSLVersion"/>
				  </d2>-->
          </li>
        </ctl>
      </ctls>
    </c>
  </xsl:template>
  <!-- SQL de lecture de la Colonne IDIMSPANCONTRACT_H -->
  <xsl:template name="SQL_IDIMSPANCONTRACT_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pContractType" select="'null'"/>

    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <sql cd="select" rt="IDIMSPANCONTRACT_H" cache="true">
      select c.IDIMSPANCONTRACT_H
      from dbo.IMSPANCONTRACT_H c
      inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H ) and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )<xsl:value-of select="$vSpace"/>
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
      <!-- Param Exchange Code / Acronym -->
      <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
      <!-- Param Commodity Code -->
      <p n="COMMODITYCODE" v="{$pCommodityCode}"/>
      <!-- Param Contract Type / Product Type Code -->
      <p n="CONTRACTTYPE" v="{$pContractType}"/>
    </sql>
  </xsl:template>
  <!-- Lire et Ajouter la Colonne IDIMSPANGRPCTR_H -->
  <xsl:template name="Col_IDIMSPANCONTRACT_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pContractType"/>

    <c n="IDIMSPANCONTRACT_H" dk="true" t="i">
      <xsl:call-template name="SQL_IDIMSPANCONTRACT_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pExchangeAcronym" select="$pExchangeAcronym"/>
        <xsl:with-param name="pCommodityCode" select="$pCommodityCode"/>
        <xsl:with-param name="pContractType" select="$pContractType"/>
      </xsl:call-template>
      <ctls>
        <ctl a="RejectRow" rt="true">
          <sl fn="IsNull()"/>
          <li st="REJECT" ex="true">
            <code>SYS</code>
            <number>2006</number>
            <d1>
              <xsl:value-of select="$vXSLFileNameRiskDataSPANCommon"/>[<xsl:value-of select="$vXSLVersionRiskDataSPANCommon"/>]
            </d1>
            <d2>
              (<xsl:value-of select="$pCommodityCode"/>/<xsl:value-of select="$pContractType"/>)
            </d2>
          </li>
        </ctl>
      </ctls>
    </c>
  </xsl:template>
  <!-- SQL de lecture de la Colonne IDIMSPANTIER_H -->
  <xsl:template name="SQL_IDIMSPANTIER_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <xsl:param name="pCombinedContractCode"/>
    <xsl:param name="pTierNumber"/>

    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <sql cd="select" rt="IDIMSPANTIER_H" cache="true">
      select t.IDIMSPANTIER_H
      from dbo.IMSPANTIER_H t
      inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H ) and ( g.COMBCOMCODE = @COMBINEDCOMMODITYCODE )
      inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )<xsl:value-of select="$vSpace"/>
      <!-- Vérification de IDIMSPAN_H -->
      <xsl:call-template name="innerIDIMSPAN_H">
        <xsl:with-param name="pTable" select="'e'"/>
      </xsl:call-template>
      and ( t.SPREADTYPE = 'A' )
      and ( t.TIERNUMBER = @TIERNUMBER )<xsl:value-of select="$vSpace"/>
      <!-- Vérification de IDIMSPAN_H -->
      <xsl:call-template name="paramInnerIDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <p n="TIERNUMBER" v="{$pTierNumber}"/>
      <p n="COMBINEDCOMMODITYCODE" v="{$pCombinedContractCode}"/>
    </sql>
  </xsl:template>
  <!-- SQL de lecture de la table DERIVATIVECONTRACT -->
  <!-- PM 20131219 [19375] Ajout paramètres pPrice pour le calcul du Cabinet Price Variable en base 10000 et plus -->
  <xsl:template name="SQL_DERIVATIVECONTRACT">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pPrice" select="0"/>

    <!-- PM 20131219 [19375] Tenir compte de la base 10000 et plus pour le calcul du Cabinet Price -->
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <!-- PM 20160811 [22390][22388] Ajout convertion de base pour CABINETOPT -->
    <sql cd="select" cache="true">
      <xsl:attribute name="rt">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      with DC_CABINETPRICE(IDDC, CABINETOPT)
      as
      (
      select dc.IDDC,
      case when dc.CONTRACTMULTIPLIER = 0 then 0 else (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end) * dc.CABINETOPTVALUE / dc.CONTRACTMULTIPLIER end as CABINETOPT
      from dbo.DERIVATIVECONTRACT dc
      )
      select dc.IDDC,
      dc.IDM,
      case when dc.INSTRUMENTDEN >= 100 then cp.CABINETOPT else ((cp.CABINETOPT - (floor(cp.CABINETOPT) * 0.0000000001)) * dc.INSTRUMENTDEN / 100) + (floor(cp.CABINETOPT) * 0.0000000001) end as CABINETOPT,
      case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end * @PRICE as VARIABLECABINETOPT,
      dc.PRICEDECLOCATOR,
      dc.STRIKEDECLOCATOR,
      dc.PRICEALIGNCODE,
      dc.STRIKEALIGNCODE,
      case when dc.ISAUTOSETTING = 1 then dc.ISAUTOSETTING else null end as ISAUTOSETTING,
      case when dc.ISAUTOSETTINGASSET = 1 then dc.ISAUTOSETTINGASSET else null end as ISAUTOSETTINGASSET
      from dbo.DERIVATIVECONTRACT dc
      join DC_CABINETPRICE cp on cp.IDDC = dc.IDDC
      inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
      and ( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM )
      and ( ( mk.DTDISABLED is null ) or ( mk.DTDISABLED > @BUSINESSDATETIME ) )
      <!--							             and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )-->
      where ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BUSINESSDATETIME ) )
      <!--			   and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
      and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
      and ( dc.CATEGORY = @CATEGORY )
      <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
      <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
      <p n="CONTRACTSYMBOL" v="{$pContractSymbol}"/>
      <p n="CATEGORY" v="{$pCategory}"/>
      <p n="PRICE" t="dc" v="{$pPrice}"/>
    </sql>
  </xsl:template>
  <!-- SQL de lecture de la table DERIVATIVEATTRIB à partir d'un IDASSET -->
  <xsl:template name="SQL_DERIVATIVEATTRIB_From_Asset">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pIdAsset"/>
    <xsl:param name="pContractMultiplier"/>
    <xsl:param name="pFactor"/>

    <!-- PM 20131202 [19321] Modification du SELECT pour renvoyer NULL si pas de différence avec l'existant -->
    <!-- Si le ContractMultiplier en paramètre et le même que celui du DC, alors la colonne CONTRACTMULTIPLIER retourne NULL, sinon retourne la valeur du paramètre -->
    <!-- PM 20131211 [19259] Ajout colonne ISAUTOSETTING du DC -->
    <!-- PM 20140107 [19452] Ajout type des paramètres -->
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <sql cd="select" cache="true">
      <xsl:attribute name="rt">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      select da.IDDERIVATIVEATTRIB,
      case when (dc.CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER) then null else @CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER,
      case when (dc.FACTOR != case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end) then null else (case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end) end as FACTOR,
      case when dc.ISAUTOSETTING = 1 then dc.ISAUTOSETTING else null end as ISAUTOSETTING
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
      inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
      where (a.IDASSET = @IDASSET)
      and (((dc.CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER)
      or (da.CONTRACTMULTIPLIER is null) or (da.CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER)
      )
      or ((dc.FACTOR != case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end)
      or (da.FACTOR is null) or (da.FACTOR != case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end))
      )
      <p n="IDASSET" t="i" v="{$pIdAsset}"/>
      <p n="CONTRACTMULTIPLIER" t="dc" v="{$pContractMultiplier}"/>
      <p n="FACTOR" t="dc" v="{$pFactor}"/>
    </sql>
  </xsl:template>
  <!-- SQL de lecture de la table ASSET_ETD -->
  <xsl:template name="SQL_ASSET_ETD">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pFileFormat"/>
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pStrikePrice"/>
    <xsl:param name="pIsImportFullFile"/>

    <!-- FL 20130507 : Rajout dans le select et le where de (case when dc.INSTRUMENTDEN = 10000 then 100 else 1 end) 
         pour resoudre Pb XI: CORN Future/Option on XCBT ou le prix et le strike sont exprimés en Déciamles 
         (Gestion de la base 10000) -->
    <!-- PM 20131016 [19072] power(10, ... => power(10.000000000, ... : car en SqlServer power retourne le même type que passé en paramètre -->
    <!-- PM 20131022 : (case when dc.INSTRUMENTDEN = 10000 then 100 else 1 end) => (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end)-->
    <!-- PM 20131022 [19086] : Gestion des strikes en 1/4 (0.25, 0.75) dont il manque le dernier chiffre décimal-->
    <!-- Cas de l'option 5-Year U.S. Treasury Note (25)-->
    <!-- PM 20140528 [19992] Gestion plus dynamique de la conversion des strikes-->
    <!-- Dernier chiffre du strike -->
    <xsl:variable name="vStrikeLastDigit">
      <xsl:choose>
        <xsl:when test="$pCategory='O'">
          <xsl:value-of select="number(substring( $pStrikePrice, string-length( $pStrikePrice ), 1 ))"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAddStrikeValue">
      <xsl:choose>
        <xsl:when test="(($vStrikeLastDigit=1) or ($vStrikeLastDigit=6))">0.25</xsl:when>
        <xsl:when test="(($vStrikeLastDigit=2) or ($vStrikeLastDigit=7))">0.50</xsl:when>
        <xsl:when test="(($vStrikeLastDigit=3) or ($vStrikeLastDigit=8))">0.75</xsl:when>
        <xsl:otherwise>0.00</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <!-- PM 20140528 [19992] Gestion plus dynamique de la conversion des strikes-->
    <sql cd="select" cache="true">
      <xsl:attribute name="rt">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      select distinct tblmain.IDASSET, tblmain.IDC, tblmain.STRIKEPRICE
      from (
      select a.IDASSET, dc.IDC_PRICE IDC,
      case when dc.CATEGORY = 'F' then null
      else @STRIKEPRICE * (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end)
      <xsl:if test="$pFileFormat != 'UP'">
        / (case when dc.STRIKEDECLOCATOR is null then 1 else power(10.000000000, dc.STRIKEDECLOCATOR ) end)
        + ((case dc.STRIKESPANCODE
        when 'Quarters' then (case when @LASTDIGIT in (2,7) then @ADDSTRIKE else 0.00 end)
        when 'Eighths' then @ADDSTRIKE
        else 0.00 end)
        / (case when dc.STRIKEDECLOCATOR is null then 1 else power(10.000000000, dc.STRIKEDECLOCATOR ) end))
      </xsl:if>
      end as STRIKEPRICE
      from dbo.ASSET_ETD a
      inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
      and ( ( da.DTDISABLED is null ) or ( da.DTDISABLED > @BUSINESSDATETIME ) )
      <!--									  and ( ( da.DTENABLED is null ) or ( da.DTENABLED &lt;= @BusinessDateTime ) )-->
      inner join dbo.MATURITY ma on ( ma.IDMATURITY = da.IDMATURITY )
      and ( ( ma.DTDISABLED is null ) or ( ma.DTDISABLED > @BUSINESSDATETIME ) )
      <!--										and ( ( ma.DTENABLED is null ) or ( ma.DTENABLED &lt;= @BusinessDateTime ) )-->
      inner join dbo.MATURITYRULE mr on ( mr.IDMATURITYRULE = ma.IDMATURITYRULE )
      and ( ( mr.DTDISABLED is null ) or ( mr.DTDISABLED > @BUSINESSDATETIME ) )
      <!--										and ( ( mr.DTENABLED is null ) or ( mr.DTENABLED &lt;= @BusinessDateTime ) )-->
      inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )
      and ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BUSINESSDATETIME ) )
      <!--										and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
      inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
      and ( ( mk.DTDISABLED is null ) or ( mk.DTDISABLED > @BUSINESSDATETIME ) )
      <!--										and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )-->
      <xsl:if test="$pIsImportFullFile='false'">	           inner join TRADE tr on ( tr.IDASSET = a.IDASSET )</xsl:if>
      where ( ( a.DTDISABLED is null ) or ( a.DTDISABLED > @BUSINESSDATETIME ) )
      <!--					      and ( ( a.DTENABLED is null ) or ( a.DTENABLED &lt;= @BusinessDateTime ) )-->
      <!--PM 20140702 [20163][20157] Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY-->
      and (( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM ) or ((mk.EXCHANGEACRONYM = 'L') and (@EXCHANGEACRONYM = 'O') and (dc.ASSETCATEGORY = 'Index')))
      and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
      and ( dc.CATEGORY = @CATEGORY )
      and (((mr.MMYFMT = 0) and (ma.MATURITYMONTHYEAR = substring( @MATURITYMONTHYEAR, 1, 6 )))
      or ( ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR ))
      <xsl:if test="$pCategory='O'">
        and ( a.PUTCALL = @PUTCALL )
        and ( a.STRIKEPRICE = @STRIKEPRICE * (case when dc.INSTRUMENTDEN >= 10000 then dc.INSTRUMENTDEN / 100 else 1 end)
        <xsl:if test="$pFileFormat != 'UP'">
          / (case when dc.STRIKEDECLOCATOR is null then 1 else power(10.000000000, dc.STRIKEDECLOCATOR ) end)
          + ((case dc.STRIKESPANCODE
          when 'Quarters' then (case when @LASTDIGIT in (2,7) then @ADDSTRIKE else 0.00 end)
          when 'Eighths' then @ADDSTRIKE
          else 0.00 end)
          / (case when dc.STRIKEDECLOCATOR is null then 1 else power(10.000000000, dc.STRIKEDECLOCATOR ) end))
        </xsl:if> )
      </xsl:if><xsl:if test="$pCategory!='O'">
        <!-- Pour un prix "non option" (actuellement future):
	             s'il existe au moins un trade négocié sur une option
	             dont le sous-jacent est l'asset en cours de traitement.-->
        union all
        select a.IDASSET, dc.IDC_PRICE IDC, null STRIKEPRICE
        from dbo.ASSET_ETD a
        inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
        and ( ( da.DTDISABLED is null ) or ( da.DTDISABLED > @BUSINESSDATETIME ) )
        <!--                    and ( ( da.DTENABLED is null ) or ( da.DTENABLED &lt;= @BusinessDateTime ) )-->
        inner join dbo.MATURITY ma on ( ma.IDMATURITY = da.IDMATURITY )
        and ( ( ma.DTDISABLED is null ) or ( ma.DTDISABLED > @BUSINESSDATETIME ) )
        <!--                    and ( ( ma.DTENABLED is null ) or ( ma.DTENABLED &lt;= @BusinessDateTime ) )-->
        inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )
        and ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BUSINESSDATETIME ) )
        <!--									  and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
        inner join dbo.MARKET mk on ( mk.IDM = dc.IDM )
        and ( ( mk.DTDISABLED is null ) or ( mk.DTDISABLED > @BUSINESSDATETIME ) )
        <!--									  and ( ( mk.DTENABLED is null ) or ( mk.DTENABLED &lt;= @BusinessDateTime ) )-->
        inner join dbo.DERIVATIVEATTRIB da_opt on ( da_opt.IDASSET = a.IDASSET )
        and ( ( da_opt.DTDISABLED is null ) or ( da_opt.DTDISABLED > @BUSINESSDATETIME ) )
        <!--										and ( ( da_opt.DTENABLED is null ) or ( da_opt.DTENABLED &lt;= @BusinessDateTime ) )-->
        inner join dbo.DERIVATIVECONTRACT dc_opt on ( dc_opt.IDDC = da_opt.IDDC ) and ( dc_opt.CATEGORY = 'O' ) and ( dc_opt.ASSETCATEGORY = 'Future' )
        and ( ( dc_opt.DTDISABLED is null ) or ( dc_opt.DTDISABLED > @BUSINESSDATETIME ) )
        <!--									  and ( ( dc_opt.DTENABLED is null ) or ( dc_opt.DTENABLED &lt;= @BusinessDateTime ) )-->
        inner join dbo.ASSET_ETD a_opt on ( a_opt.IDDERIVATIVEATTRIB = da_opt.IDDERIVATIVEATTRIB )
        and ( ( a_opt.DTDISABLED is null ) or ( a_opt.DTDISABLED > @BUSINESSDATETIME ) )
        <!--										and ( ( a_opt.DTENABLED is null ) or ( a_opt.DTENABLED &lt;= @BusinessDateTime ) )-->
        <xsl:if test="$pIsImportFullFile='false'">
          inner join dbo.TRADE tr on ( tr.IDASSET = a_opt.IDASSET )
        </xsl:if>
        where ( ( a.DTDISABLED is null ) or ( a.DTDISABLED > @BUSINESSDATETIME ) )
        <!--					   and ( ( a.DTENABLED is null ) or ( a.DTENABLED &lt;= @BusinessDateTime ) )-->
        <!--PM 20140702 [20163][20157] Gestion des contrats du LIFFE dont les données sont dans le LIFFE EQUITY-->
        and (( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM ) or ((mk.EXCHANGEACRONYM = 'L') and (@EXCHANGEACRONYM = 'O') and (dc.ASSETCATEGORY = 'Index')))
        and ( dc.CONTRACTSYMBOL = @CONTRACTSYMBOL )
        and ( dc.CATEGORY = @CATEGORY )
        and ( ma.MATURITYMONTHYEAR = @MATURITYMONTHYEAR )
      </xsl:if>
      ) tblmain
      <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
      <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
      <p n="CONTRACTSYMBOL" v="{$pContractSymbol}"/>
      <p n="CATEGORY" v="{$pCategory}"/>
      <p n="MATURITYMONTHYEAR" v="{$pMaturityMonthYear}"/>
      <p n="STRIKEPRICE" t="dc" v="{$pStrikePrice}"/>
      <p n="LASTDIGIT" t="i" v="{$vStrikeLastDigit}"/>
      <p n="ADDSTRIKE" t="dc" v="{$vAddStrikeValue}"/>
      <xsl:if test="$pCategory='O'">
        <p n="PUTCALL" v="{$pPutCall}"/>
      </xsl:if>
    </sql>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- Vérifier IDIMSPAN_H dans les requêtes SQL -->
  <xsl:template name="innerIDIMSPAN_H">
    <xsl:param name="pTable"/>
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = <xsl:value-of select="$pTable"/>.IDIMSPAN_H )
    where ( s.DTBUSINESSTIME = @BUSINESSDATETIME )
    and ( s.SETTLEMENTSESSION = @SETTLEMENTORINTRADAY)
    and ( s.EXCHANGECOMPLEX = @EXCHANGECOMPLEX )<xsl:value-of select="$vSpace"/>
  </xsl:template>
  <xsl:template name="paramInnerIDIMSPAN_H">
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
    <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
    <p n="SETTLEMENTORINTRADAY" v="{$pSettlementorIntraday}"/>
    <p n="EXCHANGECOMPLEX" v="{$pExchangeComplex}"/>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- InfoTable_IU (Pour DTUPD, IDAUPD, DTINS, IDAINS) -->
  <xsl:template name="InfoTable_IU">
    <c n="DTUPD" t="dt">
      <sl fn="GetUTCDateTimeSys()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsInsert()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="IDAUPD" t="i">
      <sl fn="GetUserId()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsInsert()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="DTINS" t="dt">
      <sl fn="GetUTCDateTimeSys()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsUpdate()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="IDAINS" t="i">
      <sl fn="GetUserId()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsUpdate()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
  </xsl:template>
  <xsl:template name="InfoTable_U">
    <c n="DTUPD" t="dt">
      <sl fn="GetUTCDateTimeSys()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsInsert()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
    <c n="IDAUPD" t="i">
      <sl fn="GetUserId()"/>
      <ctls>
        <ctl a="RejectColumn" rt="true" lt="None">
          <sl fn="IsInsert()"/>
          <li st="NONE"/>
        </ctl>
      </ctls>
    </c>
  </xsl:template>

  <!-- Récupérer Attributes des Row -->
  <xsl:template name="RowAttributes">
    <xsl:param name="pInfo"/>

    <xsl:attribute name="id">
      <xsl:value-of select="@id"/>
    </xsl:attribute>
    <xsl:attribute name="src">
      <xsl:value-of select="@src"/>
    </xsl:attribute>
    <xsl:attribute name="rid">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:attribute>
    <xsl:if test="normalize-space(@lv)!=''">
      <xsl:attribute name="lv">
        <xsl:value-of select="@lv"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:if test="normalize-space($pInfo)!=''">
      <xsl:attribute name="i">
        <xsl:value-of select="$pInfo"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <!-- Tools templates -->
  <xsl:template name="ValueOrNull">
    <xsl:param name="pValue"/>

    <xsl:choose>
      <xsl:when test="normalize-space($pValue)=''">null</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SymbolSuffixOrNull">
    <xsl:param name="pFullSymbol"/>
    <xsl:param name="pSeparator"/>

    <xsl:choose>
      <xsl:when test="contains($pFullSymbol, $pSeparator)">
        <xsl:value-of select="concat($pSeparator, substring-after($pFullSymbol, $pSeparator))"/>
      </xsl:when>
      <xsl:otherwise>null</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CallOrPutOrNull">
    <xsl:param name="pValue"/>

    <xsl:choose>
      <xsl:when test="$pValue = 'P'">0</xsl:when>
      <xsl:when test="$pValue = 'C'">1</xsl:when>
      <xsl:otherwise>null</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="NumDiv">
    <xsl:param name="pValue" select="0"/>
    <xsl:param name="pDiv" select="0"/>

    <xsl:choose>
      <xsl:when test="normalize-space(translate($pValue,'+-',' '))=''">null</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="number(translate($pValue,'+',' ')) div $pDiv"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="AttribNumDiv">
    <xsl:param name="pValue" select="0"/>
    <xsl:param name="pDiv" select="0"/>

    <xsl:attribute name="v">
      <xsl:call-template name="NumDiv">
        <xsl:with-param name="pValue" select="$pValue"/>
        <xsl:with-param name="pDiv" select="$pDiv"/>
      </xsl:call-template>
    </xsl:attribute>
  </xsl:template>

  <xsl:template name="LondonExpiryDate">
    <xsl:param name="pExpiryDate"/>

    <xsl:choose>
      <xsl:when test="($pExpiryDate!='00000000') and ($pExpiryDate!='99999999') and (string-length($pExpiryDate)=8) and (substring($pExpiryDate,7,2)='00')">
        <xsl:value-of select="substring($pExpiryDate,1,6)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="ValueOrNull">
          <xsl:with-param name="pValue" select="$pExpiryDate"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- Calcul de Puissance pour les DECIMAL LOCATOR -->
  <xsl:template name="Power">
    <xsl:param name="pBase" select="0"/>
    <xsl:param name="pPower" select="1"/>

    <xsl:choose>
      <xsl:when test="number($pPower) &lt;= 0">
        <xsl:value-of select="1"/>
      </xsl:when>
      <xsl:when test="(number($pBase)=0) or (number($pBase)=1)">
        <xsl:value-of select="$pBase"/>
      </xsl:when>
      <xsl:when test="(number($pBase)=$pBase) and (number($pPower)=$pPower) and (number($pPower) &gt; 0)">
        <xsl:variable name="Sub">
          <xsl:call-template name="Power">
            <xsl:with-param name="pBase" select="number($pBase)"/>
            <xsl:with-param name="pPower" select="number($pPower) - 1"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="$pBase * $Sub"/>
      </xsl:when>
      <xsl:otherwise>1</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="SetDecLoc">
    <xsl:param name="pValue" select="0"/>
    <xsl:param name="pDecLoc" select="0"/>
    <xsl:param name="pDefault" select="null"/>

    <xsl:choose>
      <xsl:when test="normalize-space(translate($pValue,'+-',' '))=''">
        <xsl:value-of select="$pDefault"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="Div">
          <xsl:call-template name="Power">
            <xsl:with-param name="pBase" select="10"/>
            <xsl:with-param name="pPower" select="$pDecLoc"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="number(translate($pValue,'+',' ')) div $Div"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="AttribDecLoc">
    <xsl:param name="pValue" select="0"/>
    <xsl:param name="pDecLoc" select="0"/>
    <xsl:param name="pDefault" select="null"/>

    <xsl:attribute name="v">
      <xsl:call-template name="SetDecLoc">
        <xsl:with-param name="pValue" select="$pValue"/>
        <xsl:with-param name="pDecLoc" select="$pDecLoc"/>
        <xsl:with-param name="pDefault" select="$pDefault"/>
      </xsl:call-template>
    </xsl:attribute>
  </xsl:template>
  <xsl:template name="TenExponent">
    <xsl:param name="pValue" select="0"/>
    <xsl:param name="pExp" select="0"/>

    <xsl:choose>
      <xsl:when test="normalize-space(translate($pValue,'+-',' '))=''">null</xsl:when>
      <xsl:otherwise>
        <xsl:variable name="Mult">
          <xsl:call-template name="Power">
            <xsl:with-param name="pBase" select="10"/>
            <xsl:with-param name="pPower" select="$pExp"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="number(translate($pValue,'+',' ')) * $Mult"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPAN_H ========================================== -->
  <!-- ============== (Type "0" E, P et Type "1" S) ===================== -->
  <!-- ============== (Type "10" London) ================================ -->
  <xsl:template name="IMSPAN_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Autres Param. -->
    <xsl:param name="pAskedBusinessDateTime"/>
    <xsl:param name="pImportedFileName"/>
    <xsl:param name="pImportedFileFolder"/>
    <xsl:param name="pIOInputName"/>

    <r uc="true" id="r0" src="0" rid="{./r[1]/d[@n='RID']/@v}">
      <tbl n="IMSPAN_H" a="IU">
        <!-- Supression du contenu de la table IMSPANINTERLEG_H à chaque intégration SPAN US ou C21-->
        <!-- Attention : supression de tous les Leg des InterSpread -->
        <xsl:if test="($pFileFormat = 'U') or ($pFileFormat = 'P') or ($pFileFormat = 'U2') or ($pFileFormat = 'UP')">
          <!-- RD 20170803 [23248] Ajout de (NOLOCK) et Refactoring pour éviter les Deadlock-->
          <c n="IMSPANINTERLEG_H">
            <sql cd="delete" rt="">
              delete dbo.IMSPANINTERLEG_H
              where IDIMSPANINTERSPR_H in (
              select k.IDIMSPANINTERSPR_H
              from dbo.IMSPANINTERSPR_H k (NOLOCK)
              <!-- Vérification de IDIMSPAN_H -->
              <!--<xsl:call-template name="innerIDIMSPAN_H">
                <xsl:with-param name="pTable" select="'k'"/>
              </xsl:call-template>-->
              where (k.IDIMSPAN_H = @IDIMSPAN_H)
              )
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
        </xsl:if>
        <!-- Supression du contenu de la table IMSPANINTRALEG_H à chaque intégration pour les IMSPANINTRASPR_H de SPREADTYPE = 'S'-->
        <!-- RD 20170803 [23248] Ajout de (NOLOCK) et Refactoring pour éviter les Deadlock-->
        <c n="IMSPANINTRALEG_H">
          <sql cd="delete" rt="">
            delete dbo.IMSPANINTRALEG_H
            where IDIMSPANINTRASPR_H in (
            select i.IDIMSPANINTRASPR_H
            from dbo.IMSPANINTRASPR_H i (NOLOCK)
            inner join dbo.IMSPANGRPCTR_H g (NOLOCK) on ( g.IDIMSPANGRPCTR_H = i.IDIMSPANGRPCTR_H )
            inner join dbo.IMSPANEXCHANGE_H e (NOLOCK) on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )<xsl:value-of select="$vSpace"/>
            <!-- Vérification de IDIMSPAN_H -->
            <!--<xsl:call-template name="innerIDIMSPAN_H">
              <xsl:with-param name="pTable" select="'e'"/>
            </xsl:call-template>-->
            where (e.IDIMSPAN_H = @IDIMSPAN_H)
            and ( i.SPREADTYPE = 'S' )
            )
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
        <c n="ASKEDDTBUSINESSTIME" v="{$pAskedBusinessDateTime}">
          <ctls>
            <ctl a="RejectColumn" rt="true" v="true">
              <xsl:choose>
                <xsl:when test="$pBusinessDateTime!=$pAskedBusinessDateTime">
                  <xsl:attribute name="lt">Error</xsl:attribute>
                  <li st="err" ex="true">
                    <code>SYS</code>
                    <number>5301</number>
                    <d1>
                      <xsl:value-of select="$pBusinessDateTime"/>
                    </d1>
                    <d2>
                      <xsl:value-of select="$pAskedBusinessDateTime"/>
                    </d2>
                    <d3>
                      <xsl:value-of select="$pImportedFileName"/>
                    </d3>
                    <d4>
                      <xsl:value-of select="$pImportedFileFolder"/>
                    </d4>
                    <d5>
                      <xsl:value-of select="$pIOInputName"/>
                    </d5>
                  </li>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="lt">None</xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </ctl>
          </ctls>
        </c>

        <!-- Business Function -->
        <c n="BUSINESSFUNCTION" dku="true">
          <xsl:attribute name="v">
            <xsl:choose>
              <xsl:when test="./r[1]/d[@n='BF']/@v and normalize-space(./r[1]/d[@n='BF']/@v) != ''">
                <xsl:value-of select="./r[1]/d[@n='BF']/@v"/>
              </xsl:when>
              <xsl:otherwise>CLR</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </c>
        <!-- File Identifier -->
        <c n="FILEIDENTIFIER" dku="true" v="{./r[1]/d[@n='FI']/@v}"/>
        <!-- Overall Limit Option Value Flag -->
        <c n="ISOPTIONVALUELIMIT" dku="true" t="i">
          <xsl:attribute name="v">
            <xsl:choose>
              <xsl:when test="./r[1]/d[@n='LOV']/@v = 'Y'">1</xsl:when>
              <xsl:when test="./r[1]/d[@n='LOV']/@v = 'N' or normalize-space(./d[@n='LOV']/@v) = ''">0</xsl:when>
              <xsl:otherwise>0</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </c>
        <!-- Business Date Time -->
        <c n="DTBUSINESSTIME" dk="true" t="dt" v="{$pBusinessDateTime}">
          <ctls>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDate()" f="yyyy-MM-dd HH:mm:ss"/>
              <li st="err" ex="true">
                <xsl:attribute name="msg">
                  BusinessDateTime: Invalid Date  (<xsl:value-of select="$pBusinessDateTime"/>)
                </xsl:attribute>
              </li>
            </ctl>
          </ctls>
        </c>
        <!-- Settlement or Intraday -->
        <c n="SETTLEMENTSESSION" dk="true" v="{$pSettlementorIntraday}"/>
        <!-- Business Date -->
        <c n="DTBUSINESS" dku="true" t="d" v="{./r[1]/d[@n='BD']/@v}">
          <ctls>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDate()" f="YYYY-MM-DD"/>
              <li st="err" ex="true" msg="BusinessDate: Invalid Date"/>
            </ctl>
          </ctls>
        </c>
        <!-- File Creation Date & Time -->
        <c n="DTFILE" dku="true" t="dt" v="{concat(./r[1]/d[@n='FD']/@v, ' ', ./r[1]/d[@n='FT']/@v)}">
          <ctls>
            <ctl a="RejectRow" rt="false">
              <sl fn="IsDateTime()" f="yyyy-MM-dd HH:mm:ss"/>
              <li st="err" ex="true" msg="FileCreation: Invalid DateTime"/>
            </ctl>
          </ctls>
        </c>
        <!-- File Format -->
        <c n="FILEFORMAT" dku="true" v="{$pFileFormat}"/>
        <!-- Exchange Code / Acronym -->
        <c n="EXCHANGECOMPLEX" dk="true" v="{$pExchangeComplex}"/>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </r>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANEXCHANGE_H ================================== -->
  <!-- ============== (Type "1" S, E, P) ================================ -->
  <!-- ============== (Type "20" London) ================================ -->
  <xsl:template name="IMSPANEXCHANGE_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <tbl n="IMSPANEXCHANGE_H" a="IU">
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_IDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <c n="EXCHANGEACRONYM" dk="true">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
              <xsl:value-of select="./d[@n='EC']/@v"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="./d[@n='EA']/@v"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <c n="EXCHANGESYMBOL" dku="true" v="{./d[@n='EC']/@v}"/>
      <c n="FILEIDENTIFIER" dku="true" v="{./d[@n='FI']/@v}"/>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANTIER_H ====================================== -->
  <!-- ============== (Type "3", "S" S, E, P) =========================== -->
  <!-- ============== (Type "31" London) ================================ -->
  <!-- ============== (Type "34" London) ================================ -->
  <xsl:template name="IMSPANTIER_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du contrat -->
    <xsl:param name="pCombinedContractCode"/>

    <xsl:variable name="vRID">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:variable>

    <xsl:variable name="SpreadType">
      <xsl:choose>
        <!-- SPAN Intermonth Tier (Intracommodity) -->
        <xsl:when test="$vRID='3'">A</xsl:when>
        <!-- London SPAN Month Tier -->
        <xsl:when test="$vRID='31'">A</xsl:when>
        <!-- London SPAN Intercontract Tier -->
        <xsl:when test="$vRID='34'">L</xsl:when>
        <!-- SPAN Scanning/Intercommodity Tier -->
        <xsl:otherwise>R</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Pour chaque Tier : parcourir tous les Tier Number (T%N) -->
    <xsl:for-each select="./d[(substring(@n,1,2)='TN') and (number(substring-after(@n,'TN')) = substring-after(@n,'TN'))]">
      <!-- Si le Tier Number est renseigné -->
      <xsl:if test="normalize-space(@v)!=''">
        <xsl:variable name="Tier">
          <xsl:value-of select="substring-after(@n,'TN')"/>
        </xsl:variable>
        <tbl n="IMSPANTIER_H" a="IU">
          <!-- IDIMSPANGRPCTR_H -->
          <xsl:call-template name="Col_IDIMSPANGRPCTR_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
            <xsl:with-param name="pCombinedCommodityCode" select="$pCombinedContractCode"/>
          </xsl:call-template>
          <!-- SpreadType -->
          <c n="SPREADTYPE" dk="true" v="{$SpreadType}"/>
          <!-- Tier Number -->
          <c n="TIERNUMBER" dk="true" t="i" v="{@v}"/>
          <!-- Starting et Ending Month Year -->
          <xsl:if test="$vRID!='34'">
            <xsl:variable name="vStartingMonth">
              <xsl:choose>
                <xsl:when test="$vRID='31'">
                  <xsl:call-template name="LondonExpiryDate">
                    <xsl:with-param name="pExpiryDate" select="../d[@n=concat('SM',$Tier)]/@v"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="concat(../d[@n=concat('SM',$Tier)]/@v,normalize-space(../d[@n=concat('SDW',$Tier)]/@v))"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vEndingMonth">
              <xsl:choose>
                <xsl:when test="$vRID='31'">
                  <xsl:call-template name="LondonExpiryDate">
                    <xsl:with-param name="pExpiryDate" select="../d[@n=concat('EM',$Tier)]/@v"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="concat(../d[@n=concat('EM',$Tier)]/@v,normalize-space(../d[@n=concat('EDW',$Tier)]/@v))"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <c n="STARTINGMONTHYEAR" dku="true" v="{$vStartingMonth}"/>
            <c n="ENDINGMONTHYEAR" dku="true" v="{$vEndingMonth}"/>
          </xsl:if>
          <!-- Starting et Ending Tier -->
          <xsl:if test="$vRID='34'">
            <c n="STARTTIERNUMBER" dku="true" t="i" v="{../d[@n=concat('SM',$Tier)]/@v}"/>
            <c n="ENDTIERNUMBER" dku="true" t="i" v="{../d[@n=concat('EM',$Tier)]/@v}"/>
          </xsl:if>
          <!-- Short Option Minimum Charge Rate -->
          <xsl:if test="($SpreadType='R') and (($pFileFormat='U2') or ($pFileFormat='UP'))">
            <c n="SOMCHARGERATE" dku="true" t="dc" v="{../d[@n=concat('SOM',$Tier)]/@v}"/>
          </xsl:if>
          <!-- InfoTable_IU -->
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== RowINTRASPREAD ==================================== -->
  <!-- ============== (Type "C" S, E, P ) & (Type "E" S, E ) ============ -->
  <!-- ============== (Type "32" London) ================================ -->
  <!-- ============== (Type "35" London) ================================ -->
  <xsl:template name="RowINTRASPREAD">
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du contrat -->
    <xsl:param name="pCombinedContractCode"/>

    <xsl:variable name="vRID">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:variable>

    <xsl:variable name="vSpreadType">
      <xsl:choose>
        <!-- SPAN Tier to Tier Intracommodity Spreads -->
        <xsl:when test="$vRID='C'">T</xsl:when>
        <!-- SPAN Series to Series Intracommodity Spreads -->
        <xsl:when test="$vRID='E'">S</xsl:when>
        <!-- London SPAN Leg Spread Details -->
        <xsl:when test="$vRID='32'">T</xsl:when>
        <!-- London SPAN Strategy Spread Details -->
        <xsl:when test="$vRID='35'">L</xsl:when>
        <!-- -->
        <xsl:otherwise>?</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <r uc="true">
      <xsl:call-template name="RowAttributes"/>
      <xsl:call-template name="IMSPANINTRASPR_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
        <xsl:with-param name="pSpreadType" select="$vSpreadType"/>
      </xsl:call-template>
    </r>
    <r uc="true">
      <xsl:call-template name="RowAttributes">
        <xsl:with-param name="pInfo" select="leg"/>
      </xsl:call-template>
      <xsl:call-template name="IMSPANINTRALEG_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
        <xsl:with-param name="pSpreadType" select="$vSpreadType"/>
      </xsl:call-template>
    </r>

  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANINTRASPR_H ================================== -->
  <!-- ============== (Type "C" S, E, P ) & (Type "E" S, E ) ============ -->
  <!-- ============== (Type "32" London) ================================ -->
  <!-- ============== (Type "35" London) ================================ -->
  <xsl:template name="IMSPANINTRASPR_H">
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du contrat -->
    <xsl:param name="pCombinedContractCode"/>
    <!-- Param. du spread -->
    <xsl:param name="pSpreadType"/>

    <xsl:variable name="vRID">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:variable>

    <tbl n="IMSPANINTRASPR_H" a="IU">
      <!-- IDIMSPANGRPCTR_H -->
      <xsl:call-template name="Col_IDIMSPANGRPCTR_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
        <xsl:with-param name="pCombinedCommodityCode" select="$pCombinedContractCode"/>
      </xsl:call-template>
      <!-- Spread Type -->
      <c n="SPREADTYPE" dk="true" v="{$pSpreadType}"/>
      <!-- Spread Priority -->
      <c n="SPREADPRIORITY" dk="true" t="i" v="{./d[@n='SP']/@v}"/>
      <!-- Charge Rate -->
      <!-- PM 20150331 [20908] Type integer to decimal -->
      <c n="CHARGERATE" dku="true" t="dc" v="{./d[@n='CR']/@v}"/>
      <!-- Number of Legs -->
      <xsl:choose>
        <xsl:when test="($vRID='C') or ($vRID='32')  or ($vRID='35')">
          <c n="NUMBEROFLEG" dku="true" t="i" v="{./d[@n='NL']/@v}"/>
        </xsl:when>
        <xsl:otherwise>
          <c n="NUMBEROFLEG" dku="true" t="i" v="0"/>
        </xsl:otherwise>
      </xsl:choose>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANINTRALEG_H ================================== -->
  <!-- ============== (Type "C" S, E, P ) & (Type "E" S, E ) ============ -->
  <!-- ============== (Type "32" London) ================================ -->
  <!-- ============== (Type "35" London) ================================ -->
  <xsl:template name="IMSPANINTRALEG_H">
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du contrat -->
    <xsl:param name="pCombinedContractCode"/>
    <!-- Param. du spread -->
    <xsl:param name="pSpreadType"/>

    <xsl:variable name="vRID">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:variable>

    <pms>
      <pm n="RC_IDIMSPANINTRASPR_H">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDIMSPANINTRASPR_H">
          select i.IDIMSPANINTRASPR_H
          from dbo.IMSPANINTRASPR_H i
          inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = i.IDIMSPANGRPCTR_H ) and ( g.COMBCOMCODE = @COMBINEDCOMMODITYCODE )
          inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )<xsl:value-of select="$vSpace"/>
          <!-- Vérification de IDIMSPAN_H -->
          <xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'e'"/>
          </xsl:call-template>
          and ( i.SPREADTYPE = @SPREADTYPE )
          and ( i.SPREADPRIORITY = @SPREADPRIORITY )<xsl:value-of select="$vSpace"/>
          <xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
          <p n="SPREADTYPE" v="{$pSpreadType}"/>
          <p n="SPREADPRIORITY" v="{./d[@n='SP']/@v}"/>
          <p n="COMBINEDCOMMODITYCODE" v="{$pCombinedContractCode}"/>
        </sql>
      </pm>
    </pms>

    <!-- Pour Chaque Leg Number : LN% -->
    <xsl:for-each select="./d[(substring(@n,1,2)='MS') and (number(substring-after(@n,'MS')) = substring-after(@n,'MS'))]">
      <!-- Si le Leg Number est renseigné -->
      <xsl:if test="normalize-space(@v)!=''">
        <xsl:variable name="Leg">
          <xsl:value-of select="substring-after(@n,'MS')"/>
        </xsl:variable>
        <tbl n="IMSPANINTRALEG_H" a="IU">
          <!-- ID Spread Priority -->
          <c n="IDIMSPANINTRASPR_H" dk="true" t="i" v="parameters.RC_IDIMSPANINTRASPR_H">
            <!-- EG 20130726 Add Control IsNull()-->
            <ctls>
              <ctl a="RejectRow" rt="true">
                <sl fn="IsNull()"/>
              </ctl>
            </ctls>
          </c>
          <!--Tier or Maturity-->
          <xsl:choose>
            <xsl:when test="($vRID='C') or ($vRID='32')">
              <!-- ID Tier Number -->
              <c n="IDIMSPANTIER_H" dku="true" t="i">
                <xsl:call-template name="SQL_IDIMSPANTIER_H">
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                  <xsl:with-param name="pCombinedContractCode" select="$pCombinedContractCode"/>
                  <xsl:with-param name="pTierNumber" select="../d[@n=concat('TN',$Leg)]/@v"/>
                </xsl:call-template>
              </c>
            </xsl:when>
            <xsl:when test="($vRID='E')">
              <c n="MATURITYMONTHYEAR" dk="true" v="{concat('20',normalize-space(concat(../d[@n=concat('CM',$Leg)]/@v,../d[@n=concat('DW',$Leg)]/@v)))}"/>
            </xsl:when>
            <xsl:when test="($vRID='35')">
              <xsl:variable name="vExpiryDate">
                <xsl:call-template name="LondonExpiryDate">
                  <xsl:with-param name="pExpiryDate" select="../d[@n=concat('CM',$Leg)]/@v"/>
                </xsl:call-template>
              </xsl:variable>
              <c n="MATURITYMONTHYEAR" dk="true" v="{$vExpiryDate}"/>
            </xsl:when>
          </xsl:choose>
          <!--Leg Number-->
          <xsl:choose>
            <xsl:when test="($vRID='C')">
              <c n="LEGNUMBER" dk="true" t="i" v="{../d[@n=concat('LN',$Leg)]/@v}"/>
            </xsl:when>
            <xsl:when test="($vRID='32') or ($vRID='35')">
              <c n="LEGNUMBER" dk="true" t="i" v="{$Leg}"/>
            </xsl:when>
            <xsl:when test="($vRID='E')">
              <c n="LEGNUMBER" dk="true">
                <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
                <sql cd="select" rt="LEGNUMBER">
                  select (max(j.LEGNUMBER) + 1) as LEGNUMBER
                  from dbo.IMSPANINTRALEG_H j
                  inner join dbo.IMSPANINTRASPR_H i on ( i.IDIMSPANINTRASPR_H = j.IDIMSPANINTRASPR_H ) and ( i.IDIMSPANINTRASPR_H = @ID )
                  <p n="ID" v="parameters.RC_IDIMSPANINTRASPR_H"/>
                </sql>
                <ctls>
                  <ctl a="ApplyDefault" rt="true" lt="None">
                    <sl fn="IsEmpty()"/>
                    <li st="NONE"/>
                  </ctl>
                </ctls>
                <df v="1"/>
              </c>
            </xsl:when>
          </xsl:choose>
          <!--Delta Per Spread Ratio-->
          <c n="DELTAPERSPREAD" dku="true" t="dc" v="{../d[@n=concat('DR',$Leg)]/@v}"/>
          <!--Market Side-->
          <c n="LEGSIDE" dku="true" v="{@v}"/>
          <!-- InfoTable_IU -->
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:if>
    </xsl:for-each>
    <!-- Mise à jour du nombre de Jambe créée pour les Spreads sans Tier -->
    <xsl:if test="$vRID='E'">
      <tbl n="IMSPANINTRASPR_H" a="U">
        <c n="IDIMSPANINTRASPR_H" dk="true" t="i" v="parameters.RC_IDIMSPANINTRASPR_H">
          <ctls>
            <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
          </ctls>
        </c>
        <c n="NUMBEROFLEG" dku="true" t="i">
          <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
          <sql cd="select" rt="NUMBEROFLEGS">
            select count(*) NUMBEROFLEGS
            from dbo.IMSPANINTRALEG_H j
            inner join dbo.IMSPANINTRASPR_H i on ( i.IDIMSPANINTRASPR_H = j.IDIMSPANINTRASPR_H ) and ( i.IDIMSPANINTRASPR_H = @ID )
            <p n="ID" v="parameters.RC_IDIMSPANINTRASPR_H"/>
          </sql>
        </c>
      </tbl>
    </xsl:if>
  </xsl:template>
  <!-- ============== IMSPANDLVMONTH_H ================================== -->
  <!-- ============== (Type "4" S, E, P) ================================ -->
  <!-- ============== (Type "33" London) ================================ -->
  <xsl:template name="IMSPANDLVMONTH_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Param. du contrat -->
    <xsl:param name="pCombinedContractCode"/>

    <xsl:variable name="vRID">
      <xsl:value-of select="./d[@n='RID']/@v"/>
    </xsl:variable>

    <pms>
      <pm n="IDIMSPANGRPCTR_H">
        <xsl:call-template name="SQL_IDIMSPANGRPCTR_H">
          <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
          <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
          <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          <xsl:with-param name="pCombinedCommodityCode" select="$pCombinedContractCode"/>
        </xsl:call-template>
      </pm>
    </pms>
    <!-- Pour chaque Month -->
    <xsl:for-each select="./d[(substring(@n,1,2)='CM')  and (number(substring-after(@n,'CM')) = substring-after(@n,'CM'))]">
      <!-- Si l'échéance est renseignée -->
      <xsl:if test="normalize-space(@v)!=''">
        <xsl:variable name="Month">
          <xsl:value-of select="substring-after(@n,'CM')"/>
        </xsl:variable>
        <tbl n="IMSPANDLVMONTH_H" a="IU">
          <!-- IDIMSPANGRPCTR_H -->
          <c n="IDIMSPANGRPCTR_H" dk="true" t="i" v="parameters.IDIMSPANGRPCTR_H">
            <ctls>
              <ctl a="RejectRow" rt="true">
                <sl fn="IsEmpty()"/>
                <li st="NONE"/>
              </ctl>
            </ctls>
          </c>
          <!--Contract Month (+Contract Day or Week Code, si présent)-->
          <xsl:variable name="vExpiryDate">
            <xsl:choose>
              <xsl:when test="$vRID='4'">
                <xsl:value-of select="concat(@v, normalize-space(../d[@n=concat('DW',$Month)]/@v))"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="LondonExpiryDate">
                  <xsl:with-param name="pExpiryDate" select="@v"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <c n="MATURITYMONTHYEAR" dk="true" v="{$vExpiryDate}"/>
          <!--Month Number-->
          <xsl:choose>
            <xsl:when test="$vRID='4'">
              <c n="MONTHNUMBER" dku="true" t="i" v="{../d[@n=concat('MN',$Month)]/@v}"/>
            </xsl:when>
            <xsl:otherwise>
              <c n="MONTHNUMBER" dku="true" t="i" v="{$Month}"/>
            </xsl:otherwise>
          </xsl:choose>
          <!--Charge Rate Per Delta Consumed By Spreads-->
          <c n="CONSUMEDCHARGERATE" dku="true" t="i" v="{../d[@n=concat('DC',$Month)]/@v}"/>
          <!--Charge Rate Per Delta Remaining In Outrights-->
          <c n="REMAINCHARGERATE" dku="true" t="i" v="{../d[@n=concat('DR',$Month)]/@v}"/>
          <!--Delta Sign-->
          <xsl:if test="$vRID='33'">
            <c n="DELTASIGN" dku="true" v="{../d[@n=concat('DS',$Month)]/@v}"/>
          </xsl:if>
          <!-- InfoTable_IU -->
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANGRPCOMB_H =================================== -->
  <!-- ============== (Type "5" S, E, P) ================================ -->
  <!-- ============== (Type "6" S, E, P) ================================ -->
  <!-- ============== (Type "16" London) ================================ -->
  <xsl:template name="IMSPANGRPCOMB_H">
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>

    <tbl n="IMSPANGRPCOMB_H" a="IU">
      <!-- Combined Commodity Group Code -->
      <c n="COMBINEDGROUPCODE" dk="true" v="{normalize-space(./d[@n='CG']/@v)}">
        <ctls>
          <ctl a="RejectRow" rt="true">
            <sl fn="IsEmpty()"/>
            <li st="err" ex="true" msg="Combined Commodity Group Code is empty"/>
          </ctl>
        </ctls>
      </c>
      <!-- Description -->
      <c n="DESCRIPTION" dku="true" v="{normalize-space(./d[@n='Desc']/@v)}" />
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_IDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANINTERSPR_H ================================== -->
  <!-- ============== (Type "6" S, E, P) ================================ -->
  <!-- ============== (Type "14" Liffe) ================================= -->
  <xsl:template name="IMSPANINTERSPR_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Autres paramétres -->
    <!-- PM 20131112 [19165][19144] Ajout pSpreadPriority -->
    <xsl:param name="pSpreadPriority"/>

    <tbl n="IMSPANINTERSPR_H" a="IU">
      <!-- IDIMSPAN_H -->
      <xsl:call-template name="Col_IDIMSPAN_H">
        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
      </xsl:call-template>
      <!-- Spread Credit Rate Defined (E: "6" uniquement, sinon 0) -->
      <c n="ISCDTRATESEPARATED" dku="true" t="b">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="./d[@n='SCR']/@v = 'Y'">1</xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <!-- Minimum Number of Legs Required to Form Spread. -->
      <xsl:if test="./d[@n='IRSM']/@v='04'">
        <c n="MINNUMBEROFLEG" dku="true" t="i" v="{./d[@n='ML']/@v}">
          <ctls>
            <ctl a="ApplyDefault" rt="true">
              <sl fn="IsEmpty()"/>
            </ctl>
          </ctls>
          <df v="2"/>
        </c>
      </xsl:if>
      <!-- Intercommodity Spread Method Code -->
      <c n="INTERSPREADMETHOD" dku="true" v="{./d[@n='IRSM']/@v}">
        <ctls>
          <ctl a="ApplyDefault" rt="true">
            <sl fn="IsEmpty()"/>
          </ctl>
        </ctls>
        <df v="01"/>
      </c>
      <!-- Combined Group Code -->
      <c n="COMBINEDGROUPCODE" dk="true">
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="normalize-space(./d[@n='CG']/@v)=''">ALL</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="./d[@n='CG']/@v"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <!-- Spread Priority -->
      <!--<c n="SPREADPRIORITY" dk="true" t="i" v="{./d[@n='SP']/@v}"/>-->
      <!-- PM 20131112 [19165][19144] Ajout vSpreadPriority -->
      <c n="SPREADPRIORITY" dk="true" t="i" v="{$pSpreadPriority}"/>
      <!-- Spread Credit Rate, in percent -->
      <c n="CREDITRATE" dku="true" t="dc">
        <!-- PM 20130426 Ajout variable vCreditRate et initialisation à 0 par défaut -->
        <xsl:variable name="vCreditRate">
          <xsl:choose>
            <xsl:when test="normalize-space(./d[@n='CR']/@v)=''">
              <xsl:value-of select="0"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(./d[@n='CR']/@v)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:attribute name="v">
          <xsl:choose>
            <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
              <xsl:choose>
                <xsl:when test="$vCreditRate &gt; 100">
                  <xsl:value-of select="$vCreditRate div 100"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vCreditRate"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$pFileFormat='UP'">
              <xsl:call-template name="SetDecLoc">
                <xsl:with-param name="pValue" select="$vCreditRate"/>
                <xsl:with-param name="pDecLoc" select="./d[@n='CRD']/@v"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="$pFileFormat='U2'">
              <xsl:choose>
                <xsl:when test="./d[@n='CCM']/@v='F'">
                  <xsl:value-of select="$vCreditRate div 100"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vCreditRate div 10000"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="($pFileFormat='3') or ($pFileFormat='4') or ($pFileFormat='5') or ($pFileFormat='6')">
              <xsl:value-of select="$vCreditRate * 100"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$vCreditRate"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </c>
      <!-- Si Fichier Standard ou Expanded -->
      <xsl:if test="($pFileFormat='U') or ($pFileFormat='P') or ($pFileFormat='U2')">
        <!-- Spread Group Flag -->
        <c n="SPREADGROUPTYPE" dku="true">
          <xsl:attribute name="v">
            <xsl:call-template name="ValueOrNull">
              <xsl:with-param name="pValue" select="./d[@n='SGF']/@v"/>
            </xsl:call-template>
          </xsl:attribute>
        </c>
        <!-- Si Fichier Expanded -->
        <xsl:if test="$pFileFormat='U2'">
          <!-- Credit Calculation Method -->
          <c n="CREDITCALCMETHOD" dku="true">
            <xsl:attribute name="v">
              <xsl:call-template name="ValueOrNull">
                <xsl:with-param name="pValue" select="./d[@n='CCM']/@v"/>
              </xsl:call-template>
            </xsl:attribute>
          </c>
          <!-- Regulatory Status Eligibility Code -->
          <c n="ELIGIBILITYCODE" dku="true">
            <xsl:attribute name="v">
              <xsl:call-template name="ValueOrNull">
                <xsl:with-param name="pValue" select="./d[@n='SE']/@v"/>
              </xsl:call-template>
            </xsl:attribute>
          </c>
        </xsl:if>
      </xsl:if>
      <!-- Offset Rate -->
      <xsl:if test="normalize-space(./d[@n='OR']/@v)!=''">
        <c n="OFFSETRATE" dku="true" t="dc" v="{./d[@n='OR']/@v}"/>
      </xsl:if>
      <!-- Number of legs -->
      <xsl:if test="normalize-space(./d[@n='NL']/@v)!=''">
        <c n="NUMBEROFLEG" dku="true" t="i" v="{./d[@n='NL']/@v}"/>
      </xsl:if>
      <!-- InfoTable_IU -->
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- ============== IMSPANINTERLEG_H ================================== -->
  <!-- ============== (Type "6" S, E, P) ================================ -->
  <!-- ============== (Type "14" Liffe) ================================= -->
  <xsl:template name="IMSPANINTERLEG_H">
    <!-- PARAMETRES -->
    <xsl:param name="pFileFormat"/>
    <!-- Param. necessaire à IDIMSPAN_H -->
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <!-- Autres paramétres -->
    <!-- PM 20131112 [19165][19144] Ajout pSpreadPriority -->
    <xsl:param name="pSpreadPriority"/>

    <xsl:variable name="vCombinedGroup">
      <xsl:choose>
        <xsl:when test="normalize-space(./d[@n='CG']/@v)=''">ALL</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="./d[@n='CG']/@v"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--<xsl:variable name="vSpreadPriority">
      <xsl:value-of select="./d[@n='SP']/@v"/>
    </xsl:variable>-->
    <pms>
      <pm n="IDIMSPANINTERSPR_H">
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" rt="IDIMSPANINTERSPR_H">
          select k.IDIMSPANINTERSPR_H
          from dbo.IMSPANINTERSPR_H k <xsl:value-of select="$vSpace"/>
          <!-- Vérification de IDIMSPAN_H -->
          <xsl:call-template name="innerIDIMSPAN_H">
            <xsl:with-param name="pTable" select="'k'"/>
          </xsl:call-template>
          and ( k.COMBINEDGROUPCODE = @COMBINEDGROUPCODE )
          and ( k.SPREADPRIORITY = @SPREADPRIORITY )<xsl:value-of select="$vSpace"/>
          <xsl:call-template name="paramInnerIDIMSPAN_H">
            <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
            <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
            <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
          </xsl:call-template>
          <p n="COMBINEDGROUPCODE" v="{$vCombinedGroup}"/>
          <p n="SPREADPRIORITY" t="i" v="{$pSpreadPriority}"/>
        </sql>
      </pm>
    </pms>

    <!--Pour chaque Leg-->
    <xsl:for-each select="./d[((substring(@n,1,2)='CC') and (number(substring(@n,3,1)) = substring(@n,3,1)) ) or (@n='TCC')]">
      <!-- Si le Leg est renseigné -->
      <xsl:if test="normalize-space(@v)!=''">
        <!-- Numero du LEG courant -->
        <xsl:variable name="CurrentLegNumber">
          <xsl:choose>
            <xsl:when test="@n='TCC'">0</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="substring(@n,3,1)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="ExchangeAcronym">
          <xsl:choose>
            <!-- Quand Leg "Normale" -->
            <xsl:when test="$CurrentLegNumber != '0'">
              <xsl:choose>
                <!-- Quand Fichier Standard ou London -->
                <xsl:when test="normalize-space(../d[@n=concat('EA',$CurrentLegNumber)]/@v)=''">
                  <xsl:value-of select="../d[@n=concat('EC',$CurrentLegNumber)]/@v"/>
                </xsl:when>
                <!-- Quand Fichier Expanded ou Paris Expanded -->
                <xsl:otherwise>
                  <xsl:value-of select="../d[@n=concat('EA',$CurrentLegNumber)]/@v"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <!-- Quand Target Leg -->
            <xsl:when test="$CurrentLegNumber = '0'">
              <xsl:choose>
                <!-- Quand Leg "Normale" -->
                <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
                  <xsl:value-of select="../d[@n='TEC']/@v"/>
                </xsl:when>
                <!-- Quand Fichier Expanded -->
                <xsl:when test="$pFileFormat='U2'">
                  <xsl:value-of select="../d[@n='TEA']/@v"/>
                </xsl:when>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <tbl n="IMSPANINTERLEG_H" a="IU">
          <!-- IDIMSPANINTERSPR_H -->
          <c n="IDIMSPANINTERSPR_H" dk="true" t="i" v="parameters.IDIMSPANINTERSPR_H"/>
          <!-- Numero du Leg -->
          <c n="LEGNUMBER" dk="true" t="i">
            <xsl:choose>
              <xsl:when test="($pFileFormat = 'U') or ($pFileFormat = 'P') or ($pFileFormat = 'U2') or ($pFileFormat = 'UP')">
                <!-- Les jambes du Spread peuvent être sur plusieurs lignes -->
                <xsl:choose>
                  <!-- Quand Target Leg -->
                  <xsl:when test="$CurrentLegNumber = '0'">
                    <xsl:attribute name="v">0</xsl:attribute>
                  </xsl:when>
                  <!-- Quand Leg Normale -->
                  <xsl:otherwise>
                    <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
                    <sql cd="select" rt="LEGNUMBER">
                      select (max(l.LEGNUMBER) + 1) as LEGNUMBER
                      from dbo.IMSPANINTERLEG_H l
                      inner join dbo.IMSPANINTERSPR_H k on ( k.IDIMSPANINTERSPR_H = l.IDIMSPANINTERSPR_H )<xsl:value-of select="$vSpace"/>
                      <!-- Vérification de IDIMSPAN_H -->
                      <xsl:call-template name="innerIDIMSPAN_H">
                        <xsl:with-param name="pTable" select="'k'"/>
                      </xsl:call-template>
                      and ( k.COMBINEDGROUPCODE = @CombinedGroupCode )
                      and ( k.SPREADPRIORITY = @SpreadPriority )<xsl:value-of select="$vSpace"/>
                      <!-- Vérification de IDIMSPAN_H -->
                      <xsl:call-template name="paramInnerIDIMSPAN_H">
                        <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                        <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                        <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                      </xsl:call-template>
                      <p n="COMBINEDGROUPCODE" v="{$vCombinedGroup}"/>
                      <p n="SPREADPRIORITY" t="i" v="{$pSpreadPriority}"/>
                    </sql>
                  </xsl:otherwise>
                </xsl:choose>
                <ctls>
                  <ctl a="ApplyDefault" rt="true">
                    <sl fn="IsEmpty()"/>
                  </ctl>
                </ctls>
                <df v="1"/>
              </xsl:when>
              <xsl:otherwise>
                <!-- Les jambes du Spread ne sont que sur une seule ligne -->
                <xsl:attribute name="v">
                  <xsl:value-of select="$CurrentLegNumber"/>
                </xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
          </c>
          <!-- Target Leg ou Non -->
          <c n="ISTARGET" dku="true" t="b">
            <xsl:attribute name="v">
              <xsl:choose>
                <xsl:when test="$CurrentLegNumber='0'">1</xsl:when>
                <xsl:otherwise>0</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
          </c>
          <!-- Exchange Code / Acronym -->
          <c n="EXCHANGEACRONYM" dku="true" v="{$ExchangeAcronym}"/>
          <!-- Combined Commodity Code -->
          <c n="COMBCOMCODE" dku="true" v="{@v}"/>
          <!-- IDIMSPANGRPCTR_H -->
          <xsl:if test="($pFileFormat='UP') or ($pFileFormat='U2') or ($pFileFormat='U') or ($pFileFormat='P')">
            <c n="IDIMSPANGRPCTR_H" dku="true" t="i">
              <xsl:call-template name="SQL_IDIMSPANGRPCTR_H">
                <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                <xsl:with-param name="pExchangeAcronym" select="$ExchangeAcronym"/>
                <xsl:with-param name="pCombinedCommodityCode" select="@v"/>
              </xsl:call-template>
            </c>
          </xsl:if>
          <!-- Delta / Spread Ratio -->
          <c n="DELTAPERSPREAD" dku="true" t="dc">
            <xsl:attribute name="v">
              <xsl:variable name="DR">
                <xsl:choose>
                  <!-- Quand Leg "Normale" -->
                  <xsl:when test="$CurrentLegNumber != '0'">
                    <xsl:value-of select="../d[@n=concat('DR',$CurrentLegNumber)]/@v"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="../d[@n='TDR']/@v"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:choose>
                <xsl:when test="$pFileFormat='U2'">
                  <xsl:value-of select="$DR div 10000"/>
                </xsl:when>
                <xsl:when test="$pFileFormat='UP'">
                  <xsl:call-template name="SetDecLoc">
                    <xsl:with-param name="pValue" select="$DR"/>
                    <xsl:with-param name="pDecLoc" select="../d[@n=concat('DR',$CurrentLegNumber,'D')]/@v"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$DR"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
          </c>
          <!-- Spread Side Leg (Quand Leg "Normale") -->
          <xsl:if test="$CurrentLegNumber!='0'">
            <c n="LEGSIDE" dku="true" v="{../d[@n=concat('SS',$CurrentLegNumber)]/@v}"/>
          </xsl:if>
          <!-- Tier Number -->
          <xsl:if test="normalize-space(../d[@n=concat('TN',$CurrentLegNumber)]/@v)!=''">
            <c n="TIERNUMBER" dku="true" t="i" v="{../d[@n=concat('TN',$CurrentLegNumber)]/@v}"/>
          </xsl:if>
          <!-- Jambe Requise ou non ? -->
          <xsl:choose>
            <xsl:when test="($pFileFormat!='UP') and (../d[@n='IRSM']/@v = '04')">
              <c n="ISREQUIRED" dku="true" t="b">
                <xsl:attribute name="v">
                  <xsl:choose>
                    <!-- Quand Leg "Normale" -->
                    <xsl:when test="$CurrentLegNumber != '0'">
                      <xsl:choose>
                        <xsl:when test="../d[@n=concat('R',$CurrentLegNumber)]/@v = 'N'">0</xsl:when>
                        <xsl:otherwise>1</xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                    <!-- Quand Target Leg -->
                    <xsl:when test="$CurrentLegNumber = '0'">
                      <xsl:choose>
                        <xsl:when test="../d[@n='TR']/@v = 'N'">0</xsl:when>
                        <xsl:otherwise>1</xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                  </xsl:choose>
                </xsl:attribute>
              </c>
            </xsl:when>
            <xsl:otherwise>
              <c n="ISREQUIRED" dku="true" t="b" v="1"/>
            </xsl:otherwise>
          </xsl:choose>
          <!-- Spread Credit Rate / Gain Allowance Percentage -->
          <xsl:if test="($pFileFormat='U') or ($pFileFormat='P') or ($pFileFormat='U2')">
            <c n="CREDITRATE" dku="true" t="dc">
              <xsl:attribute name="v">
                <xsl:choose>
                  <!-- Quand Fichier Expanded -->
                  <xsl:when test="$pFileFormat='U2'">
                    <xsl:value-of select="../d[@n=concat('CR',$CurrentLegNumber)]/@v"/>
                  </xsl:when>
                  <!-- Quand Fichier Standard -->
                  <xsl:when test="($pFileFormat='U') or ($pFileFormat='P')">
                    <xsl:value-of select="../d[@n='GA']/@v"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:attribute>
            </c>
          </xsl:if>
          <!-- IDIMSPANTIER_H -->
          <xsl:if test="(($pFileFormat='U2') or ($pFileFormat='U') or ($pFileFormat='P')) and (../d[@n='IRSM']/@v != '01') and (../d[@n='IRSM']/@v != '04')">
            <c n="IDIMSPANTIER_H" dku="true" t="i">
              <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
              <sql cd="select" rt="IDIMSPANTIER_H">
                select t.IDIMSPANTIER_H
                from dbo.IMSPANTIER_H t
                inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H ) and ( g.COMBCOMCODE = @COMBCOMCODE )
                inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H ) and ( e.EXCHANGEACRONYM = @EXCHANGEACRONYM )<xsl:value-of select="$vSpace"/>
                <!-- Vérification de IDIMSPAN_H -->
                <xsl:call-template name="innerIDIMSPAN_H">
                  <xsl:with-param name="pTable" select="'e'"/>
                </xsl:call-template>
                and (t.SPREADTYPE = 'R')
                and (t.TIERNUMBER = @TIERNUMBER)
                <xsl:call-template name="paramInnerIDIMSPAN_H">
                  <xsl:with-param name="pBusinessDateTime" select="$pBusinessDateTime"/>
                  <xsl:with-param name="pSettlementorIntraday" select="$pSettlementorIntraday"/>
                  <xsl:with-param name="pExchangeComplex" select="$pExchangeComplex"/>
                </xsl:call-template>
                <p n="COMBCOMCODE" v="{@v}"/>
                <p n="EXCHANGEACRONYM" v="{$ExchangeAcronym}"/>
                <p n="TIERNUMBER" v="{../d[@n=concat('TN',$CurrentLegNumber)]/@v}"/>
              </sql>
            </c>
          </xsl:if>
          <!-- InfoTable_IU -->
          <xsl:call-template name="InfoTable_IU"/>
        </tbl>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- ============== QUOTE_XXX_H ======================================= -->
  <!-- ============== (Type "81" à "84" S, E, P) ======================== -->
  <!-- ============== (Type "60" London) ================================ -->
  <!-- RD 20130417 [18587]Correction du format de la colonne TIME -->
  <!-- PM 20130917 [18889] Ajout parameter pQuoteSide avec une defaut à OfficialClose (pour la compatibilité)-->
  <!-- PM 20140116 [19456] Ajout paramètres pTime avec une defaut à 'null' (pour la compatibilité) : si pTime = 'null' on continu à utiliser pBusinessDateTime -->
  <!--PM 20140120 [19504] Ajout paramètre pIsImportPrice-->
  <xsl:template name="QUOTE_XXX_H">
    <xsl:param name="pQuoteTable"/>
    <xsl:param name="pFileFormat"/>
    <xsl:param name="pBusinessDateTime"/>
    <xsl:param name="pSettlementorIntraday"/>
    <xsl:param name="pExchangeComplex"/>
    <xsl:param name="pExchangeAcronym"/>
    <xsl:param name="pCommodityCode"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pIdAsset"/>
    <xsl:param name="pSettlementPrice"/>
    <xsl:param name="pDelta"/>
    <xsl:param name="pVolatility"/>
    <xsl:param name="pTimeToExpiration"/>
    <xsl:param name="pQuoteSide" select="'OfficialClose'"/>
    <xsl:param name="pTime" select="'null'"/>
    <xsl:param name="pIsImportPrice" select="1"/>

    <!-- PM 20140116 [19456] Ajout variable vTime : si pTime = 'null' on continu à utiliser pBusinessDateTime -->
    <xsl:variable name="vTime">
      <xsl:choose>
        <xsl:when test="$pTime = 'null'">
          <xsl:value-of select="$pBusinessDateTime"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pTime"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="boolean($pQuoteTable)">
      <tbl n="{$pQuoteTable}" a="IU">
        <!-- mq n="QuotationHandlingMQueue" a="IU"/ -->
        <!-- IDMARKETENV -->
        <c n="IDMARKETENV" dk="true">
          <sql cd="select" rt="IDMARKETENV" cache="true">
            select IDMARKETENV
            from dbo.MARKETENV
            where ( ISDEFAULT = 1 )
          </sql>
        </c>
        <!-- IDVALSCENARIO -->
        <c n="IDVALSCENARIO" dk="true">
          <sql cd="select" rt="IDVALSCENARIO" cache="true">
            select v.IDVALSCENARIO, 1 as colorder
            from dbo.VALSCENARIO v
            inner join dbo.MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
            where v.ISDEFAULT = 1
            union all
            select v.IDVALSCENARIO, 2 as colorder
            from dbo.VALSCENARIO v
            where v.ISDEFAULT = 1 and v.IDMARKETENV is null
            order by colorder asc
          </sql>
        </c>
        <c n="IDASSET" dk="true" t="i" v="{$pIdAsset}">
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <xsl:if test="$pQuoteTable='QUOTE_ETD_H'">
          <c n="IDC" dku="true">
            <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
            <!-- PM 20140116 [] Ajout vérification sur DTDISABLED du DC -->
            <sql cd="select" rt="IDC_PRICE" cache="true">
              select distinct mk.IDM, dc.IDC_PRICE
              from dbo.MARKET mk
              inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDM = mk.IDM )
              where ( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM )
              and ( dc.CONTRACTSYMBOL  = @COMMODITYCODE )
              and ( dc.CATEGORY        = @CATEGORY )
              and ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BUSINESSDATETIME ) )
              <!--			   and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
              <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
              <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
              <p n="COMMODITYCODE" v="{$pCommodityCode}"/>
              <p n="CATEGORY" v="{$pCategory}"/>
            </sql>
          </c>
          <c n="IDM" dku="true" t="i">
            <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
            <!-- PM 20140116 [] Ajout vérification sur DTDISABLED du DC -->
            <sql cd="select" rt="IDM" cache="true">
              select distinct mk.IDM, dc.IDC_PRICE
              from dbo.MARKET mk
              inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDM = mk.IDM )
              where ( mk.EXCHANGEACRONYM = @EXCHANGEACRONYM )
              and ( dc.CONTRACTSYMBOL  = @COMMODITYCODE )
              and ( dc.CATEGORY        = @CATEGORY )
              and ( ( dc.DTDISABLED is null ) or ( dc.DTDISABLED > @BUSINESSDATETIME ) )
              <!--			   and ( ( dc.DTENABLED is null ) or ( dc.DTENABLED &lt;= @BusinessDateTime ) )-->
              <p n="BUSINESSDATETIME" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$pBusinessDateTime}"/>
              <p n="EXCHANGEACRONYM" v="{$pExchangeAcronym}"/>
              <p n="COMMODITYCODE" v="{$pCommodityCode}"/>
              <p n="CATEGORY" v="{$pCategory}"/>
            </sql>
          </c>
        </xsl:if>
        <c n="TIME" dk="true" t="dt" f="yyyy-MM-dd HH:mm:ss" v="{$vTime}">
          <ctrls>
            <ctrl a="RejectRow" rt="false">
              <sl fn="IsDate()"/>
              <li s="ERROR" ex="true" msg="Invalid Type"/>
            </ctrl>
          </ctrls>
        </c>
        <c n="VALUE" dku="true" t="dc" v="{$pSettlementPrice}">
          <!-- PM 20131218 [19360] Ne pas importer un cours null -->
          <ctls>
            <ctl a="RejectRow" rt="true">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
        <!--PM 20140120 [19504] Gestion du paramètre pIsImportPrice pour ne pas importer certains cours-->
        <xsl:if test="$pFileFormat='UP'">
          <c n ="ISIMPORTPRICE" t="i" v="{$pIsImportPrice}">
            <ctls>
              <ctl a="RejectRow" rt="0"/>
              <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
            </ctls>
          </c>
        </xsl:if>
        <c n="SPREADVALUE" dku="true" t="dc" v="0"/>
        <c n="QUOTEUNIT" dku="true" v="Price"/>
        <c n="QUOTESIDE" dk="true" v="{$pQuoteSide}"/>
        <c n="QUOTETIMING" dku="true">
          <xsl:attribute name="v">
            <xsl:choose>
              <xsl:when test="$pSettlementorIntraday = 'ITD'">Intraday</xsl:when>
              <xsl:otherwise>Close</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </c>
        <c n="ASSETMEASURE" dku="true" v="MarketQuote"/>
        <!--c n="CASHFLOWTYPE" dk="true" v="null"/-->
        <c n="ISENABLED" dku="true" t="b" v="1"/>
        <c n="SOURCE" dku="true" v="ClearingOrganization"/>
        <xsl:if test="$pQuoteTable='QUOTE_ETD_H'">
          <xsl:if test="normalize-space($pTimeToExpiration)!=''">
            <c n="TIMETOEXPIRATION" dku="true" t="dc" v="{$pTimeToExpiration}"/>
          </xsl:if>
          <xsl:if test="normalize-space($pDelta)!=''">
            <c n="DELTA" dku="true" t="dc" v="{$pDelta}"/>
          </xsl:if>
          <xsl:if test="normalize-space($pVolatility)!=''">
            <c n="VOLATILITY" dku="true" t="dc" v="{$pVolatility}"/>
          </xsl:if>
        </xsl:if>
        <!-- InfoTable_IU -->
        <xsl:call-template name="InfoTable_IU"/>
      </tbl>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- =  Mise à jour de la table DERIVATIVEATTRIB ====================== -->
  <!-- PM 20131202 [19275] Mise à jour du CONTRACTMULTIPLIER et du FACTOR de l'échéance ouverte si différent de celui du contrat -->
  <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
  <xsl:template name="Upd_DERIVATIVEATTRIB">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pIdAsset"/>
    <xsl:param name="pContractMultiplier"/>
    <xsl:param name="pFactor"/>

    <!-- Mise à jour de la table DERIVATIVEATTRIB -->
    <tbl n="DERIVATIVEATTRIB" a="U">
      <c n="IDDERIVATIVEATTRIB" dk="true" t="i">
        <xsl:call-template name="SQL_DERIVATIVEATTRIB_From_Asset">
          <xsl:with-param name="pResultColumn" select="'IDDERIVATIVEATTRIB'"/>
          <xsl:with-param name="pIdAsset" select="$pIdAsset"/>
          <xsl:with-param name="pContractMultiplier" select="$pContractMultiplier"/>
          <xsl:with-param name="pFactor" select="$pFactor"/>
        </xsl:call-template>
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="IsNull()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <!-- PM 20131211 [19259] Ajout vérification de ISAUTOSETTING du DC afin de ne pas faire de MAJ si la colonne vaut False -->
      <c n="ISAUTOSETTING" t="b">
        <xsl:call-template name="SQL_DERIVATIVEATTRIB_From_Asset">
          <xsl:with-param name="pResultColumn" select="'ISAUTOSETTING'"/>
          <xsl:with-param name="pIdAsset" select="$pIdAsset"/>
          <xsl:with-param name="pContractMultiplier" select="$pContractMultiplier"/>
          <xsl:with-param name="pFactor" select="$pFactor"/>
        </xsl:call-template>
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="IsNull()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <!-- MAJ si $pContractMultiplier != 0 -->
      <xsl:if test="(normalize-space($pContractMultiplier)!='') and (number($pContractMultiplier) = $pContractMultiplier) and (number($pContractMultiplier) != 0)">
        <c n="CONTRACTMULTIPLIER" dku="true" t="dc">
          <xsl:call-template name="SQL_DERIVATIVEATTRIB_From_Asset">
            <xsl:with-param name="pResultColumn" select="'CONTRACTMULTIPLIER'"/>
            <xsl:with-param name="pIdAsset" select="$pIdAsset"/>
            <xsl:with-param name="pContractMultiplier" select="$pContractMultiplier"/>
            <xsl:with-param name="pFactor" select="$pFactor"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectColumn" rt="true" lt="None">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
      </xsl:if>
      <xsl:if test="(normalize-space($pFactor)!='') and (number($pFactor) = $pFactor) and (number($pFactor) != 0)">
        <c n="FACTOR" dku="true" t="i">
          <xsl:call-template name="SQL_DERIVATIVEATTRIB_From_Asset">
            <xsl:with-param name="pResultColumn" select="'FACTOR'"/>
            <xsl:with-param name="pIdAsset" select="$pIdAsset"/>
            <xsl:with-param name="pContractMultiplier" select="$pContractMultiplier"/>
            <xsl:with-param name="pFactor" select="$pFactor"/>
          </xsl:call-template>
          <ctls>
            <ctl a="RejectColumn" rt="true" lt="None">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
      </xsl:if>
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
  <!-- ================================================================== -->
  <!-- =  Mise à jour de la table ASSET_ETD ============================= -->
  <!-- PM 20131202 [19275] Mise à jour du CONTRACTMULTIPLIER et du FACTOR de l'Asset si différent de celui du contrat -->
  <!-- PM 20151027 [20964] Ajout paramètre pBusinessDate -->
  <xsl:template name="Upd_ASSET_ETD">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pIdAsset"/>
    <xsl:param name="pContractMultiplier"/>
    <xsl:param name="pFactor"/>

    <!-- Mise à jour de la table ASSET_ETD -->
    <tbl n="ASSET_ETD" a="U">
      <c n="IDASSET" dk="true" t="i" v="{$pIdAsset}">
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="IsNull()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <!-- PM 20131211 [19259] Ajout vérification de ISAUTOSETTING du DC afin de ne pas faire de MAJ si la colonne vaut False -->
      <c n="ISAUTOSETTING" t="b">
        <!-- PM 20140107 [19452] Ajout type du paramètre -->
        <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
        <sql cd="select" cache="true" rt="ISAUTOSETTING">
          select case when dc.ISAUTOSETTING = 1 then dc.ISAUTOSETTING else null end as ISAUTOSETTING
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
          inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
          where (a.IDASSET = @IDASSET)
          <p n="IDASSET" t="i" v="{$pIdAsset}"/>
        </sql>
        <ctls>
          <ctl a="RejectRow" rt="true" lt="None">
            <sl fn="IsNull()"/>
          </ctl>
          <ctl a="RejectColumn" rt="true" lt="None" v="true"/>
        </ctls>
      </c>
      <!-- MAJ si $pContractMultiplier != 0 -->
      <xsl:if test="(normalize-space($pContractMultiplier)!='') and (number($pContractMultiplier) = $pContractMultiplier) and (number($pContractMultiplier) != 0)">
        <!-- PM 20151027 [20964] Ajout envoie message à QuotationHandling pour le CONTRACTMULTIPLIER -->
        <mq n="QuotationHandlingMQueue" a="U">
          <p n="TIME" t="d" f="yyyy-MM-dd" v="{$pBusinessDate}"/>
          <p n="IDASSET" t="i" v="{$pIdAsset}"/>
          <p n="IsCashFlowsVal" t="b" v="true"/>
        </mq>

        <c n="CONTRACTMULTIPLIER" dku="true" t="dc">
          <!-- PM 20131202 [19321] Modification du SELECT pour renvoyer NULL si pas de différence avec l'existant -->
          <!-- PM 20140107 [19452] Ajout type des paramètres -->
          <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
          <sql cd="select" cache="true" rt="CONTRACTMULTIPLIER">
            select case when dc.CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER then null else @CONTRACTMULTIPLIER end as CONTRACTMULTIPLIER
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            where (a.IDASSET = @IDASSET)
            and ((dc.CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER)
            or (a.CONTRACTMULTIPLIER is null) or (a.CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
            <p n="IDASSET" t="i" v="{$pIdAsset}"/>
            <p n="CONTRACTMULTIPLIER" t="dc" v="{$pContractMultiplier}"/>
          </sql>
          <ctls>
            <ctl a="RejectColumn" rt="true" lt="None">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
      </xsl:if>
      <!-- MAJ si $pFactor != 0 -->
      <xsl:if test="(normalize-space($pFactor)!='') and (number($pFactor) = $pFactor) and (number($pFactor) != 0)">
        <c n="FACTOR" dku="true" t="i">
          <!-- PM 20131202 [19321] Modification du SELECT pour renvoyer NULL si pas de différence avec l'existant -->
          <!-- PM 20140107 [19452] Ajout type des paramètres -->
          <!-- FI 20140108 [19460] Les parameters SQL doivent être en majuscule -->
          <sql cd="select" cache="true" rt="FACTOR">
            select case when (dc.FACTOR = case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end) then null else (case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end) end as FACTOR
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
            inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
            where (a.IDASSET = @IDASSET)
            and ((dc.FACTOR != case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end)
            or (a.FACTOR is null) or (a.FACTOR != case when dc.ASSETCATEGORY = 'Future' then 1 else @FACTOR end))
            <p n="IDASSET" t="i" v="{$pIdAsset}"/>
            <p n="FACTOR" t="dc" v="{$pFactor}"/>
          </sql>
          <ctls>
            <ctl a="RejectColumn" rt="true" lt="None">
              <sl fn="IsNull()"/>
            </ctl>
          </ctls>
        </c>
      </xsl:if>
      <xsl:call-template name="InfoTable_IU"/>
    </tbl>
  </xsl:template>
</xsl:stylesheet>