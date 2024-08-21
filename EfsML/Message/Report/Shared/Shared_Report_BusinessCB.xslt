<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--  
================================================================================================================
Summary : Spheres report - Shared - Business templates for CashBalance trades
          XSL for preparing of business data in order to generate PDF document of report based on CashBalance trades
          
File    : Shared_Report_BusinessCB.xslt
================================================================================================================
Version : v4.5.5791                                           
Date    : 20151109
Author  : RD
Comment : [21052] Bug affichage des taxes
================================================================================================================
Version : v4.0.5291                                           
Date    : 20140627
Author  : RD
Comment : [20147] Tous les flux XML véhiculent FIXML_SECURITYEXCHANGE du marché, 
          il faut donc utiliser cette info quand on interroge MarketRepository
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking
================================================================================================================
Version : v1.0.0.5 (Ticket 18048, Spheres 2.6.4.0)
Date    : 20120903
Author  : MF
Comment : New Attributes :
        - Using the detail nodes to compute fee VAT, as we do into the account statement
================================================================================================================
Version : v1.0.0.4 (Ticket 18065 Item 7, Spheres 2.6.4.0)
Date    : 20120827
Author  : MF
Comment : New Attributes :
        - "showTotal" the attribute value is used to valorize the EFS/xsl "condition" attribute, in the report model (see template gGroupDisplaySettings)
           the possible values are: 'show', 'hide'. Search for Ticket 18065 Item 7 
================================================================================================================
Version : v1.0.0.3 (Ticket 18065, Spheres 2.6.4.0)
Date    : 20120801
Author  : MF
Comment : 
    - detailed financial report, financial report including the amount details by derivative contract and/or clearing house, 
    any modification is marked with "Ticket 18065". 
    - new template DerivativeContractDetails
================================================================================================================
Version : v1.0.0.2 (Spheres 2.6.3.0)
Date    : 20120626
Author  : MF
Comment : 
 template matching trade nodes, added special attributes "rowspan" and "subtable" for nodes cbStream/amount.
 Attributes deprecated : 
 - "amountAvailable" : the available amount becomes an indipendent amount node
 - "subpar" : not useful, this attribute is not used inside of the template DisplayDetailedCashBalanceForGroup
 New Attributes :
 - "subtable": when this attribute is true, the curernt amount owning this attribute is rendered with an additional margin to mark the 
               amount item as subordinate in comparison with the previous amounts. 
               Possible values : true/false
 - "rowspan": used together with "subtitle". the rowspan attribute indicates the rows spanned by the current subtitle cell rendered in the 
               DisplayDetailedCashBalanceForGroup template.
================================================================================================================
Version : v1.0.0.1 (Spheres 2.6.0.0)
Date    :  
Author  : MF
Comment : 
 Additional attributes for business nodes "amount". These attributes deserve for display purposes during the xsl-fo (PDF source) rendering.
 New Attributes: 
 - "crDr" : containing the CR/DR indicator suffix to indicate the payment direction (CR: creditor, DR: debtor)
 - "subtitle": when specified the amount having this attribute and all the next ones will be prefixed by the label contained inside of the
                attribute. the number of the amounts prefixed by this label are specified by the "rowspan" attribute. 
                If the subtitle is empty it will not be considered by the xsl-fo level (CreateCashBalanceGroups).
 - "keyForSum": used to set the amount as used for following sum/grouping operations. The sum operations will be performed in the 
                 CreateCashBalanceGroups template for the final group "TotEx". Just some amounts need this, because of normally all the amounts 
                 will be summed/grouped by the "name" attribute. The fee/brokerage amounts share the same name then we need another attribute to 
                 make different sums by payment type.
 - "newBlock": when this attribute is true, then a new empty line is rendered on the screen and a new header is put 
               for all the amounts following the current one (the current included)
               owning this attribute. Possible values : true/false
