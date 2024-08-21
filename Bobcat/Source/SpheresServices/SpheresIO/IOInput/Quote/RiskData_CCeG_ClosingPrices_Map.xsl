<!--
===================================================================================================
Summary : CCeG - RISKDATA
File    : RiskData_CCeG_ClosingPrices_Map.xsl
===================================================================================================
Version : v3.5.0.0                                           
Date    : 20130705
Author  : BD/FL
Comment : Dorénavant, l'élément suivant CCeG - RISKDATA - UNDERLYINGPRICES qui initialement n'importait 
          quotidiennement que les cours des sous-jacents des equities importe désormais les cours 
          des sous-jacents Index (FTSE MIB Index).  

          Pour rappel ces cours sont intégrés avec le type cotations : OfficialClose
          
          Pour plus d'info ce référé au ticket  N° 18800
===================================================================================================
Version : v3.2.0.0                                           
Date    : 20130430
Author  : CC
Comment : File ClosingPriceImport-IDEM.xsl renamed to RiskData_CCeG_ClosingPrices_Map.xsl
===================================================================================================
Version : v3.2.0.0                                           
Date    : 20130214
Author  : PL
Comment : Management of Market-Id='5' and Market-Id='8'
          Check $vYM != $gNull
===================================================================================================
Date    : 20110711                                          
Author  : MF                                                 
Description: 
    Import IDEM quotes for F&O assets and index/equity underlyings.
    Sources: CLASSFILE txt file; RISKARRAY txt file
    Destinations: QUOTE_ETD_H table object; QUOTE_INDEX_H table object; QUOTE_EQUITY_H table object
