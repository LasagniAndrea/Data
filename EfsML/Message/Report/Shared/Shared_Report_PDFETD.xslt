<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!--  
================================================================================================================
Summary : Spheres report - Shared - PDF templates for ETD trades
          XSL for generating PDF document of report based on ETD trades
          
File    : Shared_Report_PDFETD.xslt
================================================================================================================
Version : v4.0.5291
Date    : 20140627
Author  : RD
Comment : [20147] Tous les flux XML véhiculent FIXML_SECURITYEXCHANGE du marché, 
          il faut donc utiliser cette info quand on interroge MarketRepository
================================================================================================================
Version 1.0.0.0 (Spheres 2.5.0.0)
 Date: 17/09/2010
 Author: RD
 Description: first version  
 
Version 1.0.1.0 (Spheres 2.6.4.0)
 Date: 22/08/2012 [Ticket 18073]
 Author: MF
 Description: Add converted prices to the report. the strike price and the transaction price will be displayed using the 
 derivative contract specific numerical base and the given style.
 
================================================================================================================
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->

  <!--//-->
  <xsl:include href="Shared_Report_Main.xslt" />
  <xsl:include href="Shared_Report_BusinessETD.xslt" />

  <!-- ================================================== -->
  <!--        Main Template                               -->
  <!-- ================================================== -->

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Display Templates                                                     -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!--DisplayPageBody template-->
  <xsl:template name="DisplayPageBody">
    <xsl:param name="pBook" />
    <xsl:param name="pBookPosition" />
    <xsl:param name="pCountBook" />
    <xsl:param name="pSetLastPage" select="true()" />
    <xsl:param name="pGroups" />
    <xsl:param name="pIsDisplayStarLegend" />
    <xsl:param name="pIsHFormulaMandatory" select="true()" />

    <xsl:call-template name="DisplayPageBodyCommon">
      <xsl:with-param name="pBook" select="$pBook" />
      <xsl:with-param name="pBookPosition" select="$pBookPosition" />
      <xsl:with-param name="pCountBook" select="$pCountBook" />
      <xsl:with-param name="pSetLastPage" select="$pSetLastPage" />
      <xsl:with-param name="pGroups" select="$pGroups" />
      <xsl:with-param name="pIsDisplayStarLegend" select="$pIsDisplayStarLegend"/>
      <xsl:with-param name="pIsHFormulaMandatory" select="$pIsHFormulaMandatory"/>
    </xsl:call-template>
  </xsl:template>

  <!--DisplayBookAdditionalDetail template-->
  <xsl:template name="DisplayBookAdditionalDetail">
    <xsl:call-template name="CommonDisplayBookAdditionalDetail" />
  </xsl:template>

  <xsl:template match="serie" mode="display">
    <xsl:param name="pGroupColumnsToDisplay"/>
    <xsl:param name="pGroupName"/>

    <xsl:variable name="vCurrentSerie" select="."/>

    <!-- ===== Display Trades details===== -->
    <xsl:apply-templates select="msxsl:node-set($gBusinessTrades)/trade[@href=$vCurrentSerie/trade/@href]" mode="display-specific">
      <xsl:sort select="stgy" data-type="text" order="descending"/>
      <xsl:sort select="posActions/posAction[(string-length(@dtUnclearing)>0) or (@type!='ENTRY' and @type!='UPDENTRY')]" data-type="text" order="descending"/>
      <xsl:sort select="@tradeDate" data-type="text"/>
      <xsl:sort select="@timeStamp" data-type="text"/>
      <xsl:sort select="@lastPx" data-type="number"/>
      <xsl:sort select="@side" data-type="number"/>

      <xsl:with-param name="pCurrentTradeWithStatus" select="$vCurrentSerie/trade"/>
      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay"/>
    </xsl:apply-templates>
    <!-- ===== Display SubTotals===== -->
    <xsl:for-each select="msxsl:node-set($gColSubTotalColDisplaySettings)/group[@name=$pGroupName]/row">
      <xsl:sort select="@sort" data-type="number"/>

      <xsl:variable name="vConditionData">
        <xsl:for-each select="./condition">
          <condition>
            <xsl:copy-of select="./@*"/>
            <xsl:choose>
              <xsl:when test="./subtotal">
                <xsl:for-each select="./subtotal">
                  <subtotal>
                    <xsl:copy-of select="./@*"/>
                    <xsl:attribute name="value">
                      <xsl:call-template name="GetSerieSubTotalData">
                        <xsl:with-param name="pSerie" select="$vCurrentSerie"/>
                        <xsl:with-param name="pSubTotalName" select="./@name"/>
                      </xsl:call-template>
                    </xsl:attribute>
                  </subtotal>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="value">
                  <xsl:call-template name="GetSerieSubTotalData">
                    <xsl:with-param name="pSerie" select="$vCurrentSerie"/>
                    <xsl:with-param name="pSubTotalName" select="./@subtotal"/>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
          </condition>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="vIsRowToDisplay">
        <xsl:choose>
          <xsl:when test="./condition">
            <xsl:choose>
              <xsl:when test="msxsl:node-set($vConditionData)/condition[(count(subtotal) > 0) and (count(subtotal) = count(subtotal[string-length(@value) = 0]))]">
                <xsl:value-of select="false()"/>
              </xsl:when>
              <xsl:when test="msxsl:node-set($vConditionData)/condition[string-length(@value) = 0]">
                <xsl:value-of select="false()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="true()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="true()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$vIsRowToDisplay = 'true'">
        <fo:table-row font-size="{$gDetTrdFontSize}"
                      font-weight="normal"
                      text-align="right"
                      keep-with-previous="always">

          <xsl:for-each select="subtotal">
            <xsl:sort select="@sort" data-type="number"/>

            <xsl:call-template name="CreateEmptyCellsForDetailedTrades">
              <xsl:with-param name="pnNumberOfCells" select="@empty-columns" />
            </xsl:call-template>

            <!--SubTotal Label-->
            <xsl:if test="label">

              <fo:table-cell>

                <xsl:if test="string-length(label/@rows-spanned)>0">
                  <xsl:attribute name="number-rows-spanned">
                    <xsl:value-of select="number(label/@rows-spanned)" />
                  </xsl:attribute>
                </xsl:if>

                <xsl:if test="string-length(label/@columns-spanned)>0">
                  <xsl:attribute name="number-columns-spanned">
                    <xsl:value-of select="number(label/@columns-spanned)" />
                  </xsl:attribute>
                </xsl:if>

                <xsl:variable name="vSubTotalLabel">
                  <xsl:call-template name="GetSerieSubTotalLabel">
                    <xsl:with-param name="pSerie" select="$vCurrentSerie" />
                    <xsl:with-param name="pSubTotalName" select="./@name"/>
                  </xsl:call-template>
                </xsl:variable>

                <xsl:if test="string-length($vSubTotalLabel) > 0">
                  <xsl:attribute name="font-weight">
                    <xsl:choose>
                      <xsl:when test="string-length(label/@font-weight)>0">
                        <xsl:value-of select="label/@font-weight" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'bold'" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                  <xsl:attribute name="padding">
                    <xsl:value-of select="$gDetTrdCellPadding" />
                  </xsl:attribute>
                  <xsl:attribute name="text-align">
                    <xsl:choose>
                      <xsl:when test="string-length(label/@text-align)>0">
                        <xsl:value-of select="label/@text-align" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'right'" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>

                  <fo:block>
                    <xsl:value-of select="$vSubTotalLabel"/>
                  </fo:block>
                </xsl:if>
              </fo:table-cell>
            </xsl:if>

            <fo:table-cell>

              <xsl:if test="string-length(@rows-spanned)>0">
                <xsl:attribute name="number-rows-spanned">
                  <xsl:value-of select="number(@rows-spanned)" />
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="string-length(@columns-spanned)>0">
                <xsl:attribute name="number-columns-spanned">
                  <xsl:value-of select="number(@columns-spanned)" />
                </xsl:attribute>
              </xsl:if>

              <xsl:variable name="vSubTotalData">
                <xsl:choose>
                  <xsl:when test="./../@condition = ./@name">
                    <xsl:value-of select="$vConditionData"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="GetSerieSubTotalData">
                      <xsl:with-param name="pSerie" select="$vCurrentSerie"/>
                      <xsl:with-param name="pSubTotalName" select="./@name"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <xsl:choose>
                <xsl:when test="string-length($vSubTotalData) > 0">

                  <xsl:attribute name="font-weight">
                    <xsl:choose>
                      <xsl:when test="string-length(@font-weight)>0">
                        <xsl:value-of select="@font-weight" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'bold'" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>
                  <xsl:attribute name="padding">
                    <xsl:value-of select="$gDetTrdCellPadding" />
                  </xsl:attribute>
                  <xsl:attribute name="text-align">
                    <xsl:choose>
                      <xsl:when test="string-length(@text-align)>0">
                        <xsl:value-of select="@text-align" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="'right'" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:attribute>

                  <xsl:if test="string-length(@font-size)>0">
                    <xsl:attribute name="font-size">
                      <xsl:value-of select="@font-size" />
                    </xsl:attribute>
                  </xsl:if>
                  <xsl:if test="string-length(@display-align)>0">
                    <xsl:attribute name="display-align">
                      <xsl:value-of select="@display-align" />
                    </xsl:attribute>
                  </xsl:if>

                  <fo:block>
                    <!-- 20120821 MF Ticket 18073 typenode attribute check, to select all the node contents including tags (using copy-of) -->
                    <xsl:choose>
                      <xsl:when test="@typenode = 'true'">
                        <xsl:copy-of select="msxsl:node-set($vSubTotalData)/node()/node()"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$vSubTotalData"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </xsl:when>
                <xsl:otherwise>
                  <fo:block>
                    <xsl:value-of select="$gcEspace"/>
                  </fo:block>
                </xsl:otherwise>
              </xsl:choose>


            </fo:table-cell>
          </xsl:for-each>
        </fo:table-row>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="stgy" mode="display">
    <xsl:param name="pGroupColumnsToDisplay"/>

    <xsl:variable name="vCurrentStgy" select="."/>

    <!-- ===== Display Trades details===== -->
    <xsl:apply-templates select="msxsl:node-set($gBusinessTrades)/trade[@href=$vCurrentStgy/trade/@href]" mode="display-specific">
      <xsl:sort select="stgy/@legno" data-type="number"/>

      <xsl:with-param name="pCurrentTradeWithStatus" select="$vCurrentStgy/trade"/>
      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay"/>
    </xsl:apply-templates>

  </xsl:template>

  <xsl:template match="column" mode="display-getcolumn">
    <xsl:param name="pGroup"/>

    <xsl:variable name="vHeaderData">
      <xsl:choose>
        <xsl:when test="@name='TS-Price'">
          <xsl:value-of select="$pGroup/@cuPrice"/>
        </xsl:when>
        <xsl:when test="@name='TS-Premium' and $pGroup/@cat='O'">
          <xsl:value-of select="$pGroup/@cuPrice"/>
        </xsl:when>
        <xsl:when test="@name='POS-NOV' and $pGroup/@cat='O'">
          <xsl:value-of select="$pGroup/@cuPrice"/>
        </xsl:when>
        <xsl:when test="@name='POS-UMG'">
          <xsl:value-of select="$pGroup/@cuPrice"/>
        </xsl:when>
        <xsl:when test="@name='PA-RMG' or @name='PA-CASHSTL' or @name='PA-TradeImpactedPrice'">
          <xsl:value-of select="$pGroup/@cuPrice"/>
        </xsl:when>
        <xsl:when test="@name='TS-Brokerage' or @name='INV-BrokeragePrev'">
          <xsl:value-of select="$pGroup/@cuBro"/>
        </xsl:when>
        <xsl:when test="@name='TS-BrokerageTrd' or @name='INV-BrokerageTrdPrev'">
          <xsl:value-of select="$pGroup/@cuBroTrd"/>
        </xsl:when>
        <xsl:when test="@name='TS-BrokerageClr' or @name='INV-BrokerageClrPrev'">
          <xsl:value-of select="$pGroup/@cuBroClr"/>
        </xsl:when>
        <xsl:when test="@name='TS-Fee' or @name='INV-FeePrev'">
          <xsl:value-of select="$pGroup/@cuFee"/>
        </xsl:when>
        <xsl:when test="@name='TS-FeeTrd' or @name='INV-FeeTrdPrev'">
          <xsl:value-of select="$pGroup/@cuFeeTrd"/>
        </xsl:when>
        <xsl:when test="@name='TS-FeeClr' or @name='INV-FeeClrPrev'">
          <xsl:value-of select="$pGroup/@cuFeeClr"/>
        </xsl:when>
        <xsl:when test="@name='INV-Delta'">
          <xsl:value-of select="$pGroup/@cuDelta"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="GetColumnToDisplay">
      <xsl:with-param name="pGroup" select="$pGroup" />
      <xsl:with-param name="pHeaderData" select="$vHeaderData" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="Trade-display">
    <xsl:param name="pGroupColumnsToDisplay" />
    <xsl:param name="pTrade"/>
    <xsl:param name="pTradeWithStatus"/>
    <xsl:param name="pIsFirstTrade"/>
    <xsl:param name="pIsLastTrade"/>

    <!--Master column data-->
    <fo:table-row font-size="{$gDetTrdFontSize}" font-weight="normal">
      <xsl:apply-templates select="$pGroupColumnsToDisplay[string-length(@master-column)=0]" mode="display-data">
        <xsl:sort select="@number" data-type="number"/>
        <xsl:with-param name="pRowData" select="$pTrade" />
        <xsl:with-param name="pTradeWithStatus" select="$pTradeWithStatus" />
        <xsl:with-param name="pIsFirst" select="$pIsFirstTrade" />
        <xsl:with-param name="pIsLast" select="$pIsLastTrade" />
      </xsl:apply-templates>
    </fo:table-row>
    <!--Sub column data-->
    <fo:table-row font-size="{$gDetTrdFontSize}" font-weight="normal" keep-with-previous="always">
      <xsl:apply-templates select="$pGroupColumnsToDisplay[string-length(@master-column)>0]" mode="display-data-2nd">
        <xsl:sort select="@number" data-type="number"/>
        <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay" />
        <xsl:with-param name="pRowData" select="$pTrade" />
        <xsl:with-param name="pTradeWithStatus" select="$pTradeWithStatus" />
        <xsl:with-param name="pIsFirst" select="$pIsFirstTrade" />
        <xsl:with-param name="pIsLast" select="$pIsLastTrade" />
      </xsl:apply-templates>
    </fo:table-row>
  </xsl:template>

  <!--GetCommonSerieSubTotalLabel template-->
  <xsl:template name="GetCommonSerieSubTotalLabel">
    <xsl:param name="pSerie"/>
    <xsl:param name="pSubTotalName"/>

    <xsl:choose>
      <xsl:when test="$pSubTotalName = 'TS-Book'"/>
      <xsl:when test="$pSubTotalName = 'TS-BookDisplayname'"/>
      <xsl:when test="$pSubTotalName = 'TS-QtySumBuy'"/>
      <xsl:when test="$pSubTotalName = 'TS-QtySumSell'"/>
      <xsl:when test="$pSubTotalName = 'TS-QtyPos'"/>
      <xsl:when test="$pSubTotalName = 'TS-QtyPosBuy'"/>
      <xsl:when test="$pSubTotalName = 'TS-QtyPosSell'"/>
      <xsl:when test="$pSubTotalName = 'TS-PRMSum'"/>
      <xsl:when test="$pSubTotalName = 'TS-PRMSumDrCr'"/>
      <!-- ===== AvgPxBuy and Converted AvgPxBuy ===== -->
      <!-- 20120827 MF Ticket 18073 adding TS-ConvertedAvgPxBuy -->
      <xsl:when test="$pSubTotalName = 'TS-AvgPxBuy' or $pSubTotalName = 'TS-ConvertedAvgPxBuy'">
        <xsl:choose>
          <xsl:when test="number($pSerie/sum/long/@avgPx) > 0">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-AvgPxLong'" />
            </xsl:call-template>
            <xsl:value-of select="string(' :')" />
          </xsl:when>
          <xsl:when test="number($pSerie/sum/short/@avgPx) > 0">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-AvgPxShort'" />
            </xsl:call-template>
            <xsl:value-of select="string(' :')" />
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== AvgPxSell and Converted AvgPxSell ===== -->
      <!-- 20120827 MF Ticket 18073 adding TS-ConvertedAvgPxSell -->
      <xsl:when test="$pSubTotalName = 'TS-AvgPxSell' or $pSubTotalName = 'TS-ConvertedAvgPxSell'">
        <xsl:if test="number($pSerie/sum/long/@avgPx) > 0 and number($pSerie/sum/short/@avgPx) > 0">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-AvgPxShort'" />
          </xsl:call-template>
          <xsl:value-of select="string(' :')" />
        </xsl:if>
      </xsl:when>
      <!-- ===== BrokerageSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageSum'"/>
      <!-- ===== BrokerageSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageSumDrCr'"/>
      <!-- ===== BrokerageTrdSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageTrdSum'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="$pSubTotalName" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageTrdSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageTrdSumDrCr'"/>
      <!-- ===== BrokerageClrSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageClrSum'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="$pSubTotalName" />
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageClrSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageClrSumDrCr'"/>
      <!-- ===== FeeSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeSum'"/>
      <!-- ===== FeeSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeSumDrCr'"/>
      <!-- ===== FeeTrdSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeTrdSum'"/>
      <!-- ===== FeeTrdSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeTrdSumDrCr'"/>
      <!-- ===== FeeClrSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeClrSum'"/>
      <!-- ===== FeeClrSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeClrSumDrCr'"/>
    </xsl:choose>

  </xsl:template>
  <!--GetCommonSerieSubTotalData template-->
  <xsl:template name="GetCommonSerieSubTotalData">
    <xsl:param name="pSerie"/>
    <xsl:param name="pSubTotalName"/>

    <xsl:choose>
      <!-- ===== Book ===== -->
      <xsl:when test="$pSubTotalName = 'TS-Book'">
        <xsl:value-of select="$pSerie/@book" />
      </xsl:when>
      <!-- ===== BookDisplayname ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BookDisplayname'">
        <xsl:variable name="vBookDisplayName" select="$gDataDocumentRepository/book[identifier=$pSerie/@book]/displayname"/>
        <xsl:if test="(string-length($vBookDisplayName) > 0) and  (string($vBookDisplayName) != string($pSerie/@book))">
          <xsl:value-of select="$vBookDisplayName" />
        </xsl:if>
      </xsl:when>
      <!-- ===== QtySumBuy ===== -->
      <xsl:when test="$pSubTotalName = 'TS-QtySumBuy'">
        <xsl:if test="number($pSerie/sum/long/@qty) >= 0">
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="$pSerie/sum/long/@qty" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== QtySumSell ===== -->
      <xsl:when test="$pSubTotalName = 'TS-QtySumSell'">
        <xsl:if test="number($pSerie/sum/short/@qty) >= 0">
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="$pSerie/sum/short/@qty" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== QtyPos ===== -->
      <xsl:when test="$pSubTotalName = 'TS-QtyPos'">
        <xsl:value-of select="$pSerie/sum/pos/@slf"/>
      </xsl:when>
      <!-- ===== QtyPosBuy ===== -->
      <xsl:when test="$pSubTotalName = 'TS-QtyPosBuy'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/pos/@slf = 'Long'">
            <xsl:call-template name="format-integer">
              <xsl:with-param name="integer" select="$pSerie/sum/pos/@qty" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== QtyPosSell ===== -->
      <xsl:when test="$pSubTotalName = 'TS-QtyPosSell'">
        <xsl:choose>
          <xsl:when test="$pSerie/sum/pos/@slf = 'Short'">
            <xsl:call-template name="format-integer">
              <xsl:with-param name="integer" select="$pSerie/sum/pos/@qty" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== PRMSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-PRMSum'">
        <xsl:if test="$pSerie/sum/prm and string-length($pSerie/sum/prm/@amount) > 0">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="$pSerie/sum/prm/@amount" />
            <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== PRMSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-PRMSumDrCr'">
        <xsl:if test="string-length($pSerie/sum/prm/@amount) > 0 and number($pSerie/sum/prm/@amount) > 0">
          <xsl:call-template name="AmountSufix">
            <xsl:with-param name="pAmountSens" select="$pSerie/sum/prm/@pr" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== AvgPxBuy ===== -->
      <xsl:when test="$pSubTotalName = 'TS-AvgPxBuy'">
        <xsl:choose>
          <xsl:when test="number($pSerie/sum/long/@avgPx) > 0">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="concat($pSerie/sum/long/@avgPx,'')" />
              <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="number($pSerie/sum/short/@avgPx) > 0">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="concat($pSerie/sum/short/@avgPx,'')" />
              <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Converted AvgPxBuy ===== -->
      <xsl:when test="$pSubTotalName = 'TS-ConvertedAvgPxBuy'">
        <xsl:choose>
          <xsl:when test="number($pSerie/sum/long/@avgPx) > 0">
            <xsl:copy-of select="$pSerie/sum/convertedLong"/>
          </xsl:when>
          <xsl:when test="number($pSerie/sum/short/@avgPx) > 0">
            <xsl:copy-of select="$pSerie/sum/convertedShort"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== AvgPxSell ===== -->
      <xsl:when test="$pSubTotalName = 'TS-AvgPxSell'">
        <xsl:if test="number($pSerie/sum/short/@avgPx) > 0 and number($pSerie/sum/long/@avgPx) > 0">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="concat($pSerie/sum/short/@avgPx,'')" />
            <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- ===== Converted AvgPxSell ===== -->
      <xsl:when test="$pSubTotalName = 'TS-ConvertedAvgPxSell'">
        <xsl:if test="number($pSerie/sum/short/@avgPx) > 0 and number($pSerie/sum/long/@avgPx) > 0">
          <xsl:copy-of select="$pSerie/sum/convertedShort"/>
        </xsl:if>
      </xsl:when>
      <!-- ===== BrokerageSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageSum'">
        <xsl:call-template name="GetPaymentSubTotalData">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType='Brokerage']"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageSumDrCr'">
        <xsl:call-template name="GetPaymentSubTotalDrCr">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType='Brokerage']"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageTrdSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageTrdSum'">
        <xsl:call-template name="GetPaymentSubTotalData">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcBroTrd]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageTrdSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageTrdSumDrCr'">
        <xsl:call-template name="GetPaymentSubTotalDrCr">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcBroTrd]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageClrSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageClrSum'">
        <xsl:call-template name="GetPaymentSubTotalData">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcBroClr]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== BrokerageClrSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-BrokerageClrSumDrCr'">
        <xsl:call-template name="GetPaymentSubTotalDrCr">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcBroClr]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== FeeSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeSum'">
        <xsl:call-template name="GetPaymentSubTotalData">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType='Fee']"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== FeeSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeSumDrCr'">
        <xsl:call-template name="GetPaymentSubTotalDrCr">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType='Fee']"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== FeeTrdSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeTrdSum'">
        <xsl:call-template name="GetPaymentSubTotalData">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcFeeTrd]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== FeeTrdSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeTrdSumDrCr'">
        <xsl:call-template name="GetPaymentSubTotalDrCr">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcFeeTrd]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== FeeClrSum ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeClrSum'">
        <xsl:call-template name="GetPaymentSubTotalData">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcFeeClr]"/>
        </xsl:call-template>
      </xsl:when>
      <!-- ===== FeeClrSumDrCr ===== -->
      <xsl:when test="$pSubTotalName = 'TS-FeeClrSumDrCr'">
        <xsl:call-template name="GetPaymentSubTotalDrCr">
          <xsl:with-param name="pPayment" select="$pSerie/sum/payment[@paymentType=$gcFeeClr]"/>
        </xsl:call-template>
      </xsl:when>

    </xsl:choose>
  </xsl:template>

  <!-- .................................. -->
  <!--   Other                            -->
  <!-- .................................. -->
  <!--AmountSufix-->
  <xsl:template name="AmountSufix">
    <xsl:param name="pAmountSens"/>

    <xsl:choose>
      <xsl:when test="$pAmountSens = $gcRec">
        <xsl:value-of select="$gcCredit"/>
      </xsl:when>
      <xsl:when test="$pAmountSens = $gcPay">
        <xsl:value-of select="$gcDebit"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!--DisplayEmptyAmountSufix-->
  <xsl:template name="DisplayEmptyAmountSufix">
    <xsl:param name="pBackground-color" select="'white'"/>

    <xsl:attribute name="color">
      <xsl:value-of select="$pBackground-color"/>
    </xsl:attribute>
    <xsl:value-of select="concat($gcEspace,'DR')" />
  </xsl:template>

  <!-- .................................. -->
  <!--   Common Column                    -->
  <!-- .................................. -->
  <!-- ===== GetCommonColumnDataToDisplay ===== -->
  <xsl:template name="GetCommonColumnDataToDisplay">
    <xsl:param name="pColumnName" />
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:choose>
      <!-- ===== Book ===== -->
      <xsl:when test="$pColumnName='TS-Book'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/BOOK/@data"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== OrderNum ===== -->
      <xsl:when test="$pColumnName='TS-OrderNum'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pRowData/stgy">
                <xsl:value-of select="$pRowData/stgy/@ordID"/>
              </xsl:when>
              <xsl:when test="string-length($pRowData/@ordID) > 0">
                <xsl:value-of select="$pRowData/@ordID"/>
              </xsl:when>
              <!--OptionExercice-->
              <xsl:when test="$pRowData/@trdTyp = '45'">
                <xsl:choose>
                  <!--TransactionFromExercise-->
                  <xsl:when test="$pRowData/@trdSubTyp = '9'">
                    <xsl:call-template name="getSpheresTranslation">
                      <xsl:with-param name="pResourceName" select="'TS-EXE'" />
                    </xsl:call-template>
                  </xsl:when>
                  <!--TransactionFromAssignment-->
                  <xsl:when test="$pRowData/@trdSubTyp = '10'">
                    <xsl:call-template name="getSpheresTranslation">
                      <xsl:with-param name="pResourceName" select="'TS-ASS'" />
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Stgy ===== -->
      <xsl:when test="$pColumnName='TS-Stgy'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pRowData/stgy">
                <xsl:value-of select="'(Strategy)'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetTradeNumDataToDisplay">
                  <xsl:with-param name="pExecID" select="$pRowData/@execID_link"/>
                  <xsl:with-param name="pHref" select="$pRowData/@tradeId_link"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Stgy Type===== -->
      <xsl:when test="$pColumnName='TS-StgyType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/stgy/@type"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeDate ===== -->
      <xsl:when test="$pColumnName='TS-TradeDate'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-shortdate_ddMMMyy">
              <xsl:with-param name="year" select="substring($pRowData/@tradeDate , 1, 4)" />
              <xsl:with-param name="month" select="substring($pRowData/@tradeDate , 6, 2)" />
              <xsl:with-param name="day" select="substring($pRowData/@tradeDate , 9, 2)" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeTime ===== -->
      <xsl:when test="$pColumnName='TS-TradeTime'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-time2">
              <xsl:with-param name="xsd-date-time" select="$pRowData/@timeStamp" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TradeNumber ===== -->
      <xsl:when test="$pColumnName='TS-TradeNum'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="GetTradeNumDataToDisplay">
              <xsl:with-param name="pExecID" select="$pRowData/@execID"/>
              <xsl:with-param name="pHref" select="$pRowData/@href"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Stgy Leg===== -->
      <xsl:when test="$pColumnName='TS-StgyLeg'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="$pRowData/stgy">
              <xsl:value-of select="concat('(Leg ',$pRowData/stgy/@legno,')')"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== MarketTitle ===== -->
      <xsl:when test="$pColumnName='TS-MarketTitle'">
        <xsl:choose>
          <xsl:when test="$pRowData/keyValue">
            <xsl:variable name="vKeyValue" select="$pRowData/keyValue[@href=$gSortingKeys[text()='MARKET']/@id]/@data" />
            <xsl:call-template name="GetMarketDataToDisplay">
              <xsl:with-param name="pMarket" select="$vKeyValue"/>
              <xsl:with-param name="pMarketRepository" select="$gDataDocumentRepository/market[fixml_SecurityExchange=$vKeyValue]"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pRowData/data[@name='market']/@displayname"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== DCTitle ===== -->
      <xsl:when test="$pColumnName='TS-DCTitle'">
        <xsl:choose>
          <xsl:when test="$pRowData/keyValue">
            <xsl:variable name="vDC" select="$pRowData/keyValue[@href=$gSortingKeys[text()='DERIVATIVECONTRACT']/@id]/@data" />
            <xsl:call-template name="GetContractDataToDisplay">
              <xsl:with-param name="pContract" select="$vDC"/>
              <xsl:with-param name="pContractRepository" select="$gDataDocumentRepository/derivativeContract[identifier=$vDC]"/>
              <xsl:with-param name="pIsShort" select="false()"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pRowData/data[@name='dc']/@displayname"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== DCUNLTitle ===== -->
      <xsl:when test="$pColumnName='TS-DCUNLTitle'">
        <xsl:variable name="vDC" select="$pRowData/keyValue[@href=$gSortingKeys[text()='DERIVATIVECONTRACT']/@id]/@data" />
        <xsl:variable name="vContractRepository" select="$gDataDocumentRepository/derivativeContract[identifier=$vDC]"/>

        <xsl:choose>
          <xsl:when test="$vContractRepository/assetCategory = 'Future'">
            <xsl:if test="string-length($vContractRepository/idDC_Unl) >0">
              <xsl:call-template name="GetContractDataToDisplay">
                <xsl:with-param name="pContract" select="$vContractRepository/idDC_Unl"/>
                <xsl:with-param name="pContractRepository" select="$gDataDocumentRepository/derivativeContract[@OTCmlId=$vContractRepository/idDC_Unl]"/>
                <xsl:with-param name="pIsShort" select="false()"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:if test="string-length($vContractRepository/idAsset_Unl) >0">
              <xsl:variable name="vAssetRepository" select="$gDataDocumentRepository/asset[@OTCmlId=$vContractRepository/idAsset_Unl]"/>
              <xsl:if test="$vAssetRepository">
                <xsl:value-of select="concat($vAssetRepository/identifier,' - ', $vAssetRepository/displayname)"/>
              </xsl:if>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:when>
      <!-- ===== MARKET===== -->
      <xsl:when test="$pColumnName='TS-MARKET'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="GetMarketDataToDisplay">
              <xsl:with-param name="pMarket" select="$pRowData/MARKET/@data"/>
              <xsl:with-param name="pMarketRepository" select="$gDataDocumentRepository/market[fixml_SecurityExchange=$pRowData/MARKET/@data]"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== DERIVATIVECONTRACT===== -->
      <xsl:when test="$pColumnName='TS-DERIVATIVECONTRACT'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="GetContractDataToDisplay">
              <xsl:with-param name="pContract" select="$pRowData/DERIVATIVECONTRACT/@data"/>
              <xsl:with-param name="pContractRepository" select="$gDataDocumentRepository/derivativeContract[identifier=$pRowData/DERIVATIVECONTRACT/@data]"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== RecapCashFlowsTitle ===== -->
      <xsl:when test="$pColumnName='TS-RecapCashFlowsTitle'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'ALLOC-RecapFlow'" />
        </xsl:call-template>

        <xsl:choose>
          <xsl:when test="$pRowData/@cuDesc != ''">
            <xsl:value-of select="concat(' ',$pRowData/@cuDesc)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pRowData/@currency" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FxRate ===== -->
      <xsl:when test="$pColumnName='TS-FxRate'">
        <xsl:variable name="vFxRate">
          <xsl:if test="$pRowData/@fxRate">
            <xsl:value-of select="$pRowData/@fxRate"/>
          </xsl:if>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="string-length($vFxRate) = 0 or $pRowData/@currency = $gReportingCurrency">
            <xsl:value-of select="''"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-FxRateTitle'" />
            </xsl:call-template>
            <xsl:value-of select="concat($gcEspace,$gReportingCurrency,'/',$pRowData/@currency,':',$gcEspace)" />
            <xsl:choose>
              <xsl:when test="number($vFxRate) = number(-1)">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="number($vFxRate)" />
                  <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- ===== FxRate2 ===== -->
      <xsl:when test="$pColumnName='Report-FxRate'">
        <xsl:choose>
          <xsl:when test="$pRowData/@currency = $gReportingCurrency"/>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SpotRate'" />
            </xsl:call-template>
            <xsl:value-of select="$gcEspace"/>

            <xsl:variable name="vExchangeRateNode">
              <xsl:call-template name="GetExchangeRate_Repository">
                <xsl:with-param name="pFlowCcy" select="$pRowData/@currency" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vExchangeRate" select="msxsl:node-set($vExchangeRateNode)/fxrate"/>
            <xsl:choose>
              <xsl:when test="$vExchangeRate/rate">
                <xsl:call-template name="DisplayCurrencyPair">
                  <xsl:with-param name="pQuotedCurrencyPair" select="$vExchangeRate/quotedCurrencyPair"/>
                </xsl:call-template>
                <xsl:value-of select="concat(':',$gcEspace)" />
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="number($vExchangeRate/rate)" />
                  <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="concat($gReportingCurrency,'/',$pRowData/@currency,':',$gcEspace)"/>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- ===== TrdType ===== -->
      <xsl:when test="$pColumnName='TS-TradeType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vTrdTypeEnum" select="$gDataDocumentRepository/enums[@id='ENUMS.CODE.TrdTypeEnum']/enum[value/text()=$pRowData/@trdTyp]"/>
            <xsl:choose>
              <xsl:when test="string-length($vTrdTypeEnum/extattrb/text())>0">
                <xsl:value-of select="$vTrdTypeEnum/extattrb/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vTrdTypeEnum/value/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== SecondaryTrdType ===== -->
      <xsl:when test="$pColumnName='TS-SecondaryTradeType'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vSecondaryTrdTypeEnum" select="$gDataDocumentRepository/enums[@id='ENUMS.CODE.TrdTypeEnum']/enum[value/text()=$pRowData/@trdTyp2]"/>
            <xsl:choose>
              <xsl:when test="string-length($vSecondaryTrdTypeEnum/extattrb/text())>0">
                <xsl:value-of select="$vSecondaryTrdTypeEnum/extattrb/text()" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vSecondaryTrdTypeEnum/value/text()" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Maturity ===== -->
      <xsl:when test="$pColumnName='TS-Maturity'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vMMY" select="$pRowData/mmy/@data" />
            <xsl:variable name="vDay" select="substring($vMMY , 7)" />
            <xsl:variable name="vMaturity">
              <xsl:call-template name="format-shortdate_ddMMMyy">
                <xsl:with-param name="year" select="substring($vMMY , 1, 4)" />
                <xsl:with-param name="month" select="substring($vMMY , 5, 2)" />
                <xsl:with-param name="day" select="$vDay" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test="string-length($vDay)=0">
                <xsl:call-template name="FirstUCase">
                  <xsl:with-param name="source" select="$vMaturity" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vMaturity"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== PutCall ===== -->
      <xsl:when test="$pColumnName='TS-PutCall'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:value-of select="$pRowData/putCall/@data"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Strike ===== -->
      <xsl:when test="$pColumnName='TS-Strike'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/strkPx/@data) >0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="concat($pRowData/strkPx/@data,'')" />
                <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Converted Strike, node convertedStrkPx ===== -->
      <xsl:when test="$pColumnName='TS-ConvertedStrike'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:copy-of select="$pRowData/convertedStrkPx"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Price ===== -->
      <xsl:when test="$pColumnName='TS-Price'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="concat($pRowData/@lastPx,'')" />
              <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Converted Price, node convertedLastPx ===== -->
      <xsl:when test="$pColumnName='TS-ConvertedPrice'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:copy-of select="$pRowData/convertedLastPx" />
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== PriceCu ===== -->
      <xsl:when test="$pColumnName='TS-PriceCu'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:variable name="vDCGlobal" select="string($pRowData/DERIVATIVECONTRACT/@data)" />
            <xsl:value-of select="string($gDataDocumentRepository/derivativeContract[identifier=$vDCGlobal]/idC_Price/text())"/>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Premium ===== -->
      <xsl:when test="$pColumnName='TS-Premium'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/prm/@amount) > 0">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pRowData/prm/@amount" />
                <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Premium DrCr===== -->
      <xsl:when test="$pColumnName='TS-PremiumDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:if test="string-length($pRowData/prm/@amount) > 0 and 
                    number($pRowData/prm/@amount) > 0">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pAmountSens" select="$pRowData/prm/@pr" />
              </xsl:call-template>
            </xsl:if>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Brokerage ===== -->
      <xsl:when test="$pColumnName='TS-Brokerage'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus//fees/payment[@paymentType=$gcBro]/@status = $gcNOkStatus">
                <xsl:value-of select="$gcNOkStatus"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBro]/@amountET) > 0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcBro]/@amountET" />
                    <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Brokerage DrCr===== -->
      <xsl:when test="$pColumnName='TS-BrokerageDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcBro]/@status = $gcNOkStatus"/>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBro]/@amountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcBro]/@amountET) > 0">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcBro]/@pr" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokerageTrd ===== -->
      <xsl:when test="$pColumnName='TS-BrokerageTrd'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcBroTrd]/@status = $gcNOkStatus">
                <xsl:value-of select="$gcNOkStatus"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroTrd]/@amountET) > 0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcBroTrd]/@amountET" />
                    <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokerageTrd DrCr===== -->
      <xsl:when test="$pColumnName='TS-BrokerageTrdDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus//fees/payment[@paymentType=$gcBroTrd]/@status = $gcNOkStatus"/>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroTrd]/@amountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcBroTrd]/@amountET) > 0">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcBroTrd]/@pr" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokerageClr ===== -->
      <xsl:when test="$pColumnName='TS-BrokerageClr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus//fees/payment[@paymentType=$gcBroClr]/@status = $gcNOkStatus">
                <xsl:value-of select="$gcNOkStatus"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroClr]/@amountET) > 0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcBroClr]/@amountET" />
                    <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== BrokerageClr DrCr ===== -->
      <xsl:when test="$pColumnName='TS-BrokerageClrDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcBroClr]/@status = $gcNOkStatus"/>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcBroClr]/@amountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcBroClr]/@amountET) > 0">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcBroClr]/@pr" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Fee ===== -->
      <xsl:when test="$pColumnName='TS-Fee'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcFee]/@status = $gcNOkStatus">
                <xsl:value-of select="$gcNOkStatus"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFee]/@amountET) > 0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcFee]/@amountET" />
                    <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== Fee DrCr ===== -->
      <xsl:when test="$pColumnName='TS-FeeDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus//fees/payment[@paymentType=$gcFee]/@status = $gcNOkStatus"/>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFee]/@amountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcFee]/@amountET) > 0">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcFee]/@pr" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeeTrd ===== -->
      <xsl:when test="$pColumnName='TS-FeeTrd'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcFeeTrd]/@status = $gcNOkStatus">
                <xsl:value-of select="$gcNOkStatus"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeTrd]/@amountET) > 0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcFeeTrd]/@amountET" />
                    <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeeTrd DrCr ===== -->
      <xsl:when test="$pColumnName='TS-FeeTrdDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcFeeTrd]/@status = $gcNOkStatus"/>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeTrd]/@amountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcFeeTrd]/@amountET) > 0">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcFeeTrd]/@pr" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeeClr ===== -->
      <xsl:when test="$pColumnName='TS-FeeClr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcFeeClr]/@status = $gcNOkStatus">
                <xsl:value-of select="$gcNOkStatus"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeClr]/@amountET) > 0">
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="$pRowData//fees/payment[@paymentType=$gcFeeClr]/@amountET" />
                    <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
      <!-- ===== FeeClr DrCr ===== -->
      <xsl:when test="$pColumnName='TS-FeeClrDrCr'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pTradeWithStatus/fees/payment[@paymentType=$gcFeeClr]/@status = $gcNOkStatus"/>
              <xsl:otherwise>
                <xsl:if test="string-length($pRowData//fees/payment[@paymentType=$gcFeeClr]/@amountET) > 0 and 
                    number($pRowData//fees/payment[@paymentType=$gcFeeClr]/@amountET) > 0">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pAmountSens" select="$pRowData//fees/payment[@paymentType=$gcFeeClr]/@pr" />
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()"/>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!-- ===== GetCommonColumnIsAltData ===== -->
  <xsl:template name="GetCommonColumnIsAltData">
    <xsl:param name="pColumnName" />
    <xsl:param name="pRowData" />
    <xsl:param name="pIsGetDetail" select="false()" />

    <xsl:choose>
      <!-- ===== Stgy ===== -->
      <xsl:when test="$pColumnName='TS-Stgy'">
        <xsl:choose>
          <xsl:when test="$pIsGetDetail = false()">
            <xsl:choose>
              <xsl:when test="$pRowData/stgy">
                <xsl:value-of select="false()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="true()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$pIsGetDetail = true()">
            <xsl:value-of select="false()"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="false()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:variable name="vValue">
      <xsl:call-template name="GetSpecificColumnDynamicData">
        <xsl:with-param name="pValue" select="$pValue"/>
        <xsl:with-param name="pRowData" select="$pRowData"/>
        <xsl:with-param name="pParentRowData" select="$pParentRowData"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="GetComonColumnDynamicData">
      <xsl:with-param name="pValue" select="$vValue"/>
      <xsl:with-param name="pRowData" select="$pRowData"/>
      <xsl:with-param name="pParentRowData" select="$pParentRowData"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ===== GetContractDataToDisplay ===== -->
  <xsl:template name="GetContractDataToDisplay">
    <xsl:param name="pContract"/>
    <xsl:param name="pContractRepository"/>
    <xsl:param name="pIsShort" select="true()"/>

    <xsl:choose>
      <xsl:when test="$pContractRepository">

        <xsl:value-of select="concat($pContractRepository/identifier,' - ', $pContractRepository/displayname)"/>

        <xsl:if test="$pIsShort=false()">
          <xsl:variable name="vIdc">
            <xsl:if test="string-length($pContractRepository/idC_Nominal) > 0">
              <xsl:value-of select="concat(' - ', $pContractRepository/idC_Nominal)"/>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="vContractDen">
            <xsl:if test="(string-length($pContractRepository/instrumentDen) > 0) and (string($pContractRepository/instrumentDen) != string('100'))">
              <xsl:variable name="vContractDenRes">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-INSTRUMENTDEN'" />
                </xsl:call-template>
              </xsl:variable>
              <!--//-->
              <xsl:value-of select="concat(' - ', $vContractDenRes, ' : 1/', $pContractRepository/instrumentDen)"/>
            </xsl:if>
          </xsl:variable>
          <xsl:variable name="vContractMltp">
            <xsl:if test="string-length($pContractRepository/contractMultiplier) > 0">
              <xsl:variable name="vContractMltpRes">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-CONTRACTMULTIPLIER'" />
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name="vContractMltpValue">
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="$pContractRepository/contractMultiplier" />
                  <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                </xsl:call-template>
              </xsl:variable>
              <!--//-->
              <xsl:value-of select="concat(' - ', $vContractMltpRes, ' : ', $vContractMltpValue)"/>
            </xsl:if>
          </xsl:variable>
          <xsl:value-of select="concat($vIdc, $vContractMltp, $vContractDen)"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pContract"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--GetSpecificColumnDynamicData-->
  <xsl:template name="GetSpecificColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:value-of select="$pValue"/>
  </xsl:template>

  <!--GetPaymentSubTotalData-->
  <xsl:template name="GetPaymentSubTotalData">
    <xsl:param name="pPayment"/>

    <xsl:choose>
      <xsl:when test="$pPayment/@status = $gcNOkStatus">
        <xsl:value-of select="$gcEspace"/>
      </xsl:when>
      <xsl:when test="string-length($pPayment/@amountET) > 0">
        <xsl:call-template name="format-number">
          <xsl:with-param name="pAmount" select="$pPayment/@amountET" />
          <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!--GetPaymentSubTotalDrCr-->
  <xsl:template name="GetPaymentSubTotalDrCr">
    <xsl:param name="pPayment"/>

    <xsl:choose>
      <xsl:when test="$pPayment/@status = $gcNOkStatus">
        <xsl:value-of select="$gcEspace"/>
      </xsl:when>
      <xsl:when test="string-length($pPayment/@amountET) > 0">
        <xsl:call-template name="AmountSufix">
          <xsl:with-param name="pAmountSens" select="$pPayment/@pr" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
