<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - COMS - commodity spot - Business                                    -->
  <!-- File    : \EfsML\Message\Report_v2\COMS\CommoditySpot_Report_v2_Business.xslt                  -->
  <!-- ============================================================================================== -->
  <!-- Version : v6.0.6232                                                                            -->
  <!-- Date    : 20170123                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->


  <!-- ============================================================================================== -->
  <!--                                         Keys                                                   -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                         Variables                                              -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->

  <!-- .......................................................................... -->
  <!--              Trades Confirmations and Open Positions                       -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizCOMS_TradeSubTotal                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Periodic report: By clearing date, create a "Business" XML for Commodity spot Trades Confirmations sections
       ................................................ -->
  <xsl:template match="trades" mode="BizCOMS_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates/predicate/@trdTyp]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade[@trdTyp=$vSubTotalList/predicates/predicate/@trdTyp]"/>

    <xsl:call-template name="BizCOMS_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select ="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCOMS_TradeSubTotal                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for equitySecurityTransaction Unsettled and Settled Trades sections
       ................................................ -->
  <xsl:template match="unsettledTrades|settledTrades" mode="BizCOMS_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates=false()]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade"/>

    <xsl:call-template name="BizCOMS_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCOMS_TradeSubTotal                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for Commodity spot Trades Confirmations and Open Positions sections
       ................................................ -->
  <!-- RD 20170426 [21113] Modify -->
  <xsl:template name="BizCOMS_TradeSubTotal">
    <!-- Liste des trades (élément trades/trade ou posTrades/trade) -->
    <xsl:param name="pEfsmlTrade"/>
    <!-- Liste des sous-totaux (élément trades/subTotal ou posTrades/subTotal)-->
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pSectionKey"/>

    <!--Un trade COMS contient l'élément commoditySpot-->
    <xsl:variable name="vCOMSTradeList" select="$gTrade[@tradeId=$pEfsmlTrade/@tradeId and commoditySpot]"/>
    <xsl:variable name="vCOMSSubTotalList" select="$pSubTotal[@assetCategory='Commodity' and @idI=$vCOMSTradeList/commoditySpot/productType/@OTCmlId]"/>

    <xsl:if test="$vCOMSSubTotalList">

      <date bizDt="{current()/@bizDt}">
        <!-- pour chaque marché du repository -->
        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <!-- Liste des assets Commodity du marché courant -->
          <xsl:variable name="vMarketRepository-commodity" select="$gRepository/commodity[idM=$vRepository-market/@OTCmlId]"/>

          <!-- Liste des trades sur les assets du Marché courant -->
          <xsl:variable name="vMarketCommonTradeList" select="$gCommonData/trade[@idAsset=$vMarketRepository-commodity/@OTCmlId]"/>

          <xsl:variable name="vMarketTradeList" select="$vCOMSTradeList[@tradeId=$vMarketCommonTradeList/@tradeId]"/>
          <!-- Liste des trades négociés du Marché courant -->
          <xsl:variable name="vMarketEfsmlTradeList" select="$pEfsmlTrade[@tradeId=$vMarketTradeList/@tradeId]"/>
          <!-- Liste des sous-totaux du Marché courant -->
          <xsl:variable name="vMarketSubTotalList" select="$vCOMSSubTotalList[@idAsset=$vMarketRepository-commodity/@OTCmlId]"/>

          <xsl:if test="$vMarketTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">
              <xsl:variable name="vMarketSubTotalCopyNode">
                <xsl:copy-of select="$vMarketSubTotalList"/>
              </xsl:variable>
              <xsl:variable name="vMarketSubTotalCopy" select="msxsl:node-set($vMarketSubTotalCopyNode)/subTotal"/>
              <xsl:variable name="vSubTotal-idB_List" select="$vMarketSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idB',@idB)[1])]"/>
              <!-- pour chaque Book présent dans les sous-totaux du Marché courant -->
              <xsl:for-each select="$vSubTotal-idB_List">
                <!-- Book Courant-->
                <xsl:variable name="vSubTotal-idB" select="current()"/>
                <!-- Liste des trades du book courant -->
                <xsl:variable name="vBookCOMSTradeList" select="$vMarketTradeList[commoditySpot/RptSide/@Acct=$gRepository/book[@OTCmlId=$vSubTotal-idB/@idB]/identifier]"/>

                <xsl:if test="$vBookCOMSTradeList">

                  <book OTCmlId="{$vSubTotal-idB/@idB}" sectionKey="{$pSectionKey}">

                    <!-- Liste des trades (Efsml) du book courant -->
                    <xsl:variable name="vBookEfsmlTradeList" select="$vMarketEfsmlTradeList[@tradeId=$vBookCOMSTradeList/@tradeId]"/>
                    <!-- Liste des sous-totaux du book courant -->
                    <xsl:variable name="vBookCOMSSubTotalList" select="$vCOMSSubTotalList[@idB=$vSubTotal-idB/@idB]"/>

                    <xsl:variable name="vBookCOMSSubTotalCopyNode">
                      <xsl:copy-of select="$vBookCOMSSubTotalList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookCOMSSubTotalCopy" select="msxsl:node-set($vBookCOMSSubTotalCopyNode)/subTotal"/>
                    <xsl:variable name="vSubTotal-Instr_List" select="$vBookCOMSSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idI',@idI)[1])]"/>

                    <!-- Liste des instruments des sous-totaux du book courant -->
                    <xsl:for-each select="$vSubTotal-Instr_List">

                      <xsl:variable name="vSubTotal-idI" select="current()"/>
                      <!-- Liste des trades de l'instrument courant -->
                      <xsl:variable name="vInstrCOMSTradeList" select="$vBookCOMSTradeList[commoditySpot/productType/@OTCmlId=$vSubTotal-idI/@idI]"/>
                      <!-- Liste des sous-totaux du l'instrument courant -->
                      <xsl:variable name="vInstrCOMSSubTotalList" select="$vBookCOMSSubTotalList[@idI=$vSubTotal-idI/@idI]"/>


                      <xsl:if test="$vInstrCOMSTradeList">
                        <!-- Liste des trades (Efsml) de l'instrument courant -->
                        <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrCOMSTradeList/@tradeId]"/>

                        <xsl:variable name="vInstrCOMSSubTotalCopyNode">
                          <xsl:copy-of select="$vInstrCOMSSubTotalList"/>
                        </xsl:variable>
                        <xsl:variable name="vInstrCOMSSubTotalCopy" select="msxsl:node-set($vInstrCOMSSubTotalCopyNode)/subTotal"/>
                        <xsl:variable name="vSubTotal-idAsset_List" select="$vInstrCOMSSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idAsset',@idAsset)[1])]"/>

                        <!-- Liste des assets de l'instrument courant-->
                        <xsl:for-each select="$vSubTotal-idAsset_List">
                          <xsl:variable name="vSubTotal-idAsset" select="current()"/>

                          <!-- Liste des trades (gCommonData) pour l'asset courant -->
                          <xsl:variable name="vAssetCOMSCommonTradeList" select="$gCommonData/trade[@idAsset=$vSubTotal-idAsset/@idAsset]"/>
                          <!-- Liste des trades pour l'asset courant -->
                          <xsl:variable name="vAssetCOMSTradeList" select="$vInstrCOMSTradeList[@tradeId=$vAssetCOMSCommonTradeList/@tradeId]"/>
                          <!-- Liste des sous-totaux pour l'asset courant -->
                          <xsl:variable name="vAssetCOMSSubTotalList" select="$vInstrCOMSSubTotalList[@idAsset=$vSubTotal-idAsset/@idAsset]"/>

                          <xsl:variable name="vRepository-commodity" select="$vMarketRepository-commodity[@OTCmlId=$vSubTotal-idAsset/@idAsset]"/>

                          <xsl:variable name="vCcy" select="$vRepository-commodity/idC/text()"/>

                          <xsl:if test="$vAssetCOMSTradeList">

                            <asset OTCmlId="{$vSubTotal-idAsset/@idAsset}"
                                   expDt="{$vAssetCOMSCommonTradeList[1]/@expDt}"
                                   assetCategory="{$vSubTotal-idAsset/@assetCategory}" assetName="commodity"
                                   family="COMS" idI="{$vSubTotal-idI/@idI}"
                                   ccy="{$vCcy}">

                              <xsl:variable name="vAssetEfsmlTradeList" select="$vInstrEfsmlTradeList[@tradeId=$vAssetCOMSTradeList/@tradeId]"/>

                              <xsl:call-template name="Business_GetPattern">
                                <xsl:with-param name="pCcy" select="$vCcy"/>
                                <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                <xsl:with-param name="pFmtLastPx" select="$vAssetCOMSCommonTradeList/@fmtLastPx"/>
                                <xsl:with-param name="pFmtAvgPx" select="$vAssetCOMSSubTotalList/long/@fmtAvgPx | $vAssetCOMSSubTotalList/short/@fmtAvgPx"/>
                              </xsl:call-template>

                              <!-- Liste des trades de l'asset courant-->
                              <xsl:for-each select="$vAssetEfsmlTradeList">

                                <xsl:variable name="vEfsmlTrade" select="current()"/>
                                <xsl:variable name="vCOMSTrade" select="$vAssetCOMSTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                <xsl:variable name="vCommonTrade" select="$vAssetCOMSCommonTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>

                                <!-- RD 20170426 [21113] Add Electronic Trade Id -->
                                <xsl:variable name="vTrdNum">
                                  <xsl:call-template name="GetTrdNum_Fixml">
                                    <xsl:with-param name="pTrade" select="$vCOMSTrade"/>
                                  </xsl:call-template>
                                </xsl:variable>

                                <trade trdNum="{$vTrdNum}" tradeId="{$vEfsmlTrade/@tradeId}"
                                       lastPx="{$vCommonTrade/@lastPx}"
                                       trdDt="{$vCommonTrade//@trdDt}"
                                       stlDt="{$vCommonTrade/@stlDt}">

                                  <xsl:choose>
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or $pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_SET">
                                      <amount>
                                        <!--amount1 (montant affiché sur la 1er ligne) -->
                                        <amount1 det="GAM" withside="1">
                                          <xsl:copy-of select="$vEfsmlTrade/gam"/>
                                        </amount1>
                                      </amount>
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
  <!--              Tools                                                         -->
  <!-- .......................................................................... -->


</xsl:stylesheet>
