<?xml version="1.0" encoding="utf-16"?>
<!-- Repertoire :  I:\INST_SPHERES\Analyses\Spheres OTCml - F&Oml\Repository\MarketData\LSD (Listed Derivatives)\Price_RiskArray\XSL
-
- Nom Du Script : ClosingPriceImport-SPAN_US-map.xsl
-
- Language : XSL
-
- Auteur   : FL
-
- Version  : 1.0
-
- Creation : 08/02/2011
-
- Modifcation : 15/03/2011 par FL
-
- Designation : Script permet de gÃ©nÃ©rer le fichier PostMapping pour sphÃ¨res IO pour intÃ©grer les cotations ETD.
-               Ce script est commun aux intÃ©grations faites pour intÃ©grer les fichiers SPAN US au format (U2) Expanded Unpacked(U2),
-               Paris Expanded(UP) et Standard Format(U).
-               Les taches dâ€™importation respectives attachÃ©es Ã  ce XSL sont LSD_MarketData_Price_Span Expanded, 
-               LSD_MarketData_Price_Span_Paris Expanded et LSD_MarketData_Price_Span_Standard_Format.
-               La liste des tables de cotation mises Ã  jour via ce script sont : QUOTE_ETD_H, QUOTE_EQUITY_H et QUOTE_INDEX_H -->

