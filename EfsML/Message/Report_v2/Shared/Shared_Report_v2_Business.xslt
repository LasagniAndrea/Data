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
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140905                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                         Variables                                              -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->
  <!-- ................................................ -->
  <!-- Business_AccountSummary-group                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for Account Summary Section
       ................................................ -->
  <xsl:template name="Business_AccountSummary-group">

    <xsl:variable name="vAllCurrencyNode">
      <xsl:for-each select="$gCBTrade/cashBalanceReport/cashBalanceStream">
        <xsl:sort select="@ccy"/>
        <xsl:variable name="vCcyPattern">
          <xsl:call-template name="GetCcyPattern">
            <xsl:with-param name="pCcy" select="current()/@ccy"/>
          </xsl:call-template>
        </xsl:variable>
        <currency ccy="{@ccy}" pattern="{$vCcyPattern}"/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="vAllCurrency" select="msxsl:node-set($vAllCurrencyNode)/currency"/>
    <xsl:variable name="vCcyCount" select="count($vAllCurrency)"/>

    <!-- FI 20150921 [21311] 
         méthode UK : cashBalanceReport/exchangeCashBalanceStream est généré par le process CashBalance et existe systématiquement en 
         Méthode DEFAULT : cashBalanceReport/exchangeCashBalanceStream est généré par le process de messagerie (Ce calcul est fait uniquement s'il existe plusieurs streams)
    -->
    <xsl:variable name="vIsHideBaseCcy" select="($vCcyCount = 1 and $vAllCurrency[1]/@ccy=$gExchangeCcy) or not($gCBTrade/cashBalanceReport/exchangeCashBalanceStream)"/>

    <xsl:variable name="vGroupCount">
      <xsl:choose>
        <xsl:when test="$vIsHideBaseCcy">
          <xsl:value-of select="ceiling(($vCcyCount) div $gBlockSettings_Summary/@number-ccy)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="ceiling(($vCcyCount + 1) div $gBlockSettings_Summary/@number-ccy)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="Business_GetCurrencyGroup">
      <xsl:with-param name="pGroupNumber" select="number('1')"/>
      <xsl:with-param name="pGroupCount" select="$vGroupCount"/>
      <xsl:with-param name="pIsHideBaseCcy" select="$vIsHideBaseCcy"/>
      <xsl:with-param name="pAllCurrency" select="$vAllCurrency"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_JournalEntries-book                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for Journal Entries Section
       ................................................ -->
  <!-- FI 20150914 [XXXXX] Modify -->
  <!-- FI 20160623 [XXXXX] Modify -->
  <xsl:template name="Business_JournalEntries-book">

    <xsl:for-each select="$gRepository/book">

      <xsl:variable name="vRepository-book" select="current()"/>
      <!-- RD 20160616 [22244] En dur les Custody fees à zéro sont ignorés (voir modif FI 20150914 [21103] ci-dessous) -->
      <xsl:variable name="vBookJournalEntries" select="$gPosSynthetics/posSynthetic[@idB=$vRepository-book/@OTCmlId and fda] | 
                    $gPosSynthetics/posSynthetic[@idB=$vRepository-book/@OTCmlId and bwa] | 
                    $gStlPosSynthetics/posSynthetic[@idB=$vRepository-book/@OTCmlId and skp[@amt!=0]] | 
                    $gCashPayments/cashPayment[@idB=$vRepository-book/@OTCmlId]"/>

      <xsl:if test="$vBookJournalEntries">

        <book OTCmlId="{$vRepository-book/@OTCmlId}">

          <xsl:for-each select="$gRepository/currency">

            <xsl:variable name="vCurrency" select="current()"/>
            <xsl:variable name="vCurrencyJournalEntries" select="$vBookJournalEntries[fda[@ccy=$vCurrency/identifier]] | 
                          $vBookJournalEntries[bwa[@ccy=$vCurrency/identifier]] | 
                          $vBookJournalEntries[skp[@ccy=$vCurrency/identifier]] | 
                          $vBookJournalEntries[@ccy=$vCurrency/identifier]"/>

            <xsl:if test="$vCurrencyJournalEntries">
              <xsl:variable name="vCcyPattern">
                <xsl:call-template name="GetCcyPattern">
                  <xsl:with-param name="pCcy" select="$vCurrency/identifier"/>
                </xsl:call-template>
              </xsl:variable>
              <currency ccy="{$vCurrency/identifier}">
                <pattern ccy="{$vCcyPattern}"/>
                <!-- FI 20150914 [XXXXX] Add lbl attribute -->
                <xsl:for-each select="$vCurrencyJournalEntries[name()='posSynthetic']">

                  <xsl:variable name="vPosSynthetic" select="current()"/>
                  <xsl:variable name="vPosSyntheticParent" select="parent::node()"/>
                  <xsl:variable name="vAssetName">
                    <xsl:call-template name="GetAssetNodeName">
                      <xsl:with-param name="pAssetCategory" select="$vPosSynthetic/@assetCategory"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <!-- FI 20160623 [XXXXX] vBizDt valorisé à partir de l'élément $vPosSyntheticParent -->
                  <xsl:variable name="vBizDt">
                    <xsl:value-of select="$vPosSyntheticParent/@bizDt"/>
                  </xsl:variable>
                  <!-- FI 20160623 [XXXXX] attributes recDt and valDt -->
                  <xsl:if test="$vPosSynthetic/bwa">
                    <posSynthetic OTCmlId="{$vPosSynthetic/@idAsset}"
                                  recDt="{$vBizDt}"
                                  bizDt="{$vBizDt}"
                                  eventType="BWA"
                                  keyCategory="{concat($vPosSynthetic/@assetCategory, '+', $vPosSynthetic/@idAsset)}"
                                  keyName="{concat($vAssetName, '+', $vPosSynthetic/@idAsset)}"
                                  lbl="{$vPosSynthetic/bwa/@lbl}"/>
                  </xsl:if>
                  <xsl:if test="$vPosSynthetic/fda">
                    <posSynthetic OTCmlId="{$vPosSynthetic/@idAsset}"
                                  recDt="{$vBizDt}"
                                  bizDt="{$vBizDt}"
                                  eventType="FDA"
                                  keyCategory="{concat($vPosSynthetic/@assetCategory, '+', $vPosSynthetic/@idAsset)}"
                                  keyName="{concat($vAssetName, '+', $vPosSynthetic/@idAsset)}"
                                  lbl="{$vPosSynthetic/fda/@lbl}"/>

                  </xsl:if>
                  <!-- FI 20150914 [21103] En dur les Custody fees à zéro sont ignorés 
                  Il faudrait branché un specified dans la modelisation du cashBalance pour eviter ce codage en dur
                  -->
                  <xsl:if test="$vPosSynthetic/skp[@amt!=0]">
                    <posSynthetic OTCmlId="{$vPosSynthetic/@idAsset}"
                                  recDt="{$vBizDt}"
                                  bizDt="{$vBizDt}"
                                  eventType="CUS"
                                  keyCategory="{concat($vPosSynthetic/@assetCategory, '+', $vPosSynthetic/@idAsset)}"
                                  keyName="{concat($vAssetName, '+', $vPosSynthetic/@idAsset)}"
                                  lbl="{$vPosSynthetic/skp/@lbl}"/>
                  </xsl:if>
                </xsl:for-each>

                <!-- FI 20150914 [XXXXX] Add lbl attribute -->
                <xsl:for-each select="$vCurrencyJournalEntries[name()='cashPayment']">
                  <xsl:variable name="vCashPayment" select="current()"/>
                  <xsl:variable name="vCashPaymentParent" select="parent::node()"/>
                  <!-- FI 20160623 [XXXXX] attributes recDt and valDt -->
                  <cashPayment OTCmlId="{$vCashPayment/@OTCmlId}"
                               recDt="{$vCashPayment/@trdDt}"
                               bizDt="{$vCashPaymentParent/@bizDt}"
                               eventType="{$vCashPayment/@eventType}"
                               paymentType="{$vCashPayment/@paymentType}"
                               lbl="{$vCashPayment/@lbl}"/>
                </xsl:for-each>
              </currency>
            </xsl:if>
          </xsl:for-each>
        </book>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!--                                                  -->
  <!-- Business_Collaterals-book                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for Collateral Section
       ................................................ -->
  <!-- FI 20160530 [21885] Add -->
  <xsl:template name="Business_Collaterals-book">
    <xsl:for-each select="$gRepository/book">
      <xsl:variable name="vRepository-book" select="current()"/>
      <xsl:variable name="vBookCollateral" select="$gCollaterals/collateral[@idB=$vRepository-book/@OTCmlId]"/>

      <xsl:if test="$vBookCollateral">
        <book OTCmlId="{$vRepository-book/@OTCmlId}">

          <xsl:for-each select="$gRepository/currency">
            <xsl:variable name="vCurrency" select="current()"/>
            <xsl:variable name="vCurrencyCollateral" select="$vBookCollateral[@ccy=$vCurrency/identifier]"/>

            <xsl:if test="$vCurrencyCollateral">
              <xsl:variable name="vCcyPattern">
                <xsl:call-template name="GetCcyPattern">
                  <xsl:with-param name="pCcy" select="$vCurrency/identifier"/>
                  <xsl:with-param name="pFmtQty" select="$vCurrencyCollateral/@fmtQty"/>
                </xsl:call-template>
              </xsl:variable>

              <currency ccy="{$vCurrency/identifier}">
                <pattern ccy="{$vCcyPattern}"/>
                <xsl:for-each select="$vCurrencyCollateral">
                  <xsl:sort select="./parent::node()/@bizDt" data-type="text"/>
                  <xsl:sort select="./@lbl" data-type="text"/>
                  <xsl:sort select="./@colId" data-type="text"/>

                  <xsl:variable name="vCollateral" select="current()"/>
                  <xsl:variable name="vBizDt" select="./parent::node()/@bizDt"/>
                  <collateral bizDt="{$vBizDt}"
                              OTCmlId="{$vCollateral/@OTCmlId}"
                              colId ="{$vCollateral/@colId}"
                              displayName ="{$vCollateral/@displayName}"
                              description ="{$vCollateral/@description}"
                              lbl="{$vCollateral/@lbl}"
                              side="{$vCollateral/@side}"
                              amt="{$vCollateral/@amt}"
                              ccy="{$vCollateral/@ccy}">
                    <xsl:if test ="$vCollateral/@fmtQty">
                      <xsl:attribute name ="fmtQty">
                        <xsl:value-of select="$vCollateral/@fmtQty"/>
                      </xsl:attribute>
                    </xsl:if>

                    <xsl:for-each select="./haircut[@idA>0]">
                      <xsl:sort select="$gRepository/actor[@OTCmlId=current()/@idA]/identifier" data-type="text"/>
                      <haircut>
                        <xsl:attribute name ="value">
                          <xsl:value-of select ="./@value"/>
                        </xsl:attribute>
                        <xsl:attribute name ="cssCode">
                          <xsl:choose>
                            <xsl:when test ="string-length($gRepository/actor[@OTCmlId=current()/@idA]/ISO10383_ALPHA4)>0">
                              <xsl:value-of select ="$gRepository/actor[@OTCmlId=current()/@idA]/ISO10383_ALPHA4"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select ="$gRepository/actor[@OTCmlId=current()/@idA]/identifier"/>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:attribute>
                      </haircut>
                    </xsl:for-each>
                    <xsl:copy-of select ="./haircut[not(@idA)]"/>
                  </collateral>
                </xsl:for-each>
              </currency>
            </xsl:if>
          </xsl:for-each>
        </book>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!--                                                  -->
  <!-- Business_UnderlyingStocks-book                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for UnderlyingStocks Section
       ................................................ -->
  <!-- FI 20160613 [22256] Add -->
  <xsl:template name="Business_UnderlyingStocks-book">
    <xsl:for-each select="$gRepository/book">
      <xsl:variable name="vRepository-book" select="current()"/>
      <xsl:variable name="vBookUnderlyingStock" select="$gUnderlyingStocks/underlyingStock[@idB=$vRepository-book/@OTCmlId]"/>

      <xsl:if test="$vBookUnderlyingStock">
        <book OTCmlId="{$vRepository-book/@OTCmlId}">
          <xsl:for-each select="$vBookUnderlyingStock">
            <xsl:sort select="./parent::node()/@bizDt" data-type="text"/>
            <!--lbl => Représente l'asset (donnée customizable)-->
            <xsl:sort select="./@lbl" data-type="text"/>
            <!--unsId => Identifier du dépôt -->
            <xsl:sort select="./@unsId" data-type="text"/>

            <xsl:variable name="vCurrencyUnderlyingStock" select="current()"/>
            <xsl:variable name="vUnderlyingStock" select="current()"/>
            <xsl:variable name="vBizDt" select="./parent::node()/@bizDt"/>

            <underlyingStock bizDt="{$vBizDt}"
                        OTCmlId="{$vUnderlyingStock/@OTCmlId}"
                        unsId ="{$vUnderlyingStock/@unsId}"
                        displayName ="{$vUnderlyingStock/@displayName}"
                        description ="{$vUnderlyingStock/@description}"
                        stockCover="{$vUnderlyingStock/@stockCover}"
                        lbl="{$vUnderlyingStock/@lbl}"
                        qtyAvailable="{$vUnderlyingStock/@qtyAvailable}">
              <xsl:choose>
                <xsl:when test ="$vUnderlyingStock/@stockCover='PriorityStockFuture'">
                  <fut qtyUsed="{$vUnderlyingStock/@qtyUsedFut}"/>
                  <opt qtyUsed="{$vUnderlyingStock/@qtyUsedOpt}"/>
                </xsl:when>
                <xsl:when test ="$vUnderlyingStock/@stockCover='PriorityStockOption'">
                  <opt qtyUsed="{$vUnderlyingStock/@qtyUsedOpt}"/>
                  <fut qtyUsed="{$vUnderlyingStock/@qtyUsedFut}"/>
                </xsl:when>
                <xsl:when test ="$vUnderlyingStock/@stockCover='StockFuture'">
                  <fut qtyUsed="{$vUnderlyingStock/@qtyUsedFut}"/>
                </xsl:when>
                <xsl:when test ="$vUnderlyingStock/@stockCover='StockOption'">
                  <opt qtyUsed="{$vUnderlyingStock/@qtyUsedOpt}"/>
                </xsl:when>
              </xsl:choose>
            </underlyingStock>
          </xsl:for-each>
        </book>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Tools                                                         -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Business_GetPattern                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get pattrens 
       ................................................ -->
  <!-- FI 20151009 [21311] Refactoring -->
  <!-- FI 20151019 [21317] Modify (add pFmtAccIntRt) -->
  <xsl:template name="Business_GetPattern">
    <xsl:param name="pCcy"/>
    <!-- Liste de qty. Lorsque présent génère l'attribut qty (pattern obtenu à partir de la liste pFmtQty)-->
    <xsl:param name="pFmtQty"/>
    <!-- Liste de prix. Lorsque présent génère l'attribut lastPx (pattern obtenu à partir de la liste pFmtLastPx)-->
    <xsl:param name="pFmtLastPx"/>
    <!-- Liste de prix. Lorsque présent génère l'attribut clrPx (pattern obtenu à partir de la liste pFmtClrPx)-->
    <xsl:param name="pFmtClrPx"/>
    <!-- Liste de prix. Lorsque présent génère l'attribut unlPx (pattern obtenu à partir de la liste pFmtUnlPx)-->
    <xsl:param name="pFmtUnlPx"/>
    <!-- Liste de prix. Lorsque présent génère l'attribut stike (pattern obtenu à partir de la liste pFmtStrike)-->
    <xsl:param name="pFmtStrike"/>
    <!-- Liste de prix. Lorsque présent génère l'attribut avgPx (pattern obtenu à partir de la liste pFmtAvgPx)-->
    <xsl:param name="pFmtAvgPx"/>
    <!-- Liste de prix. Lorsque présent génère l'attribut accIntRt (pattern obtenu à partir de la liste pFmtAccIntRt)-->
    <xsl:param name="pFmtAccIntRt"/>

    <xsl:variable name="vCcyPattern">
      <xsl:call-template name="GetCcyPattern">
        <xsl:with-param name="pCcy" select="$pCcy"/>
      </xsl:call-template>
    </xsl:variable>

    <pattern ccy="{$vCcyPattern}">
      <xsl:if test ="$pFmtQty">
        <xsl:attribute name="qty">
          <xsl:call-template name="GetPricesPattern">
            <xsl:with-param name="pPrices" select="$pFmtQty"/>
            <xsl:with-param name="pHighPrec" select="number(0)"/>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:if>

      <xsl:if test ="$pFmtLastPx">
        <xsl:variable name="vIsFmtPrice">
          <xsl:call-template name="IsFmtPrice">
            <xsl:with-param name="pPrice" select="$pFmtLastPx[1]"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$vIsFmtPrice='false'">
          <xsl:attribute name="lastPx">
            <xsl:call-template name="GetPricesPattern">
              <xsl:with-param name="pPrices" select="$pFmtLastPx"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$pFmtAccIntRt">
        <xsl:attribute name="accIntRt">
          <xsl:call-template name="GetPricesPattern">
            <xsl:with-param name="pPrices" select="$pFmtAccIntRt"/>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:if>

      <xsl:if test ="$pFmtClrPx">
        <xsl:variable name="vIsFmtPrice">
          <xsl:call-template name="IsFmtPrice">
            <xsl:with-param name="pPrice" select="$pFmtClrPx[1]"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$vIsFmtPrice='false'">
          <xsl:attribute name="clrPx">
            <xsl:call-template name="GetPricesPattern">
              <xsl:with-param name="pPrices" select="$pFmtClrPx"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$pFmtAvgPx">
        <xsl:variable name="vIsFmtPrice">
          <xsl:call-template name="IsFmtPrice">
            <xsl:with-param name="pPrice" select="$pFmtAvgPx[1]"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$vIsFmtPrice='false'">
          <xsl:attribute name="avgPx">
            <xsl:call-template name="GetPricesPattern">
              <xsl:with-param name="pPrices" select="$pFmtAvgPx"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$pFmtUnlPx">
        <xsl:variable name="vIsFmtPrice">
          <xsl:call-template name="IsFmtPrice">
            <xsl:with-param name="pPrice" select="$pFmtUnlPx[1]"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$vIsFmtPrice='false'">
          <xsl:attribute name="unlPx">
            <xsl:call-template name="GetPricesPattern">
              <xsl:with-param name="pPrices" select="$pFmtUnlPx"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>

      <xsl:if test ="$pFmtStrike">
        <xsl:variable name="vIsFmtPrice">
          <xsl:call-template name="IsFmtPrice">
            <xsl:with-param name="pPrice" select="$pFmtStrike[1]"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$vIsFmtPrice='false'">
          <xsl:attribute name="strike">
            <xsl:call-template name="GetPricesPattern">
              <xsl:with-param name="pPrices" select="$pFmtStrike"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>
    </pattern>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetFeeSubtotal                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get fee subtotal 
       ................................................ -->
  <xsl:template name="Business_GetFeeSubtotal">
    <xsl:param name="pFee"/>

    <xsl:variable name="vFeeSubTotalNode">
      <xsl:call-template name="Business_GetFee">
        <xsl:with-param name="pFee" select="$pFee"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vFeeSubTotal" select="msxsl:node-set($vFeeSubTotalNode)/fee"/>
    <xsl:if test="$vFeeSubTotal">
      <subTotal>
        <xsl:copy-of select="$vFeeSubTotal"/>
      </subTotal>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetCurrencyGroup                        -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify (chgt de signature) -->
  <xsl:template name="Business_GetCurrencyGroup">
    <!-- Index de 1 à N -->
    <xsl:param name="pGroupNumber"/>
    <!-- Nbr de groupe au total -->
    <xsl:param name="pGroupCount"/>
    <!-- true() s'il n'existe pas de colonne Contrevaleur -->
    <xsl:param name="pIsHideBaseCcy"/>
    <!-- Représente les devises des streams -->
    <xsl:param name="pAllCurrency"/>

    <xsl:variable name="vIsLastGroup" select="$pGroupNumber = $pGroupCount"/>

    <xsl:variable name="vStart" select="number(($pGroupNumber - 1) * $gBlockSettings_Summary/@number-ccy + 1)"/>
    <xsl:variable name="vEnd" select="number($pGroupNumber * $gBlockSettings_Summary/@number-ccy)"/>
    <xsl:variable name="vGroupCurrency" select="$pAllCurrency[position() >= $vStart and  $vEnd >= position()]"/>

    <xsl:variable name="vGroupEmptyCurrencyCount">
      <xsl:choose>
        <xsl:when test="$vIsLastGroup = false()">
          <xsl:value-of select="number('0')"/>
        </xsl:when>
        <xsl:when test="$pIsHideBaseCcy = false()">
          <xsl:value-of select="$gBlockSettings_Summary/@number-ccy - count($vGroupCurrency) - 1"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gBlockSettings_Summary/@number-ccy - count($vGroupCurrency)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--RD 20170119 [22797] 
    Afficher les taux de change si le pavé "Base currency" est à afficher-->
    <group sort="{$pGroupNumber}" empty-currency="{$vGroupEmptyCurrencyCount}" isHideBaseCcy="{$pIsHideBaseCcy}">
      <xsl:variable name ="vCBStreams" select ="$gCBTrade/cashBalanceReport/cashBalanceStream"/>
      <xsl:variable name ="vAllFee" select="$vCBStreams/cashAvailable/constituent/cashFlows/constituent/fee"/>
      <xsl:variable name ="vAllSafekeeping" select="$vCBStreams/cashAvailable/constituent/cashFlows/constituent/safekeeping"/>

      <xsl:for-each select="$vGroupCurrency">
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>
        <currency >
          <xsl:copy-of select="$vGroupCurrency[@ccy=$vCurrentCcy]/@*"/>
          <!-- FI 20150921 [21311] si methode CSBDEFAULT mise en place du détail associé à l'Appel/restitution de deposit) -->
          <xsl:if test ="$gCBTrade/cashBalanceReport/settings/cashBalanceMethod/text()='CSBDEFAULT'">
            <marginCallDet>
              <previous>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/previousMarginConstituent/marginRequirement"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/previousMarginConstituent/collateralAvailable"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/previousMarginConstituent/collateralUsed"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/previousMarginConstituent/cashAvailable"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/previousMarginConstituent/cashUsed"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/previousMarginConstituent/uncoveredMarginRequirement"/>
              </previous>
              <current>
                <marginRequirement>
                  <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/marginRequirement/@*"/>
                </marginRequirement>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/collateralAvailable"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/collateralUsed"/>
                <cashAvailable>
                  <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/cashAvailable/@*"/>
                </cashAvailable>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/cashUsed"/>
                <xsl:copy-of select="$vCBStreams[@ccy=$vCurrentCcy]/uncoveredMarginRequirement"/>
              </current>
            </marginCallDet>
          </xsl:if>
        </currency>
      </xsl:for-each>

      <xsl:call-template name="Business_GetCurrencyFee">
        <xsl:with-param name="pFee" select="$vAllFee[@ccy=$vGroupCurrency/@ccy]"/>
      </xsl:call-template>
      <xsl:call-template name="Business_GetCurrencySafekeeping">
        <xsl:with-param name="pFee" select="$vAllSafekeeping[@ccy=$vGroupCurrency/@ccy]"/>
      </xsl:call-template>

      <xsl:if test="$vIsLastGroup and $pIsHideBaseCcy = false()">
        <xsl:variable name ="vCBExchangeStream" select ="$gCBTrade/cashBalanceReport/exchangeCashBalanceStream"/>
        <xsl:variable name ="vExchangeFee" select="$gCBTrade/cashBalanceReport/exchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/fee"/>
        <xsl:variable name ="vExchangeSafekeeping" select="$gCBTrade/cashBalanceReport/exchangeCashBalanceStream/cashAvailable/constituent/cashFlows/constituent/safekeeping"/>

        <exchangeCurrency ccy="{$gExchangeCcy}" pattern="{$gExchangeCcyPattern}">
          <!-- FI 20150921 [21311] si methode CSBDEFAULT mise en place du détail associé à l'Appel/restitution de deposit) -->
          <xsl:if test ="$vCBExchangeStream and $gCBTrade/cashBalanceReport/settings/cashBalanceMethod/text()='CSBDEFAULT'">
            <marginCallDet>
              <previous>
                <xsl:copy-of select="$vCBExchangeStream/previousMarginConstituent/marginRequirement"/>
                <xsl:copy-of select="$vCBExchangeStream/previousMarginConstituent/collateralAvailable"/>
                <xsl:copy-of select="$vCBExchangeStream/previousMarginConstituent/collateralUsed"/>
                <xsl:copy-of select="$vCBExchangeStream/previousMarginConstituent/cashAvailable"/>
                <xsl:copy-of select="$vCBExchangeStream/previousMarginConstituent/cashUsed"/>
                <xsl:copy-of select="$vCBExchangeStream/previousMarginConstituent/uncoveredMarginRequirement"/>
              </previous>
              <current>
                <marginRequirement>
                  <xsl:copy-of select="$vCBExchangeStream/marginRequirement/@*"/>
                </marginRequirement>
                <xsl:copy-of select="$vCBExchangeStream/collateralAvailable"/>
                <xsl:copy-of select="$vCBExchangeStream/collateralUsed"/>
                <cashAvailable>
                  <xsl:copy-of select="$vCBExchangeStream/cashAvailable/@*"/>
                </cashAvailable>
                <xsl:copy-of select="$vCBExchangeStream/cashUsed"/>
                <xsl:copy-of select="$vCBExchangeStream/uncoveredMarginRequirement"/>
              </current>
            </marginCallDet>
          </xsl:if>

          <xsl:call-template name="Business_GetExchangeCurrencyFee">
            <xsl:with-param name="pFee" select="$vExchangeFee"/>
          </xsl:call-template>
          <xsl:call-template name="Business_GetExchangeCurrencySafekeeping">
            <xsl:with-param name="pFee" select="$vExchangeSafekeeping"/>
          </xsl:call-template>
        </exchangeCurrency>
      </xsl:if>
    </group>

    <xsl:if test="$vIsLastGroup = false()">
      <xsl:call-template name="Business_GetCurrencyGroup">
        <xsl:with-param name="pGroupNumber" select="$pGroupNumber + number('1')"/>
        <xsl:with-param name="pGroupCount" select="$pGroupCount"/>
        <xsl:with-param name="pIsHideBaseCcy" select="$pIsHideBaseCcy"/>
        <xsl:with-param name="pAllCurrency" select="$pAllCurrency"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetFee                                  -->
  <!-- ................................................ -->
  <!-- RD 20170224 [22885] Check Unclearing-->
  <xsl:template name="Business_GetFee">
    <xsl:param name="pFee"/>
    <xsl:param name="pParentUnclearingCheck" select="false()"/>

    <xsl:variable name="vFeeCopyNode">
      <xsl:copy-of select="$pFee"/>
    </xsl:variable>
    <xsl:variable name="vFeeCopy" select="msxsl:node-set($vFeeCopyNode)/fee"/>
    <xsl:variable name="vFee-paymentType" select="$vFeeCopy[generate-id()=generate-id(key('kFee-paymentType',@paymentType))]"/>

    <xsl:for-each select="$vFee-paymentType">

      <xsl:variable name="vCurrentPaymentType" select="current()"/>
      <xsl:variable name="vPaymentFee" select="$pFee[@paymentType=$vCurrentPaymentType/@paymentType]"/>
      
      <xsl:variable name="vFeeCcyCopyNode">
        <xsl:copy-of select="$vPaymentFee"/>
      </xsl:variable>
      <xsl:variable name="vFeeCcyCopy" select="msxsl:node-set($vFeeCcyCopyNode)/fee"/>
      <xsl:variable name="vFee-ccy" select="$vFeeCcyCopy[generate-id()=generate-id(key('kCcy',@ccy))]"/>

      <xsl:for-each select="$vFee-ccy">

        <xsl:variable name="vCurrentCcy" select="current()"/>
        <xsl:variable name="vCurrentCcyFee" select="$vPaymentFee[@ccy=$vCurrentCcy/@ccy]"/>

        <xsl:choose>
          <xsl:when test="$pParentUnclearingCheck=true()">
            <xsl:variable name="vCurrentCcyFeeDebit" select="$vCurrentCcyFee[(string-length(parent::node()/@dtUnclearing)=0 and @side=$gcDebit)
                          or (string-length(parent::node()/@dtUnclearing)>0 and @side=$gcCredit)]"/>
            <xsl:variable name="vCurrentCcyFeeCredit" select="$vCurrentCcyFee[(string-length(parent::node()/@dtUnclearing)=0 and @side=$gcCredit)
                          or (string-length(parent::node()/@dtUnclearing)>0 and @side=$gcDebit)]"/>

            <xsl:variable name="vTotalAmount_Node">
              <xsl:if test="$vCurrentCcyFeeDebit">
                <fee side="{$gcDebit}" amt="{sum($vCurrentCcyFeeDebit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
              </xsl:if>
              <xsl:if test="$vCurrentCcyFeeCredit">
                <fee side="{$gcCredit}" amt="{sum($vCurrentCcyFeeCredit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
              </xsl:if>
            </xsl:variable>

            <xsl:variable name="vTotalAmount">
              <xsl:call-template name="GetAmount-amt">
                <xsl:with-param name="pAmount" select="msxsl:node-set($vTotalAmount_Node)/fee"/>
                <xsl:with-param name="pCcy" select="$vCurrentCcy/@ccy"/>
              </xsl:call-template>
            </xsl:variable>
                        
            <fee amt="{$vTotalAmount}" ccy="{$vCurrentCcy/@ccy}" paymentType="{$vCurrentPaymentType/@paymentType}"/>
            
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vCurrentCcyFeeDebit" select="$vCurrentCcyFee[@side=$gcDebit]"/>
            <xsl:variable name="vCurrentCcyFeeCredit" select="$vCurrentCcyFee[@side=$gcCredit]"/>

            <xsl:choose>
              <xsl:when test="$vCurrentCcyFeeDebit = false() and $vCurrentCcyFeeCredit = false()">
                <fee amt="{sum($vCurrentCcyFee/@amt)}" ccy="{$vCurrentCcy/@ccy}" paymentType="{$vCurrentPaymentType/@paymentType}"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="$vCurrentCcyFeeDebit">
                  <fee side="{$gcDebit}" amt="{sum($vCurrentCcyFeeDebit/@amt)}" ccy="{$vCurrentCcy/@ccy}" paymentType="{$vCurrentPaymentType/@paymentType}"/>
                </xsl:if>
                <xsl:if test="$vCurrentCcyFeeCredit">
                  <fee side="{$gcCredit}" amt="{sum($vCurrentCcyFeeCredit/@amt)}" ccy="{$vCurrentCcy/@ccy}" paymentType="{$vCurrentPaymentType/@paymentType}"/>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetCurrencyFee                          -->
  <!-- ................................................ -->
  <xsl:template name="Business_GetCurrencyFee">
    <xsl:param name="pFee"/>

    <xsl:variable name="vFeeCopyNode">
      <xsl:copy-of select="$pFee"/>
    </xsl:variable>
    <xsl:variable name="vFeeCopy" select="msxsl:node-set($vFeeCopyNode)/fee"/>
    <xsl:variable name="vFee-paymentType" select="$vFeeCopy[generate-id()=generate-id(key('kFee-paymentType',@paymentType))]"/>

    <xsl:for-each select="$vFee-paymentType">

      <xsl:variable name="vCurrentPaymentType" select="current()"/>

      <fee paymentType="{$vCurrentPaymentType/@paymentType}">
        <xsl:variable name="vFeeCcyCopyNode">
          <xsl:copy-of select="$vFeeCopy[@paymentType=$vCurrentPaymentType/@paymentType]"/>
        </xsl:variable>
        <xsl:variable name="vFeeCcyCopy" select="msxsl:node-set($vFeeCcyCopyNode)/fee"/>
        <xsl:variable name="vFee-ccy" select="$vFeeCcyCopy[generate-id()=generate-id(key('kCcy',@ccy))]"/>
        <xsl:for-each select="$vFee-ccy">

          <xsl:variable name="vCurrentCcy" select="current()"/>
          <xsl:variable name="vCurrentCcyFee" select="$vFeeCcyCopy[@ccy=$vCurrentCcy/@ccy]"/>
          <xsl:variable name="vCurrentCcyFeeDebit" select="$vCurrentCcyFee[@side=$gcDebit]"/>
          <xsl:variable name="vCurrentCcyFeeCredit" select="$vCurrentCcyFee[@side=$gcCredit]"/>

          <xsl:if test="$vCurrentCcyFeeDebit">
            <amount side="{$gcDebit}" amt="{sum($vCurrentCcyFeeDebit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
          </xsl:if>
          <xsl:if test="$vCurrentCcyFeeCredit">
            <amount side="{$gcCredit}" amt="{sum($vCurrentCcyFeeCredit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
          </xsl:if>
        </xsl:for-each>

        <xsl:call-template name="Business_GetCurrencyFeeTax">
          <xsl:with-param name="pFeeCcyCopy" select="$vFeeCcyCopy"/>
          <xsl:with-param name="pFee-ccy" select="$vFee-ccy"/>
        </xsl:call-template>
      </fee>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetCurrencySafekeeping                  -->
  <!-- ................................................ -->
  <xsl:template name="Business_GetCurrencySafekeeping">
    <xsl:param name="pFee"/>

    <xsl:variable name="vFeeCopyNode">
      <xsl:copy-of select="$pFee"/>
    </xsl:variable>
    <xsl:variable name="vFeeCopy" select="msxsl:node-set($vFeeCopyNode)/safekeeping"/>
    <xsl:variable name="vFee-paymentType" select="$vFeeCopy[generate-id()=generate-id(key('kFee-paymentType',@paymentType))]"/>

    <xsl:for-each select="$vFee-paymentType">

      <xsl:variable name="vCurrentPaymentType" select="current()"/>

      <safekeeping paymentType="{$vCurrentPaymentType/@paymentType}">
        <xsl:variable name="vFeeCcyCopyNode">
          <xsl:copy-of select="$vFeeCopy[@paymentType=$vCurrentPaymentType/@paymentType]"/>
        </xsl:variable>
        <xsl:variable name="vFeeCcyCopy" select="msxsl:node-set($vFeeCcyCopyNode)/safekeeping"/>
        <xsl:variable name="vFee-ccy" select="$vFeeCcyCopy[generate-id()=generate-id(key('kCcy',@ccy))]"/>
        <xsl:for-each select="$vFee-ccy">

          <xsl:variable name="vCurrentCcy" select="current()"/>
          <xsl:variable name="vCurrentCcyFee" select="$vFeeCcyCopy[@ccy=$vCurrentCcy/@ccy]"/>
          <xsl:variable name="vCurrentCcyFeeDebit" select="$vCurrentCcyFee[@side=$gcDebit]"/>
          <xsl:variable name="vCurrentCcyFeeCredit" select="$vCurrentCcyFee[@side=$gcCredit]"/>

          <xsl:if test="$vCurrentCcyFeeDebit">
            <amount side="{$gcDebit}" amt="{sum($vCurrentCcyFeeDebit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
          </xsl:if>
          <xsl:if test="$vCurrentCcyFeeCredit">
            <amount side="{$gcCredit}" amt="{sum($vCurrentCcyFeeCredit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
          </xsl:if>
        </xsl:for-each>

        <xsl:call-template name="Business_GetCurrencyFeeTax">
          <xsl:with-param name="pFeeCcyCopy" select="$vFeeCcyCopy"/>
          <xsl:with-param name="pFee-ccy" select="$vFee-ccy"/>
        </xsl:call-template>
      </safekeeping>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetCurrencyFee                          -->
  <!-- ................................................ -->
  <xsl:template name="Business_GetCurrencyFeeTax">
    <xsl:param name="pFeeCcyCopy"/>
    <xsl:param name="pFee-ccy"/>

    <xsl:variable name="vTaxTypeCopyNode">
      <xsl:copy-of select="$pFeeCcyCopy/tax"/>
    </xsl:variable>
    <xsl:variable name="vTaxTypeCopy" select="msxsl:node-set($vTaxTypeCopyNode)/tax"/>
    <xsl:variable name="vTax-taxType" select="$vTaxTypeCopy[generate-id()=generate-id(key('kTax-taxType',@taxType))]"/>

    <xsl:for-each select="$vTax-taxType">

      <xsl:variable name="vCurrentTaxType" select="current()"/>

      <xsl:variable name="vTaxIdCopyNode">
        <xsl:copy-of select="$vTaxTypeCopy[@taxType=$vCurrentTaxType/@taxType]"/>
      </xsl:variable>
      <xsl:variable name="vTaxIdCopy" select="msxsl:node-set($vTaxIdCopyNode)/tax"/>
      <xsl:variable name="vTax-taxId" select="$vTaxIdCopy[generate-id()=generate-id(key('kTax-taxId',@taxId))]"/>

      <xsl:for-each select="$vTax-taxId">

        <xsl:variable name="vCurrentTaxId" select="current()"/>

        <xsl:variable name="vTaxRateCopyNode">
          <xsl:copy-of select="$vTaxIdCopy[@taxId=$vCurrentTaxId/@taxId]"/>
        </xsl:variable>
        <xsl:variable name="vTaxRateCopy" select="msxsl:node-set($vTaxRateCopyNode)/tax"/>
        <xsl:variable name="vTax-rate" select="$vTaxRateCopy[generate-id()=generate-id(key('kTax-rate',@rate))]"/>

        <xsl:for-each select="$vTax-rate">

          <xsl:variable name="vCurrentTaxRate" select="current()"/>

          <tax taxType="{$vCurrentTaxId/@taxType}" taxId="{$vCurrentTaxId/@taxId}" rate="{$vCurrentTaxRate/@rate}">

            <xsl:variable name="vCurrentTaxAmount" select="sum($vTaxRateCopy[@rate=$vCurrentTaxRate/@rate]/@amt)"/>

            <xsl:for-each select="$pFee-ccy">

              <xsl:variable name="vCurrentCcy" select="current()"/>
              <xsl:variable name="vCurrentCcyTaxDebit" select="$pFeeCcyCopy[@side=$gcDebit and @ccy=$vCurrentCcy/@ccy]/tax[@taxId=$vCurrentTaxId/@taxId and @rate=$vCurrentTaxRate/@rate]"/>
              <xsl:variable name="vCurrentCcyTaxCredit" select="$pFeeCcyCopy[@side=$gcCredit and @ccy=$vCurrentCcy/@ccy]/tax[@taxId=$vCurrentTaxId/@taxId and @rate=$vCurrentTaxRate/@rate]"/>

              <xsl:if test="$vCurrentCcyTaxDebit">
                <amount side="{$gcDebit}" amt="{sum($vCurrentCcyTaxDebit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
              </xsl:if>
              <xsl:if test="$vCurrentCcyTaxCredit">
                <amount side="{$gcCredit}" amt="{sum($vCurrentCcyTaxCredit/@amt)}" ccy="{$vCurrentCcy/@ccy}"/>
              </xsl:if>
            </xsl:for-each>
          </tax>
        </xsl:for-each>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Business_GetCurrencyFee                          -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify  -->
  <xsl:template name="Business_GetExchangeCurrencyFee">
    <xsl:param name="pFee"/>

    <!-- RD 20160406 [21284] Add filter @paymentType -->
    <xsl:for-each select="$pFee[string-length(@paymentType) >0]">
      <xsl:variable name="vCurrentFee" select="current()"/>
      <!-- FI 20150921 [21311]-->
      <!-- 2 noeuds fee pour obtenir une structure identique à ce qui est généré par Business_GetCurrencyFee -->
      <fee paymentType="{$vCurrentFee/@paymentType}">
        <amount side="{$vCurrentFee/@side}" amt="{$vCurrentFee/@amt}" ccy="{$vCurrentFee/@ccy}"/>
        <xsl:for-each select="$vCurrentFee/tax">
          <xsl:variable name="vCurrentTax" select="current()"/>
          <tax taxType="{$vCurrentTax/@taxType}" taxId="{$vCurrentTax/@taxId}" rate="{$vCurrentTax/@rate}">
            <amount side="{$vCurrentFee/@side}" amt="{$vCurrentTax/@amt}" ccy="{$vCurrentFee/@ccy}" />
          </tax>
        </xsl:for-each>
      </fee>
    </xsl:for-each>
  </xsl:template>

  <!-- ........................................................ -->
  <!-- Business_GetExchangeCurrencySafekeeping                  -->
  <!-- ........................................................ -->
  <!-- FI 20150921 [21311] Modify  -->
  <xsl:template name="Business_GetExchangeCurrencySafekeeping">
    <xsl:param name="pFee"/>

    <!-- RD 20160406 [21284] Add filter @paymentType -->
    <xsl:for-each select="$pFee[string-length(@paymentType) >0]">
      <xsl:variable name="vCurrentFee" select="current()"/>
      <!-- FI 20150921 [21311]-->
      <!-- Structure identique à ce qui est généré par Business_GetCurrencySafekeeping -->
      <safekeeping paymentType="{$vCurrentFee/@paymentType}">
        <amount side="{$vCurrentFee/@side}" amt="{$vCurrentFee/@amt}" ccy="{$vCurrentFee/@ccy}"/>
        <xsl:for-each select="$vCurrentFee/tax">
          <xsl:variable name="vCurrentTax" select="current()"/>
          <tax taxType="{$vCurrentTax/@taxType}" taxId="{$vCurrentTax/@taxId}" rate="{$vCurrentTax/@rate}">
            <amount side="{$vCurrentFee/@side}" amt="{$vCurrentTax/@amt}" ccy="{$vCurrentFee/@ccy}" />
          </tax>
        </xsl:for-each>
      </safekeeping>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetAssetNodeName                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get an XML node name of the Asset by CategoryAsset
       ................................................ -->
  <xsl:template name="GetAssetNodeName">
    <xsl:param name="pAssetCategory"/>

    <xsl:choose>
      <xsl:when test="$pAssetCategory = 'EquityAsset'">
        <xsl:value-of select="'equity'"/>
      </xsl:when>
      <xsl:when test="$pAssetCategory = 'Index'">
        <xsl:value-of select="'index'"/>
      </xsl:when>
      <xsl:when test="$pAssetCategory = 'FxRateAsset'">
        <xsl:value-of select="'fxRate'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'N/A'"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>
