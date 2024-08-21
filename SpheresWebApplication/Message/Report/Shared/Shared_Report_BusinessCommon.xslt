<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--  
================================================================================================================
                                                     HISTORY OF THE MODIFICATIONS
Version 1.0.0.0 (Spheres 2.5.0.0)
 Date: 17/09/2010
 Author: RD
 Description: first version  
 
Version 1.0.0.1 (Spheres 2.6.3.0)
 Date: 26/06/2012
 Author: MF
 Description: CreateCashBalanceGroups template, added special attributes "rowspan" and "subtable" for nodes group/amount
================================================================================================================
  -->

  <!--
================================================================================================================
                                                     ALgorithme de l'affichage de l'Asterix
                                                     
1- Appliquer les règles ci-dessous, que pour les brokerages payés ou reçus par le compte de l’avis d’opéré
      a.	Tous les Brokerages doivent être tous payés ou bien tous reçus par le compte de l’avis d’opéré, sinon on affiche un Astérix, dans la case Brokerage correspondant au Trade
      b.	Tous les Brokerages doivent être dans la même devise, sinon on affiche un Astérix, dans la case Brokerage correspondant au Trade.

2-	S’il y’a un Astérix sur au moins une ligne d’un sous-total Brokerage, on affiche un Astérix pour le sous-total.
3-	Tous les Brokerages d’un sous-total doivent être dans la même devise, sinon on affiche un Astérix pour le sous-total.
4-	Tous les Brokerages d’un sous-total doivent être du même payeur, et du même receveur, sinon on affiche un Astérix pour le sous-total.