===================================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output
    method="xml"
    omit-xml-declaration="no"
    encoding="UTF-8"
    indent="yes"
    media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="Quote_Common_SQL.xsl"/>

  <xsl:variable name="vVersionRiskDataImport">v0.0.0.1</xsl:variable>
  <xsl:variable name="vFileNameRiskDataImport">ClosingPriceImport-IDEM.xsl</xsl:variable>

  <xsl:variable name="vBusinessDate">
    <xsl:value-of select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
  </xsl:variable>

  <!-- V1: IDEM import only, unique exchange symbol [02]  -->
  <!-- TODO V2: IDEX import, energy contracts, exchange symbol [05]  -->
  <!--<xsl:variable name="vExchangeSymbol">
    <xsl:value-of select="'2'"/>
  </xsl:variable>-->

  <!-- match the file node -->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>
      <!-- match any row node inside the actual file node -->
      <xsl:apply-templates select="row">
      </xsl:apply-templates>
    </file>
  </xsl:template>

  <!-- match any row node -->
  <xsl:template match="row">
    <xsl:choose>
      <!-- CLASSFILE - insert underlying quotes  -->
      <xsl:when test="
                      data[contains(@status, 'success')] and 
                      data[contains(@name, 'Product')] and 
                      data[contains(@name, 'Class')]">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <xsl:variable name="vCurrency">
            <xsl:choose>
              <xsl:when test="data[@name='Currency'] = 'EU'">
                <xsl:value-of select="'EUR'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gNull"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:choose>
            <!-- index case -->
            <xsl:when test="data[@name='ProductType'] = 'I'">
              <!-- BD/FL 20130705 [18553-18800] : Rajout du parameters IdAsset -->
              <parameters>
                <parameter name="IdAsset">
                  <SQL command="select" result="IDASSET" cache="true">
                    select ass_ind.IDASSET
                    from dbo.ASSET_INDEX ass_ind
                    inner join dbo.MARKET m on ( m.IDM = ass_ind.IDM_RELATED ) and ( m.EXCHANGESYMBOL = @EXCHANGESYMBOL )
                    where ass_ind.ISINCODE = @ISINCODE
                    <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                      <xsl:with-param name="pTable" select="'ass_ind'"/>
                    </xsl:call-template>
                    <Param name="DT" datatype="date">
                      <xsl:value-of select="$vBusinessDate"/>
                    </Param>
                    <Param name="EXCHANGESYMBOL" datatype="string">
                      <xsl:value-of select="format-number(data[@name='MarketId'], '0')" />
                    </Param>
                    <Param name="ISINCODE" datatype="string">
                      <xsl:value-of select="data[@name='Isin']" />
                    </Param>
                  </SQL>
                </parameter>
              </parameters>

              <xsl:call-template name="QUOTE_H_IU">
                <xsl:with-param name="pTableName" select="'QUOTE_INDEX_H'"/>
                <xsl:with-param name="pSequenceNumber" select="'1'"/>
                <xsl:with-param name="pContractSymbol" select="data[@name='Symbol']"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IdAsset'"/>
                <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                <xsl:with-param name="pContractSymbolSuffix" select="$gNull"/>
                <xsl:with-param name="pCategory" select="data[@name='ProductType']"/>
                <xsl:with-param name="pMaturityMonthYear" select="$gNull"/>
                <xsl:with-param name="pPutCall" select="$gNull"/>
                <xsl:with-param name="pStrike" select="$gNull" />
                <xsl:with-param name="pCurrency" select="$vCurrency"/>
                <xsl:with-param name="pExchangeSymbol" select="$gNull"/>
                <xsl:with-param name="pValue" select="data[@name='UnderlyingPrice']"/>
                <xsl:with-param name="pIsWithControl" select="$gTrue"/>
              </xsl:call-template>

            </xsl:when>
            <!-- equity case -->
            <xsl:when test="data[@name='ProductType'] = 'E'">

              <!-- BD 20130306 [18723] : Rajout du parameters IdAsset -->
              <!-- Dans le cas de la CCeG, les Futures et Options sur Equities sont sur le même Marché (XDMI) -->
              <parameters>
                <parameter name="IdAsset">
                  <SQL command="select" result="IDASSET" cache="true">
                    select ass_eqt.IDASSET
                    from dbo.ASSET_EQUITY ass_eqt
                    inner join dbo.ASSET_EQUITY_RDCMK ass_eqt_rdc on ( ass_eqt_rdc.IDASSET = ass_eqt.IDASSET )
                    inner join dbo.MARKET m on ( m.IDM = ass_eqt_rdc.IDM_RELATED ) and ( m.EXCHANGESYMBOL = @EXCHANGESYMBOL )
                    where ass_eqt.ISINCODE = @ISINCODE
                    <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                      <xsl:with-param name="pTable" select="'ass_eqt'"/>
                    </xsl:call-template>
                    <Param name="DT" datatype="date">
                      <xsl:value-of select="$vBusinessDate"/>
                    </Param>
                    <Param name="EXCHANGESYMBOL" datatype="string">
                      <xsl:value-of select="format-number(data[@name='MarketId'], '0')" />
                    </Param>
                    <Param name="ISINCODE" datatype="string">
                      <xsl:value-of select="data[@name='Isin']" />
                    </Param>
                  </SQL>
                </parameter>
              </parameters>

              <xsl:call-template name="QUOTE_H_IU">
                <xsl:with-param name="pTableName" select="'QUOTE_EQUITY_H'"/>
                <xsl:with-param name="pSequenceNumber" select="'1'"/>
                <xsl:with-param name="pContractSymbol" select="data[@name='Symbol']"/>
                <xsl:with-param name="pIdAsset" select="'parameters.IdAsset'"/>
                <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                <xsl:with-param name="pContractSymbolSuffix" select="$gNull"/>
                <xsl:with-param name="pCategory" select="data[@name='ProductType']"/>
                <xsl:with-param name="pMaturityMonthYear" select="$gNull"/>
                <xsl:with-param name="pPutCall" select="$gNull"/>
                <xsl:with-param name="pStrike" select="$gNull" />
                <xsl:with-param name="pCurrency" select="$vCurrency"/>
                <xsl:with-param name="pExchangeSymbol" select="$gNull"/>
                <xsl:with-param name="pValue" select="data[@name='UnderlyingPrice']"/>
                <xsl:with-param name="pIsWithControl" select="$gTrue"/>
              </xsl:call-template>

            </xsl:when>
          </xsl:choose>
        </row>
      </xsl:when>

      <!-- RISKARRAY - insert F&O contracts quotes -->
      <xsl:otherwise>
        <xsl:variable name="vYM">
          <xsl:choose>
            <xsl:when test="data[@name='YM'] = '000000'">
              <xsl:value-of select="$gNull"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="data[@name='YM']"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vPutCall">
          <xsl:choose>
            <xsl:when test="data[@name='ClassType'] = 'F'">
              <xsl:value-of select="'F'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="data[@name='PutCall'] = 'C'">
                  <xsl:value-of select="'1'"/>
                </xsl:when>
                <xsl:when test="data[@name='PutCall'] = 'P'">
                  <xsl:value-of select="'0'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$gNull"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vCurrency">
          <xsl:choose>
            <xsl:when test="data[@name='Currency'] = 'EU'">
              <xsl:value-of select="'EUR'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gNull"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <!-- PL 20130220 Management data only where the date is filled (eg. No update for FTMIB, MINI, MFI100 and FDIV) -->
        <xsl:if test="$vYM != $gNull">
          <row>
            <xsl:call-template name="IORowAtt"/>

            <xsl:call-template name="QUOTE_H_IU">
              <xsl:with-param name="pTableName" select="'QUOTE_ETD_H'"/>
              <xsl:with-param name="pSequenceNumber" select="'1'"/>
              <xsl:with-param name="pISO10383" select="'XDMI'"/>
              <xsl:with-param name="pContractSymbol" select="data[@name='Symbol']"/>
              <xsl:with-param name="pBusinessDate" select="data[@name='Date']"/>
              <xsl:with-param name="pContractSymbolSuffix" select="$gNull"/>
              <xsl:with-param name="pCategory" select="data[@name='ClassType']"/>
              <xsl:with-param name="pMaturityMonthYear" select="$vYM"/>
              <xsl:with-param name="pPutCall" select="$vPutCall"/>
              <xsl:with-param name="pStrike" select="data[@name='StrkPrx']" />
              <xsl:with-param name="pCurrency" select="$vCurrency"/>
              <!-- PL 20130214 -->
              <!--<xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>-->
              <xsl:with-param name="pExchangeSymbol" select="data[@name='MarketId']"/>
              <xsl:with-param name="pValue" select="data[@name='MarkPrice']"/>
              <xsl:with-param name="pIsWithControl" select="$gTrue"/>
            </xsl:call-template>
          </row>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
