<!-- Ce fichier de transformation exploite le retour (au format XML) de la procédure UP_RESULT_CA -->
<!-- Affiche un compte rendu de l'application d'une Corporate Action avec :  -->
<!-- 1. Descriptif des DCs (Cum, Ex et ExAdj)  -->
<!-- 2. Descriptif des DAs pour chaque DC (Cum, Ex et ExAdj)  -->
<!-- 3. Descriptif des ASSETs pour chaque DA (Cum, Ex et ExAdj)  -->
<!-- 4. Descriptif des TRADEs en POSITION pour chaque ACTIF (Cum, ExAdj)  -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html" indent="no" encoding="UTF-8" omit-xml-declaration="yes"></xsl:output>
  <xsl:param name="pCurrentCulture" select="'en-GB'"/>

  <!-- xslt includes -->
  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\Library\StrFunc.xslt"/>

  <xsl:template match="/">
    <xsl:apply-templates select="results"/>
  </xsl:template>

  <!-- Template racine -->
  <xsl:template match="results">
    <div id="divResultsCA">
      <xsl:apply-templates select="corporateAction"/>
    </div>
  </xsl:template>

  <!-- Template principal : CORPORATE ACTION -->
  <!-- Affiche l'identifiant de la CA, sa date d'effet, l'entité concernée -->
  <!-- Appel du template "derivativeContract"  -->
  <xsl:template match="corporateAction">
    <div id="resultsCA" class="resultsCA">
      <table>
        <THEAD>
          <tr>
            <th colspan="2">
              <b>
                <xsl:value-of select="identifier"/>
                <xsl:text> (id: </xsl:text>
                <xsl:value-of select="@spheresid"/>
                <xsl:text>)</xsl:text>
              </b>
            </th>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="$varResource[@name='lblCAMarket']/value"/>
            </td>
            <td>
              <b>
                <xsl:value-of select="market"/>
                <xsl:text> (id: </xsl:text>
                <xsl:value-of select="market/@spheresid"/>
                <xsl:text>)</xsl:text>
              </b>
            </td>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="$varResource[@name='lblCARefNotice']/value"/>
            </td>
            <td>
              <b>
                <xsl:value-of select="refNotice"/>
              </b>
            </td>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="$varResource[@name='lblCEEffectiveDate']/value"/>
            </td>
            <td>
              <b>
                <xsl:value-of select="effectiveDate"/>
              </b>
            </td>
          </tr>
          <tr class="footer">
            <td>
              <xsl:value-of select="$varResource[@name='Entity_Title']/value"/>
            </td>
            <td>
              <b>
                <xsl:value-of select="entity"/>
                <xsl:text> (id: </xsl:text>
                <xsl:value-of select="entity/@spheresid"/>
                <xsl:text>)</xsl:text>
              </b>
            </td>
          </tr>
          <tr>
            <td>
              <xsl:value-of select="$varResource[@name='lblCEAdjMethod']/value"/>
            </td>
            <td>
              <b>
                <xsl:value-of select="adjMethod"/>
              </b>
            </td>
          </tr>
        </THEAD>
      </table>
    </div>
    <xsl:apply-templates select="derivativeContract"/>
  </xsl:template>

  <!-- Template  : DERIVATIVE CONTRACT -->
  <!-- Affiche les caractéristiques des DC affectés par la CA avec ses valeurs CUM, EX et eventuellement EXADJ -->
  <!-- Appel du template "derivativeAttrib"  -->
  <xsl:template match="derivativeContract">
    <div id="resultsDC" class="resultsDC">
      <table>
        <!-- HEADER DERIVATIVECONTRACT -->
        <tr>
          <td colspan="2">
            <span class="titleDC">
              <xsl:value-of select="$varResource[@name='CONTRACT']/value"/>
            </span>
          </td>
          <td>
            <xsl:value-of select="$varResource[@name='CARENAMING']/value"/>
            <xsl:text> : </xsl:text>
            <b>
              <xsl:value-of select="renaming/method"/>
              <xsl:if test ="renaming/category != '-'">
                <xsl:text> - </xsl:text>
                <xsl:value-of select="renaming/category"/>
              </xsl:if>
            </b>
          </td>
          <td>
            <xsl:value-of select="$varResource[@name='RFACTOR']/value"/>
            <xsl:text> : </xsl:text>
            <b>
              <xsl:value-of select="rFactor"/>
            </b>
          </td>
        </tr>
        <tr class="header">
          <td>
            <xsl:value-of select="$varResource[@name='Characteristics']/value"/>
          </td>
          <td>
            <xsl:value-of select="'CUM'"/>
          </td>
          <td>
            <xsl:value-of select="'EX Recycled'"/>
          </td>
          <td>
            <xsl:value-of select="'EX'"/>
          </td>
          <td>
            <xsl:value-of select="'EX Adj.'"/>
          </td>
        </tr>
        <!-- BODY DERIVATIVECONTRACT-->
        <tr>
          <td>
            <xsl:value-of select="$varResource[@name='ID']/value"/>
          </td>
          <td>
            <xsl:value-of select="@spheresid"/>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="recDerivativeContract/@spheresid"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <b>
              <xsl:value-of select="exDerivativeContract/@spheresid"/>
            </b>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract">
              <b>
                <xsl:value-of select="exAdjDerivativeContract/@spheresid"/>
              </b>
            </xsl:if>
          </td>
        </tr>
        <tr>
          <td>
            <xsl:value-of select="$varResource[@name='IDENTIFIER']/value"/>
          </td>
          <td>
            <xsl:value-of select="identifier"/>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="recDerivativeContract"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exDerivativeContract">
              <b>
                <xsl:value-of select="exDerivativeContract"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract">
              <b>
                <xsl:value-of select="exAdjDerivativeContract"/>
              </b>
            </xsl:if>
          </td>
        </tr>
        <tr>
          <td>
            <xsl:value-of select="$varResource[@name='SYMBOL']/value"/>
          </td>
          <td>
            <xsl:value-of select="contractsymbol"/>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="recDerivativeContract/@contractsymbol"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exDerivativeContract">
              <b>
                <xsl:value-of select="exDerivativeContract/@contractsymbol"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract">
              <b>
                <xsl:value-of select="exAdjDerivativeContract/@contractsymbol"/>
              </b>
            </xsl:if>
          </td>
        </tr>
        <tr>
          <td>
            <xsl:value-of select="$varResource[@name='IDASSET_UNL']/value"/>
          </td>
          <td>
            <xsl:value-of select="underlyer"/>
            <xsl:text> (</xsl:text>
            <xsl:value-of select="underlyer/@category"/>
            <xsl:text>)</xsl:text>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="recDerivativeContract/@underlyer"/>
              </b>
              <xsl:text> (</xsl:text>
              <xsl:value-of select="underlyer/@category"/>
              <xsl:text>)</xsl:text>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exDerivativeContract">
              <b>
                <xsl:value-of select="exDerivativeContract/@underlyer"/>
              </b>
              <xsl:text> (</xsl:text>
              <xsl:value-of select="underlyer/@category"/>
              <xsl:text>)</xsl:text>

            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract">
              <b>
                <xsl:value-of select="exAdjDerivativeContract/@underlyer"/>
              </b>
              <xsl:text> (</xsl:text>
              <xsl:value-of select="underlyer/@category"/>
              <xsl:text>)</xsl:text>
            </xsl:if>
          </td>
        </tr>
        <tr>
          <td>
            <xsl:value-of select="$varResource[@name='CSIZE']/value"/>
          </td>
          <td>
            <xsl:value-of select="contractSize/cumValue"/>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="contractSize/cumValue"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exDerivativeContract">
              <b>
                <xsl:value-of select="contractSize/cumValue"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract">
              <b>
                <xsl:value-of select="contractSize/exValue"/>
              </b>
            </xsl:if>
          </td>
        </tr>
        <tr>
          <td>
            <xsl:value-of select="$varResource[@name='CADTENABLED']/value"/>
          </td>
          <td>
            <xsl:value-of select="dtenabled"/>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="recDerivativeContract/@dtenabled"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exDerivativeContract">
              <b>
                <xsl:value-of select="exDerivativeContract/@dtenabled"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract">
              <b>
                <xsl:value-of select="exAdjDerivativeContract/@dtenabled"/>
              </b>
            </xsl:if>
          </td>
        </tr>
        <tr class="footer">
          <td>
            <xsl:value-of select="$varResource[@name='CADTDISABLED']/value"/>
          </td>
          <td>
            <b>
              <xsl:value-of select="dtdisabled"/>
            </b>
          </td>
          <td>
            <xsl:if test ="recDerivativeContract">
              <b>
                <xsl:value-of select="recDerivativeContract/@dtdisabled"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exDerivativeContract">
              <b>
                <xsl:value-of select="exDerivativeContract/@dtdisabled"/>
              </b>
            </xsl:if>
          </td>
          <td>
            <xsl:if test ="exAdjDerivativeContract/@dtdisabled">
              <b>
                <xsl:value-of select="exAdjDerivativeContract/@dtdisabled"/>
              </b>
            </xsl:if>
          </td>
        </tr>

        <xsl:if test ="derivativeAttrib">
          <tr>
            <td colspan="5">
              <div id="resultsDA" class="resultsDA">
                <table>
                  <tr>
                    <td colspan="5">
                      <span class="subtitle">
                        <xsl:value-of select="$varResource[@name='CADERIVATIVEATTRIB']/value"/>
                      </span>
                    </td>
                  </tr>
                  <xsl:apply-templates select="derivativeAttrib"/>
                </table>
              </div>
            </td>
          </tr>
        </xsl:if>
      </table>
    </div>
  </xsl:template>


  <!-- Template  : DERIVATIVE ATTRIB -->
  <!-- Affiche les caractéristiques des DA affectés par la CA avec ses valeurs CUM, EX et eventuellement EXADJ -->
  <!-- Appel du template "derivativeAsset"  -->
  <xsl:template match="derivativeAttrib">
    <!-- HEADER DERIVATIVATTRIB-->
    <tr class="header">
      <td>
        <xsl:value-of select="$varResource[@name='Characteristics']/value"/>
      </td>
      <td>
        <xsl:value-of select="'CUM'"/>
      </td>
      <td>
        <xsl:value-of select="'EX'"/>
      </td>
      <td>
        <xsl:value-of select="'EX Adj.'"/>
      </td>
    </tr>
    <!-- BODY DERIVATIVATTRIB-->
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='ID']/value"/>
      </td>
      <td>
        <xsl:value-of select="@spheresid"/>
      </td>
      <td>
        <xsl:if test ="exDerivativeAttrib">
          <b>
            <xsl:value-of select="exDerivativeAttrib/@spheresid"/>
          </b>
        </xsl:if>
      </td>

      <td>
        <xsl:if test ="exAdjDerivativeAttrib">
          <b>
            <xsl:value-of select="exAdjDerivativeAttrib/@spheresid"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CAMATURITY']/value"/>
      </td>
      <td>
        <xsl:value-of select="@maturity"/>
      </td>
      <td>
        <xsl:if test ="exDerivativeAttrib">
          <b>
            <xsl:value-of select="exDerivativeAttrib/@maturity"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjDerivativeAttrib">
          <b>
            <xsl:value-of select="exAdjDerivativeAttrib/@maturity"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CSIZE']/value"/>
      </td>
      <td>
        <xsl:value-of select="contractSize/cumValue"/>
      </td>
      <td>
        <xsl:if test ="exDerivativeAttrib">
          <b>
            <xsl:value-of select="contractSize/cumValue"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjDerivativeAttrib">
          <b>
            <xsl:value-of select="contractSize/exValue"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CADTENABLED']/value"/>
      </td>
      <td>
        <xsl:value-of select="dtenabled"/>
      </td>
      <td>
        <xsl:if test ="exDerivativeAttrib">
          <b>
            <xsl:value-of select="exDerivativeAttrib/@dtenabled"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjDerivativeAttrib">
          <b>
            <xsl:value-of select="exAdjDerivativeAttrib/@dtenabled"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr class="footer">
      <td>
        <xsl:value-of select="$varResource[@name='CADTDISABLED']/value"/>
      </td>
      <td>
        <b>
          <xsl:value-of select="dtdisabled"/>
        </b>
      </td>
      <td>
        <xsl:if test ="exDerivativeAttrib">
          <b>
            <xsl:value-of select="exDerivativeAttrib/@dtdisabled"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjDerivativeAttrib">
          <b>
            <xsl:value-of select="exAdjDerivativeAttrib/@dtdisabled"/>
          </b>
        </xsl:if>
      </td>
    </tr>

    <xsl:if test="asset">
      <tr>
        <td colspan="4">
          <div id="resultsASSET" class="resultsASSET">
            <table>
              <tr>
                <td colspan="4">
                  <span class="subtitle">
                    <xsl:value-of select="$varResource[@name='Asset']/value"/>
                  </span>
                </td>
              </tr>
              <xsl:apply-templates select="asset"/>
            </table>
          </div>
        </td>
      </tr>
    </xsl:if>

  </xsl:template>

  <!-- Template  : ASSET -->
  <!-- Affiche les caractéristiques des ASSET affectés par la CA avec ses valeurs CUM, EX et eventuellement EXADJ -->
  <!-- Appel du template "trade"  -->
  <xsl:template match="asset">
    <!-- HEADER ASSET-->
    <tr class="header">
      <td>
        <xsl:value-of select="$varResource[@name='Characteristics']/value"/>
      </td>
      <td>
        <xsl:value-of select="'CUM'"/>
      </td>
      <td>
        <xsl:value-of select="'EX'"/>
      </td>
      <td>
        <xsl:value-of select="'EX Adj.'"/>
      </td>
    </tr>
    <!-- BODY ASSET-->
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='ID']/value"/>
      </td>
      <td>
        <xsl:value-of select="@spheresid"/>
      </td>
      <td>
        <xsl:if test="exAsset">
          <b>
            <xsl:value-of select="exAsset/@spheresid"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test="exAdjAsset">
          <b>
            <xsl:value-of select="exAdjAsset/@spheresid"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='IDENTIFIER']/value"/>
      </td>
      <td>
        <xsl:value-of select="identifier"/>
      </td>
      <td>
        <xsl:if test="exAsset">
          <b>
            <xsl:value-of select="exAsset"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test="exAdjAsset">
          <b>
            <xsl:value-of select="exAdjAsset"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CSIZE']/value"/>
      </td>
      <td>
        <xsl:value-of select="contractSize/cumValue"/>
      </td>
      <td>
        <xsl:if test ="exAsset">
          <b>
            <xsl:value-of select="contractSize/cumValue"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjAsset">
          <b>
            <xsl:value-of select="contractSize/exValue"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <xsl:if test ="@category = 'O'">
      <tr>
        <td>
          <xsl:value-of select="$varResource[@name='STRIKE']/value"/>
        </td>
        <td>
          <xsl:value-of select="strikePrice/cumValue"/>
        </td>
        <td>
          <xsl:if test ="exAsset">
            <b>
              <xsl:value-of select="strikePrice/cumValue"/>
            </b>
          </xsl:if>
        </td>
        <td>
          <xsl:if test ="exAdjAsset">
            <b>
              <xsl:value-of select="strikePrice/exValue"/>
            </b>
          </xsl:if>
        </td>
      </tr>
    </xsl:if>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CADCLOSINGPRICE']/value"/>
      </td>
      <td>
        <xsl:if test ="dailyClosingPrice/cumValue">
          <xsl:value-of select="dailyClosingPrice/cumValue"/>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAsset">
          <xsl:if test ="dailyClosingPrice/cumValue">
            <b>
              <xsl:value-of select="dailyClosingPrice/cumValue"/>
            </b>
          </xsl:if>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjAsset">
          <b>
            <xsl:if test ="dailyClosingPrice/exValue">
              <b>
                <xsl:value-of select="dailyClosingPrice/exValue"/>
              </b>
            </xsl:if>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CADTENABLED']/value"/>
      </td>
      <td>
        <xsl:value-of select="dtenabled"/>
      </td>
      <td>
        <xsl:if test ="exAsset">
          <b>
            <xsl:value-of select="exAsset/@dtenabled"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjAsset">
          <b>
            <xsl:value-of select="exAdjAsset/@dtenabled"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="$varResource[@name='CADTDISABLED']/value"/>
      </td>
      <td>
        <b>
          <xsl:value-of select="dtdisabled"/>
        </b>
      </td>
      <td>
        <xsl:if test ="exAsset">
          <b>
            <xsl:value-of select="exAsset/@dtdisabled"/>
          </b>
        </xsl:if>
      </td>
      <td>
        <xsl:if test ="exAdjAsset">
          <b>
            <xsl:value-of select="exAdjAsset/@dtdisabled"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <xsl:if test ="equalisationPayment > 0">
      <tr class="footer">
        <td>
          <xsl:value-of select="$varResource[@name='lblCEEqualisationPayment']/value"/>
        </td>
        <td colspan="3">
          <b>
            <xsl:value-of select="equalisationPayment"/>
          </b>
        </td>
      </tr>
    </xsl:if>
    <xsl:if test ="trade">
      <tr>
        <td colspan="4">
          <div id="resultsTRADE" class="resultsTRADE">
            <table>
              <tr>
                <td colspan="5">
                  <span class="subtitle">
                    <xsl:value-of select="$varResource[@name='CAOPENINTEREST']/value"/>
                  </span>
                </td>
              </tr>
              <xsl:apply-templates select="trade"/>
            </table>
          </div>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>

  <!-- Template  : TRADE -->
  <!-- Affiche les caractéristiques des TRADES en position au moement de la CA et chaque nouvelle ouverture de position après ajustement -->
  <xsl:template match="trade">

    <!-- HEADER TRADE-->
    <tr class="header">
      <td>
        <xsl:value-of select="$varResource[@name='Characteristics']/value"/>
      </td>
      <td>
        <xsl:value-of select="$varResource[@name='ID']/value"/>
      </td>
      <td>
        <xsl:value-of select="$varResource[@name='IDENTIFIER']/value"/>
      </td>
      <td>
        <xsl:value-of select="$varResource[@name='QTY']/value"/>
      </td>
      <td>
        <xsl:value-of select="$varResource[@name='lblCEEqualisationPayment']/value"/>
      </td>
    </tr>
    <!-- BODY TRADE-->
    <tr>
      <td>
        <xsl:value-of select="'CUM'"/>
      </td>
      <td>
        <xsl:value-of select="@spheresid"/>
      </td>
      <td>
        <xsl:value-of select="identifier"/>
      </td>
      <td>
        <xsl:value-of select="qty"/>
      </td>
      <td>
        <xsl:if test ="equalisationPayment > 0">
          <b>
            <xsl:value-of select="equalisationPayment"/>
          </b>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td>
        <xsl:value-of select="'EX Adj.'"/>
      </td>
      <td>
        <b>
          <xsl:value-of select="exAdjtrade/@spheresid"/>
        </b>
      </td>
      <td>
        <b>
          <xsl:value-of select="exAdjtrade/identifier"/>
        </b>
      </td>
      <td>
        <xsl:value-of select="qty"/>
      </td>
      <td>
        <xsl:text> </xsl:text>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>