================================================================================================================
Version : v1.0.0.0 (Spheres 2.5.0.0)
Date    : 20100917
Author  : RD
Comment : First version
================================================================================================================  
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Shared_Report_VariablesCB.xslt" />

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                                VARIABLES                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <xsl:key name="kFeePaymentType" match="amount" use="@paymentType"/>

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Business Templates                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!--Trade templates-->
  <xsl:template match="trade" mode="business">
    <!-- useful data like attributes -->
    <xsl:variable name="vTradeId" select="string(./tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid' and string-length(text())>0 and text()!='0']/text())"/>
    <xsl:variable name="vBook" select="string(./tradeHeader/partyTradeIdentifier/bookId[@bookIdScheme='http://www.euro-finance-systems.fr/otcml/bookid' and string-length(text())>0]/text())"/>
    <xsl:variable name="vEntityHref" select="string(./cashBalance/entityPartyReference/@href)"/>
    <xsl:variable name="vIDB" select="number($gDataDocumentRepository/book[identifier=$vBook]/@OTCmlId)"/>

    <xsl:variable name="vPrefixedReportingCurrency" select="concat($gPrefixCurrencyId, $gReportingCurrency)"/>
    <xsl:variable name="vRoundDirReportingCurrency" select="$gDataDocumentRepository/currency[@id=$vPrefixedReportingCurrency]/rounddir"/>
    <xsl:variable name="vRoundPrecReportingCurrency" select="$gDataDocumentRepository/currency[@id=$vPrefixedReportingCurrency]/roundprec"/>

    <xsl:variable name="vManagementBalance" select="normalize-space(./cashBalance/settings/managementBalance/text())"/>
    <!--<xsl:variable name="vUseAvailableCash" select="normalize-space(./cashBalance/settings/useAvailableCash/text())"/>-->

    <trade href="{$vTradeId}"
           tradeDate="{string(./tradeHeader/tradeDate)}"
           timeStamp="{substring(./tradeHeader/tradeDate/@timeStamp,1,5)}"
           repCuRoundDir="{$vRoundDirReportingCurrency}" repCuRoundPrec="{$vRoundPrecReportingCurrency}"
           managementBalance="{$vManagementBalance}" useAvailableCash="{$gUseAvailableCash}">

      <!--Book-->
      <BOOK data="{$vBook}"/>

      <xsl:for-each select="./cashBalance/cashBalanceStream">

        <xsl:variable name="vCurrency" select="string(currency/text())"/>
        <xsl:variable name="vPrefixedCurrencyId" select="concat($gPrefixCurrencyId, $vCurrency)"/>
        <xsl:variable name="vCurrencyDescription" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]/description"/>

        <xsl:variable name="vRoundDir" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]/rounddir"/>
        <xsl:variable name="vRoundPrec" select="$gDataDocumentRepository/currency[@id=$vPrefixedCurrencyId]/roundprec"/>

        <xsl:variable name="vCurrencyFxRate">
          <xsl:call-template name="FxRate">
            <xsl:with-param name="pFlowCurrency" select="$vCurrency" />
            <xsl:with-param name="pExCurrency" select="$gReportingCurrency" />
          </xsl:call-template>
        </xsl:variable>

        <!-- 20120803 MF Ticket 18065 - adding derivative contracts datas, needed by the DerivativeContractDetails template  -->
        <xsl:variable name="vDerivativeContractsRepository">
          <xsl:copy-of select="$gDataDocumentRepository/derivativeContract"/>
        </xsl:variable>

        <!-- 20120803 MF Ticket 18065 - the market repository has been added TODAY (20120803) by FDA into the cash balance XML. 
                                        This is a DONT: we did not want the market repository to be added into the XML because of volumetry.
                                        An alternative is to pass directly the market name in the DC details.
                                        -->
        <xsl:variable name="vMarketRepository">
          <xsl:copy-of select="$gDataDocumentRepository/market"/>
        </xsl:variable>

        <!-- 20120803 MF Ticket 18065 - adding actor party datas, needed by the DerivativeContractDetails template -->
        <xsl:variable name="vPartyRepository">
          <xsl:copy-of select="../../../party"/>
        </xsl:variable>

        <cbStream currency="{$vCurrency}" cuDesc="{$vCurrencyDescription}" cuRoundDir="{$vRoundDir}" cuRoundPrec="{$vRoundPrec}" fxRate="{$vCurrencyFxRate}">

          <xsl:if test="$vManagementBalance = 'true'">

            <!-- Solde Espèces Veille       -->
            <xsl:variable name="vAmountBeforeCashBalance" select="number(cashAvailable/constituent/previousCashBalance/amount/amount/text())"/>
            <xsl:variable name="vExAmountBeforeCashBalance">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountBeforeCashBalance" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vCrDrBeforeCashBalance">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountBeforeCashBalance" />
                <xsl:with-param name="pPayerHref" select="cashAvailable/constituent/previousCashBalance/payerPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="cashAvailable/constituent/previousCashBalance/receiverPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashDayBefore" crDr="{$vCrDrBeforeCashBalance}"
                    amount="{$vAmountBeforeCashBalance}" exAmount="{$vExAmountBeforeCashBalance}"/>
          </xsl:if>

          <!-- Mouvement Espèces       -->
          <xsl:variable name="vAmountCashMouvements" select="number(cashAvailable/constituent/cashBalancePayment/amount/amount/text())"/>

          <!-- Uniquement si la gestion des soldes est activée, ou bien il existe un Mouvement Espèces -->
          <xsl:if test="$vManagementBalance = 'true' or number($vAmountCashMouvements) > 0">

            <xsl:variable name="vExvAmountCashMouvements">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountCashMouvements" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>
            <!-- Le sens (Débit/Crédit) est inversé sur l'état, car:
            - Sur le trade CashBalance: 
            * Le Versement  est toujours Payé par le Client à l’Entité et Reçu par le Clearer  de la part de l’Entité.
            * Le  Retrait est  toujours Reçu par le Client de la part l’Entité et Payé par le Clearer à l’Entité.
            - Sur le report: 
            * Pour un Client: le Versement est au Crédit et le Retrait est au Débit.
            * Pour un Clearer: le Versement est au Crédit et le Retrait est au Débit.-->
            <xsl:variable name="vCrDrAmountCashMouvements">
              <xsl:call-template name="AmountSufix">
                <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                <xsl:with-param name="pAmount" select="$vAmountCashMouvements" />
                <xsl:with-param name="pPayerHref" select="cashAvailable/constituent/cashBalancePayment/receiverPartyReference/@href" />
                <xsl:with-param name="pReceiverHref" select="cashAvailable/constituent/cashBalancePayment/payerPartyReference/@href" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashMouvements" crDr="{$vCrDrAmountCashMouvements}"
                    amount="{$vAmountCashMouvements}" exAmount="{$vExvAmountCashMouvements}"/>
          </xsl:if>

          <!-- Primes sur Options       -->

          <!-- 20120803 MF Ticket 18065 START -->

          <xsl:variable name="vNodeSetFees">
            <xsl:copy-of select="cashAvailable/constituent/cashFlows/constituent//fee//detail"/>
          </xsl:variable>

          <!-- Compute the row span for all the premium, variationMargin, cashSettlement, Fees group -->
          <xsl:variable name="vRowSpanCashFlowConstituent">
            <xsl:choose>
              <xsl:when test="$gIsReportWithDCDetails = true()">
                <xsl:value-of select="
                   count(cashAvailable/constituent/cashFlows/constituent/premium/detail) +  
                   count(cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) +      
                   count(cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) +   
                   count(msxsl:node-set($vNodeSetFees)//detail[count(. | key('amounts-by-marketdc', concat(@Exch, @Sym))[1]) = 1]) + 
                   4"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="4"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$gIsReportWithDCDetails = true()">
            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="$vRowSpanCashFlowConstituent"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="cashAvailable/constituent/cashFlows/constituent/premium"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'optionPremium'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="$vCurrencyFxRate"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="false()"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="vHideEmptyCellAndAmountNamePremium">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/premium/detail) > 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'true'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'false'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 20120803 MF Ticket 18065 END -->

          <!-- 20120827 MF Ticket 18065 Item 7 -->
          <xsl:variable name="vShowTotalPremium">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/premium/detail) = 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'hide'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'show'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vAmountOptionPremium" select="number(cashAvailable/constituent/cashFlows/constituent/premium/paymentAmount/amount/text())"/>
          <xsl:variable name="vExAmountOptionPremium">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountOptionPremium" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrAmountOptionPremium">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountOptionPremium" />
              <xsl:with-param name="pPayerHref" select="cashAvailable/constituent/cashFlows/constituent/premium/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="cashAvailable/constituent/cashFlows/constituent/premium/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="optionPremium" crDr="{$vCrDrAmountOptionPremium}"
                  amount="{$vAmountOptionPremium}" exAmount="{$vExAmountOptionPremium}" rowspan="{$vRowSpanCashFlowConstituent}"
                  hideEmptyCell="{$vHideEmptyCellAndAmountNamePremium}" hideAmountName="{$vHideEmptyCellAndAmountNamePremium}"
                  showTotal="{$vShowTotalPremium}"/>

          <!-- Appels de Marge      -->

          <!-- 20120803 MF Ticket 18065 START -->

          <xsl:if test="$gIsReportWithDCDetails = true()">
            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="1"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="cashAvailable/constituent/cashFlows/constituent/variationMargin"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'variationMargin'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="$vCurrencyFxRate"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="true()"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="vHideAmountNameVariationMargin">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) > 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'true'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'false'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 20120803 MF Ticket 18065 END -->

          <!-- 20120827 MF Ticket 18065 Item 7 -->
          <xsl:variable name="vShowTotalVariationMargin">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/variationMargin/detail) = 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'hide'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'show'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vAmountVariationMargin" select="number(cashAvailable/constituent/cashFlows/constituent/variationMargin/paymentAmount/amount/text())"/>
          <xsl:variable name="vExAmountVariationMargin">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountVariationMargin" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrAmountVariationMargin">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountVariationMargin" />
              <xsl:with-param name="pPayerHref" select="cashAvailable/constituent/cashFlows/constituent/variationMargin/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="cashAvailable/constituent/cashFlows/constituent/variationMargin/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="variationMargin" crDr="{$vCrDrAmountVariationMargin}"
                  amount="{$vAmountVariationMargin}" exAmount="{$vExAmountVariationMargin}"
                  hideEmptyCell="'true'" hideAmountName="{$vHideAmountNameVariationMargin}"
                  showTotal="{$vShowTotalVariationMargin}"/>

          <!-- Cash Settlement     -->

          <!-- 20120803 MF Ticket 18065 START -->

          <xsl:if test="$gIsReportWithDCDetails = true()">
            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="1"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="cashAvailable/constituent/cashFlows/constituent/cashSettlement"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'cashSettlement'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="$vCurrencyFxRate"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="true()"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="vHideAmountNameCashSettlement">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) > 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'true'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'false'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 20120803 MF Ticket 18065 END -->

          <!-- 20120827 MF Ticket 18065 Item 7 -->
          <xsl:variable name="vShowTotalCashSettlement">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/cashSettlement/detail) = 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'hide'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'show'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vAmountCashSettlement" select="number(cashAvailable/constituent/cashFlows/constituent/cashSettlement/paymentAmount/amount/text())"/>
          <xsl:variable name="vExAmountCashSettlement">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountCashSettlement" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrAmountCashSettlement">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountCashSettlement" />
              <xsl:with-param name="pPayerHref" select="cashAvailable/constituent/cashFlows/constituent/cashSettlement/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="cashAvailable/constituent/cashFlows/constituent/cashSettlement/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="cashSettlement" crDr="{$vCrDrAmountCashSettlement}"
                  amount="{$vAmountCashSettlement}" exAmount="{$vExAmountCashSettlement}"
                  hideEmptyCell="'true'" hideAmountName="{$vHideAmountNameCashSettlement}"
                  showTotal="{$vShowTotalCashSettlement}"/>

          <!-- Frais TTC    -->

          <!-- 20120803 MF Ticket 18065 START -->

          <xsl:if test="$gIsReportWithDCDetails = true()">
            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="1"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="cashAvailable/constituent/cashFlows/constituent/fee"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'feesVAT'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="$vCurrencyFxRate"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="true()"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="vHideAmountNameFee">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/fee/detail) > 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'true'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'false'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 20120803 MF Ticket 18065 END -->

          <!-- 20120827 MF Ticket 18065 Item 7 -->
          <xsl:variable name="vShowTotalFeesVat">
            <xsl:choose>
              <xsl:when test="count(cashAvailable/constituent/cashFlows/constituent/fee/detail) = 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'hide'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'show'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 20120906 MF Ticket 18048 - Using the detail nodes to compute fee VAT, as we do into the account statement - START -->
          <!-- RD 20151109 [21052] Bug affichage des taxes  -->
          <xsl:variable name="vHTCFees">
            <xsl:choose>
              <xsl:when test="cashAvailable/constituent/cashFlows/constituent/fee/detail">
                <xsl:value-of select="
                      sum(cashAvailable/constituent/cashFlows/constituent/fee/detail[@AmtSide = $gcCredit]/@Amt)
                      -
                      sum(cashAvailable/constituent/cashFlows/constituent/fee/detail[@AmtSide = $gcDebit]/@Amt)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="0"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vTaxAmount">
            <xsl:choose>
              <xsl:when test="cashAvailable/constituent/cashFlows/constituent/fee/detail/tax">
                <xsl:value-of select="
                      sum(cashAvailable/constituent/cashFlows/constituent/fee/detail[@AmtSide = $gcCredit]/tax/@Amt) 
                      - 
                      sum(cashAvailable/constituent/cashFlows/constituent/fee/detail[@AmtSide = $gcDebit]/tax/@Amt)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="0"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vTTCAmount" select="number($vHTCFees + $vTaxAmount)"/>
          <xsl:variable name="vCrDr">
            <xsl:call-template name="TotalAmountSuffix">
              <xsl:with-param name="pAmount" select="$vTTCAmount" />
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vAmountFeesVat">
            <xsl:call-template name="abs">
              <xsl:with-param name="n" select="$vTTCAmount" />
            </xsl:call-template>
          </xsl:variable>

          <!-- 20120906 MF Ticket 18048 - Using the detail nodes to compute fee VAT, as we do into the account statement - END -->

          <xsl:variable name="vExAmountFeesVat">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountFeesVat" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrAmountFeesVat">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountFeesVat" />
              <xsl:with-param name="pPayerHref" select="cashAvailable/constituent/cashFlows/constituent/fee/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="cashAvailable/constituent/cashFlows/constituent/fee/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="feesVAT" crDr="{$vCrDrAmountFeesVat}"
                  amount="{$vAmountFeesVat}" exAmount="{$vExAmountFeesVat}"
                  hideEmptyCell="'true'" hideAmountName="{$vHideAmountNameFee}"
                  showTotal="{$vShowTotalFeesVat}"/>

          <!-- Appel/Restitution de Déposit -->

          <xsl:variable name="vAmountReturnCallDeposit" select="number(marginCall/paymentAmount/amount/text())"/>
          <xsl:variable name="vExAmountReturnCallDeposit">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountReturnCallDeposit" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrAmountReturnCallDeposit">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountReturnCallDeposit" />
              <xsl:with-param name="pPayerHref" select="marginCall/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="marginCall/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="returnCallDeposit" crDr="{$vCrDrAmountReturnCallDeposit}"
                  amount="{$vAmountReturnCallDeposit}" exAmount="{$vExAmountReturnCallDeposit}"/>

          <!-- Solde Espèces Jour       -->

          <xsl:variable name="vAmountTodayCashBalance" select="number(cashBalance/amount/amount/text())"/>
          <xsl:variable name="vExAmountTodayCashBalance">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountTodayCashBalance" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrTodayCashBalance">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountTodayCashBalance" />
              <xsl:with-param name="pPayerHref" select="cashBalance/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="cashBalance/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="todayCashBalance" crDr="{$vCrDrTodayCashBalance}"
                  amount="{$vAmountTodayCashBalance}" exAmount="{$vExAmountTodayCashBalance}"/>

          <!--   Détails frais et taxes                                           -->

          <!-- 20120626 MF -->
          <!-- [display/sort fee amounts] 1. Build a temporary list containing all the payment types in the current cash available node, adding their own sort order  -->
          <xsl:variable name="vAllFeePaymentTypeAmount">
            <xsl:for-each select="cashAvailable/constituent/cashFlows/constituent/fee/paymentType">

              <xsl:variable name="vFeePaymentTypeXSettings" select="text()"/>

              <amount paymentType="{$vFeePaymentTypeXSettings}"
                      sort="{msxsl:node-set($gcFeeDisplaySettings)/paymentType[@name=$vFeePaymentTypeXSettings]/@sort}"/>

            </xsl:for-each>
          </xsl:variable>

          <!-- 20120626 MF  -->
          <!-- [display/sort fee amounts] 2. generate a key in order to have a single item for each payment type loaded in vAllFeePaymentTypeAmount -->
          <xsl:variable name="vAllFeePaymentType"
                        select="msxsl:node-set($vAllFeePaymentTypeAmount)/amount[generate-id()=generate-id(key('kFeePaymentType',@paymentType))]"/>

          <!-- [display/sort fee amounts] 3. cache all the fee nodes containing payment amounts  -->
          <xsl:variable name="vCacheConstituentFees">
            <xsl:copy-of select="msxsl:node-set(cashAvailable/constituent/cashFlows/constituent/fee)"/>
          </xsl:variable>

          <!-- [display/sort fee amounts] 4. for each elements key related to a specific payment type we search the related amounts, sorted by
                                          the sort attribute in order to dispaly all the payments wit the order specified in gcFeeDisplaySettings -->
          <xsl:for-each select="$vAllFeePaymentType">
            <xsl:sort select="@sort" data-type="number"/>

            <!-- the position is used to build the newBlock attribute only for the first payment amount element, the newBlock attribute
            will be produced the rendering of a new line and a new header, in order to simulate a new table element in the financial report -->
            <xsl:variable name="vFeePosition" select="position()"/>
            <xsl:variable name="vFeePaymentType" select="string(@paymentType)"/>

            <!--   Gross payment                                         -->
            <!-- [display/sort fee amounts] 5. retrieve all the fee (in the cache vCacheConstituentFees ) 
                                               with the specific current fee payment type-->
            <xsl:for-each select="msxsl:node-set($vCacheConstituentFees)/fee[paymentType=$vFeePaymentType]">

              <xsl:variable name="vFeeGrossAmount" select="number(paymentAmount/amount/text())"/>

              <xsl:if test="$vFeeGrossAmount > 0">

                <xsl:variable name="vTypeFeeGrossAmount" select="paymentType"/>

                <!-- 20120803 MF Ticket 18065 START -->

                <!-- Compute the row span for the current Fee item (total item included) -->
                <xsl:variable name="vRowSpanCashFlowDetailFeeGross">
                  <xsl:choose>
                    <xsl:when test="$gIsReportWithDCDetails = true()">
                      <xsl:value-of select="count(detail) + 1"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="1"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <xsl:if test="$gIsReportWithDCDetails = true()">
                  <xsl:call-template name="DerivativeContractDetails">
                    <xsl:with-param name="pRowspanParent" select="$vRowSpanCashFlowDetailFeeGross"/>
                    <xsl:with-param name="pXmlNodeAmount">
                      <xsl:copy-of select="."/>
                    </xsl:with-param>
                    <xsl:with-param name="pAmountPrefix" select="'feeGrossAmount'"/>
                    <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
                    <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
                    <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
                    <xsl:with-param name="pCurrencyFxRate" select ="$vCurrencyFxRate"/>
                    <xsl:with-param name="pTTC" select ="false()"/>
                    <xsl:with-param name="pHideEmptyCell" select="false()"/>
                    <xsl:with-param name="pKeyForSum" select="concat('detail','-',$vTypeFeeGrossAmount)"/>
                  </xsl:call-template>
                </xsl:if>

                <xsl:variable name="vHideAmountNameFeeGross">
                  <xsl:choose>
                    <xsl:when test="count(detail) > 0  and $gIsReportWithDCDetails = true()">
                      <xsl:value-of select="'true'"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'false'"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <!-- 20120803 MF Ticket 18065 END -->

                <xsl:variable name="vExFeeGrossAmount">
                  <xsl:call-template name="ExchangeAmount">
                    <xsl:with-param name="pAmount" select="$vFeeGrossAmount" />
                    <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
                  </xsl:call-template>
                </xsl:variable>

                <xsl:variable name="vFeePayer" select="payerPartyReference/@href"/>
                <xsl:variable name="vFeeReceiver" select="receiverPartyReference/@href"/>

                <xsl:variable name="vCrDrFeeGrossAmount">
                  <xsl:call-template name="AmountSufix">
                    <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                    <xsl:with-param name="pAmount" select="$vFeeGrossAmount" />
                    <xsl:with-param name="pPayerHref" select="$vFeePayer" />
                    <xsl:with-param name="pReceiverHref" select="$vFeeReceiver" />
                  </xsl:call-template>
                </xsl:variable>

                <!-- vHowManyTaxes : count all the taxes amount applied to the current payment gross amount.
                                     This value is used to group with a row span (rowspan="{$vHowManyTaxes}) 
                                     all the tax amounts related to the gross
                                     amount, the gross one included (count(tax/taxDetail) + 1) -->
                <xsl:variable name="vHowManyTaxes">
                  <xsl:choose>
                    <xsl:when test="$gIsReportWithDCDetails = true()">
                      <!-- 2010803 MF Ticket 18065 - for the financial report including DC details the column/row hierarchy model 
                    (defined inside of \Report\DetailedFinancialReport.xsl) uses an additional virtual row, 
                    then we add + 1 for the virtual row -->
                      <xsl:value-of select="count(tax/taxDetail) + number($vRowSpanCashFlowDetailFeeGross) + 1"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="count(tax/taxDetail) + 1"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>

                <!-- 2010803 MF Ticket 18065 - for the financial report including DC details the row span is not the same
                for the details panel related to a specific currency and the final countervalue panel, then we define a specific rowspan 
                for the countervalue panel -->
                <xsl:variable name="vRowSpanForCounterValue" select="count(tax/taxDetail) + 1"/>

                <amount name="feeGrossAmount" crDr="{$vCrDrFeeGrossAmount}"
                        amount="{$vFeeGrossAmount}" exAmount="{$vExFeeGrossAmount}"
                        keyForSum="{$vTypeFeeGrossAmount}" rowspan="{$vHowManyTaxes}" rowspanCounterValue="{$vRowSpanForCounterValue}"
                        hideEmptyCell="'false'" hideAmountName="{$vHideAmountNameFeeGross}">
                  <xsl:if test="$vFeePosition = 1">
                    <xsl:attribute name="newBlock">
                      <xsl:value-of select="'feesConstituents'"/>
                    </xsl:attribute>
                  </xsl:if>
                </amount>

                <!--   Tax Details                                         -->
                <xsl:for-each select="msxsl:node-set(tax/taxDetail)">
                  <xsl:variable name="vFeeDetailAmount" select="number(taxAmount/amount/amount/text())"/>

                  <xsl:variable name="vExFeeDetailAmount">
                    <xsl:call-template name="ExchangeAmount">
                      <xsl:with-param name="pAmount" select="$vFeeDetailAmount" />
                      <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name="vCrDrFeeDetailAmount">
                    <xsl:call-template name="AmountSufix">
                      <xsl:with-param name="pEntityHref" select="$vEntityHref" />
                      <xsl:with-param name="pAmount" select="$vFeeDetailAmount" />
                      <xsl:with-param name="pPayerHref" select="$vFeePayer" />
                      <xsl:with-param name="pReceiverHref" select="$vFeeReceiver" />
                    </xsl:call-template>
                  </xsl:variable>

                  <xsl:variable name="vFeeDetailRate">
                    <xsl:value-of select="number(taxSource/spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetailRate']) * 100"/>
                  </xsl:variable>
                  <xsl:variable name="vFeeDetailName">
                    <xsl:value-of select="taxSource/spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetail']"/>
                  </xsl:variable>

                  <amount name="feeTax" crDr="{$vCrDrFeeDetailAmount}"
                          amount="{$vFeeDetailAmount}" exAmount="{$vExFeeDetailAmount}"
                          taxDetailName="{$vFeeDetailName}" taxDetailRate="{$vFeeDetailRate}"
                          keyForSum="{concat($vFeeDetailName,'-',$vTypeFeeGrossAmount)}">
                  </amount>

                </xsl:for-each>
              </xsl:if>
            </xsl:for-each>
          </xsl:for-each>

          <!--   Total gross payments + taxes                                         -->
          <xsl:if test="cashAvailable/constituent/cashFlows/constituent/fee[number(paymentAmount/amount/text()) > 0]">
            <amount name="totalFeesVAT" crDr="{$vCrDrAmountFeesVat}"
                    amount="{$vAmountFeesVat}" exAmount="{$vExAmountFeesVat}"/>
          </xsl:if>

          <!--   Dernier arrêté                                           -->

          <!-- Encours deposit     -->

          <xsl:variable name="vHowManyOrders">
            <xsl:choose>
              <xsl:when test="$gUseAvailableCash = 'true'">
                <xsl:value-of select="6"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="4"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vAmountPreviousMarginRequirement"
                        select="number(previousMarginConstituent/marginRequirement/amount/amount/text())"/>
          <xsl:variable name="vExAmountPreviousMarginRequirement">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountPreviousMarginRequirement" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="previousMarginRequirement"
                  amount="{$vAmountPreviousMarginRequirement}" exAmount="{$vExAmountPreviousMarginRequirement}"
                  rowspan="{$vHowManyOrders}"/>

          <!-- Garanties(*)    (disponible)     -->

          <xsl:variable name="vAmountPreviousCollateralAvailable"
                        select="number(previousMarginConstituent/collateralAvailable/amount/amount/text())"/>

          <!-- Garanties(*)    (Utilisée)    -->

          <xsl:variable name="vAmountPreviousCollateralUsed"
                        select="number(previousMarginConstituent/collateralUsed/amount/amount/text())"/>

          <xsl:variable name="vExAmountPreviousCollateralUsed">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountPreviousCollateralUsed" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="previousCollateralAvailable" amount="{$vAmountPreviousCollateralAvailable}"/>

          <amount name="previousCollateralUsed"
                  amount="{$vAmountPreviousCollateralUsed}" exAmount="{$vExAmountPreviousCollateralUsed}"/>

          <xsl:if test="$gUseAvailableCash = 'true'">

            <!--   Solde CR en Garantie (disponible)    -->
            <xsl:variable name="vAmountPreviousCashAvailable"
                          select="number(previousMarginConstituent/cashAvailable/amount/amount/text())"/>

            <!-- Solde CR en Garantie   (Utilisée)    -->
            <xsl:variable name="vAmountPreviousCashUsed" select="number(previousMarginConstituent/cashUsed/amount/amount/text())"/>
            <xsl:variable name="vExAmountPreviousCashUsed">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountPreviousCashUsed" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="previousCashAvailable" amount="{$vAmountPreviousCashAvailable}"/>

            <amount name="previousCashUsed"
                    amount="{$vAmountPreviousCashUsed}" exAmount="{$vExAmountPreviousCashUsed}"/>

          </xsl:if>

          <!--   Défaut de garantie (A) -->
          <xsl:variable name="vAmountPreviousUncoveredMarginRequirement"
                        select="number(previousMarginConstituent/uncoveredMarginRequirement/amount/amount/text())"/>
          <xsl:variable name="vExAmountPreviousUncoveredMarginRequirement">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountPreviousUncoveredMarginRequirement" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="previousUncoveredMarginRequirement"
                  amount="{$vAmountPreviousUncoveredMarginRequirement}" exAmount="{$vExAmountPreviousUncoveredMarginRequirement}"/>

          <!--   Arrêté du jour  -->

          <!-- Encours deposit     -->

          <!-- 20120803 MF Ticket 18065 START -->

          <!-- Compute the row span for all the premium, variationMargin, cashSettlement, Fees group -->
          <xsl:variable name="vRowSpanCashFlowDetailReturnCallDeposit">
            <xsl:choose>
              <xsl:when test="$gIsReportWithDCDetails = true() and $gIsReportWithCSSDetails = true()">
                <xsl:value-of select="
                   count(marginRequirement/detail) + number($vHowManyOrders)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vHowManyOrders"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$gIsReportWithDCDetails = true() and $gIsReportWithCSSDetails = true()">
            <xsl:call-template name="DerivativeContractDetails">
              <xsl:with-param name="pRowspanParent" select="$vRowSpanCashFlowDetailReturnCallDeposit"/>
              <xsl:with-param name="pXmlNodeAmount">
                <xsl:copy-of select="marginRequirement"/>
              </xsl:with-param>
              <xsl:with-param name="pAmountPrefix" select="'marginRequirement'"/>
              <xsl:with-param name="pDerivativeContractsRepository" select="$vDerivativeContractsRepository"/>
              <xsl:with-param name="pMarketRepository" select="$vMarketRepository"/>
              <xsl:with-param name="pPartyRepository" select="$vPartyRepository"/>
              <xsl:with-param name="pCurrencyFxRate" select ="$vCurrencyFxRate"/>
              <xsl:with-param name="pTTC" select ="true()"/>
              <xsl:with-param name="pHideEmptyCell" select="false()"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:variable name="vHideEmptyCellAmountNameMarginRequirement">
            <xsl:choose>
              <xsl:when test="count(marginRequirement/detail) > 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'true'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'false'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- 20120803 MF Ticket 18065 END -->

          <!-- 20120827 MF Ticket 18065 Item 7 -->
          <xsl:variable name="vShowTotalMarginRequirement">
            <xsl:choose>
              <xsl:when test="count(marginRequirement/detail) = 0  and $gIsReportWithDCDetails = true()">
                <xsl:value-of select="'hide'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'show'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vAmountMarginRequirement" select="number(marginRequirement/amount/amount/text())"/>
          <xsl:variable name="vExAmountMarginRequirement">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountMarginRequirement" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="marginRequirement"
                  amount="{$vAmountMarginRequirement}" exAmount="{$vExAmountMarginRequirement}" rowspan="{$vRowSpanCashFlowDetailReturnCallDeposit}"
                  hideEmptyCell="{$vHideEmptyCellAmountNameMarginRequirement}" hideAmountName="{$vHideEmptyCellAmountNameMarginRequirement}"
                  showTotal="{$vShowTotalMarginRequirement}"/>

          <!-- Garanties(*)    (disponible)     -->

          <xsl:variable name="vAmountCollateralAvailable" select="number(collateralAvailable/amount/amount/text())"/>

          <!-- Garanties(*)    (Utilisée)    -->

          <xsl:variable name="vAmountCollateralUsed" select="number(collateralUsed/amount/amount/text())"/>
          <xsl:variable name="vExAmountCollateralUsed">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountCollateralUsed" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="collateralAvailable" amount="{$vAmountCollateralAvailable}"/>

          <amount name="collateralUsed"
                  amount="{$vAmountCollateralUsed}" exAmount="{$vExAmountCollateralUsed}"/>

          <xsl:if test="$gUseAvailableCash = 'true'">

            <!--   Solde CR en Garantie (disponible)    -->
            <xsl:variable name="vAmountCashAvailable" select="number(cashAvailable/amount/amount/text())"/>

            <!-- Solde CR en Garantie   (Utilisée)    -->
            <xsl:variable name="vAmountCashUsed" select="number(cashUsed/amount/amount/text())"/>

            <xsl:variable name="vExAmountCashUsed">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAmountCashUsed" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashAvailable" amount="{$vAmountCashAvailable}"/>

            <amount name="cashUsed"
                    amount="{$vAmountCashUsed}" exAmount="{$vExAmountCashUsed}"/>
          </xsl:if>

          <!-- Défaut de garantie (B)   -->

          <xsl:variable name="vAmountUncoveredMarginRequirement" select="number(uncoveredMarginRequirement/amount/amount/text())"/>
          <xsl:variable name="vExAmountUncoveredMarginRequirement">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAmountUncoveredMarginRequirement" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>

          <amount name="uncoveredMarginRequirement"
                  amount="{$vAmountUncoveredMarginRequirement}" exAmount="{$vExAmountUncoveredMarginRequirement}"/>

          <!-- Appel/Restitution de Déposit -->

          <xsl:variable name="vCrDrReturnCallDepositAB">
            <xsl:call-template name="AmountSufix">
              <xsl:with-param name="pEntityHref" select="$vEntityHref" />
              <xsl:with-param name="pAmount" select="$vAmountReturnCallDeposit" />
              <xsl:with-param name="pPayerHref" select="marginCall/payerPartyReference/@href" />
              <xsl:with-param name="pReceiverHref" select="marginCall/receiverPartyReference/@href" />
            </xsl:call-template>
          </xsl:variable>

          <!-- Ressource selon s'il s'agit d'un Appel ou bien d'une Restitution-->
          <xsl:variable name="vCrDrResource">
            <xsl:choose>
              <xsl:when test="string-length($vCrDrReturnCallDepositAB)>0">
                <xsl:value-of select="concat('CSHBAL-returnCallDepositAB-',$vCrDrReturnCallDepositAB)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'CSHBAL-returnCallDepositAB'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <amount name="returnCallDepositAB"
                  amount="{$vAmountReturnCallDeposit}" exAmount="{$vExAmountReturnCallDeposit}"
                  crDrResource="{$vCrDrResource}" crDr="{$vCrDrReturnCallDepositAB}"/>

        </cbStream>

      </xsl:for-each>
    </trade>
  </xsl:template>

  <!--CreateGroups template-->
  <xsl:template name="CreateGroups">
    <xsl:param name="pBook" />
    <xsl:param name="pBookTrades" />

    <xsl:variable name="vBookTrades">
      <xsl:choose>
        <xsl:when test="$pBookTrades">
          <xsl:copy-of select="$pBookTrades"/>
        </xsl:when>
        <xsl:when test="$gIsMonoBook = 'true'">
          <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade[BOOK/@data=$pBook]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <groups>
      <xsl:if test="$gIsMonoBook = 'true'">
        <xsl:attribute name="book">
          <xsl:value-of select="$pBook"/>
        </xsl:attribute>
      </xsl:if>

      <xsl:if test="$gcIsBusinessDebugMode">
        <xsl:copy-of select="msxsl:node-set($vBookTrades)/trade[position() = 1]"/>
      </xsl:if>

      <xsl:call-template name="CreateCashBalanceGroups">
        <xsl:with-param name="pBook" select="$pBook" />
        <xsl:with-param name="pBookTrade" select="msxsl:node-set($vBookTrades)/trade[position() = 1]" />
      </xsl:call-template>
    </groups>

  </xsl:template>

  <!--AmountSufix-->
  <xsl:template name="AmountSufix">
    <xsl:param name="pEntityHref"/>
    <xsl:param name="pAmount"/>
    <xsl:param name="pPayerHref"/>
    <xsl:param name="pReceiverHref"/>

    <xsl:if test="number($pAmount) > 0">
      <xsl:choose>
        <xsl:when test="$pPayerHref = $pEntityHref">
          <xsl:value-of select="$gcCredit"/>
        </xsl:when>
        <xsl:when test="$pReceiverHref = $pEntityHref">
          <xsl:value-of select="$gcDebit"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- 20120806 MF Ticket 18065 START -->

  <!-- Key for grouping amount derivative contract detail nodes by exchange market. Used into the DerivativeContractDetails template  -->
  <xsl:key name="amounts-by-market" match="detail" use="@Exch" />
  <!-- Key for grouping amount derivative contract detail nodes by clearing house. Used into the DerivativeContractDetails template  -->
  <xsl:key name="amounts-by-css" match="detail" use="@cssHref" />
  <!-- Key for grouping amount derivative contract detail nodes by contract 
  (for a specific market, used on node-set already filtered using the amounts-by-market key). 
  Used into the DerivativeContractDetails and trade match templates -->
  <xsl:key name="amounts-by-dc" match="detail" use="@Sym" />
  <!-- Key for grouping amount derivative contract detail nodes by market and contract 
  Used into the DerivativeContractDetails and trade match templates -->
  <xsl:key name="amounts-by-marketdc" match="detail" use="concat(@Exch, @Sym)" />

  <!-- DerivativeContractDetails renders on screen the derivative contract detail found in the input node pXmlNodeAmount, containing
  the current amount constituent. This method creates one amount node for each node detail found into the pXmlNodeAmount source node. -->
  <xsl:template name="DerivativeContractDetails">
    <!-- Given rowspan used to validate the rowspan of the first rendered amount. 
    This parameter is mandatory and it must consider all the possible cases (e.g: not existent details).
    Type: int -->
    <xsl:param name="pRowspanParent"/>
    <!-- Root amount node. Pass a specific node amount of the XML source (e.g: premium, variationMargin, cashSettlement, etc ) 
    in order to build the amount nodes related to the amount//detail node-set. 
    This parameter is mandatory. 
    Type: node-set, use xsl:copy-of -->
    <xsl:param name="pXmlNodeAmount"/>
    <!-- The prefix used to define the name attribute of any amount node. Any built amount will have as name : pAmountPrefix + '-detail'
    This parameter is mandatory. 
    Type: string -->
    <xsl:param name="pAmountPrefix"/>
    <!-- Containing all the derivativeContract nodes of the cash balance repository.
    This parameter is mandatory.  
    Type: node-set, use xsl:copy-of -->
    <xsl:param name="pDerivativeContractsRepository"/>
    <!-- Containing all the market nodes of the cash balance repository.
    This parameter is mandatory.
    Type: node-set, use xsl:copy-of
    -->
    <xsl:param name="pMarketRepository"/>
    <!-- Containing all the party nodes defined into the cash balance
    This parameter is mandatory.
    Type: node-set, use xsl:copy-of
    -->
    <xsl:param name="pPartyRepository"/>
    <!-- Containing the current exchange rate between the current cash balance stream currency and the countervalue currency of the report
    This parameter is mandatory. If you pass ZERO the countervalue amount will NOT be computed, that will be an empty string.
    Type: decimal
    -->
    <xsl:param name="pCurrencyFxRate"/>
    <!--
    Flag to activate the sum of the taxes related over the related derivative conctract detail node
    This parameter is optional. Default value: false
    Type: bool
    -->
    <xsl:param name="pTTC" select="false()"/>
    <!--
    Flag to force the removal/hide of the first empty cell
    This parameter is optional. Default value: false
    Type: bool
    -->
    <xsl:param name="pHideEmptyCell" select="false()"/>
    <!--
    Optional value to fill the keyForSum attribute, used habitually in order to group and sum the amount sharing the same key, but 
    in this case it will be used as dataLink reference according with the provided XSL engine
    This parameter is optional. Default value: ''
    Type: string
    -->
    <xsl:param name="pKeyForSum" select ="''"/>
    <!--
    Optional value to fill the cbDate attribute (containing the payment date for a specific amount).
    This parameter is optional. Default value: ''
    Type: string
    -->
    <xsl:param name="cbDate" select="''"/>

    <xsl:variable name="vAmountName" select="concat($pAmountPrefix, '-detail')"/>

    <xsl:variable name="vAmountsByMarket">
      <xsl:copy-of select="msxsl:node-set($pXmlNodeAmount)//detail[@Exch != '']"/>
    </xsl:variable>

    <!-- Grouping and looping by market -->
    <xsl:for-each select="msxsl:node-set($vAmountsByMarket)/detail[count(. | key('amounts-by-market', @Exch)[1]) = 1]">

      <xsl:variable name="vMarketPosition" select="position()"/>
      <xsl:variable name="vCurrentMarket" select="@Exch"/>
      <xsl:variable name="vCurrentMarketName" select="msxsl:node-set($pMarketRepository)/market[fixml_SecurityExchange = $vCurrentMarket]/shortIdentifier/text()"/>

      <xsl:variable name="vAmountsForCurrentMarket">
        <xsl:copy-of select="msxsl:node-set($vAmountsByMarket)/detail[@Exch = $vCurrentMarket]"/>
      </xsl:variable>

      <!-- Grouping and looping by derivative contracts -->
      <xsl:for-each select="msxsl:node-set($vAmountsForCurrentMarket)/detail[count(. | key('amounts-by-dc', @Sym)[1]) = 1]">

        <xsl:variable name="vDCPosition" select="position()"/>
        <xsl:variable name="vCurrentDC" select="@Sym"/>
        <xsl:variable name="vCurrentDCName" select="
                      concat(
                      msxsl:node-set($pDerivativeContractsRepository)/derivativeContract[identifier = $vCurrentDC]/contractSymbol/text(), 
                      ' - ', 
                      msxsl:node-set($pDerivativeContractsRepository)/derivativeContract[identifier = $vCurrentDC]/displayname/text())"/>

        <xsl:variable name="vDCAmount" select="
                      sum(msxsl:node-set($vAmountsByMarket)/detail[@Exch = $vCurrentMarket and @Sym = $vCurrentDC and @AmtSide = $gcCredit]/@Amt)
                      -
                      sum(msxsl:node-set($vAmountsByMarket)/detail[@Exch = $vCurrentMarket and @Sym = $vCurrentDC and @AmtSide = $gcDebit]/@Amt)"/>

        <xsl:variable name="vTaxAmount">
          <xsl:choose>
            <xsl:when test="$pTTC = true() and msxsl:node-set($vAmountsByMarket)/detail[@Exch = $vCurrentMarket and @Sym = $vCurrentDC]/tax">
              <xsl:value-of select="
                      sum(msxsl:node-set($vAmountsByMarket)/detail[@Exch = $vCurrentMarket and @Sym = $vCurrentDC]/tax[@AmtSide = $gcCredit]/@Amt) 
                      - 
                      sum(msxsl:node-set($vAmountsByMarket)/detail[@Exch = $vCurrentMarket and @Sym = $vCurrentDC]/tax[@AmtSide = $gcDebit]/@Amt)"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="0"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vDCAmountWithTaxes" select="number($vDCAmount + $vTaxAmount)"/>

        <xsl:variable name="vCrDr">
          <xsl:call-template name="TotalAmountSuffix">
            <xsl:with-param name="pAmount" select="$vDCAmountWithTaxes" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vAbsDCAmountWithTaxes">
          <xsl:call-template name="abs">
            <xsl:with-param name="n" select="$vDCAmountWithTaxes" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vExAmount">
          <xsl:choose>
            <xsl:when test="number($pCurrencyFxRate) = 0">
              <xsl:value-of select="''"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAbsDCAmountWithTaxes" />
                <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vLineRowspan">
          <xsl:choose>
            <xsl:when test="number($vMarketPosition) = 1 and number($vDCPosition) = 1">
              <xsl:value-of select="$pRowspanParent"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="1"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vLineRowspanAmount">
          <xsl:choose>
            <xsl:when test="number($vMarketPosition) = 1 and number($vDCPosition) = 1">
              <xsl:value-of select="count(msxsl:node-set($pXmlNodeAmount)//detail[count(. | key('amounts-by-marketdc', concat(@Exch,@Sym))[1]) = 1]) + 1"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="1"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vLineRowspanMarket">
          <xsl:choose>
            <xsl:when test="number($vDCPosition) = 1">
              <xsl:value-of select="count(msxsl:node-set($vAmountsForCurrentMarket)/detail[count(. | key('amounts-by-dc', @Sym)[1]) = 1])"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="1"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vHideEmptyCell">
          <xsl:choose>
            <xsl:when test="$pHideEmptyCell = false() and number($vMarketPosition) = 1 and number($vDCPosition) = 1">
              <xsl:value-of select="'false'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'true'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vHideAmountName">
          <xsl:choose>
            <xsl:when test="number($vMarketPosition) = 1 and number($vDCPosition) = 1">
              <xsl:value-of select="'false'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'true'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vHideMarketName">
          <xsl:choose>
            <xsl:when test="number($vDCPosition) = 1">
              <xsl:value-of select="'false'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'true'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <amount name="{$vAmountName}" nameAmount="{$pAmountPrefix}" nameMarket="{$vCurrentMarketName}" nameDc="{$vCurrentDCName}" crDr="{$vCrDr}"
              amount="{$vAbsDCAmountWithTaxes}" exAmount="{$vExAmount}"
                rowspan ="{$vLineRowspan}" rowspanAmount = "{$vLineRowspanAmount}" rowspanMarketCss="{$vLineRowspanMarket}"
                hideEmptyCell="{$vHideEmptyCell}" hideAmountName="{$vHideAmountName}" hideMarketName="{$vHideMarketName}">
          <xsl:if test="$pKeyForSum != ''">
            <xsl:attribute name="keyForSum" >
              <xsl:value-of select="$pKeyForSum"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="$cbDate != ''">
            <xsl:attribute name="cbDate" >
              <xsl:value-of select="$cbDate"/>
            </xsl:attribute>
          </xsl:if>
        </amount>

      </xsl:for-each>
      <!-- end loop on derivative contract -->
    </xsl:for-each>
    <!-- end loop on markets -->

    <xsl:variable name="vAmountsByCss">
      <xsl:copy-of select="msxsl:node-set($pXmlNodeAmount)//detail[@cssHref != '']"/>
    </xsl:variable>

    <!-- Grouping and looping by on clearing houses -->
    <xsl:for-each select="msxsl:node-set($vAmountsByCss)/detail[count(. | key('amounts-by-css', @cssHref)[1]) = 1]">

      <xsl:variable name="vCssPosition" select="position()"/>
      <xsl:variable name="vCurrentCss" select="@cssHref"/>
      <xsl:variable name="vCurrentCssDescription"
                    select="msxsl:node-set($pPartyRepository)/party[@id = $vCurrentCss]/partyId[@partyIdScheme = 'http://www.euro-finance-systems.fr/otcml/actorDescription']/text()"/>

      <xsl:variable name="vAmountCss"
                          select="
                      sum(msxsl:node-set($vAmountsByCss)/detail[@cssHref = $vCurrentCss and @AmtSide = $gcCredit]/@Amt) 
                      - 
                      sum(msxsl:node-set($vAmountsByCss)/detail[@cssHref = $vCurrentCss and @AmtSide = $gcDebit]/@Amt)"/>

      <xsl:variable name="vTaxAmount">
        <xsl:choose>
          <xsl:when test="$pTTC = true() and msxsl:node-set($vAmountsByCss)/detail[@cssHref = $vCurrentCss]/tax">
            <xsl:value-of select="
                      sum(msxsl:node-set($vAmountsByCss)/detail[@cssHref = $vCurrentCss]/tax[@AmtSide = $gcCredit]/@Amt) 
                      - 
                      sum(msxsl:node-set($vAmountsByCss)/detail[@cssHref = $vCurrentCss]/tax[@AmtSide = $gcDebit]/@Amt)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="0"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vAmountCssWithTaxes" select="number($vAmountCss + $vTaxAmount)"/>

      <xsl:variable name="vCrDrCss">
        <xsl:call-template name="TotalAmountSuffix">
          <xsl:with-param name="pAmount" select="$vAmountCssWithTaxes" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vAbsCssWithTaxes">
        <xsl:call-template name="abs">
          <xsl:with-param name="n" select="$vAmountCssWithTaxes" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vExAmountCss">
        <xsl:choose>
          <xsl:when test="number($pCurrencyFxRate) = 0">
            <xsl:value-of select="''"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAbsCssWithTaxes" />
              <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vLineRowspan">
        <xsl:choose>
          <xsl:when test="number($vCssPosition) = 1">
            <xsl:value-of select="$pRowspanParent"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vLineRowspanAmount">
        <xsl:choose>
          <xsl:when test="number($vCssPosition) = 1">
            <xsl:value-of select="count(msxsl:node-set($pXmlNodeAmount)//detail[count(. | key('amounts-by-css', @cssHref)[1]) = 1]) + 1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vHideEmptyCell">
        <xsl:choose>
          <xsl:when test="$pHideEmptyCell = false() and number($vCssPosition) = 1">
            <xsl:value-of select="'false'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'true'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vHideAmountName">
        <xsl:choose>
          <xsl:when test="number($vCssPosition) = 1">
            <xsl:value-of select="'false'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'true'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <amount name="{$vAmountName}" nameAmount="{$pAmountPrefix}" nameCss="{$vCurrentCssDescription}" crDr="{$vCrDrCss}"
            amount="{$vAbsCssWithTaxes}" exAmount="{$vExAmountCss}"
              rowspan ="{$vLineRowspan}" rowspanAmount = "{$vLineRowspanAmount}" rowspanMarketCss="1"
              hideEmptyCell="{$vHideEmptyCell}" hideAmountName="{$vHideAmountName}"/>

    </xsl:for-each>
    <!-- end loop on clearing houses -->

  </xsl:template>

  <!-- 20120806 MF Ticket 18065 END -->

</xsl:stylesheet>