<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

  
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:include href="Quote_Common_SQL.xsl"/>

  <xsl:param name="pPassNumber" select="1"/>
  <xsl:param name="pPassRowsToProcess" select="2"/>

  <xsl:decimal-format name="decimalFormat" decimal-separator="." />

  <xsl:template match="/iotask">
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

  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter" >
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

  <xsl:template name="StrikePriceDiv">
    <xsl:param name="pHowManyZero"/>
    <xsl:choose>
      <xsl:when test="number($pHowManyZero) > 0">
        <xsl:variable name="vRecursiveResult">
          <xsl:call-template name="StrikePriceDiv">
            <xsl:with-param name="pHowManyZero" select="number($pHowManyZero) - 1"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:value-of select="10 * number($vRecursiveResult)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="1"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

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


      <!-- RÃ©cupÃ©ration des donnÃ©es ne figurant qu'en entÃªte de fichier 
           ( RecordId( RID = 0 (Fichier Span Expanded ....) ou RID = 1(Fichier Span Standard) ) )  -->
      <!-- Business Date -->
      <xsl:variable name ="BusinessDate" >
        <xsl:value-of select="row[(data[@name='RID']='0') or (data[@name='RID']='1')]/data[@name='BusDt']"/>
      </xsl:variable>


      <!-- Set File Format : 
            (U2) for Expanded Unpacked     ( RID => 0 )
            (UP) for Paris Expanded Format ( RID => 0 )
            (U)  for Standard Format       ( RID => 1 ) -->
      <xsl:variable name ="FileFormat" >
        <xsl:value-of select="row[(data[@name='RID']='0') or (data[@name='RID']='1')]/data[@name='FiFmt']"/>
      </xsl:variable>

      <!-- Set Intra Flag : Settlement (S) or Intraday (I) Flag  -->
      <xsl:variable name ="Quotetiming" >
        <xsl:choose>
          <xsl:when test="$FileFormat != 'U'">
            <xsl:choose>
              <xsl:when test="row[(data[@name='RID']='0')]/data[@name='SetIntra'] = 'S'">Close</xsl:when>
              <xsl:when test="row[(data[@name='RID']='0')]/data[@name='SetIntra'] = 'I'">Intraday</xsl:when>
              <xsl:otherwise>Close</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>Close</xsl:otherwise>
        </xsl:choose>

      </xsl:variable>
      <!-- Calcul du premier Row Ã  prendre en compte ( tient compte du decoupage quand le fichier est de gros volume ) -->
      <xsl:variable name="vStartRowNumber">
        <xsl:copy-of select="$pPassRowsToProcess * ($pPassNumber - 1)"/>
      </xsl:variable>

      <!-- Calcul du dernier Row Ã  prendre en compte ( tient compte du decoupage quand le fichier est de gros volume ) -->
      <xsl:variable name="vEndRowNumber">
        <xsl:copy-of select="$pPassRowsToProcess * $pPassNumber"/>
      </xsl:variable>

      <!-- Calcul du RID permettant de rÃ©cuperer les cotations en Fonction de Format de fichier SPAN cf. File Format -->
      <xsl:variable name ="RID8283" >
        <xsl:choose>
          <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'U' ">82</xsl:when>
          <xsl:when test="$FileFormat = 'UP'">83</xsl:when>
          <xsl:otherwise>82</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Traitement de chaque row ayant pour RecordId(RID) :
              82 pour les fichiers ayant pour format 'Expanded Unpacked(U2)' ou 'Standard (U)'
              83 pour les fichiers ayant pour format 'Paris Expanded Format(UP)' -->
      <xsl:for-each select="row[(position() > $vStartRowNumber) and ($vEndRowNumber >= position()) and (data[@name='RID']=$RID8283)]">
        <row>

          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>

          <xsl:attribute name="src">
            <xsl:value-of select="@src"/>
          </xsl:attribute>

          <!-- Exchange Acronym -->
          <xsl:variable name ="Market" >
            <xsl:choose>
              <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'UP' ">
                <xsl:value-of select="data[@name='ExchAc']"/>
              </xsl:when>

              <xsl:when test="$FileFormat = 'U'" >
                <xsl:value-of select="data[@name='ExchCd']"/>
              </xsl:when>

              <xsl:otherwise></xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- Commodity (Product) Code -->
          <xsl:variable name ="ContractSymbol" >
            <xsl:value-of select="data[@name='PrdCd']"/>
          </xsl:variable>

          <!-- Product Type Code :
              Pour format 'Expanded Unpacked(U2)' ou 'Paris Expanded Format(UP)'
                  STOCK for Equitie
                  PHY for Physical (Indice )
                  FUT for Future
                  CMB for Combination
                  OOP for Option on Physical (Indice)
                  OOF for Option on Future
                  OOS for Option on Stock ( Equitie )
                  OOC for Option on Combination 
              Pour format 'Standard (U)
                  FUT for Future
                  OPT for Option -->
          <xsl:variable name ="ProductTypeCode" >
            <xsl:choose>
              <xsl:when test="$FileFormat = 'U'" >
                <xsl:choose>
                  <xsl:when test="data[@name='PrdTyp'] = 'P'">OPT</xsl:when>
                  <xsl:when test="data[@name='PrdTyp'] = 'C'">OPT</xsl:when>
                  <xsl:otherwise>FUT</xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="data[@name='PrdTyp']"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- Option Right Code - for an option only :
                   P for Put 
                   C for Call -->
          <xsl:variable name ="PutCall" >

            <xsl:choose>

              <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'UP'">
                <xsl:choose>
                  <xsl:when test="data[@name='OptTyp'] = 'P'">0</xsl:when>
                  <xsl:when test="data[@name='OptTyp'] = 'C'">1</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:when>

              <xsl:when test="$FileFormat = 'U'" >
                <xsl:choose>
                  <xsl:when test="data[@name='PrdTyp'] = 'P'">0</xsl:when>
                  <xsl:when test="data[@name='PrdTyp'] = 'C'">1</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:when>

              <xsl:otherwise></xsl:otherwise>

            </xsl:choose>

          </xsl:variable>

          <!-- Category :
                  F for Future 
                  O for Option  -->
          <xsl:variable name ="Category" >
            <xsl:choose>
              <xsl:when test="string-length($PutCall) > 0">O</xsl:when>
              <xsl:when test="string-length($PutCall) = 0">F</xsl:when>
              <xsl:otherwise></xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- Futures Contract Month - Option Contract Month -->
          <xsl:variable name ="MaturityMonth" >
            <xsl:choose>
              <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'UP'">
                <xsl:choose>
                  <xsl:when test="string-length($PutCall) > 0">
                    <xsl:value-of select="data[@name='OptMth']"/>
                  </xsl:when>
                  <xsl:when test="string-length($PutCall) = 0">
                    <xsl:value-of select="data[@name='FutMth']"/>
                  </xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:when>

              <xsl:when test="$FileFormat = 'U'">
                <xsl:choose>
                  <xsl:when test="string-length($PutCall) > 0">
                    20<xsl:value-of select="data[@name='OptMth']"/>
                  </xsl:when>
                  <xsl:when test="string-length($PutCall) = 0">
                    20<xsl:value-of select="data[@name='FutMth']"/>
                  </xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:when>

              <xsl:otherwise></xsl:otherwise>

            </xsl:choose>
          </xsl:variable>

          <!-- Option Strike Price -->
          <xsl:variable name="vStrike">
            <xsl:value-of select="data[@name='Strk']"/>
          </xsl:variable>

          <!-- Mise en forme Du Strike -->
          <xsl:variable name ="Strike" >
            <xsl:choose>
              <xsl:when test="$FileFormat = 'UP'">
                <!-- Mise en forme Du Strike dans le cas d'un fichier span au format Expanded Paris 
                     Closing Price Decimal Locator -->
                <xsl:variable name="vStrikeDivisor">
                  <xsl:call-template name="StrikePriceDiv">
                    <xsl:with-param name="pHowManyZero" select="number(data[@name='StrkNbDec'])"/>
                  </xsl:call-template>
                </xsl:variable>
                <!-- Mise en forme Du Strike -->
                <xsl:value-of select="format-number(number($vStrike) div $vStrikeDivisor, '0.00#######', 'decimalFormat')" />
              </xsl:when>
              <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'U' ">
                <!-- Dans le cas d'un fichier span au format Expanded ou Standard la mise en forme se fera plus tard 
                     car on a besoin de rÃ©cupÃ©rÃ© la valeur de la colonne PRICEDECLOCATOR dans la table DERIVATIVECONTRACT -->
                <xsl:value-of select="$vStrike" />
              </xsl:when>
            </xsl:choose>
          </xsl:variable>

          <!-- Closing Price -->
          <xsl:variable name="vClosingPrice">
            <xsl:value-of select="data[@name='Price']"/>
          </xsl:variable>

          <!-- Mise en forme Du Closing Price -->
          <xsl:variable name ="vFormatedClosingPrice" >
            <xsl:choose>
              <xsl:when test="$FileFormat = 'UP'">
                <!-- Mise en forme Du Closing Price dans le cas d'un fichier span au format Expanded Paris 
                     Closing Price Decimal Locator -->
                <xsl:variable name="vClosingPriceDivisor">
                  <xsl:call-template name="StrikePriceDiv">
                    <xsl:with-param name="pHowManyZero" select="number(data[@name='PriceNbDec'])"/>
                  </xsl:call-template>
                </xsl:variable>
                <!-- Mise en forme Du Closing Price -->
                <xsl:value-of select="format-number(number($vClosingPrice) div $vClosingPriceDivisor, '0.00#######', 'decimalFormat')" />
              </xsl:when>
              <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'U' ">
                <!--Dans le cas d'un fichier span au format Expanded ou Standard la mise en forme se fera plus tard 
                    car on a besoin De rÃ©cupÃ©rÃ© la valeur de la colonne PRICEDECLOCATOR dans la table DERIVATIVECONTRACT -->
                <xsl:value-of select="$vClosingPrice" />
              </xsl:when>
            </xsl:choose>
          </xsl:variable>

          <xsl:choose>

            <!-- Dans le cas ou ProductTypeCode Ã  ces diffÃ©rentes valeur : 
                  FUT for Future, OOP for Option on Physical (Indice), OOF for Option on Future
                  OOS for Option on Stock ( Equitie ),OOC for Option on Combination 
                  la table de cotation Ã  mettre Ã  jours est QUOTE_ETD_H -->
            <xsl:when test="$ProductTypeCode = 'FUT' or $ProductTypeCode = 'OOP'
                         or $ProductTypeCode = 'OOF' or $ProductTypeCode = 'OOS'
                         or $ProductTypeCode = 'OOC' or $ProductTypeCode = 'OPT'">

              <table name="QUOTE_ETD_H" action="IU" >
                <!-- mqueue name="QuotationHandlingMQueue" action="IU" / -->

                <column name="IDMARKETENV" datakey="true" datakeyupd="false" datatype="string">
                  <SQL command="select" result="IDMARKETENV">
                    select IDMARKETENV
                    from MARKETENV
                    where ISDEFAULT = 1
                  </SQL>
                </column>
                <column name="IDVALSCENARIO" datakey="true" datakeyupd="false" datatype="string">
                  <SQL command="select" result="IDVALSCENARIO">
                    select v.IDVALSCENARIO, 1 as colorder
                    from VALSCENARIO v
                    inner join MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
                    where v.ISDEFAULT = 1
                    union
                    select v.IDVALSCENARIO, 2 as colorder
                    from VALSCENARIO v
                    where v.ISDEFAULT = 1 and v.IDMARKETENV is null
                    order by colorder asc
                  </SQL>
                </column>
                <column name="IDASSET" datakey="true" datakeyupd="false" datatype="integer">
                  <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
                  <SQL command="select" result="IDASSET">
                    select distinct tblmain.IDASSET
                    from (select distinct a.IDASSET
                    from ASSET_ETD a
                    inner join DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB
                    inner join MATURITY ma on ma.IDMATURITY = da.IDMATURITY
                    inner join DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC
                    inner join MARKET mk on mk.IDM = dc.IDM
                    inner join TRADE tr on tr.IDASSET = a.IDASSET
                    where mk.EXCHANGEACRONYM = @EXCHANGEACRONYM
                    and dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
                    and dc.CATEGORY = @CATEGORY
                    and ma.MATURITYMONTHYEAR = @MATURITYMONTH
                    <xsl:if test="string-length($PutCall) > 0">
                      and a.PUTCALL = @PUTCALL
                      <xsl:choose>
                        <xsl:when test="$FileFormat = 'UP'">
                          <!-- Aucune Mise en forme du Strike car elle est deja faite auparavant -->
                          and a.STRIKEPRICE = @STRIKE
                        </xsl:when>
                        <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'U'">
                          <!-- Mise en forme du Strike dans le cas d'un fichier span au format Expanded ou 
                               Standard en fonction de la colonne STRIKEDECLOCATOR dans la table DERIVATIVECONTRACT -->
                          and a.STRIKEPRICE = case when dc.STRIKEDECLOCATOR is null then @STRIKE else (@STRIKE / power(10,dc.STRIKEDECLOCATOR)) end
                        </xsl:when>
                      </xsl:choose>
                    </xsl:if>
                    <!-- Pour un prix "non option":
                      - s’il existe au moins un trade négocié sur une option
                      - dont le SS-J est l’asset du prix en cours de traitement.-->
                    <xsl:if test="string-length($PutCall) = 0">
                      union
                      select distinct a.IDASSET
                      from ASSET_ETD a
                      inner join DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB
                      inner join MATURITY ma on ma.IDMATURITY = da.IDMATURITY
                      inner join DERIVATIVECONTRACT dc on dc.IDDC = da.IDDC
                      inner join MARKET mk on mk.IDM = dc.IDM
                      inner join DERIVATIVEATTRIB da_opt on da_opt.IDASSET = a.IDASSET
                      inner join DERIVATIVECONTRACT dc_opt on dc_opt.IDDC = da_opt.IDDC and dc_opt.CATEGORY = 'O' and dc_opt.ASSETCATEGORY = 'Future'
                      inner join ASSET_ETD a_opt on a_opt.IDDERIVATIVEATTRIB = da_opt.IDDERIVATIVEATTRIB
                      inner join TRADE tr on tr.IDASSET = a_opt.IDASSET
                      where mk.EXCHANGEACRONYM = @EXCHANGEACRONYM
                      and dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
                      and dc.CATEGORY = @CATEGORY
                      and ma.MATURITYMONTHYEAR = @MATURITYMONTH
                    </xsl:if>
                    <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                      <xsl:with-param name="pTable" select="'dc'"/>
                    </xsl:call-template>
                    ) tblmain
                    <Param name="DT" datatype="date">
                      <xsl:value-of select="$gParamDtBusiness"/>
                    </Param>
                    <Param name="EXCHANGEACRONYM" datatype="string">
                      <xsl:value-of select="$Market" />
                    </Param>
                    <Param name="CONTRACTSYMBOL" datatype="string">
                      <xsl:value-of select="$ContractSymbol" />
                    </Param>
                    <Param name="CATEGORY" datatype="string">
                      <xsl:value-of select="$Category" />
                    </Param>
                    <Param name="MATURITYMONTH" datatype="string">
                      <xsl:value-of select="$MaturityMonth" />
                    </Param>
                    <xsl:if test="string-length($PutCall) > 0">
                      <Param name="PUTCALL" datatype="string">
                        <xsl:value-of select="$PutCall" />
                      </Param>
                      <Param name="STRIKE" datatype="decimal">
                        <xsl:value-of select="$Strike" />
                      </Param>
                    </xsl:if>
                  </SQL>
                  <controls>
                    <control action="RejectRow" return="true" >
                      <SpheresLib function="IsNull()" />
                      <logInfo status="INFO" isexception="false">
                        <message>
                          &lt;b&gt;Asset not found.&lt;/b&gt;
                          Market:&lt;b&gt;<xsl:value-of select="$Market"/>&lt;/b&gt;
                          Contract:&lt;b&gt;<xsl:value-of select="$ContractSymbol"/>&lt;/b&gt;
                          Maturity:&lt;b&gt;<xsl:value-of select="$MaturityMonth"/>&lt;/b&gt;<xsl:if test="string-length($PutCall) > 0">
                            PutCall:&lt;b&gt;<xsl:value-of select="$PutCall"/>&lt;/b&gt;
                            Strike:&lt;b&gt;<xsl:value-of select="$Strike"/>&lt;/b&gt;
                          </xsl:if>
                        </message>
                      </logInfo>
                    </control>
                  </controls>
                </column>
                <column name="IDC" datakey="false" datakeyupd="true" datatype="string" >null</column>
                <column name="IDM" datakey="false" datakeyupd="true" datatype="integer">
                  <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
                  <SQL command="select" result="IDM">
                    select distinct mk.IDM
                    from MARKET mk
                    inner join DERIVATIVECONTRACT dc on mk.IDM = dc.IDM
                    where mk.EXCHANGEACRONYM = @EXCHANGEACRONYM
                    and dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
                    and dc.CATEGORY = @CATEGORY
                    <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                      <xsl:with-param name="pTable" select="'dc'"/>
                    </xsl:call-template>
                    <Param name="DT" datatype="date">
                      <xsl:value-of select="$gParamDtBusiness"/>
                    </Param>
                    <Param name="EXCHANGEACRONYM" datatype="string">
                      <xsl:value-of select="$Market" />
                    </Param>
                    <Param name="CONTRACTSYMBOL" datatype="string">
                      <xsl:value-of select="$ContractSymbol" />
                    </Param>
                    <Param name="CATEGORY" datatype="string">
                      <xsl:value-of select="$Category" />
                    </Param>
                  </SQL>
                </column>
                <column name="TIME" datakey="true" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd">
                  <xsl:value-of select="$BusinessDate"/>
                  <controls>
                    <control action="RejectRow" return="false" >
                      <SpheresLib function="IsDate()" />
                      <logInfo status="ERROR" isexception="true">
                        <message>Invalid Type</message>
                      </logInfo>
                    </control>
                  </controls>
                </column>
                <column name="VALUE" datakey="false" datakeyupd="true" datatype="decimal">
                  <xsl:choose>
                    <xsl:when test="$FileFormat = 'UP'">
                      <!-- Aucune Mise en forme du Closing Price car elle est deja faite auparavant -->
                      <xsl:value-of select="$vFormatedClosingPrice" />
                    </xsl:when>
                    <xsl:when test="$FileFormat = 'U2' or $FileFormat = 'U' ">
                      <!-- Mise en forme du Closing Price dans le cas d'un fichier span au format Expanded ou
                           Standard en fonction de la colonne PRICEDECLOCATOR dans la table DERIVATIVECONTRACT -->
                      <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
                      <SQL command="select" result="CLOSINGPRICE">
                        select
                        case when dc.PRICEDECLOCATOR is not NULL then
                        @PRICE / power(10,dc.PRICEDECLOCATOR)
                        else
                        @PRICE
                        end as CLOSINGPRICE
                        from DERIVATIVECONTRACT dc
                        inner join MARKET mk on mk.IDM = dc.IDM
                        where mk.EXCHANGEACRONYM = @EXCHANGEACRONYM
                        and dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
                        and dc.CATEGORY = @CATEGORY
                        <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                          <xsl:with-param name="pTable" select="'dc'"/>
                        </xsl:call-template>
                        <Param name="DT" datatype="date">
                          <xsl:value-of select="$gParamDtBusiness"/>
                        </Param>
                        <Param name="EXCHANGEACRONYM" datatype="string">
                          <xsl:value-of select="$Market" />
                        </Param>
                        <Param name="CONTRACTSYMBOL" datatype="string">
                          <xsl:value-of select="$ContractSymbol" />
                        </Param>
                        <Param name="CATEGORY" datatype="string">
                          <xsl:value-of select="$Category" />
                        </Param>
                        <Param name="PRICE" datatype="decimal">
                          <xsl:value-of select="$vFormatedClosingPrice" />
                        </Param>
                      </SQL>
                    </xsl:when>
                  </xsl:choose>

                </column>
                <column name="SPREADVALUE" datakey="false" datakeyupd="true" datatype="decimal">0</column>
                <column name="QUOTEUNIT" datakey="false" datakeyupd="true" datatype="string">Price</column>
                <!--FI 20110627 [17490] OfficialClose price -->
                <column name="QUOTESIDE" datakey="true" datakeyupd="true" datatype="string">OfficialClose</column>
                <column name="QUOTETIMING" datakey="false" datakeyupd="true" datatype="string">
                  <xsl:value-of select="$Quotetiming"/>
                </column>
                <column name="ASSETMEASURE" datakey="false" datakeyupd="false" datatype="string">MarketQuote</column>
                <column name="CASHFLOWTYPE" datakey="true" datakeyupd="false" datatype="string">null</column>
                <column name="ISENABLED" datakey="false" datakeyupd="true" datatype="bool">1</column>
                <!--FI 20110627 [17490] EuroFinanceSystems provider -->
                <!--FI 20110629 [17490] ClearingOrganization provider -->
                <column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">ClearingOrganization</column>
                <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
                  <SpheresLib function="GetUTCDateTimeSys()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsInsert()" />
                    </control>
                  </controls>
                </column>
                <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
                  <SpheresLib function="GetUserId()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsInsert()" />
                    </control>
                  </controls>
                </column>
                <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
                  <SpheresLib function="GetUTCDateTimeSys()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsUpdate()" />
                    </control>
                  </controls>
                </column>
                <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
                  <SpheresLib function="GetUserId()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsUpdate()" />
                    </control>
                  </controls>
                </column>
                <column name="EXTLLINK" datakey="false" datakeyupd="false" datatype="string">null</column>
              </table>
            </xsl:when>

            <!-- Dans le cas ou ProductTypeCode Ã  pour valeur STOCK for Equitie 
                 la table de cotation Ã  mettre Ã  jours est QUOTE_EQUITY_H -->
            <xsl:when test="$ProductTypeCode = 'STOCK' and $FileFormat = 'UP' ">
              <table name="QUOTE_EQUITY_H" action="IU" >

                <!-- Glop Fab 23/02/2011 :  A voir si on doit brancher ou pas le service QuotationHandling 
                     <mqueue name="QuotationHandlingMQueue" action="IU" /> -->

                <column name="IDMARKETENV" datakey="true" datakeyupd="false" datatype="string">
                  <SQL command="select" result="IDMARKETENV">
                    select IDMARKETENV
                    from MARKETENV
                    where ISDEFAULT = 1
                  </SQL>
                </column>
                <column name="IDVALSCENARIO" datakey="true" datakeyupd="false" datatype="string">
                  <SQL command="select" result="IDVALSCENARIO">
                    select v.IDVALSCENARIO, 1 as colorder
                    from VALSCENARIO v
                    inner join MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
                    where v.ISDEFAULT = 1
                    union
                    select v.IDVALSCENARIO, 2 as colorder
                    from VALSCENARIO v
                    where v.ISDEFAULT = 1 and v.IDMARKETENV is null
                    order by colorder asc
                  </SQL>
                </column>
                <column name="IDASSET" datakey="true" datakeyupd="false" datatype="integer">
                  <SQL command="select" result="IDASSET">
                    select distinct aeq.IDASSET
                    from ASSET_EQUITY aeq
                    where aeq.SYMBOL = @SYMBOL
                    and aeq.SYMBOLSUFFIX = @SYMBOLSUFFIX
                    <Param name="SYMBOL" datatype="string">
                      <xsl:value-of select="substring($ContractSymbol,1,string-length($ContractSymbol)- 4)" />
                    </Param>
                    <Param name="SYMBOLSUFFIX" datatype="string">
                      <xsl:value-of select="substring($ContractSymbol,string-length($ContractSymbol)- 2, 3 )" />
                    </Param>
                  </SQL>

                  <controls>
                    <control action="RejectRow" return="true" >
                      <SpheresLib function="IsNull()" />
                      <logInfo status="INFO" isexception="false">
                        <message>
                          &lt;b&gt;Asset not found.&lt;/b&gt;
                          Symbol:&lt;b&gt;<xsl:value-of select="substring($ContractSymbol,1,string-length($ContractSymbol)- 4)"/>&lt;/b&gt;
                          Suffix Symbol:&lt;b&gt;<xsl:value-of select="substring($ContractSymbol,string-length($ContractSymbol)- 2, 3 )"/>&lt;/b&gt;
                        </message>
                      </logInfo>
                    </control>
                  </controls>
                </column>
                <column name="TIME" datakey="true" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd">
                  <xsl:value-of select="$BusinessDate"/>
                  <controls>
                    <control action="RejectRow" return="false" >
                      <SpheresLib function="IsDate()" />
                      <logInfo status="ERROR" isexception="true">
                        <message>Invalid Type</message>
                      </logInfo>
                    </control>
                  </controls>
                </column>
                <column name="VALUE" datakey="false" datakeyupd="true" datatype="decimal">
                  <xsl:choose>
                    <xsl:when test="$FileFormat = 'UP'">
                      <!-- Aucune Mise en forme du Closing Price car elle est deja faite auparavant -->
                      <xsl:value-of select="$vFormatedClosingPrice" />
                    </xsl:when>
                  </xsl:choose>
                </column>
                <column name="SPREADVALUE" datakey="false" datakeyupd="true" datatype="decimal">0</column>
                <column name="QUOTEUNIT" datakey="false" datakeyupd="true" datatype="string">Price</column>
                <!--FI 20110627 [17490] null price -->
                <column name="QUOTESIDE" datakey="true" datakeyupd="true" datatype="string">null</column>
                <column name="QUOTETIMING" datakey="false" datakeyupd="true" datatype="string">
                  <xsl:value-of select="$Quotetiming"/>
                </column>
                <column name="ASSETMEASURE" datakey="false" datakeyupd="false" datatype="string">MarketQuote</column>
                <column name="CASHFLOWTYPE" datakey="true" datakeyupd="false" datatype="string">null</column>
                <column name="ISENABLED" datakey="false" datakeyupd="true" datatype="bool">1</column>
                <!--FI 20110627 [17490] EuroFinanceSystems provider -->
                <!--FI 20110629 [17490] ClearingOrganization provider -->
                <column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">ClearingOrganization</column>
                <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
                  <SpheresLib function="GetUTCDateTimeSys()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsInsert()" />
                    </control>
                  </controls>
                </column>
                <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
                  <SpheresLib function="GetUserId()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsInsert()" />
                    </control>
                  </controls>
                </column>
                <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
                  <SpheresLib function="GetUTCDateTimeSys()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsUpdate()" />
                    </control>
                  </controls>
                </column>
                <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
                  <SpheresLib function="GetUserId()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsUpdate()" />
                    </control>
                  </controls>
                </column>
                <column name="EXTLLINK" datakey="false" datakeyupd="false" datatype="string">null</column>
              </table>
            </xsl:when>

            <!-- Dans le cas ou ProductTypeCode Ã  pour valeur PHY for Physical (Indice )
                 la table de cotation Ã  mettre Ã  jours est QUOTE_INDEX_H -->
            <xsl:when test="$ProductTypeCode = 'PHY' and $FileFormat = 'UP' ">

              <table name="QUOTE_INDEX_H" action="IU" >
                <!-- Glop Fab 23/02/2011 :  A voir si on doit brancher ou pas le service QuotationHandling 
                     <mqueue name="QuotationHandlingMQueue" action="IU" /> -->

                <column name="IDMARKETENV" datakey="true" datakeyupd="false" datatype="string">
                  <SQL command="select" result="IDMARKETENV">
                    select IDMARKETENV
                    from MARKETENV
                    where ISDEFAULT = 1
                  </SQL>
                </column>
                <column name="IDVALSCENARIO" datakey="true" datakeyupd="false" datatype="string">
                  <SQL command="select" result="IDVALSCENARIO">
                    select v.IDVALSCENARIO, 1 as colorder
                    from VALSCENARIO v
                    inner join MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
                    where v.ISDEFAULT = 1
                    union
                    select v.IDVALSCENARIO, 2 as colorder
                    from VALSCENARIO v
                    where v.ISDEFAULT = 1 and v.IDMARKETENV is null
                    order by colorder asc
                  </SQL>
                </column>
                <column name="IDASSET" datakey="true" datakeyupd="false" datatype="integer">
                  <SQL command="select" result="IDASSET">
                    select distinct aid.IDASSET
                    from ASSET_INDEX aid
                    where aid.IDENTIFIER = @IDENTIFIER
                    <Param name="IDENTIFIER" datatype="string">
                      <xsl:choose>
                        <xsl:when test="string-length($ContractSymbol) > 3">
                          <xsl:value-of select="substring($ContractSymbol,1,string-length($ContractSymbol)- 4)" />
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="$ContractSymbol" />
                        </xsl:otherwise>
                      </xsl:choose>

                    </Param>
                  </SQL>

                  <controls>
                    <control action="RejectRow" return="true" >
                      <SpheresLib function="IsNull()" />
                      <logInfo status="INFO" isexception="false">
                        <message>
                          &lt;b&gt;Asset not found.&lt;/b&gt;
                          Identifier :&lt;b&gt;<xsl:value-of select="substring($ContractSymbol,1,string-length($ContractSymbol)- 4)"/>&lt;/b&gt;
                        </message>
                      </logInfo>
                    </control>
                  </controls>

                </column>
                <column name="TIME" datakey="true" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd">
                  <xsl:value-of select="$BusinessDate"/>
                  <controls>
                    <control action="RejectRow" return="false" >
                      <SpheresLib function="IsDate()" />
                      <logInfo status="ERROR" isexception="true">
                        <message>Invalid Type</message>
                      </logInfo>
                    </control>
                  </controls>
                </column>
                <column name="VALUE" datakey="false" datakeyupd="true" datatype="decimal">
                  <xsl:choose>
                    <xsl:when test="$FileFormat = 'UP'">
                      <!-- Aucune Mise en forme du Closing Price car elle est deja faite auparavant -->
                      <xsl:value-of select="$vFormatedClosingPrice" />
                    </xsl:when>
                  </xsl:choose>
                </column>
                <column name="SPREADVALUE" datakey="false" datakeyupd="true" datatype="decimal">0</column>
                <column name="QUOTEUNIT" datakey="false" datakeyupd="true" datatype="string">Price</column>
                <column name="QUOTESIDE" datakey="true" datakeyupd="true" datatype="string">null</column>
                <column name="QUOTETIMING" datakey="false" datakeyupd="true" datatype="string">
                  <xsl:value-of select="$Quotetiming"/>
                </column>
                <column name="ASSETMEASURE" datakey="false" datakeyupd="false" datatype="string">MarketQuote</column>
                <column name="CASHFLOWTYPE" datakey="true" datakeyupd="false" datatype="string">null</column>
                <column name="ISENABLED" datakey="false" datakeyupd="true" datatype="bool">1</column>
                <!--FI 20110629 [17490] ClearingOrganization provider -->
                <column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">ClearingOrganization</column>
                <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
                  <SpheresLib function="GetUTCDateTimeSys()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsInsert()" />
                    </control>
                  </controls>
                </column>
                <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
                  <SpheresLib function="GetUserId()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsInsert()" />
                    </control>
                  </controls>
                </column>
                <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
                  <SpheresLib function="GetUTCDateTimeSys()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsUpdate()" />
                    </control>
                  </controls>
                </column>
                <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
                  <SpheresLib function="GetUserId()" />
                  <controls>
                    <control action="RejectColumn" return="true" >
                      <SpheresLib function="IsUpdate()" />
                    </control>
                  </controls>
                </column>
                <column name="EXTLLINK" datakey="false" datakeyupd="false" datatype="string">null</column>
              </table>
            </xsl:when>

          </xsl:choose>

        </row>
      </xsl:for-each>

    </file>
  </xsl:template>

</xsl:stylesheet>
