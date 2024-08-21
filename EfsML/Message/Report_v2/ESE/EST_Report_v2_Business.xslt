<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - Shared - Common variables for all reports                           -->
  <!-- File    : \Report_v2\Shared\Shared_Report_v2_Business.xslt                                     -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.0.5738                                                                            -->
  <!-- Date    : 20150917                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [21309] Model dynamique d'affichage de la section Purchaes & Sales                   -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140905                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gPurchaseSaleModel_EST">
    <xsl:variable name="vPurchaseSaleModel" select="$gBlockSettings_Section/section[@key=$gcReportSectionKey_PSS]/product[@name='EST']/@model"/>
    <xsl:choose>
      <xsl:when test="string-length($vPurchaseSaleModel) > 0">
        <xsl:value-of select="$vPurchaseSaleModel"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gPurchaseSaleModel"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->

  <!-- ................................................ -->
  <!-- BizEST_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for equitySecurityTransaction Trades Confirmations sections
       ................................................ -->
  <xsl:template match="trades" mode="BizEST_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates/predicate/@trdTyp]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade[@trdTyp=$vSubTotalList/predicates/predicate/@trdTyp]"/>

    <xsl:call-template name="BizEST_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizEST_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for equitySecurityTransaction Unsettled and Settled Trades sections
       ................................................ -->
  <xsl:template match="unsettledTrades|settledTrades" mode="BizEST_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates=false()]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade"/>

    <xsl:call-template name="BizEST_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizEST_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for equitySecurityTransaction Open Positions sections
       ................................................ -->
  <xsl:template match="posTrades|stlPosTrades" mode="BizEST_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:call-template name="BizEST_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="current()/trade"/>
      <xsl:with-param name="pSubTotal" select="current()/subTotal"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizEST_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for equitySecurityTransaction Trades Confirmations and Open Positions sections
       ................................................ -->
  <!-- FI 20151019 [21317] Modify -->
  <xsl:template name="BizEST_TradeSubTotal">
    <xsl:param name="pEfsmlTrade"/>
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pSectionKey"/>

    <!--Un trade ETD contient l'élément exchangeTradedDerivative-->
    <xsl:variable name="vESTTradeList" select="$gTrade[@tradeId=$pEfsmlTrade/@tradeId and equitySecurityTransaction]"/>
    <xsl:variable name="vESTSubTotalList" select="$pSubTotal[@assetCategory='EquityAsset' and @idI=$vESTTradeList/equitySecurityTransaction/productType/@OTCmlId]"/>

    <xsl:if test="$vESTTradeList">
      <xsl:variable name ="vBizDt" select="current()/@bizDt"/>

      <date bizDt="{$vBizDt}">
        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <xsl:variable name="vMarketRepository-equity" select="$gRepository/equity[idM=$vRepository-market/@OTCmlId]"/>
          <xsl:variable name="vMarketESTTradeList" select="$vESTTradeList[
                        equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@Exch=$vRepository-market/fixml_SecurityExchange and
                        equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vMarketRepository-equity/@OTCmlId]"/>

          <xsl:if test="$vMarketESTTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketEfsmlTradeList" select="$pEfsmlTrade[@tradeId=$vMarketESTTradeList/@tradeId]"/>
              <xsl:variable name="vMarketESTSubTotalList" select="$vESTSubTotalList[@idAsset=$vMarketRepository-equity/@OTCmlId]"/>
              <xsl:variable name="vMarketESTSubTotalCopyNode">
                <xsl:copy-of select="$vMarketESTSubTotalList"/>
              </xsl:variable>
              <xsl:variable name="vMarketESTSubTotalCopy" select="msxsl:node-set($vMarketESTSubTotalCopyNode)/subTotal"/>
              <xsl:variable name="vSubTotal-idB_List" select="$vMarketESTSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idB',@idB))]"/>

              <xsl:for-each select="$vSubTotal-idB_List">

                <xsl:variable name="vSubTotal-idB" select="current()"/>
                <xsl:variable name="vBookESTTradeList" select="$vMarketESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Acct=$gRepository/book[@OTCmlId=$vSubTotal-idB/@idB]/identifier]"/>

                <xsl:if test="$vBookESTTradeList">

                  <book OTCmlId="{$vSubTotal-idB/@idB}">
                    <xsl:if test="string-length($pSectionKey) > 0">
                      <xsl:attribute name="sectionKey">
                        <xsl:value-of select="$pSectionKey"/>
                      </xsl:attribute>
                    </xsl:if>

                    <xsl:variable name="vBookEfsmlTradeList" select="$vMarketEfsmlTradeList[@tradeId=$vBookESTTradeList/@tradeId]"/>
                    <xsl:variable name="vBookESTSubTotalList" select="$vMarketESTSubTotalList[@idB=$vSubTotal-idB/@idB]"/>
                    <xsl:variable name="vBookESTSubTotalCopyNode">
                      <xsl:copy-of select="$vBookESTSubTotalList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookESTSubTotalCopy" select="msxsl:node-set($vBookESTSubTotalCopyNode)/subTotal"/>
                    <xsl:variable name="vSubTotal-Instr_List" select="$vBookESTSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idI',@idI))]"/>

                    <xsl:for-each select="$vSubTotal-Instr_List">

                      <xsl:variable name="vSubTotal-idI" select="current()"/>
                      <xsl:variable name="vInstrESTTradeList" select="$vBookESTTradeList[equitySecurityTransaction/productType/@OTCmlId=$vSubTotal-idI/@idI]"/>

                      <xsl:if test="$vInstrESTTradeList">

                        <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrESTTradeList/@tradeId]"/>
                        <xsl:variable name="vInstrESTSubTotalList" select="$vBookESTSubTotalList[@idI=$vSubTotal-idI/@idI]"/>
                        <xsl:variable name="vInstrESTSubTotalCopyNode">
                          <xsl:copy-of select="$vInstrESTSubTotalList"/>
                        </xsl:variable>
                        <xsl:variable name="vInstrESTSubTotalCopy" select="msxsl:node-set($vInstrESTSubTotalCopyNode)/subTotal"/>
                        <xsl:variable name="vSubTotal-Asset_List" select="$vInstrESTSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idAsset',@idAsset))]"/>

                        <xsl:for-each select="$vSubTotal-Asset_List">

                          <xsl:variable name="vSubTotal-Asset" select="current()"/>
                          <xsl:variable name="vAssetESTTradeList" select="$vInstrESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vSubTotal-Asset/@idAsset]"/>

                          <xsl:if test="$vAssetESTTradeList">

                            <xsl:variable name="vAssetEfsmlTradeList" select="$vInstrEfsmlTradeList[@tradeId=$vAssetESTTradeList/@tradeId]"/>
                            <xsl:variable name="vAsset" select="$vMarketRepository-equity[@OTCmlId=$vSubTotal-Asset/@idAsset]"/>
                            <xsl:variable name="vCcy" select="$vAsset/idC/text()"/>
                            <xsl:variable name="vAssetSubTotalList" select="$vInstrESTSubTotalList[@assetCategory=$vSubTotal-Asset/@assetCategory and @idAsset=$vSubTotal-Asset/@idAsset]"/>

                            <asset OTCmlId="{$vSubTotal-Asset/@idAsset}"
                                   assetCategory="{$vSubTotal-Asset/@assetCategory}" assetName="equity"
                                   family="ESE" idI="{$vSubTotal-idI/@idI}"
                                   ccy="{$vCcy}">

                              <!-- Recherche du prix de clôture de l'asset. 
                                   Recherche du nbr de jour des intérêts courus (diff entre date Business et dernière tombée de coupon)
                                   Ces infos  sont présentes sur les trades en position -->
                                <!-- EG 20190730 AssetMeasure -->
                              <xsl:if test ="$pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_POS or $gcReportSectionKey_STL">
                                <xsl:variable name ="vFirstTradePos" select="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vAssetESTTradeList/@tradeId][1]"/>
                                <xsl:if test ="$vFirstTradePos">
                                  <closing>
                                    <xsl:if test ="$vFirstTradePos/ain and string-length($vFirstTradePos/@meaClrPx)>0">
                                      <xsl:attribute name ="meaClrPx">
                                        <xsl:value-of select ="$vFirstTradePos/@meaClrPx"/>
                                      </xsl:attribute>
                                    </xsl:if>
                                    <xsl:attribute name ="clrPx">
                                      <xsl:value-of select ="$vFirstTradePos/@clrPx"/>
                                    </xsl:attribute>
                                  </closing>
                                </xsl:if>
                              </xsl:if>

                              <xsl:choose>
                                <!-- Lecture des prix sur PosTrades pour obtenir le Pattern associé à fmtClrPx 
                                Les élements trade enfants de stlPosaction et unsettledTrades ne possèdent pas l'attribut fmtClrPx -->
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_POS or $gcReportSectionKey_STL">
                                  <xsl:call-template name="Business_GetPattern">
                                    <xsl:with-param name="pCcy" select="$vCcy"/>
                                    <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                    <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetESTTradeList/@tradeId]/@fmtLastPx"/>
                                    <xsl:with-param name="pFmtClrPx" select="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vAssetESTTradeList/@tradeId]/@fmtClrPx"/>
                                    <xsl:with-param name="pFmtAvgPx" select="$vAssetSubTotalList/long/@fmtAvgPx | $vAssetSubTotalList/short/@fmtAvgPx"/>
                                  </xsl:call-template>
                                </xsl:when>
                                <xsl:otherwise>
                                  <!--Remarque l'attribut fmtClrPx n'existe uniquement pour la section gcReportSectionKey_POS (Position détaillée en date business) -->
                                  <xsl:call-template name="Business_GetPattern">
                                    <xsl:with-param name="pCcy" select="$vCcy"/>
                                    <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                    <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetESTTradeList/@tradeId]/@fmtLastPx"/>
                                    <xsl:with-param name="pFmtClrPx" select="$vAssetEfsmlTradeList/@fmtClrPx"/>
                                    <xsl:with-param name="pFmtAvgPx" select="$vAssetSubTotalList/long/@fmtAvgPx | $vAssetSubTotalList/short/@fmtAvgPx"/>
                                  </xsl:call-template>
                                </xsl:otherwise>
                              </xsl:choose>


                              <xsl:for-each select="$vAssetEfsmlTradeList">

                                <xsl:variable name="vEfsmlTrade" select="current()"/>
                                <xsl:variable name="vESTTrade" select="$vAssetESTTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                <!-- FI 20150716 [20892] GLOP Ne faudrait-il pas mieux  alimenter ces données ici -->
                                <!--<xsl:variable name="clrPx">
                                    <xsl:value-of select ="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vEfsmlTrade/@tradeId]/@clrPx"></xsl:value-of>
                                  </xsl:variable>
                                -->
                                <trade trdNum="{$vEfsmlTrade/@tradeId}" tradeId="{$vEfsmlTrade/@tradeId}"
                                       lastPx="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                       trdDt="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                       stlDt="{$vCommonTrade/@stlDt}" >

                                  <!-- FI 20151019 [21317] add amount -->
                                  <xsl:choose>
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or $pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_SET">
                                      <amount>
                                        <!--amount1 (montant affiché sur la 1er ligne) -->
                                        <amount1 det="GAM" withside="1">
                                          <xsl:copy-of select="$vEfsmlTrade/gam"/>
                                        </amount1>

                                        <!--amount2 (montant affiché sur la 2nd ligne) -->
                                        <xsl:choose>
                                          <xsl:when test ="$vEfsmlTrade/net">
                                            <amount2 det="NET" withside="1">
                                              <xsl:copy-of select="$vEfsmlTrade/net"/>
                                            </amount2>
                                          </xsl:when>
                                          <xsl:when test ="$pSectionKey=$gcReportSectionKey_UNS">
                                            <xsl:if test ="$vEfsmlTrade/umg">
                                              <amount2 det="OTE" withside="1" >
                                                <xsl:copy-of select="$vEfsmlTrade/umg"/>
                                              </amount2>
                                            </xsl:if>
                                          </xsl:when>
                                        </xsl:choose>

                                        <!--amount3 (montant affiché sur la 3ème ligne) -->
                                        <xsl:choose>
                                          <xsl:when test ="$vAssetEfsmlTradeList/net and $pSectionKey=$gcReportSectionKey_UNS">
                                            <xsl:if test ="$vEfsmlTrade/umg">
                                              <amount3 det="OTE" withside="1" >
                                                <xsl:copy-of select="$vEfsmlTrade/umg"/>
                                              </amount3>
                                            </xsl:if>
                                          </xsl:when>
                                        </xsl:choose>

                                      </amount>
                                    </xsl:when>
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS or $pSectionKey=$gcReportSectionKey_STL">
                                      <xsl:variable name ="vPosTrade" select ="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                      <xsl:if test ="$vPosTrade">
                                        <amount>
                                          <!--amount1 (montant affiché sur la 1er ligne) -->
                                          <amount1 det="OPV" withside="1" >
                                            <xsl:copy-of select="$vPosTrade/mkv"/>
                                          </amount1>
                                        </amount>
                                      </xsl:if>
                                    </xsl:when>
                                  </xsl:choose>
                                  
                                  <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
                                  <xsl:call-template name="Business_GetFee">
                                    <xsl:with-param name="pFee" select="$vEfsmlTrade/fee"/>
                                  </xsl:call-template>
                                  
                                </trade>
                              </xsl:for-each>

                              <xsl:call-template name="Business_GetFeeSubtotal">
                                <xsl:with-param name="pFee" select="$vAssetEfsmlTradeList/fee"/>
                              </xsl:call-template>

                              <!-- FI 20151019 [21317] add subTotalAmount -->
                              <xsl:choose>
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or $pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_SET">
                                  <subTotalAmount>
                                    <!-- amount1 -->
                                    <amount1 det="GAM" withside="1" >
                                      <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/gam[1]/@ccy"/>
                                      <xsl:variable name="vTotal">
                                        <xsl:call-template name="GetAmount-amt">
                                          <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/gam"/>
                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                        </xsl:call-template>
                                      </xsl:variable>
                                      <gam amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                    </amount1>

                                    <!-- amount2 -->
                                    <xsl:choose>
                                      <xsl:when test ="$vAssetEfsmlTradeList/net">
                                        <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/net[1]/@ccy"/>
                                        <amount2 det="NET" withside="1" >
                                          <xsl:variable name="vTotal">
                                            <xsl:call-template name="GetAmount-amt">
                                              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/net"/>
                                              <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                            </xsl:call-template>
                                          </xsl:variable>
                                          <net amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                        </amount2>
                                      </xsl:when>
                                      <xsl:when test ="$pSectionKey=$gcReportSectionKey_UNS">
                                        <xsl:if test ="$vAssetEfsmlTradeList/umg">
                                          <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/umg[1]/@ccy"/>
                                          <amount2 det="OTE" withside="1" >
                                            <xsl:variable name="vTotal">
                                              <xsl:call-template name="GetAmount-amt">
                                                <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                                                <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                              </xsl:call-template>
                                            </xsl:variable>
                                            <umg amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                          </amount2>
                                        </xsl:if>
                                      </xsl:when>
                                    </xsl:choose>

                                    <!-- amount3 -->
                                    <xsl:choose>
                                      <xsl:when test ="$vAssetEfsmlTradeList/net and $pSectionKey=$gcReportSectionKey_UNS">
                                        <xsl:if test ="$vAssetEfsmlTradeList/umg">
                                          <!--amount3 (montant affiché sur la 3ème ligne) -->
                                          <amount3 det="OTE" withside="1">
                                            <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/umg[1]/@ccy"/>
                                            <xsl:variable name="vTotal">
                                              <xsl:call-template name="GetAmount-amt">
                                                <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                                                <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                              </xsl:call-template>
                                            </xsl:variable>
                                            <umg amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                          </amount3>
                                        </xsl:if>
                                      </xsl:when>
                                    </xsl:choose>
                                  </subTotalAmount>
                                </xsl:when>
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS or $pSectionKey=$gcReportSectionKey_STL">
                                  <subTotalAmount>
                                    <xsl:variable name ="vPosTradeLst" select ="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vAssetEfsmlTradeList/@tradeId]"/>
                                    <amount1 det="MKV" withside="1" >
                                      <xsl:variable name ="vTotalCcy" select ="$vPosTradeLst/mkv[1]/@ccy"/>
                                      <xsl:variable name="vTotal">
                                        <xsl:call-template name="GetAmount-amt">
                                          <xsl:with-param name="pAmount" select="$vPosTradeLst/mkv"/>
                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                        </xsl:call-template>
                                      </xsl:variable>
                                      <mkv amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                    </amount1>
                                  </subTotalAmount>
                                </xsl:when>
                              </xsl:choose>
                            </asset>
                          </xsl:if>
                        </xsl:for-each>
                      </xsl:if>
                    </xsl:for-each>
                  </book>
                </xsl:if>
              </xsl:for-each>
            </market>
          </xsl:if>
        </xsl:for-each>
      </date>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Purchase & Sale                                               -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizEST_PurchaseSale                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for CFDEquity Purchase&Sale section
       ................................................ -->
  <xsl:template match="posActions|stlPosActions" mode="BizEST_PurchaseSales">
    <xsl:param name="pSectionKey"/>

    <xsl:call-template name="BizEST_PurchaseSales">
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizEST_PurchaseSale                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for returnSwap Purchase&Sale section
        - UNCLEARING: Compensation générée suite à une décompensation partielle 
        - CLEARSPEC : Compensation  Spécifique 
        - CLEARBULK : Compensation globale 
        - CLEAREOD : Compensation fin de journée 
        - ENTRY : Clôture STP 
        - UPDENTRY : Clôture Fin de journée 
        - Hors Décompensation 
       ................................................ -->
  <xsl:template name="BizEST_PurchaseSales">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vPosActionList" select="current()/posAction[
                  contains(',UNCLEARING,CLEARSPEC,CLEARBULK,CLEAREOD,ENTRY,UPDENTRY,',concat(',',@requestType,',')) and @dtUnclearing = false()]"/>

    <!--Un trade ETD contient l'élément exchangeTradedDerivative-->
    <xsl:variable name="vESTTradeList" select="$gTrade[@tradeId=$vPosActionList/trades/trade/@tradeId and equitySecurityTransaction]"/>

    <xsl:if test="$vESTTradeList">

      <date bizDt="{current()/@bizDt}">
        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <xsl:variable name="vMarketRepository-equity" select="$gRepository/equity[idM=$vRepository-market/@OTCmlId]"/>
          <xsl:variable name="vMarketESTTradeList" select="$vESTTradeList[
                    equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@Exch=$vRepository-market/fixml_SecurityExchange and
                    equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vMarketRepository-equity/@OTCmlId]"/>


          <xsl:if test="$vMarketESTTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketPosActionList" select="$vPosActionList[trades/trade/@tradeId=$vMarketESTTradeList/@tradeId]"/>
              <xsl:variable name="vMarketESTTrade2List" select="$gTrade[@tradeId=$vMarketPosActionList/trades/trade2/@tradeId and equitySecurityTransaction]"/>
              <xsl:variable name="vMarketAllESTTradeList" select="$vMarketESTTradeList | $vMarketESTTrade2List"/>

              <xsl:variable name="vMarketESTTradeListCopyNode">
                <xsl:copy-of select="$vMarketESTTradeList"/>
              </xsl:variable>
              <xsl:variable name="vMarketESTTradeListCopy" select="msxsl:node-set($vMarketESTTradeListCopyNode)/trade"/>
              <xsl:variable name="vRptSide-Acct_List" select="$vMarketESTTradeListCopy/equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

              <xsl:for-each select="$vRptSide-Acct_List">

                <xsl:variable name="vRptSide-Acct" select="current()"/>
                <xsl:variable name="vBookESTTradeList" select="$vMarketESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

                <xsl:if test="$vBookESTTradeList">

                  <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}" sectionKey="{$pSectionKey}">

                    <xsl:variable name="vBookESTTrade2List" select="$vMarketESTTrade2List[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>
                    <xsl:variable name="vBookAllESTTradeList" select="$vMarketAllESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>
                    <xsl:variable name="vBookPosActionList" select="$vMarketPosActionList[trades/trade/@tradeId=$vBookESTTradeList/@tradeId]"/>

                    <xsl:variable name="vBookESTTradeListtCopyNode">
                      <xsl:copy-of select="$vBookESTTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookESTTradeListtCopy" select="msxsl:node-set($vBookESTTradeListtCopyNode)/trade"/>

                    <xsl:variable name="vInstrmt-ID_List" select="$vBookESTTradeListtCopy/equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt[generate-id()=generate-id(key('kInstrmt-ID',@ID))]"/>

                    <xsl:for-each select="$vInstrmt-ID_List">

                      <xsl:variable name="vInstrmt-ID" select="current()"/>
                      <xsl:variable name="vAssetESTTradeList" select="$vBookESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vInstrmt-ID/@ID]"/>
                      <xsl:variable name="vAssetESTCommonTradeList" select="$gCommonData/trade[@tradeId=$vAssetESTTradeList/@tradeId]"/>

                      <xsl:if test="$vAssetESTTradeList">

                        <xsl:variable name="vAsset" select="$gRepository/equity[@OTCmlId=$vInstrmt-ID/@ID]"/>
                        <xsl:variable name="vAssetESTTrade2List" select="$vBookESTTrade2List[equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vAsset/@OTCmlId]"/>
                        <xsl:variable name="vAssetAllESTTradeList" select="$vBookAllESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vAsset/@OTCmlId]"/>
                        <xsl:variable name="vAssetPosActionList" select="$vBookPosActionList[trades/trade/@tradeId=$vAssetESTTradeList/@tradeId]"/>

                        <xsl:variable name="vCcy" select="$vAsset/idC/text()"/>

                        <asset OTCmlId="{$vInstrmt-ID/@ID}"
                               assetCategory="EquityAsset" assetName="equity"
                               family="ESE" idI="{$vAssetESTTradeList[1]/equitySecurityTransaction/productType/@OTCmlId}"
                               ccy="{$vCcy}">

                          <xsl:call-template name="Business_GetPattern">
                            <xsl:with-param name="pCcy" select="$vCcy"/>
                            <xsl:with-param name="pFmtQty" select="$vAssetPosActionList/@fmtQty"/>
                            <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetAllESTTradeList/@tradeId]/@fmtLastPx"/>
                          </xsl:call-template>

                          <xsl:variable name="vAssetPosActionListCopyNode">
                            <xsl:copy-of select="$vAssetPosActionList"/>
                          </xsl:variable>
                          <xsl:variable name="vAssetPosActionListCopy" select="msxsl:node-set($vAssetPosActionListCopyNode)/posAction"/>

                          <xsl:choose>
                            <xsl:when test="$gPurchaseSaleModel_EST = $gcPurchaseSaleOverallQty">

                              <xsl:variable name="vAssetAllTradeListCopy" select="$vAssetPosActionListCopy/trades/trade | $vAssetPosActionListCopy/trades/trade2"/>
                              <xsl:variable name="vTrade-TradeId_List" select="$vAssetAllTradeListCopy[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                              <xsl:for-each select="$vTrade-TradeId_List">

                                <xsl:variable name="vTrade-TradeId" select="current()"/>
                                <xsl:variable name="vESTTrade" select="$vAssetAllESTTradeList[@tradeId=$vTrade-TradeId/@tradeId]"/>
                                <xsl:variable name="vTradePosActionList" select="$vAssetPosActionList[trades/trade/@tradeId = $vTrade-TradeId/@tradeId or trades/trade2/@tradeId = $vTrade-TradeId/@tradeId]"/>

                                <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vTrade-TradeId/@tradeId]"/>

                                <trade tradeId="{$vTrade-TradeId/@tradeId}"
                                       fmtQty="{sum($vTradePosActionList/@fmtQty)}"
                                       lastPx="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                       trdDt="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                       stlDt="{$vCommonTrade/@stlDt}"/>
                              </xsl:for-each>
                            </xsl:when>
                            <xsl:when test="$gPurchaseSaleModel_EST = $gcPurchaseSalePnLOnClosing">

                              <xsl:variable name="vClosedTrade-TradeId_List" select="$vAssetPosActionListCopy/trades/trade2[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                              <xsl:for-each select="$vClosedTrade-TradeId_List">

                                <xsl:variable name="vClosedTrade-TradeId" select="current()"/>
                                <xsl:variable name="vClosedESTTrade" select="$vAssetESTTradeList[@tradeId=$vClosedTrade-TradeId/@tradeId]"/>
                                <xsl:variable name="vClosedTradePosActionList" select="$vAssetPosActionList[trades/trade2/@tradeId = $vClosedTrade-TradeId/@tradeId]"/>

                                <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vClosedTrade-TradeId/@tradeId]"/>

                                <trade isClosed="true()"
                                       closedTradeId="{$vClosedTrade-TradeId/@tradeId}"
                                       closedLastPx="{$vClosedESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                       closedTrdDt="{$vClosedESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                       tradeId="{$vClosedTrade-TradeId/@tradeId}"
                                       fmtQty="{sum($vClosedTradePosActionList/@fmtQty)}"
                                       lastPx="{$vClosedESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                       trdDt="{$vClosedESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                       stlDt="{$vCommonTrade/@stlDt}"/>

                                <xsl:variable name="vClosedTradePosActionListCopyNode">
                                  <xsl:copy-of select="$vClosedTradePosActionList"/>
                                </xsl:variable>
                                <xsl:variable name="vClosedTradePosActionListCopy" select="msxsl:node-set($vClosedTradePosActionListCopyNode)/posAction"/>

                                <xsl:variable name="vClosingTrade-TradeId_List" select="$vClosedTradePosActionListCopy/trades/trade[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                                <xsl:for-each select="$vClosingTrade-TradeId_List">

                                  <xsl:variable name="vClosingTrade-TradeId" select="current()"/>
                                  <xsl:variable name="vClosingESTTrade" select="$vAssetESTTradeList[@tradeId=$vClosingTrade-TradeId/@tradeId]"/>
                                  <xsl:variable name="vClosingTradePosActionList" select="$vClosedTradePosActionList[trades/trade/@tradeId = $vClosingTrade-TradeId/@tradeId]"/>

                                  <xsl:variable name="vCommonClosingTrade" select="$gCommonData/trade[@tradeId=$vClosingTrade-TradeId/@tradeId]"/>

                                  <trade isClosed="false()"
                                         closedTradeId="{$vClosedTrade-TradeId/@tradeId}"
                                         closedLastPx="{$vClosedESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                         closedTrdDt="{$vClosedESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                         tradeId="{$vClosingTrade-TradeId/@tradeId}"
                                         fmtQty="{sum($vClosingTradePosActionList/@fmtQty)}"
                                         lastPx="{$vClosingESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                         trdDt="{$vClosingESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                         stlDt="{$vCommonClosingTrade/@stlDt}">
                                    <xsl:copy-of select="$vClosingTradePosActionList/rmg"/>
                                  </trade>
                                </xsl:for-each>
                              </xsl:for-each>
                            </xsl:when>
                          </xsl:choose>
                          <subTotal>
                            <long fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty) +
                                  sum($vAssetPosActionList[trades/trade2/@tradeId=$vAssetESTTrade2List[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty)}"/>
                            <short fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty) +
                                   sum($vAssetPosActionList[trades/trade2/@tradeId=$vAssetESTTrade2List[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty)}"/>
                            <xsl:call-template name="Business_GetFee">
                              <xsl:with-param name="pFee" select="$vAssetPosActionList/fee"/>
                            </xsl:call-template>
                          </subTotal>
                        </asset>
                      </xsl:if>
                    </xsl:for-each>
                  </book>
                </xsl:if>
              </xsl:for-each>
            </market>
          </xsl:if>
        </xsl:for-each>
      </date>
    </xsl:if>
  </xsl:template>


  <!-- .......................................................................... -->
  <!--              Amendment/Transfers                                           -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizESE_AmendmentTransfers                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ESE AmendmentTransfers section
        - POC : Position correction
        - POT : Position transfer
        - LateTrade(4): Trades saisis en retard
        - PortFolioTransfer(42): Trades issus d'un transfert
        ................................................ -->
  <!-- FI 20150827 [21287] add Template -->
  <xsl:template match="posActions | trades" mode="BizEST_AmendmentTransfers">

    <xsl:variable name="vBizDt">
      <xsl:value-of select="current()/@bizDt"/>
    </xsl:variable>

    <!-- POC : Position correction-->
    <!-- POT : Position transfer -->
    <xsl:variable name="vPosActionList" select="current()/posAction[contains(',POC,POT,',concat(',',@requestType,','))]"/>
    <!-- LateTrade(4): Trades saisis en retard-->
    <!-- PortFolioTransfer(42): Trades issus d'un transfert-->
    <xsl:variable name="vDailyTradesList_Node">
      <xsl:choose>
        <xsl:when test="current()/posAction">
          <xsl:copy-of select="$gDailyTrades[@bizDt=$vBizDt]/trade[contains(',4,42,',concat(',',@trdTyp,','))]"/>
        </xsl:when>
        <xsl:when test="$gPosActions[@bizDt=$vBizDt]/posAction[contains(',POC,POT,',concat(',',@requestType,','))] = false()">
          <xsl:copy-of select="current()/trade[contains(',4,42,',concat(',',@trdTyp,','))]"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vDailyTradesList" select="msxsl:node-set($vDailyTradesList_Node)/trade"/>
    <xsl:variable name="vPosActionTradesList" select="$vPosActionList/trades/trade"/>
    <xsl:variable name="vEfsmlTradeList" select="$vDailyTradesList | $vPosActionTradesList"/>


    <xsl:variable name="vESTTradeList" select="$gTrade[@tradeId=$vEfsmlTradeList/@tradeId and equitySecurityTransaction]"/>

    <xsl:if test="$vESTTradeList">
      <date bizDt="{$vBizDt}">
        <xsl:for-each select="$gRepository/market">
          <xsl:variable name="vRepository-market" select="current()"/>
          <xsl:variable name="vMarketRepository-equity" select="$gRepository/equity[idM=$vRepository-market/@OTCmlId]"/>

          <xsl:variable name="vMarketESTTradeList" select="$vESTTradeList[
                    equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@Exch=$vRepository-market/fixml_SecurityExchange and
                    equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vMarketRepository-equity/@OTCmlId]"/>

          <xsl:if test="$vMarketESTTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketESTTradeListNode">
                <xsl:copy-of select="$vMarketESTTradeList"/>
              </xsl:variable>
              <xsl:variable name="vMarketTradeListCopy" select="msxsl:node-set($vMarketESTTradeListNode)/trade"/>
              <xsl:variable name="vRptSide-Acct_List" select="$vMarketTradeListCopy/equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

              <xsl:for-each select="$vRptSide-Acct_List">
                <xsl:variable name="vRptSide-Acct" select="current()"/>
                <xsl:variable name="vBookESTTradeList" select="$vMarketESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

                <xsl:if test="$vBookESTTradeList">

                  <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}" sectionKey="{$gcReportSectionKey_AMT}">

                    <xsl:variable name="vBookESTTradeListCopyNode">
                      <xsl:copy-of select="$vBookESTTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookESTTradeListCopy" select="msxsl:node-set($vBookESTTradeListCopyNode)/trade"/>

                    <xsl:variable name="vInstrmt-ID_List" select="$vBookESTTradeListCopy/equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt[generate-id()=generate-id(key('kInstrmt-ID',@ID))]"/>
                    <xsl:for-each select="$vInstrmt-ID_List">
                      <xsl:variable name="vInstrmt-ID" select="current()"/>
                      <xsl:variable name="vAssetESTTradeList" select="$vBookESTTradeList[equitySecurityTransaction/FIXML/TrdCaptRpt/Instrmt/@ID=$vInstrmt-ID/@ID]"/>
                      <xsl:variable name="vAssetESTCommonTradeList" select="$gCommonData/trade[@tradeId=$vAssetESTTradeList/@tradeId]"/>
                      <xsl:variable name="vAssetPosActionList" select="$vPosActionList[trades/trade/@tradeId=$vAssetESTTradeList/@tradeId]"/>
                      <xsl:variable name="vAssetDailyTradesList" select="$vDailyTradesList[@tradeId=$vAssetESTTradeList/@tradeId]"/>
                      <xsl:variable name="vCcy" select="$gRepository/equity[@OTCmlId=$vInstrmt-ID/@ID]/idC/text()"/>


                      <asset OTCmlId="{$vInstrmt-ID/@ID}"
                             assetCategory="EquityAsset" assetName="equity"
                             family="ESE">
                        <!-- FI 20150827 [21287] impasse sur le multi-instrument (on supposse que les trades sont tous avec le même instruement) -->
                        <xsl:attribute name ="idI">
                          <xsl:value-of select ="$vAssetESTTradeList[1]/equitySecurityTransaction/productType/@OTCmlId"/>
                        </xsl:attribute>

                        <xsl:call-template name="Business_GetPattern">
                          <xsl:with-param name="pCcy" select="$vCcy"/>
                          <xsl:with-param name="pFmtQty" select="$vAssetDailyTradesList/@fmtQty | $vAssetPosActionList/@fmtQty"/>
                          <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetESTTradeList/@tradeId]/@fmtLastPx"/>
                        </xsl:call-template>

                        <!--Pour un transfert, Coté Source: PosAction est toujours alimenté-->
                        <!--Pour une correction: PosAction est toujours alimenté-->
                        <xsl:for-each select="$vAssetPosActionList">

                          <xsl:variable name="vPosAction" select="current()"/>
                          <xsl:variable name="vESTTrade" select="$vAssetESTTradeList[@tradeId=$vPosAction/trades/trade/@tradeId]"/>
                          <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vESTTrade/@tradeId]"/>

                          <posAction OTCmlId="{$vPosAction/@OTCmlId}">
                            <xsl:variable name="vTrdNum">
                              <xsl:call-template name="GetTrdNum_Fixml">
                                <xsl:with-param name="pTrade" select="$vESTTrade"/>
                              </xsl:call-template>
                            </xsl:variable>

                            <trade trdNum="{$vTrdNum}" tradeId="{$vESTTrade/@tradeId}"
                                   lastPx="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                   trdDt="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}">
                              <!-- Date de rglt présente uniquement si l'action (correction ou transfert) s'effectue après la date de règlement du trade 
                                   Cela pour marquer le fait que les frais et le GAM ne sont pas contrepassés puisqu'ils n'ont pas encore été payés
                              -->
                              <xsl:choose>
                                <xsl:when test ="translate($vPosAction/@dtBusiness,'-','') > translate($vCommonTrade/@stlDt,'-','')">
                                  <xsl:attribute name ="stlDt">
                                    <xsl:value-of select ="$vCommonTrade/@stlDt"/>
                                  </xsl:attribute>
                                </xsl:when>
                              </xsl:choose>
                            </trade>

                            <!--Pour un transfert, Coté source: trade2 n'est pas alimenté pour un transfert externe-->
                            <!--Pour une correction: trade2 n'est pas alimenté-->
                            <xsl:if test="$vPosAction/trades/trade2">
                              <xsl:variable name="vTrade2" select="$gTrade[@tradeId=$vPosAction/trades/trade2/@tradeId]"/>
                              <xsl:variable name="vTrdNum2">
                                <xsl:call-template name="GetTrdNum_Fixml">
                                  <xsl:with-param name="pTrade" select="$vTrade2"/>
                                </xsl:call-template>
                              </xsl:variable>
                              <trade2 trdNum="{$vTrdNum2}" tradeId="{$vTrade2/@tradeId}"/>
                            </xsl:if>

                            <amount>
                              <!--amount1 -->
                              <amount1 det="GAM" withside="1" >
                                <xsl:copy-of select="$vPosAction/gam"/>
                              </amount1>
                            </amount>
                          </posAction>
                        </xsl:for-each>

                        <!--Pour un transfert, Coté Cible: on considère les trades du jours issus du transfert-->
                        <!--Pour un trade saisi en retard: on considère les trades du jours saisis en retard-->
                        <xsl:for-each select="$vAssetDailyTradesList">

                          <xsl:variable name="vDailyTrade" select="current()"/>
                          <xsl:variable name="vESTTrade" select="$vAssetESTTradeList[@tradeId=$vDailyTrade/@tradeId]"/>
                          <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vESTTrade/@tradeId]"/>

                          <xsl:variable name="vTrdNum">
                            <xsl:call-template name="GetTrdNum_Fixml">
                              <xsl:with-param name="pTrade" select="$vESTTrade"/>
                            </xsl:call-template>
                          </xsl:variable>
                          <trade trdNum="{$vTrdNum}" tradeId="{$vESTTrade/@tradeId}"
                                 lastPx="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx}"
                                 trdDt="{$vESTTrade/equitySecurityTransaction/FIXML/TrdCaptRpt/@TrdDt}"
                                 stlDt="{$vCommonTrade/@stlDt}">
                            <xsl:variable name="vTradeSrc" select="$vDailyTrade/tradeSrc"/>
                            <xsl:if test="$vTradeSrc">
                              <xsl:variable name="vTrdNumSrc">
                                <xsl:call-template name="GetTrdNum_Fixml">
                                  <xsl:with-param name="pTrade" select="$gTrade[@tradeId=$vTradeSrc/@tradeId]"/>
                                </xsl:call-template>
                              </xsl:variable>
                              <tradeSrc trdNum="{$vTrdNumSrc}" tradeId="{$vTradeSrc/@tradeId}"/>
                            </xsl:if>

                            <amount>
                              <!--amount1 -->
                              <amount1 det="GAM" withside="1" >
                                <xsl:copy-of select="$vDailyTrade/gam"/>
                              </amount1>
                            </amount>

                          </trade>
                        </xsl:for-each>
                      </asset>
                    </xsl:for-each>
                  </book>
                </xsl:if>
              </xsl:for-each>
            </market>
          </xsl:if>
        </xsl:for-each>
      </date>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
