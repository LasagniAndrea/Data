<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">



  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->


  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->

  <!-- ................................................ -->
  <!-- BizDST_TradeSubTotal  match trades               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for debtSecurityTransaction Trades Confirmations sections
       ................................................ -->
  <!-- FI 20151019 [21317] Add -->
  <xsl:template match="trades" mode="BizDST_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <!-- Liste des trades regular REGULAR Uniquement (trdTyp="0", trdTyp="54", trdTyp="1") -->
    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates/predicate/@trdTyp]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade[@trdTyp=$vSubTotalList/predicates/predicate/@trdTyp]"/>

    <xsl:call-template name="BizDST_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ............................................................................. -->
  <!-- BizDST_TradeSubTotal   match     unsettledTrades|settledTrades                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for debtSecurityTransaction Unsettled and Settled Trades sections
       ............................................................................. -->
  <xsl:template match="unsettledTrades|settledTrades" mode="BizDST_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <!-- Prise en compte de tous les trades présents dans les éléments unsettledTrades et settledTrades -->
    <xsl:variable name="vSubTotalList" select="current()/subTotal[predicates=false()]"/>
    <xsl:variable name="vEfsmlTradeList" select="current()/trade"/>

    <xsl:call-template name="BizDST_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="$vEfsmlTradeList"/>
      <xsl:with-param name="pSubTotal" select="$vSubTotalList"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>

  
  <!-- ................................................ -->
  <!-- BizDST_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for equitySecurityTransaction Open Positions sections
       ................................................ -->
  <xsl:template match="posTrades|stlPosTrades" mode="BizDST_TradeSubTotal">
    <xsl:param name="pSectionKey"/>

    <xsl:call-template name="BizDST_TradeSubTotal">
      <xsl:with-param name="pEfsmlTrade" select="current()/trade"/>
      <xsl:with-param name="pSubTotal" select="current()/subTotal"/>
      <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
    </xsl:call-template>
  </xsl:template>


  <!-- ................................................ -->
  <!-- BizDST_TradeSubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for debtSecurityTransaction Trades Confirmations and Open Positions sections
       ................................................ -->
  <!-- FI 20151019 [21317] Add -->
  <xsl:template name="BizDST_TradeSubTotal">
    <!-- Liste des trades (trades du jour, trades en positions...etc) -->
    <xsl:param name="pEfsmlTrade"/>
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pSectionKey"/>

    <!--Un trade debtSecurityTransaction contient l'élément debtSecurityTransaction-->
    <xsl:variable name="vTradeList" select="$gTrade[@tradeId=$pEfsmlTrade/@tradeId and debtSecurityTransaction]"/>
    <xsl:variable name="vSubTotalList" select="$pSubTotal[@assetCategory='Bond' and @idI=$vTradeList/debtSecurityTransaction/productType/@OTCmlId]"/>

    <xsl:if test="$vTradeList">
      <xsl:variable name ="vBizDt" select="current()/@bizDt"/>
      <date bizDt="{$vBizDt}">
        <xsl:for-each select="$gRepository/market">
          <!-- Marché courant -->
          <xsl:variable name="vRepository-market" select="current()"/>
          <!-- Liste des assets du marché courant -->
          <xsl:variable name="vMarketRepository-asset" select="$gRepository/debtSecurity[idM=$vRepository-market/@OTCmlId]"/>
          <!-- Liste des trades rattachés au marché courant -->
          <xsl:variable name="vMarketTradeList" select="$vTradeList[debtSecurityTransaction/securityAsset/@OTCmlId=$vMarketRepository-asset/@OTCmlId]"/>

          <xsl:if test="$vMarketTradeList">
            <market OTCmlId="{$vRepository-market/@OTCmlId}">
              <!--Liste des trades (pEfsmlTrade) rattaché au marché courant-->
              <xsl:variable name="vMarketEfsmlTradeList" select="$pEfsmlTrade[@tradeId=$vMarketTradeList/@tradeId]"/>
              <!--Liste des ss-totaux des trades (pEfsmlTrade) rattaché au marché courant-->
              <xsl:variable name="vMarketSubTotalList" select="$vSubTotalList[@idAsset=$vMarketRepository-asset/@OTCmlId]"/>

              <xsl:variable name="vMarketSubTotalCopyNode">
                <xsl:copy-of select="$vMarketSubTotalList"/>
              </xsl:variable>
              <xsl:variable name="vMarketSubTotalCopy" select="msxsl:node-set($vMarketSubTotalCopyNode)/subTotal"/>
              <!-- Liste distinct des books -->
              <xsl:variable name="vSubTotal-idB_List" select="$vMarketSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idB',@idB))]"/>

              <xsl:for-each select="$vSubTotal-idB_List">

                <xsl:variable name="vSubTotal-idB" select="current()"/>
                <!-- Liste des trades rattaché au book courant -->
                <xsl:variable name="vBookTradeList" select="$vMarketTradeList[debtSecurityTransaction/RptSide/@Acct=$gRepository/book[@OTCmlId=$vSubTotal-idB/@idB]/identifier]"/>

                <xsl:if test="$vBookTradeList">

                  <book OTCmlId="{$vSubTotal-idB/@idB}">
                    <xsl:if test="string-length($pSectionKey) > 0">
                      <xsl:attribute name="sectionKey">
                        <xsl:value-of select="$pSectionKey"/>
                      </xsl:attribute>
                    </xsl:if>

                    <!-- Liste des trades (EfsmlTrade) rattaché au book courant -->
                    <xsl:variable name="vBookEfsmlTradeList" select="$vMarketEfsmlTradeList[@tradeId=$vBookTradeList/@tradeId]"/>
                    <!--Liste des ss-totaux rattaché au book courant -->
                    <xsl:variable name="vBookSubTotalList" select="$vMarketSubTotalList[@idB=$vSubTotal-idB/@idB]"/>
                    <xsl:variable name="vBookSubTotalCopyNode">
                      <xsl:copy-of select="$vBookSubTotalList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookSubTotalCopy" select="msxsl:node-set($vBookSubTotalCopyNode)/subTotal"/>

                    <xsl:variable name="vSubTotal-Instr_List" select="$vBookSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idI',@idI))]"/>
                    <!-- Liste distinct des instruments -->
                    <xsl:for-each select="$vSubTotal-Instr_List">

                      <xsl:variable name="vSubTotal-idI" select="current()"/>
                      <!-- Liste des trades rattaché à l'instument courant -->
                      <xsl:variable name="vInstrTradeList" select="$vBookTradeList[debtSecurityTransaction/productType/@OTCmlId=$vSubTotal-idI/@idI]"/>

                      <xsl:if test="$vInstrTradeList">
                        <!-- Liste des trades (EfsmlTrade) rattaché à l'instrument courant -->
                        <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrTradeList/@tradeId]"/>
                        <!-- Liste des ss-totaux des trades (EfsmlTrade) rattaché à l'instrument courant courant -->
                        <xsl:variable name="vInstrSubTotalList" select="$vBookSubTotalList[@idI=$vSubTotal-idI/@idI]"/>
                        <xsl:variable name="vInstrSubTotalCopyNode">
                          <xsl:copy-of select="$vInstrSubTotalList"/>
                        </xsl:variable>
                        <xsl:variable name="vInstrSubTotalCopy" select="msxsl:node-set($vInstrSubTotalCopyNode)/subTotal"/>
                        <!-- liste distinct des assets -->
                        <xsl:variable name="vSubTotal-Asset_List" select="$vInstrSubTotalCopy[generate-id()=generate-id(key('kSubTotal-idAsset',@idAsset))]"/>

                        <xsl:for-each select="$vSubTotal-Asset_List">
                          <!-- Pour l'asset courant-->
                          <xsl:variable name="vSubTotal-Asset" select="current()"/>
                          <!-- Liste des trades rattaché à l'asset courant-->
                          <xsl:variable name="vAssetTradeList" select="$vInstrTradeList[debtSecurityTransaction/securityAsset/@OTCmlId=$vSubTotal-Asset/@idAsset]"/>

                          <xsl:if test="$vAssetTradeList">

                            <!-- 1er trade en position rattaché à l'asset courant -->
                            <xsl:variable name ="vFirstTradePos" select="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vAssetTradeList/@tradeId][1]"/>
                            
                            <!-- Liste des trades (EfsmlTrade) rattaché à l'asset courant -->
                            <xsl:variable name="vAssetEfsmlTradeList" select="$vInstrEfsmlTradeList[@tradeId=$vAssetTradeList/@tradeId]"/>
                            <!-- Liste des ss-totaux rattaché à l'asset courant -->
                            <xsl:variable name="vAssetSubTotalList" select="$vInstrSubTotalList[@assetCategory=$vSubTotal-Asset/@assetCategory and @idAsset=$vSubTotal-Asset/@idAsset]"/>

                            <!-- Représent le repository de l'asset-->
                            <xsl:variable name="vAsset" select="$vMarketRepository-asset[@OTCmlId=$vSubTotal-Asset/@idAsset]"/>
                            <!-- Représent la devise de l'asset-->
                            <xsl:variable name="vCcy" select="$vAsset/idC/text()"/>

                            <asset OTCmlId="{$vSubTotal-Asset/@idAsset}"
                                   assetCategory="{$vSubTotal-Asset/@assetCategory}" assetName="debtSecurity"
                                   family="DSE" idI="{$vSubTotal-idI/@idI}"
                                   ccy="{$vCcy}">

                              <!-- Recherche du prix de clôture de l'asset. 
                                   Recherche du nbr de jour des intérêts courus (diff entre date Business et dernière tombée de coupon)
                                   Ces infos  sont présentes sur les trades en position -->
                              <!-- EG 20190730 Les sections sont : TRD|UNS|STL -->
                              <!-- EG 20190730 Affichage AssetMeasure et taux du coupon couru sur MKA -->
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
                                  <xsl:if test ="$vFirstTradePos/ain and string-length($vFirstTradePos/ain/@rate)>0">
                                    <xsl:attribute name ="accIntRt">
                                      <xsl:value-of select ="$vFirstTradePos/ain/@rate"/>
                                    </xsl:attribute>
                                  </xsl:if>
                                </closing>
                              </xsl:if>
                              
                               <xsl:variable name="vAccIntRt">
                                 <xsl:choose>
                                   <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS or $pSectionKey=$gcReportSectionKey_STL">
                                       <xsl:if test ="$vFirstTradePos/ain and string-length($vFirstTradePos/ain/@rate)>0">
                                         <xsl:value-of select="$vFirstTradePos/ain/@fmtRate"/>
                                       </xsl:if>
                                   </xsl:when>
                                   <xsl:otherwise>
                                     <!-- FI 20191203 read of "debtSecurityTransaction" element because "TRADEINSTRUMENT.ACCINTRATE" is not yet used -->
                                     <xsl:value-of select="$vAssetTradeList/debtSecurityTransaction/price/accruedInterestRate/text()"/>
                                   </xsl:otherwise>
                                 </xsl:choose>
                              </xsl:variable>
                              <xsl:choose>
                                <!-- Lecture des prix sur PosTrades pour obtenir le Pattern associé à fmtClrPx 
                                     Les élements trade enfants de stlPosaction et unsettledTrades ne possèdent pas l'attribut fmtClrPx -->
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_POS or $pSectionKey=$gcReportSectionKey_STL">
                                  <xsl:call-template name="Business_GetPattern">
                                    <xsl:with-param name="pCcy" select="$vCcy"/>
                                    <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                    <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetTradeList/@tradeId]/@fmtLastPx"/>
                                    <xsl:with-param name="pFmtClrPx" select="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vAssetTradeList/@tradeId]/@fmtClrPx"/>
                                    <xsl:with-param name="pFmtAvgPx" select="$vAssetSubTotalList/long/@fmtAvgPx | $vAssetSubTotalList/short/@fmtAvgPx"/>
                                    <xsl:with-param name="pFmtAccIntRt" select="msxsl:node-set($vAccIntRt)"/>
                                  </xsl:call-template>
                                </xsl:when>
                                <xsl:otherwise>
                                  <!--Remarque l'attribut fmtClrPx n'existe uniquement pour la section gcReportSectionKey_POS (Position détaillée en date business) -->
                                  <xsl:call-template name="Business_GetPattern">
                                    <xsl:with-param name="pCcy" select="$vCcy"/>
                                    <xsl:with-param name="pFmtQty" select="$vAssetEfsmlTradeList/@fmtQty"/>
                                    <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetTradeList/@tradeId]/@fmtLastPx"/>
                                    <xsl:with-param name="pFmtClrPx" select="$vAssetEfsmlTradeList/@fmtClrPx"/>
                                    <xsl:with-param name="pFmtAvgPx" select="$vAssetSubTotalList/long/@fmtAvgPx | $vAssetSubTotalList/short/@fmtAvgPx"/>
                                    <xsl:with-param name="pFmtAccIntRt" select="msxsl:node-set($vAccIntRt)"/>
                                  </xsl:call-template>
                                </xsl:otherwise>
                              </xsl:choose>


                              <xsl:for-each select="$vAssetEfsmlTradeList">

                                <xsl:variable name="vEfsmlTrade" select="current()"/>
                                <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vEfsmlTrade/@tradeId]"/>

                                <xsl:variable name ="vPriceType">
                                  <xsl:choose>
                                    <xsl:when test ="$vTrade/debtSecurityTransaction/price/dirtyPrice">
                                      <xsl:value-of select ="'dirty'"/>
                                    </xsl:when>
                                    <xsl:when test ="$vTrade/debtSecurityTransaction/price/cleanPrice">
                                      <xsl:value-of select ="'clean'"/>
                                    </xsl:when>
                                  </xsl:choose>
                                </xsl:variable>

                                <xsl:variable name ="vPrice">
                                  <xsl:choose>
                                    <xsl:when test ="$vPriceType = 'dirty'" >
                                      <xsl:value-of select ="$vTrade/debtSecurityTransaction/price/dirtyPrice/text()"/>
                                    </xsl:when>
                                    <xsl:when test ="$vPriceType = 'clean'" >
                                      <xsl:value-of select ="$vTrade/debtSecurityTransaction/price/cleanPrice/text()"/>
                                    </xsl:when>
                                  </xsl:choose>
                                </xsl:variable>

                                <!-- EG 20190730 Recherche du nombre de jours sur la bonne variable (sinon NaN) -->
                                <xsl:variable name ="vDays">
                                  <xsl:choose>
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_STL">
                                      <xsl:variable name ="vPosTrade" select ="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                      <xsl:if test ="$vPosTrade">
                                        <xsl:value-of select ="$vPosTrade/ain/@days"/>
                                      </xsl:if>
                                    </xsl:when>
                                    <xsl:otherwise>
                                      <xsl:value-of select ="$vEfsmlTrade/ain/@days"/>
                                    </xsl:otherwise>
                                  </xsl:choose>
                                </xsl:variable>

                                <!--<trade trdNum="{$vEfsmlTrade/@tradeId}" tradeId="{$vEfsmlTrade/@tradeId}"
                                       lastPx="{$vPrice}" typePx="{$vPriceType}"
                                       accIntRt="{$vTrade/debtSecurityTransaction/price/accruedInterestRate/text()}"
                                       accIntDays="{$vEfsmlTrade/ain/@days}"
                                       trdDt="{$vCommonTrade/@trdDt}"
                                       stlDt="{$vCommonTrade/@stlDt}" >-->
                                
                                <trade trdNum="{$vEfsmlTrade/@tradeId}" tradeId="{$vEfsmlTrade/@tradeId}"
                                       lastPx="{$vPrice}" typePx="{$vPriceType}"
                                       accIntRt="{$vTrade/debtSecurityTransaction/price/accruedInterestRate/text()}"
                                       accIntDays="{$vDays}"
                                       trdDt="{$vCommonTrade/@trdDt}"
                                       stlDt="{$vCommonTrade/@stlDt}" >

                                  <xsl:choose>
                                    <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or 
                                                     $pSectionKey=$gcReportSectionKey_UNS or 
                                                     $pSectionKey=$gcReportSectionKey_SET">
                                      <amount>
                                        <!--amount1 -->
                                        <xsl:choose>
                                          <xsl:when test ="$vPriceType='clean'">
                                            <amount1 det="PAM" withside="1" >
                                              <xsl:copy-of select="$vEfsmlTrade/pam"/>
                                            </amount1>
                                          </xsl:when>
                                          <xsl:when test ="$vPriceType='dirty'">
                                            <amount1 det="GAM" withside="1" >
                                              <xsl:copy-of select="$vEfsmlTrade/gam"/>
                                            </amount1>
                                          </xsl:when>
                                        </xsl:choose>
                                        <!--amount2 -->
                                        <xsl:choose>
                                          <xsl:when test ="$vEfsmlTrade/ain">
                                            <amount2 det="AIN" withside="1" >
                                              <xsl:copy-of select="$vEfsmlTrade/ain"/>
                                            </amount2>
                                          </xsl:when>
                                          <xsl:when test ="$vEfsmlTrade/net">
                                            <amount2 det="NET" withside="1" >
                                              <xsl:copy-of select="$vEfsmlTrade/net"/>
                                            </amount2>
                                          </xsl:when>
                                        </xsl:choose>
                                        <!--amount3 -->
                                        <xsl:choose>
                                          <xsl:when test ="$vEfsmlTrade/ain">
                                            <xsl:if test ="$vEfsmlTrade/net">
                                              <amount3 det="NET" withside="1" >
                                                <xsl:copy-of select="$vEfsmlTrade/net"/>
                                              </amount3>
                                            </xsl:if>
                                          </xsl:when>
                                          <xsl:when test ="$pSectionKey=$gcReportSectionKey_UNS">
                                            <xsl:if test ="$vEfsmlTrade/umg">
                                              <amount3 det="OTE" withside="1" >
                                                <xsl:copy-of select="$vEfsmlTrade/umg"/>
                                              </amount3>
                                            </xsl:if>
                                          </xsl:when>
                                        </xsl:choose>
                                        <!--amount4 -->
                                        <xsl:choose>
                                          <xsl:when test="$vEfsmlTrade/ain and $vEfsmlTrade/net and $pSectionKey=$gcReportSectionKey_UNS">
                                            <xsl:if test ="$vEfsmlTrade/umg">
                                              <amount4 det="OTE" withside="1" >
                                                <xsl:copy-of select="$vEfsmlTrade/umg"/>
                                              </amount4>
                                            </xsl:if>
                                          </xsl:when>
                                        </xsl:choose>
                                      </amount>
                                    </xsl:when>
                                    <xsl:when test =" $pSectionKey=$gcReportSectionKey_POS or $pSectionKey=$gcReportSectionKey_STL">
                                      <xsl:variable name ="vPosTrade" select ="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vEfsmlTrade/@tradeId]"/>
                                      <xsl:if test ="$vPosTrade">
                                        <amount>
                                          <xsl:choose>
                                            <xsl:when test ="$vPosTrade/pam">
                                              <amount1 det="MKP" withside="1" >
                                                <xsl:copy-of select="$vPosTrade/pam"/>
                                              </amount1>
                                              <amount2 det="MKA" withside="1" >
                                                <xsl:copy-of select="$vPosTrade/ain"/>
                                              </amount2>
                                              <amount3 det="OPV" withside="1" >
                                                <xsl:copy-of select="$vPosTrade/mkv"/>
                                              </amount3>
                                            </xsl:when>
                                            <xsl:otherwise>
                                              <amount1 det="OPV" withside="1" >
                                                <xsl:copy-of select="$vPosTrade/mkv"/>
                                              </amount1>
                                            </xsl:otherwise>
                                          </xsl:choose>
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

                              <xsl:choose>
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_TRD or $pSectionKey=$gcReportSectionKey_UNS or $pSectionKey=$gcReportSectionKey_SET">
                                  <subTotalAmount>
                                    <!-- amount1 -->
                                    <xsl:choose>
                                      <xsl:when test ="$vAssetTradeList[1]/debtSecurityTransaction/price/cleanPrice">
                                        <amount1 det="PAM" withside="1" >
                                          <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/pam[1]/@ccy"/>
                                          <xsl:if test ="$vAssetEfsmlTradeList/pam">
                                            <xsl:variable name="vTotal">
                                              <xsl:call-template name="GetAmount-amt">
                                                <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/pam"/>
                                                <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                              </xsl:call-template>
                                            </xsl:variable>
                                            <pam amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                          </xsl:if>
                                        </amount1>
                                      </xsl:when>
                                      <xsl:otherwise>
                                        <amount1 det="GAM" withside="1" >
                                          <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/gam[1]/@ccy"/>
                                          <xsl:if test ="$vAssetEfsmlTradeList/gam">
                                            <xsl:variable name="vTotal">
                                              <xsl:call-template name="GetAmount-amt">
                                                <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/gam"/>
                                                <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                              </xsl:call-template>
                                            </xsl:variable>
                                            <gam amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                          </xsl:if>
                                        </amount1>
                                      </xsl:otherwise>
                                    </xsl:choose>
                                    <!-- amount2 -->
                                    <xsl:choose>
                                      <xsl:when test ="$vAssetEfsmlTradeList/ain">
                                        <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/ain[1]/@ccy"/>
                                        <amount2 det="AIN" withside="1" >
                                          <xsl:variable name="vTotal">
                                            <xsl:call-template name="GetAmount-amt">
                                              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/ain"/>
                                              <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                            </xsl:call-template>
                                          </xsl:variable>
                                          <ain amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                        </amount2>
                                      </xsl:when>
                                      <xsl:when test ="$vAssetEfsmlTradeList/net">
                                        <amount2 det="NET" withside="1" >
                                          <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/net[1]/@ccy"/>
                                          <xsl:variable name="vTotal">
                                            <xsl:call-template name="GetAmount-amt">
                                              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/net"/>
                                              <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                            </xsl:call-template>
                                          </xsl:variable>
                                          <net amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                        </amount2>
                                      </xsl:when>
                                    </xsl:choose>
                                    <!-- amount3 -->
                                    <xsl:choose>
                                      <xsl:when test ="$vAssetEfsmlTradeList/ain and $vAssetEfsmlTradeList/net">
                                        <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/net[1]/@ccy"/>
                                        <amount3 det="NET" withside="1" >
                                          <xsl:variable name="vTotal">
                                            <xsl:call-template name="GetAmount-amt">
                                              <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/net"/>
                                              <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                            </xsl:call-template>
                                          </xsl:variable>
                                          <net amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                        </amount3>
                                      </xsl:when>
                                      <xsl:when test ="$pSectionKey=$gcReportSectionKey_UNS">
                                        <xsl:if test ="$vAssetEfsmlTradeList/umg">
                                          <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/umg[1]/@ccy"/>
                                          <amount3 det="OTE" withside="1" >
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
                                    <!-- amount4 -->
                                    <xsl:choose>
                                      <xsl:when test ="$vAssetEfsmlTradeList/ain and $vAssetEfsmlTradeList/net and $pSectionKey=$gcReportSectionKey_UNS">
                                        <xsl:if test ="$vAssetEfsmlTradeList/umg">
                                          <amount4 det="OTE" withside="1" >
                                            <xsl:variable name ="vTotalCcy" select ="$vAssetEfsmlTradeList/umg[1]/@ccy"/>
                                            <xsl:if test="$vAssetEfsmlTradeList/umg">
                                              <xsl:variable name="vTotal">
                                                <xsl:call-template name="GetAmount-amt">
                                                  <xsl:with-param name="pAmount" select="$vAssetEfsmlTradeList/umg"/>
                                                  <xsl:with-param name="pCcy" select="$vTotalCcy"/>
                                                </xsl:call-template>
                                              </xsl:variable>
                                              <umg amt="{$vTotal}" ccy="{$vTotalCcy}"/>
                                            </xsl:if>
                                          </amount4>
                                        </xsl:if>
                                      </xsl:when>
                                    </xsl:choose>
                                  </subTotalAmount>
                                </xsl:when>
                                <xsl:when test ="$pSectionKey=$gcReportSectionKey_POS or $pSectionKey=$gcReportSectionKey_STL">
                                  <subTotalAmount>
                                    <xsl:variable name ="vPosTradeLst" select ="$gPosTrades[@bizDt=$vBizDt]/trade[@tradeId=$vAssetEfsmlTradeList/@tradeId]"/>
                                    <amount1 det="OPV" withside="1" >
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
  <!--              Amendment/Transfers                                           -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- BizDSE_AmendmentTransfers                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for DSE AmendmentTransfers section
        - POC : Position correction
        - POT : Position transfer
        - LateTrade(4): Trades saisis en retard
        - PortFolioTransfer(42): Trades issus d'un transfert
        ................................................ -->
  <!-- FI 20151019 [21317] Add Template -->
  <xsl:template match="posActions | trades" mode="BizDST_AmendmentTransfers">

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

    <!-- Liste des trades ayant subi une transfert ou une correction -->
    <xsl:variable name="vPosActionTradesList" select="$vPosActionList/trades/trade"/>
    <!-- Liste des trades ayant subi une transfert ou une correction ou issu d'un transfert (de type trades/trade ) -->
    <xsl:variable name="vEfsmlTradeList" select="$vDailyTradesList | $vPosActionTradesList"/>
    <!-- Liste des trades ayant subi une transfert ou une correction ou issu d'un transfert (de type datadocument/trade) -->
    <xsl:variable name="vTradeList" select="$gTrade[@tradeId=$vEfsmlTradeList/@tradeId and debtSecurityTransaction]"/>

    <xsl:if test="$vTradeList">
      <date bizDt="{$vBizDt}">
        <!-- Pour chaque marché -->
        <xsl:for-each select="$gRepository/market">
          <!-- Représent le marché courant -->
          <xsl:variable name="vRepository-market" select="current()"/>
          <!-- Liste des assets debtSecurity du marché courant-->
          <xsl:variable name="vMarketRepository-asset" select="$gRepository/debtSecurity[idM=$vRepository-market/@OTCmlId]"/>
          <!-- Liste des trades du marché courant -->
          <xsl:variable name="vMarketTradeList" select="$vTradeList[debtSecurityTransaction/securityAsset/@OTCmlId=$vMarketRepository-asset/@OTCmlId]"/>
          <xsl:if test="$vMarketTradeList">
            <!-- Représente le marché courant -->
            <market OTCmlId="{$vRepository-market/@OTCmlId}">

              <xsl:variable name="vMarketTradeListNode">
                <xsl:copy-of select="$vMarketTradeList"/>
              </xsl:variable>
              <xsl:variable name="vMarketTradeListCopy" select="msxsl:node-set($vMarketTradeListNode)/trade"/>
              <!-- Liste des books en rapport avec le marché courant -->
              <xsl:variable name="vRptSide-Acct_List" select="$vMarketTradeListCopy/debtSecurityTransaction/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

              <xsl:for-each select="$vRptSide-Acct_List">
                <xsl:variable name="vAcct" select="current()/@Acct"/>
                <!-- Liste des trades en rapport avec le book courant  -->
                <xsl:variable name="vBookTradeList" select="$vMarketTradeList[debtSecurityTransaction/RptSide/@Acct=$vAcct]"/>

                <xsl:if test="$vBookTradeList">
                  <book OTCmlId="{$gRepository/book[identifier=$vAcct]/@OTCmlId}" sectionKey="{$gcReportSectionKey_AMT}">

                    <xsl:variable name="vBookTradeListCopyNode">
                      <xsl:copy-of select="$vBookTradeList"/>
                    </xsl:variable>
                    <xsl:variable name="vBookTradeListCopy" select="msxsl:node-set($vBookTradeListCopyNode)/trade"/>

                    <xsl:variable name="vInstrList" select="$vBookTradeListCopy/debtSecurityTransaction/productType[generate-id()=generate-id(key('kProduct-idI',@OTCmlId))]"/>

                    <!-- Liste distinct des instruments -->
                    <xsl:for-each select="$vInstrList">
                      <xsl:variable name="vIdI" select="current()/@OTCmlId"/>

                      <!-- Liste des trades rattachés à l'instrument courant -->
                      <xsl:variable name="vInstrTradeList" select="$vBookTradeList[debtSecurityTransaction/productType/@OTCmlId=$vIdI]"/>
                      <xsl:if test="$vInstrTradeList">

                        <xsl:variable name="vAssetTradeListCopyNode">
                          <xsl:copy-of select="$vInstrTradeList/debtSecurityTransaction/securityAsset"/>
                        </xsl:variable>
                        <xsl:variable name="vAssetTradeListCopy" select="msxsl:node-set($vAssetTradeListCopyNode)/securityAsset"/>

                        <xsl:variable name="vAssetList" select="$vAssetTradeListCopy[generate-id()=generate-id(key('kOTCmlId',@OTCmlId)[1])]"/>

                        <xsl:for-each select="$vAssetList">
                          <xsl:variable name ="vIdAsset" select ="current()/@OTCmlId"/>

                          <!-- Liste des trades rattaché à l'asset courant-->
                          <xsl:variable name="vAssetTradeList" select="$vInstrTradeList[debtSecurityTransaction/securityAsset/@OTCmlId=$vIdAsset]"/>
                          <xsl:variable name="vAssetCommonTradeList" select="$gCommonData/trade[@tradeId=$vAssetTradeList/@tradeId]"/>
                          <xsl:variable name="vAssetPosActionList" select="$vPosActionList[trades/trade/@tradeId=$vAssetTradeList/@tradeId]"/>
                          <xsl:variable name="vAssetDailyTradesList" select="$vDailyTradesList[@tradeId=$vAssetTradeList/@tradeId]"/>
                          <xsl:variable name="vCcy" select="$gRepository/debtSecurity[@OTCmlId=$vIdAsset]/idC/text()"/>

                          <asset OTCmlId="{$vIdAsset}" assetCategory="Bond" assetName="debtSecurity" family="DSE" idI="{$vIdI}">
                            <xsl:call-template name="Business_GetPattern">
                              <xsl:with-param name="pCcy" select="$vCcy"/>
                              <xsl:with-param name="pFmtQty" select="$vAssetDailyTradesList/@fmtQty | $vAssetPosActionList/@fmtQty"/>
                              <xsl:with-param name="pFmtLastPx" select="$gCommonData/trade[@tradeId=$vAssetTradeList/@tradeId]/@fmtLastPx"/>
                            </xsl:call-template>

                            <!--Pour un transfert, Coté Source: PosAction est toujours alimenté-->
                            <!--Pour une correction: PosAction est toujours alimenté-->
                            <xsl:for-each select="$vAssetPosActionList">

                              <xsl:variable name="vPosAction" select="current()"/>
                              <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vPosAction/trades/trade/@tradeId]"/>
                              <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vTrade/@tradeId]"/>

                              <posAction OTCmlId="{$vPosAction/@OTCmlId}" requestType="{$vPosAction/@requestType}">
                                <xsl:variable name ="vPriceType">
                                  <xsl:choose>
                                    <xsl:when test ="$vTrade/debtSecurityTransaction/price/dirtyPrice">
                                      <xsl:value-of select ="'dirty'"/>
                                    </xsl:when>
                                    <xsl:when test ="$vTrade/debtSecurityTransaction/price/cleanPrice">
                                      <xsl:value-of select ="'clean'"/>
                                    </xsl:when>
                                  </xsl:choose>
                                </xsl:variable>

                                <xsl:variable name ="vPrice">
                                  <xsl:choose>
                                    <xsl:when test ="$vPriceType = 'dirty'" >
                                      <xsl:value-of select ="$vTrade/debtSecurityTransaction/price/dirtyPrice/text()"/>
                                    </xsl:when>
                                    <xsl:when test ="$vPriceType = 'clean'" >
                                      <xsl:value-of select ="$vTrade/debtSecurityTransaction/price/cleanPrice/text()"/>
                                    </xsl:when>
                                  </xsl:choose>
                                </xsl:variable>

                                <trade trdNum="{$vTrade/@tradeId}" tradeId="{$vTrade/@tradeId}"
                                       lastPx="{$vPrice}"  typePx="{$vPriceType}"
                                       accIntRt="{$vTrade/debtSecurityTransaction/price/accruedInterestRate/text()}"
                                       trdDt="{$vCommonTrade/@trdDt}">
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

                                <!--Sur un transfert, Coté source: trade2 n'est pas alimenté pour un transfert externe-->
                                <!--Sur une correction: trade2 n'est pas alimenté-->
                                <xsl:if test="$vPosAction/trades/trade2">
                                  <xsl:variable name="vTrade2" select="$gTrade[@tradeId=$vPosAction/trades/trade2/@tradeId]"/>
                                  <trade2 trdNum="{$vTrade2/@tradeId}" tradeId="{$vTrade2/@tradeId}"/>
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
                              <xsl:variable name="vTrade" select="$vAssetTradeList[@tradeId=$vDailyTrade/@tradeId]"/>
                              <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$vTrade/@tradeId]"/>

                              <xsl:variable name ="vPriceType">
                                <xsl:choose>
                                  <xsl:when test ="$vTrade/debtSecurityTransaction/price/dirtyPrice">
                                    <xsl:value-of select ="'dirty'"/>
                                  </xsl:when>
                                  <xsl:when test ="$vTrade/debtSecurityTransaction/price/cleanPrice">
                                    <xsl:value-of select ="'clean'"/>
                                  </xsl:when>
                                </xsl:choose>
                              </xsl:variable>

                              <xsl:variable name ="vPrice">
                                <xsl:choose>
                                  <xsl:when test ="$vPriceType = 'dirty'" >
                                    <xsl:value-of select ="$vTrade/debtSecurityTransaction/price/dirtyPrice/text()"/>
                                  </xsl:when>
                                  <xsl:when test ="$vPriceType = 'clean'" >
                                    <xsl:value-of select ="$vTrade/debtSecurityTransaction/price/cleanPrice/text()"/>
                                  </xsl:when>
                                </xsl:choose>
                              </xsl:variable>

                              <trade trdNum="{$vTrade/@tradeId}" tradeId="{$vTrade/@tradeId}"
                                     lastPx="{$vPrice}" typePx="{$vPriceType}"
                                     accIntRt="{$vTrade/debtSecurityTransaction/price/accruedInterestRate/text()}"
                                     trdDt="{$vCommonTrade/@trdDt}"
                                     stlDt="{$vCommonTrade/@stlDt}">

                                <xsl:variable name="vTradeSrc" select="$vDailyTrade/tradeSrc"/>
                                <xsl:if test="$vTradeSrc">
                                  <tradeSrc trdNum="{$vTradeSrc/@tradeId}" tradeId="{$vTradeSrc/@tradeId}"/>
                                </xsl:if>

                                <amount>
                                  <!--amount1 -->
                                  <amount1 det="GAM" withside="1" >
                                    <xsl:copy-of select="$vDailyTrade/gam"/>
                                  </amount1>
                                </amount>
                              </trade>
                            </xsl:for-each>

                            <!-- Mise en commentaire car il n'y a pas d'affichage ds ss-totaux 
                            <xsl:call-template name="Business_GetFeeSubtotal">
                              <xsl:with-param name="pFee" select="$vAssetPosActionList/fee | $vAssetDailyTradesList/fee"/>
                            </xsl:call-template>-->


                          </asset>
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
</xsl:stylesheet>