5-	S’il y’a un Astérix sur au moins un sous total Brokerage, on affiche un Astérix pour le total.
6-	Tous les sous-totaux Brokerage doivent être dans la même devise, sinon on affiche un Astérix pour le total.
7-	Tous les sous-totaux Brokerage doivent être du même payeur, et du même receveur, sinon on affiche un Astérix pour le total.
================================================================================================================
  -->
  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Shared_Report_VariablesCommon.xslt" />

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                                VARIABLES                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!-- Key for grouping amounts with dynamic identifiers (Ex: Détails frais et taxes ) -->
  <xsl:key name="amounts-by-keyforsum" match="amount" use="@keyForSum" />

  <xsl:key name="kSpheresIdTaxDetail" match="spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetail']" use="text()"/>
  <xsl:key name="kSpheresIdTaxRate" match="spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetailRate']" use="text()"/>

  <xsl:key name="kEventFeeTaxDetail" match="TAXDETAIL" use="text()"/>
  <xsl:key name="kEventFeTaxRate" match="TAXRATE" use="text()"/>

  <xsl:key name="kSerieGroupTrdDt" match="data[@name='trdDt']" use="@value"/>
  <xsl:key name="kSerieGroupAggregate" match="data[@name='aggregate']" use="@value"/>
  <xsl:key name="kSerieGroupLastPx" match="data[@name='lastPx']" use="@value"/>

  <!-- .................................. -->
  <!--   Trade variable                   -->
  <!-- .................................. -->
  <xsl:variable name="gBusinessTrades">
    <!-- Valuate SortingKeys for each Trade -->
    <!-- RD 20101215 / Bug dans le cas d'un seul Trade dans l'avis d'opéré-->
    <!--<xsl:if test="$gIsKeysFilled">-->
    <xsl:apply-templates select="$gDataDocumentTrades" mode="business"/>
    <!--</xsl:if>-->
  </xsl:variable>
  <!--<xsl:variable name="gBusinessBooks">
    <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade/BOOK[(generate-id()=generate-id(key('kBookKey',@data)))]"/>
  </xsl:variable>-->

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Business Templates                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->
  <xsl:template name="GetMarketData">
    <xsl:param name="pKMarketValues"/>
    <xsl:param name="pKMarket2Values"/>

    <xsl:choose>
      <xsl:when test="$pKMarketValues">
        <xsl:for-each select="$pKMarketValues">
          <xsl:variable name="vCurrentMarket" select="@Exch"/>
          <xsl:call-template name="GetMarketDataDetail">
            <xsl:with-param name="pCurrentMarket" select="$vCurrentMarket"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="$pKMarket2Values">
        <xsl:for-each select="$pKMarket2Values">
          <xsl:variable name="vCurrentMarket" select="string(current()/text())"/>
          <xsl:call-template name="GetMarketDataDetail">
            <xsl:with-param name="pCurrentMarket" select="$vCurrentMarket"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="GetDCData">
    <xsl:param name="pKDCValues"/>
    <xsl:param name="pKDC2Values"/>

    <xsl:choose>
      <xsl:when test="$pKDCValues">
        <xsl:for-each select="$pKDCValues">
          <xsl:variable name="vCurrentDC" select="@Sym"/>
          <xsl:call-template name="GetDCDataDetail">
            <xsl:with-param name="pCurrentDC" select="$vCurrentDC"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="$pKDC2Values">
        <xsl:for-each select="$pKDC2Values">
          <xsl:variable name="vCurrentDC" select="string(current()/text())"/>
          <xsl:call-template name="GetDCDataDetail">
            <xsl:with-param name="pCurrentDC" select="$vCurrentDC"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetMarketDataDetail">
    <xsl:param name="pCurrentMarket"/>

    <xsl:variable name="vMarketRepository" select="$gDataDocumentRepository/market[fixml_SecurityExchange=$pCurrentMarket]"/>
    <xsl:variable name="vIsMatif">
      <xsl:choose>
        <xsl:when test="$vMarketRepository/ISO10383_ALPHA4/text()='XMAT'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vMarketDisplayname">
      <xsl:call-template name="GetMarketDataToDisplay">
        <xsl:with-param name="pMarket" select="$pCurrentMarket"/>
        <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
        <xsl:with-param name="pIsWithISO10383_ALPHA4" select="false()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vIDM" select="string($vMarketRepository/@OTCmlId)"/>

    <data name="market"
          value="{$pCurrentMarket}"
          displayname="{$vMarketDisplayname}"
          isMatif="{$vIsMatif}"
          idm="{$vIDM}"/>
  </xsl:template>
  <xsl:template name="GetDCDataDetail">
    <xsl:param name="pCurrentDC"/>

    <xsl:variable name="vDCRepository" select="$gDataDocumentRepository/derivativeContract[identifier=$pCurrentDC]"/>
    <xsl:variable name="vDCDisplayname">
      <xsl:call-template name="GetContractDataToDisplay">
        <xsl:with-param name="pContract" select="$pCurrentDC"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vDCDisplayname2" select="string($vDCRepository/displayname/text())"/>
    <xsl:variable name="vDCCategory" select="string($vDCRepository/category/text())"/>
    <xsl:variable name="vDCCurrency" select="string($vDCRepository/idC_Price/text())"/>
    <xsl:variable name="vCurrencyRepository" select="$gDataDocumentRepository/currency[@id=concat($gPrefixCurrencyId, $vDCCurrency)]"/>
    <xsl:variable name="vDecPattern">
      <xsl:call-template name="GetDecPattern">
        <xsl:with-param name="pRoundDir" select="$vCurrencyRepository/rounddir"/>
        <xsl:with-param name="pRoundPrec" select="$vCurrencyRepository/roundprec"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vDCMltp" select="string($vDCRepository/contractMultiplier/text())"/>
    <xsl:variable name="vIDDC" select="string($vDCRepository/@OTCmlId)"/>

    <data name="dc"
          value="{$pCurrentDC}"
          displayname="{$vDCDisplayname}"
          displayname2="{$vDCDisplayname2}"
          category="{$vDCCategory}"
          currency="{$vDCCurrency}" decPattern="{$vDecPattern}"
          multiplier="{$vDCMltp}"
          iddc="{$vIDDC}"/>

  </xsl:template>

  <!--
  CreateCashBalanceGroups template : considering the "trade/BOOK/cbStream" nodes (starting for a specific BOOK) 
                                     produced in the template matching the trade nodes (match="trade") 
                                     to produce the "group" nodes (by currency). for each "cbStream" node (one for each currency)
                                     we have a "group" and we copy inside all the nodes "amount" produced in the template matching the trade nodes.
                                     At last we put a new "group" node (titleGroup="TotEx"), related 
                                     to the countervalue section of the report where we put
                                     all the sum values issued by the previous amounts converted in the specific report currrency. -->
  <xsl:template name="CreateCashBalanceGroups">
    <xsl:param name="pBook"/>
    <xsl:param name="pBookTrade"/>
    <xsl:param name="pIsCreditNoteGroup" select="false()"/>

    <xsl:variable name="vRoundDirReportingCurrency" select="normalize-space($pBookTrade/@repCuRoundDir)"/>
    <xsl:variable name="vRoundPrecReportingCurrency" select="normalize-space($pBookTrade/@repCuRoundPrec)"/>

    <!-- CBStreamGroup -->
    <xsl:variable name="vCBStreamGroupNode">
      <xsl:choose>
        <xsl:when test="$pIsCreditNoteGroup">
          <xsl:copy-of select="msxsl:node-set($gGroupDisplaySettings)/group[@name='CBStreamCN']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="msxsl:node-set($gGroupDisplaySettings)/group[@name='CBStream' or @name='CBStreamEx']"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vCBStreamGroup" select="msxsl:node-set($vCBStreamGroupNode)/group"/>

    <xsl:if test="$vCBStreamGroup">
      <xsl:call-template name="CreateCashBalanceGroup">
        <xsl:with-param name="pBookTrade" select="$pBookTrade"/>
        <xsl:with-param name="pCBStreamGroup" select="$vCBStreamGroup"/>
        <xsl:with-param name="pRoundDirReportingCurrency" select="$vRoundDirReportingCurrency"/>
        <xsl:with-param name="pRoundPrecReportingCurrency" select="$vRoundPrecReportingCurrency"/>
      </xsl:call-template>

      <!--<xsl:for-each select="$pBookTrade/cbStream">
        <xsl:sort select="@currency='EUR'"/>
        <xsl:sort select="@currency='USD'"/>
        <xsl:sort select="@currency"/>

        <group sort="{$vCBStreamGroup/@sort}" name="{$vCBStreamGroup/@name}"
               repCuRoundDir="{$vRoundDirReportingCurrency}" repCuRoundPrec="{$vRoundPrecReportingCurrency}">
          <xsl:copy-of select="@*"/>
          <xsl:copy-of select="amount"/>
        </group>
      </xsl:for-each>-->
    </xsl:if>

    <!-- CBStreamExGroup -->
    <xsl:variable name="vCBStreamExGroup" select="msxsl:node-set($gGroupDisplaySettings)/group[@name='CBStreamEx']"/>

    <xsl:if test="$vCBStreamExGroup">
      <xsl:if test="count($pBookTrade/cbStream) > 1">

        <xsl:variable name="vIsFees" select="$vCBStreamExGroup/row[@name='feesVAT']"/>
        <xsl:variable name="vIsFeesDet" select="$vCBStreamExGroup/row[@name='feeGrossAmount']"/>
        <xsl:variable name="vIsPRM" select="$vCBStreamExGroup/row[@name='optionPremium']"/>
        <xsl:variable name="vIsSCU" select="$vCBStreamExGroup/row[@name='cashSettlement']"/>
        <xsl:variable name="vIsRMG" select="$vCBStreamExGroup/row[@name='futureRMG' or @name='optionRMG']"/>
        <xsl:variable name="vIsTotal" select="$vCBStreamExGroup/row[@name='todayCashBalance']"/>

        <group sort="{$vCBStreamExGroup/@sort}" name="CBStreamEx"
               currency="{$gReportingCurrency}"
               repCuRoundDir="{$vRoundDirReportingCurrency}" repCuRoundPrec="{$vRoundPrecReportingCurrency}">

          <!-- Solde Espèces Veille -->
          <xsl:if test="$pBookTrade/cbStream/amount[@name='cashDayBefore']">

            <xsl:variable name="vSumAmountBeforeCashBalance" select="
                      sum($pBookTrade/cbStream/amount[@name='cashDayBefore' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='cashDayBefore' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumBeforeCashBalance">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountBeforeCashBalance" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountBeforeCashBalance">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountBeforeCashBalance" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashDayBefore"
                    crDr="{$vCrDrSumBeforeCashBalance}" exAmount="{$vAbsSumAmountBeforeCashBalance}"/>
          </xsl:if>

          <!-- Mouvement Espèces  -->
          <!-- Uniquement si la gestion des soldes est activée, ou bien il existe un Mouvement Espèces -->
          <xsl:if test="$pBookTrade/cbStream/amount[@name='cashMouvements']">
            <xsl:variable name="vSumAmountCashMouvements"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='cashMouvements' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='cashMouvements' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumCashMouvements">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountCashMouvements" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountCashMouvements">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountCashMouvements" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashMouvements"
                    crDr="{$vCrDrSumCashMouvements}"  exAmount="{$vAbsSumAmountCashMouvements}"/>
          </xsl:if>

          <!-- Primes sur Options -->
          <xsl:if test="$vIsPRM">
            <xsl:variable name="vSumAmountOptionPremium"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='optionPremium' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='optionPremium' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumOptionPremium">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountOptionPremium" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountOptionPremium">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountOptionPremium" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="optionPremium"
                    crDr="{$vCrDrSumOptionPremium}" exAmount="{$vAbsSumAmountOptionPremium}"/>
          </xsl:if>

          <!-- Appels de Marge -->
          <xsl:if test="$pBookTrade/cbStream/amount[@name='variationMargin']">
            <xsl:variable name="vSumAmountVariationMargin"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='variationMargin' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='variationMargin' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumVariationMargin">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountVariationMargin" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountVariationMargin">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountVariationMargin" />
              </xsl:call-template>
            </xsl:variable>


            <amount name="variationMargin"
                    crDr="{$vCrDrSumVariationMargin}" exAmount="{$vAbsSumAmountVariationMargin}"/>
          </xsl:if>

          <!-- Résultat réalisé sur Futures et sur Options-->
          <xsl:if test="$vIsRMG">
            <xsl:variable name="vSumAmountFutureRMG" select="
                      sum($pBookTrade/cbStream/amount[@name='futureRMG' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='futureRMG' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vAbsAmountFutureRMG">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountFutureRMG" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrSumFutureRMG">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountFutureRMG" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountFutureRMG">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountFutureRMG" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="futureRMG"
                    crDr="{$vCrDrSumFutureRMG}" exAmount="{$vAbsSumAmountFutureRMG}"/>

            <xsl:variable name="vSumAmountOptionRMG" select="
                      sum($pBookTrade/cbStream/amount[@name='optionRMG' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='optionRMG' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vAbsAmountOptionRMG">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountOptionRMG" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrSumOptionRMG">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountOptionRMG" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountOptionRMG">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountOptionRMG" />
              </xsl:call-template>
            </xsl:variable>


            <amount name="optionRMG"
                    crDr="{$vCrDrSumOptionRMG}" exAmount="{$vAbsSumAmountOptionRMG}"/>
          </xsl:if>

          <!-- Cash Settlement -->
          <xsl:if test="$vIsSCU">
            <xsl:variable name="vSumAmountCashSettlement"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='cashSettlement' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='cashSettlement' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumCashSettlement">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountCashSettlement" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountCashSettlement">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountCashSettlement" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashSettlement"
                    crDr="{$vCrDrSumCashSettlement}" exAmount="{$vAbsSumAmountCashSettlement}"/>
          </xsl:if>

          <!-- Frais TTC  -->
          <xsl:if test="$vIsFees">
            <xsl:variable name="vSumAmountFeesVat"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='feesVAT' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='feesVAT' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumFeesVat">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountFeesVat" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountFeesVat">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountFeesVat" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="feesVAT" crDr="{$vCrDrSumFeesVat}"
                    exAmount="{$vAbsSumAmountFeesVat}"/>
          </xsl:if>

          <!-- Appel/Restitution de Déposit  -->
          <xsl:if test="$pBookTrade/cbStream/amount[@name='returnCallDeposit']">
            <xsl:variable name="vSumAmountReturnCallDeposit" select="
                      sum($pBookTrade/cbStream/amount[@name='returnCallDeposit' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='returnCallDeposit' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumReturnCallDeposit">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountReturnCallDeposit" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountReturnCallDeposit">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountReturnCallDeposit" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="returnCallDeposit" crDr="{$vCrDrSumReturnCallDeposit}"
                    exAmount="{$vAbsSumAmountReturnCallDeposit}"/>
          </xsl:if>

          <!-- Solde Espèces Jour -->
          <xsl:if test="$vIsTotal">
            <xsl:variable name="vSumAmountTodayCashBalance"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='todayCashBalance' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='todayCashBalance' and @crDr=$gcDebit]/@exAmount)"/>

            <xsl:variable name="vCrDrSumTodayCashBalance">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vSumAmountTodayCashBalance" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vAbsSumAmountTodayCashBalance">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vSumAmountTodayCashBalance" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="todayCashBalance" crDr="{$vCrDrSumTodayCashBalance}"
                    exAmount="{$vAbsSumAmountTodayCashBalance}"/>
          </xsl:if>

          <!-- Détails frais et taxes -->
          <xsl:if test="$vIsFeesDet">
            <xsl:variable name="vAmountsByKeyforsum">
              <xsl:copy-of select="$pBookTrade/cbStream/amount[@keyForSum != '']"/>
            </xsl:variable>

            <xsl:for-each select="
                      msxsl:node-set($vAmountsByKeyforsum)/amount[count(. | key('amounts-by-keyforsum', @keyForSum)[1]) = 1]">

              <xsl:variable name="vCurrentKey" select="@keyForSum"/>

              <xsl:variable name="vSumAmountFee"
                          select="
                      sum($pBookTrade/cbStream/amount[@keyForSum = $vCurrentKey and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@keyForSum = $vCurrentKey and @crDr=$gcDebit]/@exAmount)"/>

              <xsl:variable name="vCrDrSumAmountFee">
                <xsl:call-template name="TotalAmountSuffix">
                  <xsl:with-param name="pAmount" select="$vSumAmountFee" />
                </xsl:call-template>
              </xsl:variable>

              <xsl:variable name="vAbsSumAmountFee">
                <xsl:call-template name="abs">
                  <xsl:with-param name="n" select="$vSumAmountFee" />
                </xsl:call-template>
              </xsl:variable>

              <xsl:variable name="vFeePosition" select="position()"/>

              <amount name="{@name}" crDr="{$vCrDrSumAmountFee}"
                      exAmount="{$vAbsSumAmountFee}" keyForSum="{$vCurrentKey}">
                <xsl:copy-of select="@taxDetailRate"/>
                <xsl:copy-of select="@taxDetailName"/>
                <!-- 2010803 MF Ticket 18065 - for the financial report including DC details the row span is not the same
                for the details panel related to a specific currency and the countervalue panel, then we use a specific rowspanCounterValue -->
                <xsl:attribute name="rowspan">
                  <xsl:value-of select="@rowspanCounterValue"/>
                </xsl:attribute>
              </amount>

            </xsl:for-each>

            <!-- Totale Frais TTC -->
            <xsl:if test="$pBookTrade/cbStream/amount[@name='totalFeesVAT' and number(@exAmount) > 0]">

              <xsl:variable name="vSumAmountTotalFeesVAT"
                          select="
                      sum($pBookTrade/cbStream/amount[@name='totalFeesVAT' and @crDr=$gcCredit]/@exAmount) 
                      - 
                      sum($pBookTrade/cbStream/amount[@name='totalFeesVAT' and @crDr=$gcDebit]/@exAmount)"/>

              <xsl:variable name="vCrDrSumAmountTotalFeesVAT">
                <xsl:call-template name="TotalAmountSuffix">
                  <xsl:with-param name="pAmount" select="$vSumAmountTotalFeesVAT" />
                </xsl:call-template>
              </xsl:variable>

              <xsl:variable name="vAbsSumAmountTotalFeesVAT">
                <xsl:call-template name="abs">
                  <xsl:with-param name="n" select="$vSumAmountTotalFeesVAT" />
                </xsl:call-template>
              </xsl:variable>

              <amount name="totalFeesVAT" crDr="{$vCrDrSumAmountTotalFeesVAT}" exAmount="{$vAbsSumAmountTotalFeesVAT}"/>
            </xsl:if>
          </xsl:if>
        </group>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="CreateCashBalanceGroup">
    <xsl:param name="pBookTrade"/>
    <xsl:param name="pCBStreamGroup"/>
    <xsl:param name="pRoundDirReportingCurrency"/>
    <xsl:param name="pRoundPrecReportingCurrency"/>

    <xsl:for-each select="$pBookTrade/cbStream">
      <xsl:sort select="@currency"/>

      <group sort="{$pCBStreamGroup/@sort}" name="{$pCBStreamGroup/@name}"
             repCuRoundDir="{$pRoundDirReportingCurrency}" repCuRoundPrec="{$pRoundPrecReportingCurrency}">
        <xsl:copy-of select="@*"/>
        <xsl:copy-of select="amount"/>
      </group>
    </xsl:for-each>
  </xsl:template>

  <!--TotalAmountSuffix-->
  <xsl:template name="TotalAmountSuffix">
    <xsl:param name="pAmount"/>

    <xsl:choose>
      <xsl:when test="number($pAmount) > 0">
        <xsl:value-of select="$gcCredit"/>
      </xsl:when>
      <xsl:when test="0 > number($pAmount) ">
        <xsl:value-of select="$gcDebit"/>
      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <!-- FxRate -->
  <xsl:template name="FxRate">
    <xsl:param name="pFlowCurrency"/>
    <xsl:param name="pExCurrency"/>
    <xsl:param name="pFixingDate"/>
    <xsl:param name="pIsLastFxRate" select="false()"/>

    <xsl:choose>
      <xsl:when test="$pFlowCurrency = $pExCurrency">
        <xsl:value-of select="number(1)"/>
      </xsl:when>
      <xsl:otherwise>

        <xsl:variable name="idFxRate" select="concat($gPrefixCurrencyId, $pFlowCurrency)"/>
        <xsl:variable name="vFxRateRepository">
          <xsl:choose>
            <xsl:when test="$pFixingDate">
              <xsl:choose>
                <xsl:when test="$pIsLastFxRate = true()">
                  <xsl:variable name="vAllFxRate">
                    <xsl:for-each select="$gDataDocumentRepository/currency[@id=$idFxRate]/fxrate">
                      <xsl:sort data-type="text" select="fixingDate/text()"/>

                      <xsl:variable name="vCompare">
                        <xsl:call-template name="CompareDate">
                          <xsl:with-param name="pDate1" select="$pFixingDate"/>
                          <xsl:with-param name="pDate2" select="fixingDate/text()"/>
                        </xsl:call-template>
                      </xsl:variable>
                      <xsl:if test="$vCompare = 0 or $vCompare = 1">
                        <xsl:copy-of select="."/>
                      </xsl:if>

                    </xsl:for-each>
                  </xsl:variable>
                  <xsl:copy-of select="msxsl:node-set($vAllFxRate)/fxrate[last()]"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:copy-of select="$gDataDocumentRepository/currency[@id=$idFxRate]/fxrate[fixingDate/text() = $pFixingDate]"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:copy-of select="$gDataDocumentRepository/currency[@id=$idFxRate]/fxrate"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <!-- o UNDONE MF 20110913 -	(voir mal PL 13/09/2011 11:07) o	Utiliser la valeur du rate en tenant compte des 3 
          éléments de quotedCurrencyPair  
          Dans un 1er temps, pour gagner du temps, je te suggère de faire l’impasse et d’utiliser le taux en l’état-->
        <xsl:variable name="fxRate" select="msxsl:node-set($vFxRateRepository)/fxrate/rate"/>
        <xsl:variable name="currency1" select="msxsl:node-set($vFxRateRepository)/fxrate/quotedCurrencyPair/currency1/text()"/>
        <xsl:variable name="currency2" select="msxsl:node-set($vFxRateRepository)/fxrate/quotedCurrencyPair/currency2/text()"/>
        <xsl:variable name="quoteBasis" select="msxsl:node-set($vFxRateRepository)/fxrate/quotedCurrencyPair/quoteBasis/text()"/>

        <xsl:choose>
          <!--Taux inexistant-->
          <xsl:when test="boolean(msxsl:node-set($vFxRateRepository)/fxrate/rate) = false()">
            <xsl:value-of select="number(-1)"/>
          </xsl:when>
          <xsl:when test="number($fxRate) = 0">
            <xsl:value-of select="number(0)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$quoteBasis='Currency1PerCurrency2'">
                <xsl:choose>
                  <xsl:when test="$pExCurrency=$currency1">
                    <xsl:value-of select="number(1) div number($fxRate)"/>
                  </xsl:when>
                  <xsl:when test="$pExCurrency=$currency2">
                    <xsl:value-of select="number($fxRate)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="number(-1)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:when test="$quoteBasis='Currency2PerCurrency1'">
                <xsl:choose>
                  <xsl:when test="$pExCurrency=$currency1">
                    <xsl:value-of select="number($fxRate)"/>
                  </xsl:when>
                  <xsl:when test="$pExCurrency=$currency2">
                    <xsl:value-of select="number(1) div number($fxRate)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="number(-1)"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--ExchangeAmount-->
  <xsl:template name="ExchangeAmount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pFxRate"/>

    <xsl:choose>
      <xsl:when test="(number($pAmount) = 0) or (0 >= number($pFxRate))">
        <xsl:value-of select="number(0)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="number($pAmount) div number($pFxRate)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--abs-->
  <xsl:template name="abs">
    <xsl:param name="n" />

    <xsl:if test="string-length($n) > 0">
      <xsl:choose>
        <xsl:when test="number($n) >= 0">
          <xsl:value-of select="number($n)" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number(-1) * number($n)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <!--AddAttributeOptionnal-->
  <xsl:template name="AddAttributeOptionnal">
    <xsl:param name="pName" />
    <xsl:param name="pValue" />

    <xsl:if test="string-length($pValue) > 0">
      <xsl:attribute name="{$pName}">
        <xsl:value-of select="$pValue"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template name="AddAttributeMandatory">
    <xsl:param name="pName"/>
    <xsl:param name="pValue"/>

    <xsl:attribute name="{$pName}">
      <xsl:choose>
        <xsl:when test="string-length($pValue) > 0 ">
          <xsl:value-of select="$pValue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gcNA"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:attribute>
  </xsl:template>

  <!-- ===== GetTradeAggregationID ===== -->
  <xsl:template name="GetTradeAggregationID">
    <xsl:param name="pHref"/>
    <xsl:param name="pTxt"/>
    <xsl:param name="pInptSrc"/>
    <xsl:param name="pOrdID"/>
    <xsl:param name="pExecID"/>

    <xsl:choose>
      <xsl:when test="string-length($pOrdID) > 0">
        <xsl:value-of select="$pOrdID"/>
      </xsl:when>
      <xsl:when test="string-length($pExecID) > 0">
        <xsl:value-of select="$pExecID"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pHref"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>