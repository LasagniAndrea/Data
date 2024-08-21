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
  <!--                                         Keys                                                   -->
  <!-- ============================================================================================== -->
  <xsl:key name="kSubTotal-keyCategory" match="subTotal" use="concat(@assetCategory, '+', @idAsset)"/>
  <xsl:key name="kFpMLAsset-keyName" match="*" use="concat(name(), '+', @OTCmlId)"/>
  <xsl:key name="kRTSAsset-keyName" match="*" use="@keyName"/>

  <!-- ============================================================================================== -->
  <!--                                         Variables                                              -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gRTSAssetList" select="',equity,index,fxRate,'"/>

  <xsl:variable name="gCFDEquityRepository_Node">
    <xsl:call-template name="GetRTSAssetRepository">
      <xsl:with-param name="pRTSAssetList" select="',equity,'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="gCFDForexRepository_Node">
    <xsl:call-template name="GetRTSAssetRepository">
      <xsl:with-param name="pRTSAssetList" select="',fxRate,'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="gPurchaseSaleModel_RTS">
    <xsl:variable name="vPurchaseSaleModel" select="$gBlockSettings_Section/section[@key=$gcReportSectionKey_PSS]/product[@name='RTS']/@model"/>
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

  <!-- .......................................................................... -->
  <!--              Trades Confirmations and Open Positions                       -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizCFDEquity_TradeSubTotal                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Periodic report: By clearing date, create a "Business" XML for CFD Equity Trades Confirmations sections
       ................................................ -->
  <xsl:template match="trades" mode="BizCFDEquity_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates/predicate/@trdTyp]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade[@trdTyp=$vSubTotalList/predicates/predicate/@trdTyp]"/>

    <xsl:call-template name="BizRTS_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDEquityRepository_Node)"/>
      <xsl:with-param name="pSectionKey" select ="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCFDForex_TradeSubTotal                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Periodic report: By clearing date, create a "Business" XML for CFD Forex Trades Confirmations sections
       ................................................ -->
  <xsl:template match="trades" mode="BizCFDForex_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates/predicate/@trdTyp]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade[@trdTyp=$vSubTotalList/predicates/predicate/@trdTyp]"/>

    <xsl:call-template name="BizRTS_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDForexRepository_Node)"/>
      <xsl:with-param name="pSectionKey" select ="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCFDEquity_TradeSubTotal                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Periodic report: By clearing date, create a "Business" XML for CFD Equity Open Positions sections
       ................................................ -->
  <xsl:template match="posTrades" mode="BizCFDEquity_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:call-template name="BizRTS_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="current()/trade"/>
      <xsl:with-param name="pSubTotal" select="current()/subTotal"/>
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDEquityRepository_Node)"/>
      <xsl:with-param name="pSectionKey" select ="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCFDForex_TradeSubTotal                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Periodic report: By clearing date, create a "Business" XML for CFD Forex Open Positions sections
       ................................................ -->
  <xsl:template match="posTrades" mode="BizCFDForex_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:call-template name="BizRTS_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="current()/trade"/>
      <xsl:with-param name="pSubTotal" select="current()/subTotal"/>
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDForexRepository_Node)"/>
      <xsl:with-param name="pSectionKey" select ="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizRTS_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for returnSwap Trades Confirmations and Open Positions sections
       ................................................ -->
  <xsl:template name="BizRTS_TradeSubTotal">
    <xsl:param name="pEfsmlTrade"/>
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pRTSAssetRepository"/>
    <xsl:param name="pSectionKey"/>

    <!--Un trade RTS contient l'élément returnSwap-->
    <xsl:variable name="vRTSAssetList" select="$pRTSAssetRepository/*[@idM=$gRepository/market/@OTCmlId]"/>
    <xsl:variable name="vRTSTradeList" select="$gTrade[@tradeId=$pEfsmlTrade/@tradeId and returnSwap/returnLeg/underlyer/singleUnderlyer/*[concat(name(),'+',@OTCmlId)=$vRTSAssetList/@keyName]]"/>
    <xsl:variable name="vRTSSubTotalList" select="$pSubTotal[@idI=$vRTSTradeList/returnSwap/productType/@OTCmlId]"/>

    <xsl:if test="$vRTSTradeList">

      <xsl:variable name ="vBizDt" select="current()/@bizDt"/>

      <date bizDt="{$vBizDt}">

        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <xsl:variable name="vMarketRTSAssetList" select="$vRTSAssetList[@idM=$vRepository-market/@OTCmlId]"/>
          <xsl:variable name="vMarketRTSTradeList" select="$vRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[concat(name(),'+',@OTCmlId)=$vMarketRTSAssetList/@keyName]]"/>

          <xsl:if test="$vMarketRTSTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketEfsmlTradeList" select="$pEfsmlTrade[@tradeId=$vMarketRTSTradeList/@tradeId]"/>
              <xsl:variable name="vMarketRTSSubTotalList" select="$vRTSSubTotalList[concat(@assetCategory,'+',@idAsset)=$vMarketRTSAssetList/@keyCategory]"/>

              <xsl:variable name="vMarketRTSSubTotalCopyNode">
                <xsl:copy-of select="$vMarketRTSSubTotalList"/>
              </xsl:variable>
              <xsl:variable name="vMarketRTSSubTotalCopy" select="msxsl:node-set($vMarketRTSSubTotalCopyNode)/subTotal"/>
              <xsl:variable name="vSubTotal-idB_List" select="$vMarketRTSSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idB',@idB))]"/>

              <xsl:for-each select="$vSubTotal-idB_List">

                <xsl:variable name="vSubTotal-idB" select="current()"/>
                <xsl:variable name="vBookRTSTradeList" select="$vMarketRTSTradeList[returnSwap/RptSide/@Acct=$gRepository/book[@OTCmlId=$vSubTotal-idB/@idB]/identifier]"/>

                <xsl:if test="$vBookRTSTradeList">

                  <book OTCmlId="{$vSubTotal-idB/@idB}" sectionKey="{$pSectionKey}">

                    <xsl:variable name="vBookEfsmlTradeList" select="$vMarketEfsmlTradeList[@tradeId=$vBookRTSTradeList/@tradeId]"/>
                    <xsl:variable name="vBookRTSSubTotalList" select="$vMarketRTSSubTotalList[@idB=$vSubTotal-idB/@idB]"/>

                    <xsl:variable name="vBookRTSSubTotalCopyNode">
                      <xsl:copy-of select="$vBookRTSSubTotalList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookRTSSubTotalCopy" select="msxsl:node-set($vBookRTSSubTotalCopyNode)/subTotal"/>
                    <xsl:variable name="vSubTotal-Instr_List" select="$vBookRTSSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idI',@idI))]"/>

                    <xsl:for-each select="$vSubTotal-Instr_List">

                      <xsl:variable name="vSubTotal-idI" select="current()"/>
                      <xsl:variable name="vInstrRTSTradeList" select="$vBookRTSTradeList[returnSwap/productType/@OTCmlId=$vSubTotal-idI/@idI]"/>

                      <xsl:if test="$vInstrRTSTradeList">

                        <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrRTSTradeList/@tradeId]"/>
                        <xsl:variable name="vInstrRTSSubTotalList" select="$vBookRTSSubTotalList[@idI=$vSubTotal-idI/@idI]"/>
                        <xsl:variable name="vInstrRTSSubTotalCopyNode">
                          <xsl:copy-of select="$vInstrRTSSubTotalList"/>
                        </xsl:variable>
                        <xsl:variable name="vInstrRTSSubTotalCopy" select="msxsl:node-set($vInstrRTSSubTotalCopyNode)/subTotal"/>
                        <xsl:variable name="vSubTotal-Asset_List" select="$vInstrRTSSubTotalCopy[generate-id()=generate-id(key('kSubTotal-keyCategory',concat(@assetCategory, '+', @idAsset)))]"/>

                        <xsl:for-each select="$vSubTotal-Asset_List">

                          <xsl:variable name="vSubTotal-Asset" select="current()"/>
                          <xsl:variable name="vRTSAsset" select="$vMarketRTSAssetList[@keyCategory=concat($vSubTotal-Asset/@assetCategory, '+', $vSubTotal-Asset/@idAsset)]"/>
                          <xsl:variable name="vAsset" select="$gRepository/*[name()=$vRTSAsset/@assetName and @OTCmlId=$vRTSAsset/@OTCmlId]"/>
                          <xsl:variable name="vAssetRTSTradeList" select="$vBookRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[name()=$vRTSAsset/@assetName]/@OTCmlId=$vRTSAsset/@OTCmlId]"/>

                          <xsl:if test="$vAssetRTSTradeList">

                            <xsl:variable name="vAssetEfsmlTradeList" select="$vInstrEfsmlTradeList[@tradeId=$vAssetRTSTradeList/@tradeId]"/>
                            <xsl:variable name="vCcy" select="$vAsset/idC/text()"/>
                            <xsl:variable name="vAssetSubTotalList" select="$vInstrRTSSubTotalList[@assetCategory=$vSubTotal-Asset/@assetCategory and @idAsset=$vSubTotal-Asset/@idAsset]"/>

                            <asset OTCmlId="{$vSubTotal-Asset/@idAsset}"
                                   assetCategory="{$vSubTotal-Asset/@assetCategory}" assetName="{$vRTSAsset/@assetName}"
                                   family="RTS" idI="{$vSubTotal-idI/@idI}"
                                   ccy="{$vCcy}">

                              <xsl:call-template name="Business_GetPattern">
                                <xsl:with-param name="pCcy" select="$vCcy"/>
                                <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetRTSTradeList/@tradeId]/@fmtLastPx"/>
                                <xsl:with-param name="pFmtClrPx" select="$vAssetEfsmlTradeList/@fmtClrPx"/>
                                <xsl:with-param name="pFmtAvgPx" select="$vAssetSubTotalList/long/@fmtAvgPx | $vAssetSubTotalList/short/@fmtAvgPx"/>
                              </xsl:call-template>

                              <xsl:for-each select="$vAssetEfsmlTradeList">

                                <xsl:variable name="vEfsmlTrade" select="current()"/>
                                <xsl:variable name="vRTSTrade" select="$vAssetRTSTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>

                                <trade trdNum="{$vEfsmlTrade/@tradeId}" tradeId="{$vEfsmlTrade/@tradeId}"
                                       lastPx="{$vRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                       trdDt="{$gCommonData/trade[@tradeId=$vEfsmlTrade/@tradeId]/@trdDt}">

                                  <xsl:choose>
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or $pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_SET">
                                      <amount>
                                        <!--amount1 -->
                                        <amount1 withside="0">
                                          <notional amt="{$vRTSTrade/returnSwap/returnLeg/notional/notionalAmount/amount}"
                                                    ccy="{$vRTSTrade/returnSwap/returnLeg/notional/notionalAmount/currency}" />
                                        </amount1>
                                      </amount>
                                    </xsl:when>
                                    <!-- FI 20150127 [XXXXX] add amount1 sur position -->
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS">
                                      <amount>
                                        <amount1 withside="1">
                                          <xsl:copy-of select="$vEfsmlTrade/umg"/>
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
                                  <xsl:variable name ="vTotalCcy" select ="$vAssetRTSTradeList[1]/returnSwap/returnLeg/notional/notionalAmount/currency"/>
                                  <subTotalAmount>
                                    <!-- amount1 -->
                                    <amount1 withside="0">
                                      <xsl:variable name="vTotal" select ="sum($vAssetRTSTradeList/returnSwap/returnLeg/notional/notionalAmount/amount)"/>
                                      <notional amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                    </amount1>
                                  </subTotalAmount>
                                </xsl:when>
                                <!-- FI 20150127 [XXXXX] add amount1 sur position -->
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS">
                                  <subTotalAmount>
                                    <!-- amount1 -->
                                    <amount1 withside="1">
                                      <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/umg[1]/@ccy"/>
                                      <xsl:variable name="vTotal">
                                        <xsl:call-template name="GetAmount-amt">
                                          <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                        </xsl:call-template>
                                      </xsl:variable>
                                      <umg amt="{$vTotal}" ccy="{$vTotalCcy}"/>
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
  <!--              Amendment/Transfers                                           -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizCFDEquity_AmendmentTransfers                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for CFDEquity AmendmentTransfers section
       ................................................ -->
  <xsl:template match="posActions | trades" mode="BizCFDEquity_AmendmentTransfers">
    <xsl:call-template name="BizRTS_AmendmentTransfers">
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDEquityRepository_Node)"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCFDForex_AmendmentTransfers                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for CFDForex AmendmentTransfers section
       ................................................ -->
  <xsl:template match="posActions | trades" mode="BizCFDForex_AmendmentTransfers">
    <xsl:call-template name="BizRTS_AmendmentTransfers">
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDForexRepository_Node)"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizRTS_AmendmentTransfers                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for returnSwap AmendmentTransfers section
        - POC : Position correction
        - POT : Position transfer
        - LateTrade(4): Trades saisis en retard
        - PortFolioTransfer(42): Trades issus d'un transfert
        ................................................ -->
  <!-- FI 20150827 [21287] Modify -->
  <xsl:template name="BizRTS_AmendmentTransfers">
    <xsl:param name="pRTSAssetRepository"/>

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

    <!--Considérer uniquement les PosAction dont le trade2 n'est pas sur le book principal-->
    <xsl:variable name="vPosActionTradesList" select="$vPosActionList/trades/trade"/>
    <xsl:variable name="vEfsmlTradeList" select="$vDailyTradesList | $vPosActionTradesList"/>

    <!--Un trade RTS contient l'élément returnSwap-->
    <xsl:variable name="vRTSTradeList" select="$gTrade[@tradeId=$vEfsmlTradeList/@tradeId and returnSwap]"/>

    <xsl:if test="$vRTSTradeList">

      <date bizDt="{$vBizDt}">

        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <xsl:variable name="vMarketRTSAssetList" select="$pRTSAssetRepository/*[@idM=$vRepository-market/@OTCmlId]"/>
          <xsl:variable name="vMarketRTSTradeList" select="$vRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[concat(name(),'+',@OTCmlId)=$vMarketRTSAssetList/@keyName]]"/>

          <xsl:if test="$vMarketRTSTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketRTSTradeListCopyNode">
                <xsl:copy-of select="$vMarketRTSTradeList"/>
              </xsl:variable>
              <xsl:variable name="vMarketRTSTradeListCopy" select="msxsl:node-set($vMarketRTSTradeListCopyNode)/trade"/>
              <xsl:variable name="vRptSide-Acct_List" select="$vMarketRTSTradeListCopy/returnSwap/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

              <xsl:for-each select="$vRptSide-Acct_List">

                <xsl:variable name="vRptSide-Acct" select="current()"/>
                <xsl:variable name="vBookRTSTradeList" select="$vMarketRTSTradeList[returnSwap/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

                <xsl:if test="$vBookRTSTradeList">

                  <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}">

                    <xsl:variable name="vBookRTSTradeListtCopyNode">
                      <xsl:copy-of select="$vBookRTSTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookRTSTradeListtCopy" select="msxsl:node-set($vBookRTSTradeListtCopyNode)/trade"/>
                    <xsl:variable name="vFpMLAsset-keyName_List" select="$vBookRTSTradeListtCopy/returnSwap/returnLeg/underlyer/singleUnderlyer/*[contains($pRTSAssetRepository/asset/@list,concat(',',name(),','))][generate-id()=generate-id(key('kFpMLAsset-keyName',concat(name(), '+', @OTCmlId)))]"/>

                    <xsl:for-each select="$vFpMLAsset-keyName_List">

                      <xsl:variable name="vFpMLAsset-keyName" select="current()"/>
                      <xsl:variable name="vAssetName">
                        <xsl:call-template name="GetRTSAssetNodeName">
                          <xsl:with-param name="pAsset" select="$vFpMLAsset-keyName"/>
                        </xsl:call-template>
                      </xsl:variable>
                      <xsl:variable name="vRTSAsset" select="$pRTSAssetRepository/*[@keyName=concat($vAssetName, '+', $vFpMLAsset-keyName/@OTCmlId)]"/>
                      <xsl:variable name="vAsset" select="$gRepository/*[name()=$vRTSAsset/@assetName and @OTCmlId=$vRTSAsset/@OTCmlId]"/>
                      <xsl:variable name="vAssetRTSTradeList" select="$vBookRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[name()=$vRTSAsset/@assetName]/@OTCmlId=$vRTSAsset/@OTCmlId]"/>

                      <xsl:variable name="vAssetPosActionList" select="$vPosActionList[trades/trade/@tradeId=$vAssetRTSTradeList/@tradeId]"/>
                      <xsl:variable name="vAssetDailyTradesList" select="$vDailyTradesList[@tradeId=$vAssetRTSTradeList/@tradeId]"/>

                      <xsl:variable name="vCcy" select="$vAsset/idC/text()"/>

                      <asset OTCmlId="{$vFpMLAsset-keyName/@OTCmlId}"
                             assetCategory="{$vRTSAsset/@assetCategory}" assetName="{$vRTSAsset/@assetName}"
                             family="RTS" idI="{$vAssetRTSTradeList[1]/returnSwap/productType/@OTCmlId}"
                             ccy="{$vCcy}">

                        <xsl:call-template name="Business_GetPattern">
                          <xsl:with-param name="pCcy" select="$vCcy"/>
                          <xsl:with-param name="pFmtQty" select="$vAssetDailyTradesList/@fmtQty | $vAssetPosActionList/@fmtQty"/>
                          <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetRTSTradeList/@tradeId]/@fmtLastPx"/>
                        </xsl:call-template>

                        <!--Pour un transfert, Coté Source: PosAction est toujours alimenté-->
                        <!--Pour une correction: PosAction est toujours alimenté-->
                        <xsl:for-each select="$vAssetPosActionList">

                          <xsl:variable name="vPosAction" select="current()"/>
                          <xsl:variable name="vRTSTrade" select="$vAssetRTSTradeList[@tradeId=$vPosAction/trades/trade/@tradeId]"/>

                          <posAction OTCmlId="{$vPosAction/@OTCmlId}">
                            <!-- FI 20150827 [21287] l'attribut trdDt est alimenté avec la trade date du trade  -->
                            <trade trdNum="{$vRTSTrade/@tradeId}" tradeId="{$vRTSTrade/@tradeId}"
                                   lastPx="{$vRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                   trdDt="{$gCommonData/trade[@tradeId=$vRTSTrade/@tradeId]/@trdDt}"/>

                            <!--Pour un transfert, Coté source: trade2 n'est pas alimenté pour un transfert externe-->
                            <!--Pour une correction: trade2 n'est pas alimenté-->
                            <xsl:if test="$vPosAction/trades/trade2">
                              <xsl:variable name="vTrade2" select="$gTrade[@tradeId=$vPosAction/trades/trade2/@tradeId]"/>
                              <trade2 trdNum="{$vTrade2/@tradeId}" tradeId="{$vTrade2/@tradeId}"/>
                            </xsl:if>
                          </posAction>
                        </xsl:for-each>
                        <!--Pour un transfert, Coté Cible: on considère les trades du jours issus du transfert-->
                        <!--Pour un trade saisi en retard: on considère les trades du jours saisis en retard-->
                        <xsl:for-each select="$vAssetDailyTradesList">

                          <xsl:variable name="vDailyTrade" select="current()"/>
                          <xsl:variable name="vDailyRTSTrade" select="$vAssetRTSTradeList[@tradeId=$vDailyTrade/@tradeId]"/>

                          <trade trdNum="{$vDailyTrade/@tradeId}" tradeId="{$vDailyTrade/@tradeId}"
                                 lastPx="{$vDailyRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                 trdDt="{$gCommonData/trade[@tradeId=$vDailyTrade/@tradeId]/@trdDt}">

                            <xsl:variable name="vTradeSrc" select="$vDailyTrade/tradeSrc"/>

                            <xsl:if test="$vTradeSrc">
                              <tradeSrc trdNum="{$vTradeSrc/@tradeId}" tradeId="{$vTradeSrc/@tradeId}"/>
                            </xsl:if>

                            <amount>
                              <!--amount1 -->
                              <amount1 withside="0">
                                <notional amt="{$vDailyRTSTrade/returnSwap/returnLeg/notional/notionalAmount/amount}"
                                          ccy="{$vDailyRTSTrade/returnSwap/returnLeg/notional/notionalAmount/currency}" />
                              </amount1>
                            </amount>

                          </trade>
                        </xsl:for-each>

                        <!--<xsl:variable name="vFeeSubTotalNode">
                          <xsl:call-template name="Business_GetFee">
                            <xsl:with-param name="pFee" select="$vAssetPosActionList/fee | $vAssetDailyTradesList/fee"/>
                          </xsl:call-template>
                        </xsl:variable>
                        <xsl:variable name="vFeeSubTotal" select="msxsl:node-set($vFeeSubTotalNode)/fee"/>
                        <xsl:if test="$vFeeSubTotal">
                          <subTotal>
                            <xsl:copy-of select="$vFeeSubTotal"/>
                          </subTotal>
                        </xsl:if>-->
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

  <!-- .......................................................................... -->
  <!--              Purchase & Sale                                               -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizCFDEquity_PurchaseSale                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for CFDEquity Purchase&Sale section
       ................................................ -->
  <xsl:template match="posActions" mode="BizCFDEquity_PurchaseSale">
    <xsl:call-template name="BizRTS_PurchaseSale">
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDEquityRepository_Node)"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizCFDForex_PurchaseSale                         -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for CFDForex Purchase&Sale section
       ................................................ -->
  <xsl:template match="posActions" mode="BizCFDForex_PurchaseSale">
    <xsl:call-template name="BizRTS_PurchaseSale">
      <xsl:with-param name="pRTSAssetRepository" select="msxsl:node-set($gCFDForexRepository_Node)"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizRTS_PurchaseSale                              -->
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
  <xsl:template name="BizRTS_PurchaseSale">
    <xsl:param name="pRTSAssetRepository"/>

    <xsl:variable name="vPosActionList" select="current()/posAction[
                  contains(',UNCLEARING,CLEARSPEC,CLEARBULK,CLEAREOD,ENTRY,UPDENTRY,',concat(',',@requestType,',')) and
                  (@dtUnclearing = false() or string-length(@dtUnclearing) = 0)]"/>

    <!--Un trade RTS contient l'élément returnSwap-->
    <xsl:variable name="vRTSTradeList" select="$gTrade[@tradeId=$vPosActionList/trades/trade/@tradeId and returnSwap/returnLeg/underlyer/singleUnderlyer/*[concat(name(), '+', @OTCmlId)=$pRTSAssetRepository/*/@keyName]]"/>

    <xsl:if test="$vRTSTradeList">
      <date bizDt="{current()/@bizDt}">

        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <xsl:variable name="vMarketRTSAssetList" select="$pRTSAssetRepository/*[@idM=$vRepository-market/@OTCmlId]"/>
          <xsl:variable name="vMarketRTSTradeList" select="$vRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[concat(name(), '+', @OTCmlId)=$vMarketRTSAssetList/@keyName]]"/>

          <xsl:if test="$vMarketRTSTradeList">

            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketPosActionList" select="$vPosActionList[trades/trade/@tradeId=$vMarketRTSTradeList/@tradeId]"/>
              <xsl:variable name="vMarketRTSTrade2List" select="$gTrade[@tradeId=$vMarketPosActionList/trades/trade2/@tradeId and returnSwap]"/>
              <xsl:variable name="vMarketAllRTSTradeList" select="$vMarketRTSTradeList | $vMarketRTSTrade2List"/>

              <xsl:variable name="vMarketRTSTradeListCopyNode">
                <xsl:copy-of select="$vMarketRTSTradeList"/>
              </xsl:variable>
              <xsl:variable name="vMarketRTSTradeListCopy" select="msxsl:node-set($vMarketRTSTradeListCopyNode)/trade"/>
              <xsl:variable name="vRptSide-Acct_List" select="$vMarketRTSTradeListCopy/returnSwap/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

              <xsl:for-each select="$vRptSide-Acct_List">

                <xsl:variable name="vRptSide-Acct" select="current()"/>
                <xsl:variable name="vBookRTSTradeList" select="$vMarketRTSTradeList[returnSwap/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

                <xsl:if test="$vBookRTSTradeList">

                  <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}">

                    <xsl:variable name="vBookRTSTrade2List" select="$vMarketRTSTrade2List[returnSwap/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>
                    <xsl:variable name="vBookAllRTSTradeList" select="$vMarketAllRTSTradeList[returnSwap/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>
                    <xsl:variable name="vBookPosActionList" select="$vMarketPosActionList[trades/trade/@tradeId=$vBookRTSTradeList/@tradeId]"/>

                    <xsl:variable name="vBookRTSTradeListtCopyNode">
                      <xsl:copy-of select="$vBookRTSTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookRTSTradeListtCopy" select="msxsl:node-set($vBookRTSTradeListtCopyNode)/trade"/>
                    <xsl:variable name="vFpMLAsset-keyName_List" select="$vBookRTSTradeListtCopy/returnSwap/returnLeg/underlyer/singleUnderlyer/*[contains($pRTSAssetRepository/asset/@list,concat(',',name(),','))][generate-id()=generate-id(key('kFpMLAsset-keyName',concat(name(), '+', @OTCmlId)))]"/>

                    <xsl:for-each select="$vFpMLAsset-keyName_List">

                      <xsl:variable name="vFpMLAsset-keyName" select="current()"/>
                      <xsl:variable name="vAssetName">
                        <xsl:call-template name="GetRTSAssetNodeName">
                          <xsl:with-param name="pAsset" select="$vFpMLAsset-keyName"/>
                        </xsl:call-template>
                      </xsl:variable>
                      <xsl:variable name="vRTSAsset" select="$pRTSAssetRepository/*[@keyName=concat($vAssetName, '+', $vFpMLAsset-keyName/@OTCmlId)]"/>
                      <xsl:variable name="vAssetRTSTradeList" select="$vBookRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[name()=$vRTSAsset/@assetName]/@OTCmlId=$vRTSAsset/@OTCmlId]"/>

                      <xsl:if test="$vAssetRTSTradeList">

                        <xsl:variable name="vAsset" select="$gRepository/*[name()=$vRTSAsset/@assetName and @OTCmlId=$vRTSAsset/@OTCmlId]"/>
                        <xsl:variable name="vAssetRTSTrade2List" select="$vBookRTSTrade2List[returnSwap/returnLeg/underlyer/singleUnderlyer/*[name()=$vRTSAsset/@assetName]/@OTCmlId=$vRTSAsset/@OTCmlId]"/>
                        <xsl:variable name="vAssetAllRTSTradeList" select="$vBookAllRTSTradeList[returnSwap/returnLeg/underlyer/singleUnderlyer/*[name()=$vRTSAsset/@assetName]/@OTCmlId=$vRTSAsset/@OTCmlId]"/>
                        <xsl:variable name="vAssetPosActionList" select="$vBookPosActionList[trades/trade/@tradeId=$vAssetRTSTradeList/@tradeId]"/>

                        <xsl:variable name="vCcy" select="$vAsset/idC/text()"/>

                        <asset OTCmlId="{$vFpMLAsset-keyName/@OTCmlId}"
                               assetCategory="{$vRTSAsset/@assetCategory}" assetName="{$vRTSAsset/@assetName}"
                               family="RTS" idI="{$vAssetRTSTradeList[1]/returnSwap/productType/@OTCmlId}"
                               ccy="{$vCcy}">

                          <xsl:call-template name="Business_GetPattern">
                            <xsl:with-param name="pCcy" select="$vCcy"/>
                            <xsl:with-param name="pFmtQty" select="$vAssetPosActionList/@fmtQty"/>
                            <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetAllRTSTradeList/@tradeId]/@fmtLastPx"/>
                          </xsl:call-template>

                          <xsl:variable name="vAssetPosActionListCopyNode">
                            <xsl:copy-of select="$vAssetPosActionList"/>
                          </xsl:variable>
                          <xsl:variable name="vAssetPosActionListCopy" select="msxsl:node-set($vAssetPosActionListCopyNode)/posAction"/>

                          <xsl:choose>
                            <xsl:when test="$gPurchaseSaleModel_RTS = $gcPurchaseSaleOverallQty">

                              <xsl:variable name="vAssetAllTradeListCopy" select="$vAssetPosActionListCopy/trades/trade | $vAssetPosActionListCopy/trades/trade2"/>
                              <xsl:variable name="vTrade-TradeId_List" select="$vAssetAllTradeListCopy[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                              <xsl:for-each select="$vTrade-TradeId_List">

                                <xsl:variable name="vTrade-TradeId" select="current()"/>
                                <xsl:variable name="vRTSTrade" select="$vAssetAllRTSTradeList[@tradeId=$vTrade-TradeId/@tradeId]"/>
                                <xsl:variable name="vTradePosActionList" select="$vAssetPosActionList[trades/trade/@tradeId = $vTrade-TradeId/@tradeId or trades/trade2/@tradeId = $vTrade-TradeId/@tradeId]"/>

                                <trade tradeId="{$vTrade-TradeId/@tradeId}"
                                       fmtQty="{sum($vTradePosActionList/@fmtQty)}"
                                       lastPx="{$vRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                       trdDt="{$gCommonData/trade[@tradeId=$vTrade-TradeId/@tradeId]/@trdDt}"/>

                              </xsl:for-each>
                            </xsl:when>
                            <xsl:when test="$gPurchaseSaleModel_RTS = $gcPurchaseSalePnLOnClosing">

                              <xsl:variable name="vClosedTrade-TradeId_List" select="$vAssetPosActionListCopy/trades/trade2[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                              <xsl:for-each select="$vClosedTrade-TradeId_List">

                                <xsl:variable name="vClosedTrade-TradeId" select="current()"/>
                                <xsl:variable name="vClosedRTSTrade" select="$vAssetRTSTradeList[@tradeId=$vClosedTrade-TradeId/@tradeId]"/>
                                <xsl:variable name="vClosedTradePosActionList" select="$vAssetPosActionList[trades/trade2/@tradeId = $vClosedTrade-TradeId/@tradeId]"/>

                                <trade isClosed="true()"
                                       closedTradeId="{$vClosedTrade-TradeId/@tradeId}"
                                       closedLastPx="{$vClosedRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                       closedTrdDt="{$gCommonData/trade[@tradeId=$vClosedTrade-TradeId/@tradeId]/@trdDt}"
                                       tradeId="{$vClosedTrade-TradeId/@tradeId}"
                                       fmtQty="{sum($vClosedTradePosActionList/@fmtQty)}"
                                       lastPx="{$vClosedRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                       trdDt="{$gCommonData/trade[@tradeId=$vClosedTrade-TradeId/@tradeId]/@trdDt}"/>

                                <xsl:variable name="vClosedTradePosActionListCopyNode">
                                  <xsl:copy-of select="$vClosedTradePosActionList"/>
                                </xsl:variable>
                                <xsl:variable name="vClosedTradePosActionListCopy" select="msxsl:node-set($vClosedTradePosActionListCopyNode)/posAction"/>

                                <xsl:variable name="vClosingTrade-TradeId_List" select="$vClosedTradePosActionListCopy/trades/trade[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                                <xsl:for-each select="$vClosingTrade-TradeId_List">

                                  <xsl:variable name="vClosingTrade-TradeId" select="current()"/>
                                  <xsl:variable name="vClosingRTSTrade" select="$vAssetRTSTradeList[@tradeId=$vClosingTrade-TradeId/@tradeId]"/>
                                  <xsl:variable name="vClosingTradePosActionList" select="$vClosedTradePosActionList[trades/trade/@tradeId = $vClosingTrade-TradeId/@tradeId]"/>

                                  <trade isClosed="false()"
                                         closedTradeId="{$vClosedTrade-TradeId/@tradeId}"
                                         closedLastPx="{$vClosedRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                         closedTrdDt="{$gCommonData/trade[@tradeId=$vClosedTrade-TradeId/@tradeId]/@trdDt}"
                                         tradeId="{$vClosingTrade-TradeId/@tradeId}"
                                         fmtQty="{sum($vClosingTradePosActionList/@fmtQty)}"
                                         lastPx="{$vClosingRTSTrade/returnSwap/returnLeg/rateOfReturn/initialPrice/netPrice/amount/text()}"
                                         trdDt="{$gCommonData/trade[@tradeId=$vClosingTrade-TradeId/@tradeId]/@trdDt}">
                                    <xsl:copy-of select="$vClosingTradePosActionList/rmg"/>
                                  </trade>
                                </xsl:for-each>
                              </xsl:for-each>
                            </xsl:when>
                          </xsl:choose>
                          <subTotal>
                            <long fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetRTSTradeList[returnSwap/RptSide/@Side = '1']/@tradeId]/@fmtQty) +
                              sum($vAssetPosActionList[trades/trade2/@tradeId=$vAssetRTSTrade2List[returnSwap/RptSide/@Side = '1']/@tradeId]/@fmtQty)}"/>
                            <short fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetRTSTradeList[returnSwap/RptSide/@Side = '2']/@tradeId]/@fmtQty) +
                               sum($vAssetPosActionList[trades/trade2/@tradeId=$vAssetRTSTrade2List[returnSwap/RptSide/@Side = '2']/@tradeId]/@fmtQty)}"/>
                            <xsl:copy-of select="$vAssetPosActionList/rmg"/>
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
  <!--              Tools                                                         -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- GetRTSAssetRepository                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get an enrich repository of the returnSwap underlying Asset 
       ................................................ -->
  <xsl:template name="GetRTSAssetRepository">
    <xsl:param name="pRTSAssetList" select="$gRTSAssetList"/>

    <asset list="{$pRTSAssetList}"/>

    <xsl:for-each select="$gRepository/*[contains($pRTSAssetList,concat(',',name(),','))]">
      <xsl:variable name="vCurrentAsset" select="current()"/>
      <xsl:variable name="vAssetCategory">
        <xsl:choose>
          <xsl:when test="$vCurrentAsset[name() = 'equity']">
            <xsl:value-of select="'EquityAsset'"/>
          </xsl:when>
          <xsl:when test="$vCurrentAsset[name() = 'fxRate']">
            <xsl:value-of select="'FxRateAsset'"/>
          </xsl:when>
          <xsl:when test="$vCurrentAsset[name() = 'index']">
            <xsl:value-of select="'Index'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'NA'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vAssetName">
        <xsl:choose>
          <xsl:when test="$vCurrentAsset[name() = 'equity']">
            <xsl:value-of select="'equity'"/>
          </xsl:when>
          <xsl:when test="$vCurrentAsset[name() = 'fxRate']">
            <xsl:value-of select="'fxRate'"/>
          </xsl:when>
          <xsl:when test="$vCurrentAsset[name() = 'index']">
            <xsl:value-of select="'index'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'N/A'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:element name="{$vAssetName}">
        <xsl:attribute name="OTCmlId">
          <xsl:value-of select="$vCurrentAsset/@OTCmlId"/>
        </xsl:attribute>
        <xsl:attribute name="assetCategory">
          <xsl:value-of select="$vAssetCategory"/>
        </xsl:attribute>
        <xsl:attribute name="assetName">
          <xsl:value-of select="$vAssetName"/>
        </xsl:attribute>
        <xsl:attribute name="idM">
          <xsl:choose>
            <xsl:when test="$vCurrentAsset/idM">
              <xsl:value-of select="$vCurrentAsset/idM"/>
            </xsl:when>
            <xsl:when test="$vAssetName='fxRate'">
              <xsl:value-of select="$gRepository/market[identifier='FOREX']/@OTCmlId"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="keyCategory">
          <xsl:value-of select="concat($vAssetCategory, '+', $vCurrentAsset/@OTCmlId)"/>
        </xsl:attribute>
        <xsl:attribute name="keyName">
          <xsl:value-of select="concat($vAssetName, '+', $vCurrentAsset/@OTCmlId)"/>
        </xsl:attribute>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetRTSAssetNodeName                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get an XML node name of the returnSwap underlying Asset 
       ................................................ -->
  <xsl:template name="GetRTSAssetNodeName">
    <xsl:param name="pAsset"/>

    <xsl:choose>
      <xsl:when test="$pAsset[name() = 'equity']">
        <xsl:value-of select="'equity'"/>
      </xsl:when>
      <xsl:when test="$pAsset[name() = 'index']">
        <xsl:value-of select="'index'"/>
      </xsl:when>
      <xsl:when test="$pAsset[name() = 'fxRate']">
        <xsl:value-of select="'fxRate'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'N/A'"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>
