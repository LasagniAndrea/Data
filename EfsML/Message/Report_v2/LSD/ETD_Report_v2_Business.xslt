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
  <xsl:variable name="gPurchaseSaleModel_ETD">
    <xsl:variable name="vPurchaseSaleModel" select="$gBlockSettings_Section/section[@key=$gcReportSectionKey_PSS]/product[@name='ETD']/@model"/>
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
  <!-- BizETD_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Trades Confirmations sections
       ................................................ -->
  <xsl:template match="trades" mode="BizETD_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates/predicate/@trdTyp]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade[@trdTyp=$vSubTotalList/predicates/predicate/@trdTyp]"/>

    <xsl:call-template name="BizETD_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>

  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Open Positions sections
       ................................................ -->
  <xsl:template match="posTrades" mode="BizETD_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:call-template name="BizETD_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="current()/trade"/>
      <xsl:with-param name="pSubTotal" select="current()/subTotal"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Trades Confirmations and Open Positions sections
       ................................................ -->
  <!-- FI 20151009 [21311] Modify -->
  <!-- FI 20150126 [21825] Modify -->
  <xsl:template name="BizETD_TradeSubTotal">
    <!-- Liste des trades (élément trades/trade ou posTrades/trade) -->
    <xsl:param name="pEfsmlTrade"/>
    <!-- Liste des sous-totaux (élément trades/subTotal ou posTrades/subTotal)-->
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pSectionKey"/>

    <!--Un trade ETD contient l'élément exchangeTradedDerivative-->
    <xsl:variable name="vETDTradeList" select="$gTrade[@tradeId=$pEfsmlTrade/@tradeId and exchangeTradedDerivative]"/>
    <xsl:variable name="vETDSubTotalList" select="$pSubTotal[@assetCategory='ExchangeTradedContract' and @idI=$vETDTradeList/exchangeTradedDerivative/productType/@OTCmlId]"/>

    <xsl:if test="$vETDSubTotalList">

      <date bizDt="{current()/@bizDt}">
        <!-- pour chaque marché du repository -->
        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <!-- Liste des assets ETD du marché courant -->
          <xsl:variable name="vMarketRepository-etd" select="$gRepository/etd[idM=$vRepository-market/@OTCmlId]"/>

          <!-- pour chaque DC du marché courant-->
          <xsl:for-each select="$gRepository/derivativeContract[@OTCmlId=$vMarketRepository-etd/idDC]">
            <xsl:variable name="vRepository-derivativeContract" select="current()"/>

            <!-- Liste des assets du DC courant -->
            <xsl:variable name="vDCRepository-alletd" select="$vMarketRepository-etd[idDC=$vRepository-derivativeContract/@OTCmlId]"/>

            <!-- FI 20150126 [21825] Rupture par etd.Multiplier -->
            <!-- Liste des différents contract Multiplier du DC courant -->
            <xsl:variable name="vDCRepository-alletdCopyNode">
              <xsl:copy-of select="$vDCRepository-alletd"/>
            </xsl:variable>
            <xsl:variable name="vDCRepository-alletdCopy" select="msxsl:node-set($vDCRepository-alletdCopyNode)/etd"/>

            <xsl:variable name="vDCRepository-alletdContractMultiplierList" select="$vDCRepository-alletdCopy[generate-id()=generate-id(key('ketd-ContratMultiplier',contractMultiplier/text())[1])]"/>
            <xsl:for-each select="$vDCRepository-alletdContractMultiplierList">
              <xsl:variable name="vContratMultplier" select ="current()/contractMultiplier/text()"/>

              <!-- Liste assets du DC courant et du contract Multiplier courant -->
              <!-- FI 20150126 [21825] add restriction sur contractMultiplier  -->
              <xsl:variable name="vDCRepository-etd" select="$vMarketRepository-etd[idDC=$vRepository-derivativeContract/@OTCmlId and contractMultiplier/text()=$vContratMultplier]"/>

              <!-- Liste des trades sur les assets du DC courant -->
              <xsl:variable name="vDCTradeList" select="$vETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID=$vDCRepository-etd/@OTCmlId]"/>

              <xsl:if test="$vDCTradeList">

                <market OTCmlId="{$vRepository-market/@OTCmlId}">
                  <!-- Liste des Assets négociés du DC courant -->
                  <xsl:variable name="vDCAssetList" select="$vDCRepository-etd[@OTCmlId=$vDCTradeList/exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID]"/>
                  <!-- Liste des trades négociés du DC courant -->
                  <xsl:variable name="vDCEfsmlTradeList" select="$pEfsmlTrade[@tradeId=$vDCTradeList/@tradeId]"/>
                  <!-- Liste des sous-totaux du DC courant -->
                  <xsl:variable name="vDCSubTotalList" select="$vETDSubTotalList[@idAsset=$vDCAssetList/@OTCmlId]"/>

                  <xsl:variable name="vCcy" select="$vRepository-derivativeContract/idC_Price"/>

                  <!-- FI 20150126 [21825] add attribute contractMultiplier. Tous les assets enfants le même contractMultiplier -->
                  <derivativeContract OTCmlId="{$vRepository-derivativeContract/@OTCmlId}"
                                      category="{$vRepository-derivativeContract/category}"
                                      futValuationMethod="{$vRepository-derivativeContract/futValuationMethod}"
                                      ccy="{$vCcy}"
                                      contractMultiplier="{$vContratMultplier}">

                    <!-- FI 20151009 [21311] pattern pour le strike uniquement (les strikes des assets sont tous affichés dans le même format) -->
                    <xsl:call-template name="Business_GetPattern">
                      <xsl:with-param name="pCcy" select="$vCcy"/>
                      <xsl:with-param name="pFmtStrike" select="$vDCAssetList/strikePrice/@fmtPrice"/>
                    </xsl:call-template>

                    <xsl:variable name="vDCSubTotalCopyNode">
                      <xsl:copy-of select="$vDCSubTotalList"/>
                    </xsl:variable>
                    <xsl:variable name="vDCSubTotalCopy" select="msxsl:node-set($vDCSubTotalCopyNode)/subTotal"/>
                    <!-- FI 20151104 [21311] use[1] -->
                    <xsl:variable name="vSubTotal-idB_List" select="$vDCSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idB',@idB)[1])]"/>
                    <!-- pour chaque Book présent dans les sous-totaux du DC courant -->
                    <xsl:for-each select="$vSubTotal-idB_List">
                      <!-- Book Courant-->
                      <xsl:variable name="vSubTotal-idB" select="current()"/>
                      <!-- Liste des trades du book courant -->
                      <xsl:variable name="vBookETDTradeList" select="$vDCTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Acct=$gRepository/book[@OTCmlId=$vSubTotal-idB/@idB]/identifier]"/>

                      <xsl:if test="$vBookETDTradeList">

                        <book OTCmlId="{$vSubTotal-idB/@idB}" sectionKey="{$pSectionKey}">

                          <!-- Liste des trades (Efsml) du book courant -->
                          <xsl:variable name="vBookEfsmlTradeList" select="$vDCEfsmlTradeList[@tradeId=$vBookETDTradeList/@tradeId]"/>
                          <!-- Liste des sous-totaux du book courant -->
                          <xsl:variable name="vBookETDSubTotalList" select="$vDCSubTotalList[@idB=$vSubTotal-idB/@idB]"/>

                          <xsl:variable name="vBookETDSubTotalCopyNode">
                            <xsl:copy-of select="$vBookETDSubTotalList"/>
                          </xsl:variable>
                          <xsl:variable name="vBookETDSubTotalCopy" select="msxsl:node-set($vBookETDSubTotalCopyNode)/subTotal"/>
                          <!-- FI 20151104 [21311] use[1] -->
                          <xsl:variable name="vSubTotal-Instr_List" select="$vBookETDSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idI',@idI)[1])]"/>

                          <!-- Liste des instruments des sous-totaux du book courant -->
                          <xsl:for-each select="$vSubTotal-Instr_List">

                            <xsl:variable name="vSubTotal-idI" select="current()"/>
                            <!-- Liste des trades de l'instrument courant -->
                            <xsl:variable name="vInstrETDTradeList" select="$vBookETDTradeList[exchangeTradedDerivative/productType/@OTCmlId=$vSubTotal-idI/@idI]"/>
                            <!-- Liste des sous-totaux du l'instrument courant -->
                            <xsl:variable name="vInstrETDSubTotalList" select="$vBookETDSubTotalList[@idI=$vSubTotal-idI/@idI]"/>


                            <xsl:if test="$vInstrETDTradeList">
                              <!-- Liste des trades (Efsml) de l'instrument courant -->
                              <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrETDTradeList/@tradeId]"/>


                              <xsl:variable name="vInstrETDSubTotalCopyNode">
                                <xsl:copy-of select="$vInstrETDSubTotalList"/>
                              </xsl:variable>
                              <xsl:variable name="vInstrETDSubTotalCopy" select="msxsl:node-set($vInstrETDSubTotalCopyNode)/subTotal"/>
                              <!-- FI 20151104 [21311] use[1] -->
                              <xsl:variable name="vSubTotal-idAsset_List" select="$vInstrETDSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idAsset',@idAsset)[1])]"/>

                              <!-- Liste des assets de l'instrument courant-->
                              <xsl:for-each select="$vSubTotal-idAsset_List">
                                <xsl:variable name="vSubTotal-idAsset" select="current()"/>

                                <!-- Liste des trades pour l'asset courant -->
                                <xsl:variable name="vAssetETDTradeList" select="$vInstrETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID=$vSubTotal-idAsset/@idAsset]"/>
                                <!-- Liste des trades (gCommonData) pour l'asset courant -->
                                <xsl:variable name="vAssetCommonTradeList" select="$gCommonData/trade[@tradeId=$vAssetETDTradeList/@tradeId]"/>
                                <!-- Liste des sous-totaux pour l'asset courant -->
                                <xsl:variable name="vAssetETDSubTotalList" select="$vInstrETDSubTotalList[@idAsset=$vSubTotal-idAsset/@idAsset]"/>

                                <xsl:if test="$vAssetETDTradeList">

                                  <asset OTCmlId="{$vSubTotal-idAsset/@idAsset}"
                                         expDt="{$vAssetCommonTradeList[1]/@expDt}"
                                         assetCategory="{$vSubTotal-idAsset/@assetCategory}" assetName="etd"
                                         family="LSD" idI="{$vSubTotal-idI/@idI}">

                                    <xsl:variable name="vAssetEfsmlTradeList" select="$vInstrEfsmlTradeList[@tradeId=$vAssetETDTradeList/@tradeId]"/>

                                    <!-- FI 20151009 [21311] pattern qui prend en considération les trades négociés sur l'asset courant -->
                                    <xsl:call-template name="Business_GetPattern">
                                      <xsl:with-param name="pCcy" select="$vCcy"/>
                                      <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                      <xsl:with-param name="pFmtClrPx" select="$vAssetEfsmlTradeList/@fmtClrPx"/>
                                      <xsl:with-param name="pFmtLastPx" select="$vAssetCommonTradeList/@fmtLastPx"/>
                                      <xsl:with-param name="pFmtAvgPx" select="$vAssetETDSubTotalList/long/@fmtAvgPx | $vAssetETDSubTotalList/short/@fmtAvgPx"/>
                                    </xsl:call-template>


                                    <!-- Liste des trades de l'asset courant-->
                                    <xsl:for-each select="$vAssetEfsmlTradeList">

                                      <xsl:variable name="vEfsmlTrade" select="current()"/>
                                      <xsl:variable name="vETDTrade" select="$vAssetETDTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>

                                      <xsl:variable name="vTrdNum">
                                        <xsl:call-template name="GetTrdNum_Fixml">
                                          <xsl:with-param name="pTrade" select="$vETDTrade"/>
                                        </xsl:call-template>
                                      </xsl:variable>

                                      <trade trdNum="{$vTrdNum}" tradeId="{$vEfsmlTrade/@tradeId}"
                                             lastPx="{$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                             trdDt="{$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}">

                                        <!-- FI 20151019 [21317] add amount -->
                                        <xsl:choose>
                                          <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or $pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_SET">
                                            <amount>
                                              <xsl:if test="$vRepository-derivativeContract/category= 'O' and $vRepository-derivativeContract/futValuationMethod = 'EQTY'">
                                                <amount1 withside="1">
                                                  <xsl:copy-of select="$vEfsmlTrade/prm"/>
                                                </amount1>
                                              </xsl:if>
                                            </amount>
                                          </xsl:when>
                                          <!-- FI 20150127 [XXXXX] add amount sur position -->
                                          <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS">
                                            <amount>
                                              <xsl:choose>
                                                <xsl:when test ="$vRepository-derivativeContract/category= 'O' and $vRepository-derivativeContract/futValuationMethod = 'EQTY'">
                                                  <amount1 withside="1">
                                                    <xsl:copy-of select="$vEfsmlTrade/nov"/>
                                                  </amount1>
                                                </xsl:when>
                                                <xsl:otherwise>
                                                  <amount1 withside="1">
                                                    <xsl:copy-of select="$vEfsmlTrade/umg"/>
                                                  </amount1>
                                                </xsl:otherwise>
                                              </xsl:choose>
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
                                          <xsl:if test="$vRepository-derivativeContract/category= 'O' and $vRepository-derivativeContract/futValuationMethod = 'EQTY'">
                                            <amount1 withside="1">
                                              <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/prm[1]/@ccy"/>
                                              <xsl:variable name="vTotal">
                                                <xsl:call-template name="GetAmount-amt">
                                                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/prm"/>
                                                  <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                </xsl:call-template>
                                              </xsl:variable>
                                              <prm amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                            </amount1>
                                          </xsl:if>
                                        </subTotalAmount>
                                      </xsl:when>
                                      <!-- FI 20150127 [XXXXX] add amount1 sur position -->
                                      <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS">
                                        <subTotalAmount>
                                          <!-- amount1 -->
                                          <xsl:choose>
                                            <xsl:when test="$vRepository-derivativeContract/category= 'O' and $vRepository-derivativeContract/futValuationMethod = 'EQTY'">
                                              <amount1 withside="1">
                                                <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/nov[1]/@ccy"/>
                                                <xsl:variable name="vTotal">
                                                  <xsl:call-template name="GetAmount-amt">
                                                    <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/nov"/>
                                                    <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                  </xsl:call-template>
                                                </xsl:variable>
                                                <nov amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                              </amount1>
                                            </xsl:when>
                                            <xsl:otherwise>
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
                                            </xsl:otherwise>
                                          </xsl:choose>
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
                  </derivativeContract>
                </market>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
      </date>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Amendment/Transfers/Cascading                                 -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizETD_AmendmentTransfers                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD AmendmentTransfers section
        - POC : Position correction
        - POT : Position transfer
        - LateTrade(4): Trades saisis en retard
        - PortFolioTransfer(42): Trades issus d'un transfert
        - TechnicalTrade(63): Trades généralement générés par l'action 'CLOSINGPOS'
        ................................................ -->
  <xsl:template match="posActions | trades" mode="BizETD_AmendmentTransfers">
    <!-- POC : Position correction-->
    <!-- POT : Position transfer -->
    <xsl:variable name="vPosActionList_Node">
      <xsl:copy-of select="current()/posAction[contains(',POC,POT,',concat(',',@requestType,','))]"/>
    </xsl:variable>

    <!-- LateTrade(4): Trades saisis en retard-->
    <!-- PortFolioTransfer(42): Trades issus d'un transfert-->
    <!-- TechnicalTrade(63): Trades généralement générés par l'action 'CLOSINGPOS' -->
    <xsl:variable name="vDailyTradesList_Node">
      <xsl:choose>
        <xsl:when test="current()/posAction">
          <xsl:copy-of select="$gDailyTrades[@bizDt=current()/@bizDt]/trade[contains(',4,42,',concat(',',@trdTyp,','))]"/>
        </xsl:when>
        <!-- FI 20231222 [WI788] Add TechnicalTrade only if exists CLOSINGPOS Actions -->
        <xsl:when test="$gPosActions[@bizDt=current()/@bizDt]/posAction[@requestType='CLOSINGPOS']">
          <xsl:copy-of select="current()/trade[@trdTyp='63']"/>
        </xsl:when>
        <xsl:when test="$gPosActions[@bizDt=current()/@bizDt]/posAction[contains(',POC,POT,',concat(',',@requestType,','))] = false()">
          <xsl:copy-of select="current()/trade[contains(',4,42,',concat(',',@trdTyp,','))]"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="BizETD_PosAction">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_AMT"/>
      <xsl:with-param name="pPosActionList_Node" select="$vPosActionList_Node"/>
      <xsl:with-param name="pTradesList_Node" select="$vDailyTradesList_Node"/>
    </xsl:call-template>

  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_Cascading                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Cascading section
        - CAS : Cascading
        - SHI : Shifting
       ................................................ -->
  <xsl:template match="posActions" mode="BizETD_Cascading">
    <!-- CAS : Cascading-->
    <!-- SHI : Shifting -->
    <xsl:variable name="vPosActionList_Node">
      <xsl:copy-of select="current()/posAction[contains(',CAS,SHI,',concat(',',@requestType,','))]"/>
    </xsl:variable>

    <xsl:call-template name="BizETD_PosAction">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_CAS"/>
      <xsl:with-param name="pPosActionList_Node" select="$vPosActionList_Node"/>
    </xsl:call-template>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--          Liquidations/Dénouement (EXERCISES / ASSIGNMENTS / ABANDONMENTS)  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizETD_Liquidations                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Liquidations section
       ................................................ -->
  <xsl:template match="posActions" mode="BizETD_Liquidations">

    <xsl:variable name="vPosActionList_Node">
      <xsl:copy-of select="current()/posAction[@requestType='MOF']"/>
    </xsl:variable>

    <xsl:call-template name="BizETD_PosAction">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_LIQ"/>
      <xsl:with-param name="pPosActionList_Node" select="$vPosActionList_Node"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_Deliveries                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Deliveries section
       ................................................ -->
  <xsl:template match="dlvTrades" mode="BizETD_Deliveries">

    <xsl:variable name="vDlvTradesList_Node">
      <xsl:copy-of select="current()/trade"/>
    </xsl:variable>

    <xsl:call-template name="BizETD_PosAction">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_DLV"/>
      <xsl:with-param name="pTradesList_Node" select="$vDlvTradesList_Node"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_ExeAss                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD ExeAss section
       ................................................ -->
  <xsl:template match="posActions" mode="BizETD_ExeAss">
    <xsl:variable name="vPosActionList_Node">
      <xsl:copy-of select="current()/posAction[contains(',EXE,AUTOEXE,ASS,AUTOASS,',concat(',',@requestType,','))]"/>
    </xsl:variable>
    <xsl:call-template name="BizETD_PosAction">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_EXA"/>
      <xsl:with-param name="pPosActionList_Node" select="$vPosActionList_Node"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_Abandonments                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Abandonments section
       ................................................ -->
  <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
  <xsl:template match="posActions" mode="BizETD_Abandonments">
    <xsl:variable name="vPosActionList_Node">
      <xsl:copy-of select="current()/posAction[contains(',ABN,NEX,NAS,AUTOABN,',concat(',',@requestType,','))]"/>
    </xsl:variable>
    <xsl:call-template name="BizETD_PosAction">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_ABN"/>
      <xsl:with-param name="pPosActionList_Node" select="$vPosActionList_Node"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_PosAction                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD PosActions section
       ................................................ -->
  <xsl:template name="BizETD_PosAction">
    <!-- Represente la section -->
    <xsl:param name="pSectionKey"/>
    <!-- Represente les eléments PosActions/Posaction-->
    <xsl:param name="pPosActionList_Node"/>
    <!-- Represente une liste de trades -->
    <xsl:param name="pTradesList_Node"/>

    <xsl:variable name="vCBMethod" select ="$gCBTrade/cashBalanceReport/settings/cashBalanceMethod"/>

    <xsl:variable name="vPosActionList" select="msxsl:node-set($pPosActionList_Node)/posAction"/>
    <xsl:variable name="vTradesList" select="msxsl:node-set($pTradesList_Node)/trade"/>

    <xsl:variable name="vPosActionTradesList" select="$vPosActionList/trades/trade"/>
    <xsl:variable name="vEfsmlTradeList" select="$vTradesList | $vPosActionTradesList"/>
    <!-- Liste des trades ETD impliqués dans une des actions (trade ETD contient l'élément exchangeTradedDerivative) -->
    <xsl:variable name="vETDTradeList" select="$gTrade[@tradeId=$vEfsmlTradeList/@tradeId and exchangeTradedDerivative]"/>

    <xsl:if test="$vETDTradeList">

      <xsl:variable name="vbizDt" select="current()/@bizDt"/>

      <date bizDt="{$vbizDt}">
        <!-- Liste des marchés du repository -->
        <xsl:for-each select="$gRepository/market">
          <!-- Pour chaque Marché -->
          <xsl:variable name="vRepository-market" select="current()"/>
          <!-- Liste des assets ETD du marché courant -->
          <xsl:variable name="vMarketRepository-etd" select="$gRepository/etd[idM=$vRepository-market/@OTCmlId]"/>

          <!-- Liste des DC du marché courant -->
          <xsl:for-each select="$gRepository/derivativeContract[@OTCmlId=$gRepository/etd[idM=$vRepository-market/@OTCmlId]/idDC]">

            <!-- Pour chaque DC -->
            <xsl:variable name="vRepository-derivativeContract" select="current()"/>

            <!-- Liste des assets du DC courant -->
            <xsl:variable name="vDCRepository-alletd" select="$vMarketRepository-etd[idDC=$vRepository-derivativeContract/@OTCmlId]"/>

            <!-- FI 20150126 [21825] Rupture par etd.Multiplier -->
            <!-- Liste des différents contract Multiplier du DC courant -->
            <xsl:variable name="vDCRepository-alletdCopyNode">
              <xsl:copy-of select="$vDCRepository-alletd"/>
            </xsl:variable>
            <xsl:variable name="vDCRepository-alletdCopy" select="msxsl:node-set($vDCRepository-alletdCopyNode)/etd"/>

            <xsl:variable name="vDCRepository-alletdContractMultiplierList" select="$vDCRepository-alletdCopy[generate-id()=generate-id(key('ketd-ContratMultiplier',contractMultiplier/text())[1])]"/>

            <!-- pour contract Multiplier du DC courant du marché courant-->
            <xsl:for-each select="$vDCRepository-alletdContractMultiplierList">
              <xsl:variable name="vContratMultplier" select ="current()/contractMultiplier/text()"/>

              <!-- Liste assets du DC courant et du contract Multiplier courant -->
              <!-- FI 20150126 [21825] add restriction sur contractMultiplier  -->
              <xsl:variable name="vDCRepository-etd" select="$vMarketRepository-etd[idDC=$vRepository-derivativeContract/@OTCmlId and contractMultiplier/text()=$vContratMultplier]"/>

              <!-- Liste des trades du DC courant -->
              <xsl:variable name="vDCETDTradeList" select="$vETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID=$vDCRepository-etd/@OTCmlId]"/>

              <xsl:if test="$vDCETDTradeList">

                <market OTCmlId="{$vRepository-market/@OTCmlId}">

                  <!-- Liste des assets du DC courant (Repositoty)  -->
                  <xsl:variable name="vDCAssetList" select="$vDCRepository-etd[@OTCmlId=$vDCETDTradeList/exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID]"/>
                  <!-- Liste des actions qui s'appliquent aux trades du DC courant -->
                  <xsl:variable name="vDCPosActionList" select="$vPosActionList[trades/trade/@tradeId=$vDCETDTradeList/@tradeId]"/>

                  <xsl:variable name="vCcy" select="$vRepository-derivativeContract/idC_Price"/>
                  <xsl:variable name="vFutValuationMethod" select="$vRepository-derivativeContract/futValuationMethod"/>

                  <!-- FI 20150126 [21825] add attribute contractMultiplier. Tous les assets enfants le même contractMultiplier -->
                  <derivativeContract OTCmlId="{$vRepository-derivativeContract/@OTCmlId}"
                                      category="{$vRepository-derivativeContract/category}"
                                      futValuationMethod="{$vFutValuationMethod}"
                                      ccy="{$vCcy}"
                                      contractMultiplier="{$vContratMultplier}">

                    <!-- FI 20151009 [21311] pattern pour le strike uniquement (les strikes des assets sont tous affichés dans le même format) -->
                    <xsl:call-template name="Business_GetPattern">
                      <xsl:with-param name="pCcy" select="$vCcy"/>
                      <xsl:with-param name="pFmtStrike" select="$vDCAssetList/strikePrice/@fmtPrice"/>
                    </xsl:call-template>

                    <xsl:variable name="vDCETDTradeListCopyNode">
                      <xsl:copy-of select="$vDCETDTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vDCETDTradeListCopy" select="msxsl:node-set($vDCETDTradeListCopyNode)/trade"/>

                    <!-- Liste des books présents sur les trades impliqués-->
                    <!-- FI 20151104 [21311] use[1] -->
                    <xsl:variable name="vRptSide-Acct_List" select="$vDCETDTradeListCopy/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct)[1])]"/>
                    <!-- Pour chaque Book-->
                    <xsl:for-each select="$vRptSide-Acct_List">

                      <xsl:variable name="vRptSide-Acct" select="current()"/>
                      <!-- Liste des trades du book-->
                      <xsl:variable name="vBookETDTradeList" select="$vDCETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

                      <xsl:if test="$vBookETDTradeList">

                        <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}" sectionKey="{$pSectionKey}">

                          <xsl:variable name="vBookETDTradeListCopyNode">
                            <xsl:copy-of select="$vBookETDTradeList"/>
                          </xsl:variable>
                          <xsl:variable name="vBookETDTradeListCopy" select="msxsl:node-set($vBookETDTradeListCopyNode)/trade"/>
                          <!-- FI 20151104 [21311] use[1] -->
                          <xsl:variable name="vInstrmt-ID_List" select="$vBookETDTradeListCopy/exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt[generate-id()=generate-id(key('kInstrmt-ID',@ID)[1])]"/>

                          <!-- Liste des assets parmi les trades du book courant-->
                          <xsl:for-each select="$vInstrmt-ID_List">

                            <xsl:variable name="vInstrmt-ID" select="current()"/>
                            <!-- Liste des trades sur l'asset courant -->
                            <xsl:variable name="vAssetETDTradeList" select="$vBookETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID=$vInstrmt-ID/@ID]"/>
                            <!-- Liste des trades sur l'asset courant  (CommonData) -->
                            <xsl:variable name="vAssetCommonTradeList" select="$gCommonData/trade[@tradeId=$vAssetETDTradeList/@tradeId]"/>
                            <!-- Liste des des actions qui s'appliquent aux trades sur l'asset courant-->
                            <xsl:variable name="vAssetPosActionList" select="$vDCPosActionList[trades/trade/@tradeId=$vAssetETDTradeList/@tradeId]"/>
                            <xsl:variable name="vAssetTradesList" select="$vTradesList[@tradeId=$vAssetETDTradeList/@tradeId]"/>

                            <xsl:if test="$vAssetETDTradeList">

                              <asset OTCmlId="{$vInstrmt-ID/@ID}"
                                     expDt="{$vAssetCommonTradeList[1]/@expDt}"
                                     assetCategory="ExchangeTradedContract" assetName="etd"
                                     family="LSD" idI="{$vAssetETDTradeList[1]/exchangeTradedDerivative/productType/@OTCmlId}">

                                <!-- FI 20151009 [21311] pattern qui prend en considération les trades/actions associés à l'asset courant -->
                                <xsl:call-template name="Business_GetPattern">
                                  <xsl:with-param name="pCcy" select="$vCcy"/>
                                  <xsl:with-param name="pFmtQty" select="$vAssetTradesList/@fmtQty | $vAssetPosActionList/@fmtQty | $vAssetTradesList/phy/@fmtQty"/>
                                  <xsl:with-param name="pFmtLastPx" select="$vAssetCommonTradeList/@fmtLastPx"/>
                                  <xsl:with-param name="pFmtUnlPx" select="$vAssetPosActionList/@fmtUnlPx | $vAssetTradesList/@fmtStlPx"/>
                                </xsl:call-template>

                                <xsl:for-each select="$vAssetPosActionList">

                                  <xsl:variable name="vPosAction" select="current()"/>
                                  <xsl:variable name="vETDTrade" select="$vAssetETDTradeList[@tradeId=$vPosAction/trades/trade/@tradeId]"/>

                                  <posAction OTCmlId="{$vPosAction/@OTCmlId}">
                                    <!-- RD 20170224 [22885] add dtUnclearing-->
                                    <xsl:if test="string-length($vPosAction/@dtUnclearing) > 0">
                                      <xsl:attribute name="dtUnclearing">
                                        <xsl:value-of select="$vPosAction/@dtUnclearing"/>
                                      </xsl:attribute>
                                    </xsl:if>
                                    <xsl:variable name="vTrdNum">
                                      <xsl:call-template name="GetTrdNum_Fixml">
                                        <xsl:with-param name="pTrade" select="$vETDTrade"/>
                                      </xsl:call-template>
                                    </xsl:variable>
                                    <trade trdNum="{$vTrdNum}" tradeId="{$vETDTrade/@tradeId}"
                                           lastPx="{$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                           trdDt="{$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}"/>

                                    <xsl:if test="$vPosAction/trades/trade2">
                                      <xsl:variable name="vTrade2" select="$gTrade[@tradeId=$vPosAction/trades/trade2/@tradeId]"/>
                                      <xsl:variable name="vTrdNum2">
                                        <xsl:call-template name="GetTrdNum_Fixml">
                                          <xsl:with-param name="pTrade" select="$vTrade2"/>
                                        </xsl:call-template>
                                      </xsl:variable>
                                      <trade2 trdNum="{$vTrdNum2}" tradeId="{$vTrade2/@tradeId}"/>
                                    </xsl:if>
                                    <!-- FI 20160208 [21311] add amount1, amount 2-->
                                    <!-- RD 20170224 [22885] reverse side for cancelled MOF-->
                                    <xsl:choose>
                                      <xsl:when test ="contains(concat(',',$gcReportSectionKey_AMT,',',$gcReportSectionKey_CAS,','),concat(',',$pSectionKey,','))">
                                        <amount>
                                          <xsl:if test="$vRepository-derivativeContract/category= 'O' and $vRepository-derivativeContract/futValuationMethod = 'EQTY'">
                                            <!--amount1 -->
                                            <amount1 withside="1" >
                                              <xsl:copy-of select="$vPosAction/prm"/>
                                            </amount1>
                                          </xsl:if>
                                        </amount>
                                      </xsl:when>
                                      <xsl:when test ="contains(concat(',',$gcReportSectionKey_EXA,',',$gcReportSectionKey_ABN,','),concat(',',$pSectionKey,','))">
                                        <amount>
                                          <xsl:choose>
                                            <!--<xsl:when test ="$vCBMethod = 'CSBUK' or $vCBMethod = 'CSBDEFAULT' ">-->
                                            <xsl:when test ="$vCBMethod = 'CSBUK'">
                                              <xsl:choose>
                                                <!-- AMOUNT 1-->
                                                <xsl:when test ="$vFutValuationMethod='FUT'">
                                                  <amount1 withside="1" >
                                                    <xsl:if test ="$pSectionKey=$gcReportSectionKey_EXA">
                                                      <xsl:attribute name ="det">PRM</xsl:attribute>
                                                    </xsl:if>
                                                    <xsl:copy-of select="$vPosAction/rmg"/>
                                                  </amount1>
                                                </xsl:when>
                                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_EXA and $vPosAction/scu" >
                                                  <amount1 det="CST" withside="1" >
                                                    <xsl:copy-of select="$vPosAction/scu"/>
                                                  </amount1>
                                                </xsl:when>
                                                <!-- AMOUNT 2-->
                                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_EXA and $vFutValuationMethod='FUT' and $vPosAction/scu">
                                                  <amount2 det="CST" withside="1" >
                                                    <xsl:copy-of select="$vPosAction/scu"/>
                                                  </amount2>
                                                </xsl:when>
                                              </xsl:choose>
                                            </xsl:when>
                                            <xsl:when test ="$vCBMethod = 'CSBDEFAULT'">
                                              <!-- AMOUNT 1-->
                                              <amount1 withside="1" >
                                                <xsl:if test ="$pSectionKey=$gcReportSectionKey_EXA">
                                                  <xsl:attribute name ="det">PRM</xsl:attribute>
                                                </xsl:if>
                                                <xsl:copy-of select="$vPosAction/rmg"/>
                                              </amount1>
                                              <xsl:choose>
                                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_EXA and $vPosAction/scu">
                                                  <amount2 det="CST" withside="1" >
                                                    <xsl:copy-of select="$vPosAction/scu"/>
                                                  </amount2>
                                                </xsl:when>
                                              </xsl:choose>
                                            </xsl:when>
                                          </xsl:choose>
                                        </amount>
                                      </xsl:when>
                                      <xsl:when test="$pSectionKey=$gcReportSectionKey_LIQ">
                                        <xsl:call-template name="Business_GetFee">
                                          <xsl:with-param name="pFee" select="$vPosAction/fee"/>
                                          <xsl:with-param name="pParentUnclearingCheck" select="true()"/>
                                        </xsl:call-template>
                                        <amount>
                                          <amount1 withside="1" >
                                            <xsl:choose>
                                              <xsl:when test="string-length($vPosAction/@dtUnclearing) > 0">
                                                <xsl:variable name="vReverseSide">
                                                  <xsl:choose>
                                                    <xsl:when test="$vPosAction/rmg/@side = $gcDebit">
                                                      <xsl:value-of select="$gcCredit"/>
                                                    </xsl:when>
                                                    <xsl:when test="$vPosAction/rmg/@side = $gcCredit">
                                                      <xsl:value-of select="$gcDebit"/>
                                                    </xsl:when>
                                                  </xsl:choose>
                                                </xsl:variable>
                                                <rmg side="{$vReverseSide}" amt="{$vPosAction/rmg/@amt}" ccy="{$vPosAction/rmg/@ccy}"/>
                                              </xsl:when>
                                              <xsl:otherwise>
                                                <xsl:copy-of select="$vPosAction/rmg"/>
                                              </xsl:otherwise>
                                            </xsl:choose>
                                          </amount1>
                                        </amount>
                                      </xsl:when>
                                    </xsl:choose>
                                  </posAction>
                                </xsl:for-each>

                                <xsl:for-each select="$vAssetTradesList">

                                  <xsl:variable name="vTrade" select="current()"/>
                                  <xsl:variable name="vETDTrade" select="$vAssetETDTradeList[@tradeId=$vTrade/@tradeId]"/>
                                  <xsl:variable name="VTrdTyp" select="$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdTyp" />
                                  <xsl:variable name="vTrdNum">
                                    <xsl:call-template name="GetTrdNum_Fixml">
                                      <xsl:with-param name="pTrade" select="$vETDTrade"/>
                                    </xsl:call-template>
                                  </xsl:variable>
                                  <!-- FI 20231222 [WI788] Add trdType-->
                                  <trade trdNum="{$vTrdNum}" tradeId="{$vETDTrade/@tradeId}" trdType="{$VTrdTyp}"
                                         lastPx="{$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                         trdDt="{$vETDTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}">
                                    <!-- tradeSrc-->
                                    <xsl:variable name="vTradeSrc" select="$vTrade/tradeSrc"/>
                                    <xsl:if test="$vTradeSrc">
                                      <xsl:variable name="vTrdNumSrc">
                                        <xsl:call-template name="GetTrdNum_Fixml">
                                          <xsl:with-param name="pTrade" select="$gTrade[@tradeId=$vTradeSrc/@tradeId]"/>
                                        </xsl:call-template>
                                      </xsl:variable>
                                      <tradeSrc trdNum="{$vTrdNumSrc}" tradeId="{$vTradeSrc/@tradeId}"/>
                                    </xsl:if>
                                    <!-- FI 20231222 [WI788] Section Amendments/Transfers 
                                    - trade représente le trade technique (ajouts des attributs trade2TradeIdList et trade2TrdNumList s'il existe n trades clôtrés (n>1))
                                    - trade2 représente le(s) tarde(s) clôturés
                                     -->
                                    <!-- trade2 -->
                                    <xsl:if test="$pSectionKey=$gcReportSectionKey_AMT">
                                      <xsl:variable name="vPosClosingPos" select="$gPosActions[@bizDt=$vbizDt]/posAction[@requestType='CLOSINGPOS' and trades/trade/@tradeId=$vTrade/@tradeId]"/>
                                      <xsl:if test="$vPosClosingPos">
                                        <xsl:attribute name="trade2Count">
                                          <xsl:value-of select="count($vPosClosingPos)"/>
                                        </xsl:attribute>
                                        <xsl:if test="count($vPosClosingPos)>1">
                                          <xsl:variable name="vtrade2TradeIdList">
                                            <xsl:apply-templates select="$vPosClosingPos" mode="trade2TradeIdList" />
                                          </xsl:variable>
                                          <xsl:attribute name="trade2TradeIdList">
                                            <xsl:value-of select="$vtrade2TradeIdList"/>
                                          </xsl:attribute>
                                          <xsl:variable name="vtrade2TrdNumList">
                                            <xsl:apply-templates select="$vPosClosingPos" mode="trade2TrdNumList" />
                                          </xsl:variable>
                                          <xsl:attribute name="trade2TrdNumList">
                                            <xsl:value-of select="$vtrade2TrdNumList"/>
                                          </xsl:attribute>
                                        </xsl:if>
                                        <xsl:for-each select="$vPosClosingPos">
                                          <xsl:variable name="vPosClosingPosItem" select="current()" />
                                          <xsl:variable name="vTrade2" select="$vPosClosingPosItem/trades/trade2"/>
                                          <xsl:variable name="vTrd2Num">
                                            <xsl:call-template name="GetTrdNum_Fixml">
                                              <xsl:with-param name="pTrade" select="$gTrade[@tradeId=$vTrade2/@tradeId]"/>
                                            </xsl:call-template>
                                          </xsl:variable>
                                          <trade2 trdNum="{$vTrd2Num}" tradeId="{$vTrade2/@tradeId}"/>
                                        </xsl:for-each>
                                      </xsl:if>
                                    </xsl:if>
                                    <xsl:choose>
                                      <xsl:when test ="$gcReportSectionKey_DLV = $pSectionKey">
                                        <amount>
                                          <!--amount1 -->
                                          <amount1 withside="1" >
                                            <xsl:copy-of select="$vTrade/dva"/>
                                          </amount1>
                                          <!--amount2 -->
                                          <amount2 isQty="1">
                                            <xsl:copy-of select="$vTrade/phy"/>
                                          </amount2>
                                        </amount>
                                      </xsl:when>
                                      <xsl:otherwise>
                                        <amount>
                                          <xsl:if test="$vRepository-derivativeContract/category= 'O' and $vRepository-derivativeContract/futValuationMethod = 'EQTY'">
                                            <!--amount1 -->
                                            <amount1 withside="1" >
                                              <xsl:copy-of select="$vEfsmlTradeList[@tradeId=$vTrade/@tradeId]/prm"/>
                                            </amount1>
                                          </xsl:if>
                                        </amount>
                                      </xsl:otherwise>
                                    </xsl:choose>
                                  </trade>
                                </xsl:for-each>

                                <subTotal>
                                  <!-- RD 20170224 [22885] Check Unclearing-->
                                  <xsl:choose>
                                    <xsl:when test="$pSectionKey=$gcReportSectionKey_LIQ">
                                      <long fmtQty="{sum($vAssetPosActionList[string-length(@dtUnclearing) = 0 and trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty) -
                                            sum($vAssetPosActionList[string-length(@dtUnclearing) > 0 and trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty) + 
                                            sum($vAssetTradesList[@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty)}"/>
                                      <short fmtQty="{sum($vAssetPosActionList[string-length(@dtUnclearing) = 0 and trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty) -
                                             sum($vAssetPosActionList[string-length(@dtUnclearing) > 0 and trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty) +
                                             sum($vAssetTradesList[@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty)}"/>

                                      <xsl:call-template name="Business_GetFee">
                                        <xsl:with-param name="pFee" select="$vAssetPosActionList/fee"/>
                                        <xsl:with-param name="pParentUnclearingCheck" select="true()"/>
                                      </xsl:call-template>

                                      <amount>
                                        <amount1 withside="1" >
                                          <xsl:variable name="vTotalCcy">
                                            <xsl:call-template name="GetAmount-ccy">
                                              <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg"/>
                                            </xsl:call-template>
                                          </xsl:variable>

                                          <xsl:variable name="vTotalCcyAmount" select="$vAssetPosActionList/rmg[@ccy=$vTotalCcy]"/>
                                          <xsl:variable name="vTotalCcyAmountDebit" select="$vTotalCcyAmount[(string-length(parent::node()/@dtUnclearing)=0 and @side=$gcDebit)
                                                        or (string-length(parent::node()/@dtUnclearing)>0 and @side=$gcCredit)]"/>
                                          <xsl:variable name="vTotalCcyAmountCredit" select="$vTotalCcyAmount[(string-length(parent::node()/@dtUnclearing)=0 and @side=$gcCredit)
                                                        or (string-length(parent::node()/@dtUnclearing)>0 and @side=$gcDebit)]"/>

                                          <xsl:variable name="vTotalAmount_Node">
                                            <xsl:if test="$vTotalCcyAmountDebit">
                                              <rmg side="{$gcDebit}" amt="{sum($vTotalCcyAmountDebit/@amt)}" ccy="{$vTotalCcy}"/>
                                            </xsl:if>
                                            <xsl:if test="$vTotalCcyAmountCredit">
                                              <rmg side="{$gcCredit}" amt="{sum($vTotalCcyAmountCredit/@amt)}" ccy="{$vTotalCcy}"/>
                                            </xsl:if>
                                          </xsl:variable>

                                          <xsl:variable name="vTotalAmount">
                                            <xsl:call-template name="GetAmount-amt">
                                              <xsl:with-param name="pAmount" select="msxsl:node-set($vTotalAmount_Node)/rmg"/>
                                              <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                            </xsl:call-template>
                                          </xsl:variable>
                                          <rmg amt="{$vTotalAmount}" ccy="{$vTotalCcy}"/>
                                        </amount1>
                                      </amount>
                                    </xsl:when>
                                    <xsl:otherwise>
                                      <long fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty) + 
                                        sum($vAssetTradesList[@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty)}"/>
                                      <short fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty) + 
                                         sum($vAssetTradesList[@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty)}"/>

                                      <xsl:call-template name="Business_GetFee">
                                        <xsl:with-param name="pFee" select="$vAssetPosActionList/fee"/>
                                      </xsl:call-template>

                                      <!-- FI 20160208 [21311] add amount1, amount 2-->
                                      <xsl:choose>
                                        <xsl:when test ="contains(concat(',',$gcReportSectionKey_EXA,',',$gcReportSectionKey_ABN,','),concat(',',$pSectionKey,','))">
                                          <amount>
                                            <xsl:choose>
                                              <!--<xsl:when test ="$vCBMethod = 'CSBUK' or $vCBMethod = 'CSBDEFAULT' ">-->
                                              <xsl:when test ="$vCBMethod = 'CSBUK'">
                                                <!-- Amount 1-->
                                                <xsl:choose>
                                                  <xsl:when test ="$vFutValuationMethod='FUT'">
                                                    <amount1 withside="1" >
                                                      <xsl:if test ="$pSectionKey=$gcReportSectionKey_EXA">
                                                        <xsl:attribute name ="det">PRM</xsl:attribute>
                                                      </xsl:if>
                                                      <xsl:variable name ="vTotalCcy" select ="$vAssetPosActionList/rmg[1]/@ccy"/>
                                                      <xsl:variable name="vTotal">
                                                        <xsl:call-template name="GetAmount-amt">
                                                          <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg"/>
                                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                        </xsl:call-template>
                                                      </xsl:variable>
                                                      <rmg amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                                    </amount1>
                                                  </xsl:when>
                                                  <xsl:when test ="$pSectionKey=$gcReportSectionKey_EXA and $vAssetPosActionList/scu">
                                                    <amount1 det="CST" withside="1" >
                                                      <xsl:variable name ="vTotalCcy" select ="$vAssetPosActionList/scu[1]/@ccy"/>
                                                      <xsl:variable name="vTotal">
                                                        <xsl:call-template name="GetAmount-amt">
                                                          <xsl:with-param name="pAmount" select="$vAssetPosActionList/scu"/>
                                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                        </xsl:call-template>
                                                      </xsl:variable>
                                                      <scu amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                                    </amount1>
                                                  </xsl:when>
                                                </xsl:choose>
                                                <!-- Amount 2-->
                                                <xsl:choose>
                                                  <xsl:when test ="$pSectionKey=$gcReportSectionKey_EXA and $vFutValuationMethod='FUT' and $vAssetPosActionList/scu">
                                                    <amount2 det="CST" withside="1" >
                                                      <xsl:variable name ="vTotalCcy" select ="$vAssetPosActionList/scu[1]/@ccy"/>
                                                      <xsl:variable name="vTotal">
                                                        <xsl:call-template name="GetAmount-amt">
                                                          <xsl:with-param name="pAmount" select="$vAssetPosActionList/scu"/>
                                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                        </xsl:call-template>
                                                      </xsl:variable>
                                                      <scu amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                                    </amount2>
                                                  </xsl:when>
                                                </xsl:choose>
                                              </xsl:when>
                                              <xsl:when test ="$vCBMethod = 'CSBDEFAULT'">
                                                <amount1 withside="1" >
                                                  <xsl:if test ="$pSectionKey=$gcReportSectionKey_EXA">
                                                    <xsl:attribute name ="det">PRM</xsl:attribute>
                                                  </xsl:if>
                                                  <xsl:variable name ="vTotalCcy" select ="$vAssetPosActionList/rmg[1]/@ccy"/>
                                                  <xsl:variable name="vTotal">
                                                    <xsl:call-template name="GetAmount-amt">
                                                      <xsl:with-param name="pAmount" select="$vAssetPosActionList/rmg"/>
                                                      <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                    </xsl:call-template>
                                                  </xsl:variable>
                                                  <rmg amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                                </amount1>
                                                <xsl:choose>
                                                  <xsl:when test ="$pSectionKey=$gcReportSectionKey_EXA and $vAssetPosActionList/scu">
                                                    <amount2 det="CST" withside="1" >
                                                      <xsl:variable name ="vTotalCcy" select ="$vAssetPosActionList/scu[1]/@ccy"/>
                                                      <xsl:variable name="vTotal">
                                                        <xsl:call-template name="GetAmount-amt">
                                                          <xsl:with-param name="pAmount" select="$vAssetPosActionList/scu"/>
                                                          <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                        </xsl:call-template>
                                                      </xsl:variable>
                                                      <scu amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                                    </amount2>
                                                  </xsl:when>
                                                </xsl:choose>
                                              </xsl:when>
                                            </xsl:choose>
                                          </amount>
                                        </xsl:when>
                                        <xsl:when test ="$pSectionKey=$gcReportSectionKey_DLV">
                                          <amount>
                                            <amount1 withside="1" >
                                              <xsl:variable name="vTotalCcy">
                                                <xsl:call-template name="GetAmount-ccy">
                                                  <xsl:with-param name="pAmount" select="$vAssetTradesList/dva"/>
                                                </xsl:call-template>
                                              </xsl:variable>
                                              <xsl:variable name="vTotalAmount">
                                                <xsl:call-template name="GetAmount-amt">
                                                  <xsl:with-param name="pAmount" select="$vAssetTradesList/dva"/>
                                                  <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                </xsl:call-template>
                                              </xsl:variable>
                                              <xsl:variable name="vTotalSide">
                                                <xsl:call-template name="GetAmount-side">
                                                  <xsl:with-param name="pAmount" select="$vAssetTradesList/dva"/>
                                                </xsl:call-template>
                                              </xsl:variable>
                                              <dva ccy="{$vTotalCcy}" side="{$vTotalSide}" amt="{$vTotalAmount}"/>
                                            </amount1>
                                            <amount2 isQty="1">
                                              <phy fmtQty="{sum($vAssetTradesList/phy/@fmtQty)}" qtyUnit="{$vAssetTradesList[1]/phy/@qtyUnit}"/>
                                            </amount2>
                                          </amount>
                                        </xsl:when>
                                      </xsl:choose>
                                    </xsl:otherwise>
                                  </xsl:choose>
                                </subTotal>
                              </asset>
                            </xsl:if>
                          </xsl:for-each>
                        </book>
                      </xsl:if>
                    </xsl:for-each>
                  </derivativeContract>
                </market>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
      </date>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Purchase & Sale                                               -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizETD_CorporateAction                           -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD CorporateAction section
       ................................................ -->
  <xsl:template match="posActions" mode="BizETD_CorporateAction">
    <xsl:call-template name="BizETD_PurchaseSale">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_CA"/>
      <xsl:with-param name="pPosActionList" select="current()/posAction[@requestType='CA']"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_PurchaseSale                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD Purchase&Sale section
        - UNCLEARING: Compensation générée suite à une décompensation partielle 
        - CLEARSPEC : Compensation  Spécifique 
        - CLEARBULK : Compensation globale 
        - CLEAREOD : Compensation fin de journée 
        - ENTRY : Clôture STP 
        - UPDENTRY : Clôture Fin de journée 
        - Hors Décompensation 
       ................................................ -->
  <xsl:template match="posActions" mode="BizETD_PurchaseSale">

    <xsl:variable name="vPosActionList" select="current()/posAction[
                  contains(',UNCLEARING,CLEARSPEC,CLEARBULK,CLEAREOD,ENTRY,UPDENTRY,',concat(',',@requestType,',')) and @dtUnclearing = false()]"/>

    <xsl:call-template name="BizETD_PurchaseSale">
      <xsl:with-param name="pSectionKey" select ="$gcReportSectionKey_PSS"/>
      <xsl:with-param name="pPosActionList" select="$vPosActionList"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizETD_PurchaseSale                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for ETD PurchaseSale section
       ................................................ -->
  <xsl:template name="BizETD_PurchaseSale">
    <!-- Represente la section -->
    <xsl:param name="pSectionKey"/>
    <!-- Represente les eléments PosActions/Posaction-->
    <xsl:param name="pPosActionList"/>

    <!--<xsl:variable name="vPosActionList" select="current()/posAction[
                  contains(',UNCLEARING,CLEARSPEC,CLEARBULK,CLEAREOD,ENTRY,UPDENTRY,',concat(',',@requestType,',')) and @dtUnclearing = false()]"/>-->

    <!--Un trade ETD contient l'élément exchangeTradedDerivative-->
    <!-- Liste des trades clôturants -->
    <xsl:variable name="vETDTradeList" select="$gTrade[@tradeId=$pPosActionList/trades/trade/@tradeId and exchangeTradedDerivative]"/>

    <xsl:if test="$vETDTradeList">

      <date bizDt="{current()/@bizDt}">
        <!-- Pour chaque marché du repository -->
        <xsl:for-each select="$gRepository/market">

          <xsl:variable name="vRepository-market" select="current()"/>
          <!-- Liste des assets ETD du marché -->
          <xsl:variable name="vMarketRepository-etd" select="$gRepository/etd[idM=$vRepository-market/@OTCmlId]"/>

          <!-- Pour chaque DC du marché -->
          <xsl:for-each select="$gRepository/derivativeContract[@OTCmlId=$vMarketRepository-etd/idDC]">
            <!-- DC courant -->
            <xsl:variable name="vRepository-derivativeContract" select="current()"/>

            <!-- Liste des assets du DC courant -->
            <xsl:variable name="vDCRepository-alletd" select="$vMarketRepository-etd[idDC=$vRepository-derivativeContract/@OTCmlId]"/>

            <!-- FI 20150126 [21825] Rupture par etd.Multiplier -->
            <!-- Liste des différents contract Multiplier du DC courant -->
            <xsl:variable name="vDCRepository-alletdCopyNode">
              <xsl:copy-of select="$vDCRepository-alletd"/>
            </xsl:variable>
            <xsl:variable name="vDCRepository-alletdCopy" select="msxsl:node-set($vDCRepository-alletdCopyNode)/etd"/>

            <xsl:variable name="vDCRepository-alletdContractMultiplierList" select="$vDCRepository-alletdCopy[generate-id()=generate-id(key('ketd-ContratMultiplier',contractMultiplier/text())[1])]"/>
            <xsl:for-each select="$vDCRepository-alletdContractMultiplierList">
              <xsl:variable name="vContratMultplier" select ="current()/contractMultiplier/text()"/>

              <!-- Liste assets du DC courant et du contract Multiplier courant -->
              <!-- FI 20150126 [21825] add restriction sur contractMultiplier  -->
              <xsl:variable name="vDCRepository-etd" select="$vMarketRepository-etd[idDC=$vRepository-derivativeContract/@OTCmlId and contractMultiplier/text()=$vContratMultplier]"/>

              <!-- Liste des trades clôturants en rapport avec le DC courant -->
              <xsl:variable name="vDCTradeList" select="$vETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID=$vDCRepository-etd/@OTCmlId]"/>

              <xsl:if test="$vDCTradeList">

                <!-- Marché -->
                <market OTCmlId="{$vRepository-market/@OTCmlId}">
                  <!-- Liste des assets en rapport avec le DC courant et avec la liste des trades clôturants -->
                  <xsl:variable name="vDCAssetList" select="$vDCRepository-etd[@OTCmlId=$vDCTradeList/exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID]"/>
                  <!-- Liste des actions en rapport avec la liste des trades clôturants -->
                  <xsl:variable name="vDCPosActionList" select="$pPosActionList[trades/trade/@tradeId=$vDCTradeList/@tradeId]"/>
                  <!-- Liste des trades clôturés en rapport avec le DC Courant -->
                  <xsl:variable name="vDCTrade2List" select="$gTrade[@tradeId=$vDCPosActionList/trades/trade2/@tradeId]"/>
                  <!-- Liste des trades clôturants et cloturés  en rapport avec le DC courant-->
                  <xsl:variable name="vDCAllTradeList" select="$vDCTradeList | $vDCTrade2List"/>

                  <xsl:variable name="vCcy" select="$vRepository-derivativeContract/idC_Price"/>

                  <!-- FI 20150126 [21825] add attribute contractMultiplier. Tous les assets enfants le même contractMultiplier -->
                  <derivativeContract OTCmlId="{$vRepository-derivativeContract/@OTCmlId}"
                                      category="{$vRepository-derivativeContract/category}"
                                      futValuationMethod="{$vRepository-derivativeContract/futValuationMethod}"
                                      ccy="{$vCcy}"
                                      contractMultiplier="{$vContratMultplier}">

                    <xsl:call-template name="Business_GetPattern">
                      <xsl:with-param name="pCcy" select="$vCcy"/>
                      <xsl:with-param name="pFmtStrike" select="$vDCAssetList/strikePrice/@fmtPrice"/>
                    </xsl:call-template>

                    <xsl:variable name="vDCTradeListCopyNode">
                      <xsl:copy-of select="$vDCTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vDCTradeListCopy" select="msxsl:node-set($vDCTradeListCopyNode)/trade"/>
                    <!-- FI 20151104 [21311] use[1] -->
                    <xsl:variable name="vRptSide-Acct_List" select="$vDCTradeListCopy/exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct)[1])]"/>

                    <!-- Pour chaque book en rapport avec le DC courant-->
                    <xsl:for-each select="$vRptSide-Acct_List">
                      <xsl:variable name="vRptSide-Acct" select="current()"/>
                      <!-- Liste des trades clôturants en rapport avec le book courant-->
                      <xsl:variable name="vBookETDTradeList" select="$vDCTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

                      <xsl:if test="$vBookETDTradeList">

                        <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}" sectionKey="{$pSectionKey}">

                          <xsl:variable name="vBookETDTradeListCopyNode">
                            <xsl:copy-of select="$vBookETDTradeList"/>
                          </xsl:variable>
                          <xsl:variable name="vBookETDTradeListCopy" select="msxsl:node-set($vBookETDTradeListCopyNode)/trade"/>
                          <!-- FI 20151104 [21311] use[1] -->
                          <xsl:variable name="vInstrmt-ID_List" select="$vBookETDTradeListCopy/exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt[generate-id()=generate-id(key('kInstrmt-ID',@ID)[1])]"/>
                          <!-- Pour chaque asset -->
                          <xsl:for-each select="$vInstrmt-ID_List">

                            <xsl:variable name="vInstrmt-ID" select="current()"/>
                            <!-- Liste des trades clôturants relatif à l'asset courant -->
                            <xsl:variable name="vAssetETDTradeList" select="$vBookETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID=$vInstrmt-ID/@ID]"/>
                            <!-- Liste des trades clôturants relatif à l'asset courant (de type CommonData)-->
                            <xsl:variable name="vAssetCommonTradeList" select="$gCommonData/trade[@tradeId=$vAssetETDTradeList/@tradeId]"/>

                            <xsl:if test="$vAssetETDTradeList">

                              <asset OTCmlId="{$vInstrmt-ID/@ID}"
                                     expDt="{$vAssetCommonTradeList[1]/@expDt}"
                                     assetCategory="ExchangeTradedContract" assetName="etd"
                                     family="LSD" idI="{$vAssetETDTradeList[1]/exchangeTradedDerivative/productType/@OTCmlId}">

                                <!-- Liste des actions en rapport avec l'asset courant -->
                                <xsl:variable name="vAssetPosActionList" select="$vDCPosActionList[trades/trade/@tradeId=$vAssetETDTradeList/@tradeId]"/>

                                <xsl:call-template name="Business_GetPattern">
                                  <xsl:with-param name="pCcy" select="$vCcy"/>
                                  <xsl:with-param name="pFmtQty" select="$vAssetPosActionList/@fmtQty"/>
                                  <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetCommonTradeList/@tradeId]/@fmtLastPx"/>
                                </xsl:call-template>

                                <!-- Liste des trades clôturés en rapport avec l'asset courant -->
                                <xsl:variable name="vAssetTrade2List" select="$gTrade[@tradeId=$vAssetPosActionList/trades/trade2/@tradeId]"/>
                                <!-- Liste des trades  clôturants et clôturés en rapport avec l'asset courant-->
                                <xsl:variable name="vAssetAllTradeList" select="$vAssetETDTradeList | $vAssetTrade2List"/>

                                <xsl:variable name="vAssetPosActionListCopyNode">
                                  <xsl:copy-of select="$vAssetPosActionList"/>
                                </xsl:variable>
                                <xsl:variable name="vAssetPosActionListCopy" select="msxsl:node-set($vAssetPosActionListCopyNode)/posAction"/>

                                <xsl:choose>
                                  <xsl:when test="$gPurchaseSaleModel_ETD = $gcPurchaseSaleOverallQty">

                                    <xsl:variable name="vAssetAllTradeListCopy" select="$vAssetPosActionListCopy/trades/trade | $vAssetPosActionListCopy/trades/trade2"/>
                                    <!-- Liste des tardes clôturants / clôturés-->
                                    <!-- FI 20151104 [21311] use[1] -->
                                    <xsl:variable name="vTrade-TradeId_List" select="$vAssetAllTradeListCopy[generate-id()=generate-id(key('kTradeId',@tradeId)[1])]"/>

                                    <xsl:for-each select="$vTrade-TradeId_List">

                                      <xsl:variable name="vTrade-TradeId" select="current()"/>
                                      <xsl:variable name="vTradePosActionList" select="$vAssetPosActionList[trades/trade/@tradeId = $vTrade-TradeId/@tradeId or trades/trade2/@tradeId = $vTrade-TradeId/@tradeId]"/>
                                      <xsl:variable name="vTradeClosingPosActionList" select="$vTradePosActionList[trades/trade/@tradeId = $vTrade-TradeId/@tradeId]"/>
                                      <xsl:variable name="vTrade" select="$vAssetAllTradeList[@tradeId=$vTrade-TradeId/@tradeId]"/>
                                      <xsl:variable name="vTrdNum">
                                        <xsl:call-template name="GetTrdNum_Fixml">
                                          <xsl:with-param name="pTrade" select="$vTrade"/>
                                        </xsl:call-template>
                                      </xsl:variable>

                                      <trade trdNum="{$vTrdNum}" tradeId="{$vTrade-TradeId/@tradeId}"
                                             fmtQty="{sum($vTradePosActionList/@fmtQty)}"
                                             lastPx="{$vTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                             trdDt="{$vTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}">

                                        <xsl:if test="$vTradeClosingPosActionList">
                                          <xsl:call-template name="Business_GetFee">
                                            <xsl:with-param name="pFee" select="$vTradeClosingPosActionList/fee"/>
                                          </xsl:call-template>
                                        </xsl:if>

                                      </trade>
                                    </xsl:for-each>
                                  </xsl:when>

                                  <xsl:when test="$gPurchaseSaleModel_ETD = $gcPurchaseSalePnLOnClosing">

                                    <!-- FI 20151104 [21311] use[1] et clé kTrade2-TradeId -->
                                    <xsl:variable name="vClosedTrade-TradeId_List" select="$vAssetPosActionListCopy/trades/trade2[generate-id()=generate-id(key('kTrade2-TradeId',@tradeId)[1])]"/>
                                    <xsl:for-each select="$vClosedTrade-TradeId_List">

                                      <xsl:variable name="vClosedTrade-TradeId" select="current()"/>
                                      <xsl:variable name="vClosedTradePosActionList" select="$vAssetPosActionList[trades/trade2/@tradeId = $vClosedTrade-TradeId/@tradeId]"/>
                                      <xsl:variable name="vClosedTrade" select="$vAssetAllTradeList[@tradeId=$vClosedTrade-TradeId/@tradeId]"/>

                                      <xsl:variable name="vClosedTrdNum">
                                        <xsl:call-template name="GetTrdNum_Fixml">
                                          <xsl:with-param name="pTrade" select="$vClosedTrade"/>
                                        </xsl:call-template>
                                      </xsl:variable>

                                      <trade isClosed="true()"
                                             closedTradeId="{$vClosedTrade-TradeId/@tradeId}" closedTrdNum="{$vClosedTrdNum}"
                                             closedLastPx="{$vClosedTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                             closedTrdDt="{$vClosedTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}"
                                             trdNum="{$vClosedTrdNum}" tradeId="{$vClosedTrade-TradeId/@tradeId}"
                                             fmtQty="{sum($vClosedTradePosActionList/@fmtQty)}"
                                             lastPx="{$vClosedTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                             trdDt="{$vClosedTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}"/>

                                      <xsl:variable name="vClosedTradePosActionListCopyNode">
                                        <xsl:copy-of select="$vClosedTradePosActionList"/>
                                      </xsl:variable>
                                      <xsl:variable name="vClosedTradePosActionListCopy" select="msxsl:node-set($vClosedTradePosActionListCopyNode)/posAction"/>

                                      <xsl:variable name="vClosingTrade-TradeId_List" select="$vClosedTradePosActionListCopy/trades/trade[generate-id()=generate-id(key('kTradeId',@tradeId))]"/>

                                      <xsl:for-each select="$vClosingTrade-TradeId_List">

                                        <xsl:variable name="vClosingTrade-TradeId" select="current()"/>
                                        <xsl:variable name="vClosingTradePosActionList" select="$vClosedTradePosActionList[trades/trade/@tradeId = $vClosingTrade-TradeId/@tradeId]"/>
                                        <xsl:variable name="vClosingTrade" select="$vAssetAllTradeList[@tradeId=$vClosingTrade-TradeId/@tradeId]"/>

                                        <xsl:variable name="vClosingTrdNum">
                                          <xsl:call-template name="GetTrdNum_Fixml">
                                            <xsl:with-param name="pTrade" select="$vClosingTrade"/>
                                          </xsl:call-template>
                                        </xsl:variable>

                                        <trade isClosed="false()"
                                               closedTradeId="{$vClosedTrade-TradeId/@tradeId}" closedTrdNum="{$vClosedTrdNum}"
                                               closedLastPx="{$vClosedTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                               closedTrdDt="{$vClosedTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}"
                                               trdNum="{$vClosingTrdNum}" tradeId="{$vClosingTrade-TradeId/@tradeId}"
                                               fmtQty="{sum($vClosingTradePosActionList/@fmtQty)}"
                                               lastPx="{$vClosingTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx}"
                                               trdDt="{$vClosingTrade/exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdDt}">

                                          <xsl:copy-of select="$vClosingTradePosActionList/rmg"/>

                                          <xsl:call-template name="Business_GetFee">
                                            <xsl:with-param name="pFee" select="$vClosingTradePosActionList/fee"/>
                                          </xsl:call-template>
                                        </trade>
                                      </xsl:for-each>

                                    </xsl:for-each>
                                  </xsl:when>
                                </xsl:choose>

                                <subTotal>
                                  <long fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty) +
                                  sum($vAssetPosActionList[trades/trade2/@tradeId=$vAssetTrade2List[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '1']/@tradeId]/@fmtQty)}"/>
                                  <short fmtQty="{sum($vAssetPosActionList[trades/trade/@tradeId=$vAssetETDTradeList[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@qty) +
                                   sum($vAssetPosActionList[trades/trade2/@tradeId=$vAssetTrade2List[exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side = '2']/@tradeId]/@fmtQty)}"/>
                                  <xsl:copy-of select="$vAssetPosActionList/rmg"/>
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
                  </derivativeContract>
                </market>
              </xsl:if>

            </xsl:for-each>
          </xsl:for-each>
        </xsl:for-each>
      </date>
    </xsl:if>
  </xsl:template>
  
  <!-- FI 20231222 [WI788] Add -->
  <xsl:template match="posAction" mode="trade2TradeIdList">
    <!-- Concatenate the tradeId value with a comma -->
    <xsl:value-of select="./trades/trade2/@tradeId"/>
    <xsl:if test="position() &lt; last()">,</xsl:if>
  </xsl:template>

  <!-- FI 20231222 [WI788] Add -->
  <xsl:template match="posAction" mode="trade2TrdNumList">
    <!-- Concatenate the trdNum value with a comma -->
    <xsl:variable name="vTradeId" select="./trades/trade2/@tradeId"/>
    <xsl:variable name="vTrdNum">
      <xsl:call-template name="GetTrdNum_Fixml">
        <xsl:with-param name="pTrade" select="$gTrade[@tradeId=$vTradeId]"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$vTrdNum"/>
    <xsl:if test="position() &lt; last()">,</xsl:if>
  </xsl:template>

</xsl:stylesheet>
