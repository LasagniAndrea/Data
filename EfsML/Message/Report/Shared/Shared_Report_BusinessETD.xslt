<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--  
================================================================================================================
Summary : Spheres report - Shared - Business templates for ETD trades
          XSL for preparing of business data in order to generate PDF document of report based on ETD trades
          
File    : Shared_Report_BusinessETD.xslt
================================================================================================================
Version : v3.7.5158                                           
Date    : 20140414
Author  : RD
Comment : [19815] Apply the format-number template in case of value without fractional part
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking  
================================================================================================================
Version : v1.0.2.0 (Spheres 2.6.4.0)
Date    : 20120822 [Ticket 18073]
Author  : MF
Comment : Add converted prices to the report. the strike price, the transaction price will be displayed using the 
 derivative contract specific numerical base and the given style.
================================================================================================================
Version : v1.0.1.0 (Spheres 2.6.4.0)
Date    : 20120712 [Ticket 18006]
Author  : MF
Comment : Add SecondaryTrdType [@TrdTyp2] 
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
  <xsl:include href="Shared_Report_VariablesETD.xslt" />

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                                VARIABLES                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!-- .................................. -->
  <!--   Sorting Keys variables           -->
  <!-- .................................. -->
  <xsl:variable name="gSortKey1" select="string(normalize-space($gSortingKeys[@id='sortKey1']))"/>
  <xsl:variable name="gSortKey2" select="string(normalize-space($gSortingKeys[@id='sortKey2']))"/>
  <xsl:variable name="gSortKey3" select="string(normalize-space($gSortingKeys[@id='sortKey3']))"/>
  <xsl:variable name="gSortKey4" select="string(normalize-space($gSortingKeys[@id='sortKey4']))"/>
  <xsl:variable name="gSortKey5" select="string(normalize-space($gSortingKeys[@id='sortKey5']))"/>
  <xsl:variable name="gSortKey6" select="string(normalize-space($gSortingKeys[@id='sortKey6']))"/>
  <xsl:variable name="gSortKey7" select="string(normalize-space($gSortingKeys[@id='sortKey7']))"/>
  <xsl:variable name="gSortKey8" select="string(normalize-space($gSortingKeys[@id='sortKey8']))"/>
  <xsl:variable name="gSortKey9" select="string(normalize-space($gSortingKeys[@id='sortKey9']))"/>
  <!-- Exist at least one sorting key -->
  <xsl:variable name="gIsKeysFilled" select ="boolean((string-length($gSortKey1) > 0 )
                or (string-length($gSortKey2) > 0 )
                or (string-length($gSortKey3) > 0 )
                or (string-length($gSortKey4) > 0 )
                or (string-length($gSortKey5) > 0 )
                or (string-length($gSortKey6) > 0 )
                or (string-length($gSortKey7) > 0 )
                or (string-length($gSortKey8) > 0 )
                or (string-length($gSortKey9) > 0 ))"/>

  <!-- .................................. -->
  <!--   XSL Keys variables               -->
  <!-- .................................. -->
  <xsl:key name="kSortKeyBook" match="*[@id='sortKey1']" use="@data"/>
  <!---->
  <xsl:key name="kSortKey1" match="*[@id='sortKey1']" use="@data"/>
  <xsl:key name="kSortKey2" match="*[@id='sortKey2']" use="@data"/>
  <xsl:key name="kSortKey3" match="*[@id='sortKey3']" use="@data"/>
  <xsl:key name="kSortKey4" match="*[@id='sortKey4']" use="@data"/>
  <xsl:key name="kSortKey5" match="*[@id='sortKey5']" use="@data"/>
  <xsl:key name="kSortKey6" match="*[@id='sortKey6']" use="@data"/>
  <xsl:key name="kSortKey7" match="*[@id='sortKey7']" use="@data"/>
  <xsl:key name="kSortKey8" match="*[@id='sortKey8']" use="@data"/>
  <xsl:key name="kSortKey9" match="*[@id='sortKey9']" use="@data"/>
  <!--//-->
  <xsl:key name="kAssetMarket" match="MARKET" use="@data"/>
  <xsl:key name="kAssetDC" match="DERIVATIVECONTRACT" use="@data"/>
  <xsl:key name="kAssetMaturity" match="mmy" use="@data"/>
  <xsl:key name="kAssetPC" match="putCall" use="@data"/>
  <xsl:key name="kAssetStrike" match="strkPx" use="@data"/>

  <xsl:key name="kCurrency" match="amount" use="@currency"/>
  <xsl:key name="kPaymentType" match="amount" use="@paymentType"/>
  <xsl:key name="kTaxDetail" match="amount" use="@detail"/>
  <xsl:key name="kTaxRate" match="amount" use="@rate"/>

  <xsl:key name="kOrderIdKey" match="stgy" use="@ordID"/>

  <!-- .................................. -->
  <!--   Keys Values variables            -->
  <!-- .................................. -->
  <xsl:variable name="gAllKeyBookValues" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKeyBook',@data)))]"/>
  <!--//-->
  <xsl:variable name="gAllKey1Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey1',@data)))]"/>
  <xsl:variable name="gAllKey2Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey2',@data)))]"/>
  <xsl:variable name="gAllKey3Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey3',@data)))]"/>
  <xsl:variable name="gAllKey4Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey4',@data)))]"/>
  <xsl:variable name="gAllKey5Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey5',@data)))]"/>
  <xsl:variable name="gAllKey6Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey6',@data)))]"/>
  <xsl:variable name="gAllKey7Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey7',@data)))]"/>
  <xsl:variable name="gAllKey8Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey8',@data)))]"/>
  <xsl:variable name="gAllKey9Values" select="msxsl:node-set($gBusinessTrades)/trade/*[(generate-id()=generate-id(key('kSortKey9',@data)))]"/>

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Business Templates                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->
  <!--Trade templates-->
  <xsl:template match="trade" mode="business">
    <!-- usefull data like attributes -->
    <xsl:variable name="vTradeId" select="string(./tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid' and string-length(text())>0 and text()!='0']/text())"/>
    <xsl:variable name="vBook" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Acct)"/>
    <xsl:variable name="vIDB" select="number($gDataDocumentRepository/book[identifier=$vBook]/@OTCmlId)"/>
    <xsl:variable name="vCategory" select="string(./exchangeTradedDerivative/Category)"/>
    <xsl:variable name="vLastQty" select="number(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastQty)"/>
    <xsl:variable name="vLastPx" select="number(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@LastPx)"/>
    <xsl:variable name="vIdAsset" select="number(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@ID)"/>
    <xsl:variable name="vLinkUnderlyerDelivery" select="$gDataDocumentRepository/tradeLink[identifier_a=$vTradeId and 
                    (link = 'UnderlyerDeliveryAfterAutomaticOptionAssignment'
                    or link = 'UnderlyerDeliveryAfterAutomaticOptionExercise'
                    or link = 'UnderlyerDeliveryAfterOptionAssignment'
                    or link = 'UnderlyerDeliveryAfterOptionExercise')]"/>

    <xsl:variable name="vIsStgy" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@MLegRptTyp) > 0 and 
                  (./exchangeTradedDerivative/FIXML/TrdCaptRpt/@MLegRptTyp = '2') and 
                  string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/TrdLeg/@LegNo) > 0"/>

    <!--//-->
    <!--RD 20110908-->
    <!--Ajout de execID-->
    <trade href="{$vTradeId}" lastQty="{$vLastQty}" lastPx="{$vLastPx}"
           side="{number(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Side)}"
           tradeDate="{string(./tradeHeader/tradeDate)}"
           timeStamp="{substring(./tradeHeader/tradeDate/@timeStamp,1,8)}"
           posEfct="{string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@PosEfct)}">

      <xsl:if test="$vIsStgy = false()">
        <xsl:call-template name="AddAttributeOptionnal">
          <xsl:with-param name="pName" select="'ordID'"/>
          <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@OrdID)"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'txt'"/>
        <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@Txt)"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'inptSrc'"/>
        <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@InptSrc)"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'execID'"/>
        <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@ExecID)"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'trdTyp'"/>
        <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdTyp)"/>
      </xsl:call-template>
      <!-- 20120712 MF Ticket 18006 -->
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'trdTyp2'"/>
        <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdTyp2)"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'trdSubTyp'"/>
        <xsl:with-param name="pValue" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/@TrdSubTyp)"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'tradeId_link'"/>
        <xsl:with-param name="pValue" select="string($vLinkUnderlyerDelivery/identifier/text())"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'execID_link'"/>
        <xsl:with-param name="pValue" select="string($vLinkUnderlyerDelivery/executionId/text())"/>
      </xsl:call-template>
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'maturityDate'"/>
        <xsl:with-param name="pValue" select="$gDataDocumentRepository/etd[@OTCmlId=$vIdAsset]/maturityDate"/>
      </xsl:call-template>
      <!-- 20120827 MF Ticket 18073 -->
      <xsl:call-template name="AddAttributeOptionnal">
        <xsl:with-param name="pName" select="'idAsset'"/>
        <xsl:with-param name="pValue" select="$vIdAsset"/>
      </xsl:call-template>

      <!--
      Dans un premier temps, on affiche cette donnée clearBroker sur aucun report.
      si besoin est, il suffirait de décommenter cette ligne et toutes les lignes qui s'y réfère( chercehr avec 'clearBroker')-->
      <!--clearBroker="{string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/Pty[@R='4']/@ID)}"-->
      <!--//-->
      <xsl:variable name="vPremium">
        <xsl:if test="$vCategory = 'O'">
          <xsl:variable name="vEventPRM" select="$gEvents/Event[(IDENTIFIER_TRADE=$vTradeId) and (EVENTCODE='LPP') and (EVENTTYPE='PRM' or EVENTTYPE='HPR')]"/>
          <xsl:if test="$vEventPRM">
            <xsl:variable name="vPayRec">
              <xsl:choose>
                <xsl:when test="$vEventPRM/IDB_PAY = $vIDB">
                  <xsl:value-of select="$gcPay"/>
                </xsl:when>
                <xsl:when test="$vEventPRM/IDB_REC = $vIDB">
                  <xsl:value-of select="$gcRec"/>
                </xsl:when>
              </xsl:choose>
            </xsl:variable>
            <!--//-->
            <prm pr="{$vPayRec}" currency="{$vEventPRM/UNIT}" amount="{$vEventPRM/VALORISATION}"/>
          </xsl:if>
        </xsl:if>
      </xsl:variable>
      <!-- BusinessSpecificTradeData -->
      <xsl:call-template name="BusinessSpecificTradeData">
        <xsl:with-param name="pTradeId" select="$vTradeId"/>
        <xsl:with-param name="pBook" select="$vBook"/>
        <xsl:with-param name="pIDB" select="$vIDB"/>
        <xsl:with-param name="pCategory" select="$vCategory" />
        <xsl:with-param name="pLastQty" select="$vLastQty"/>
        <xsl:with-param name="pLastPx" select="$vLastPx"/>
        <xsl:with-param name="pPremium" select="msxsl:node-set($vPremium)/prm"/>
      </xsl:call-template>
      <!--//-->
      <!-- 20120821 MF Ticket 18073 -->
      <xsl:variable name="vConvertedLastPx">
        <xsl:call-template name="GetTradeConvertedPriceValue">
          <xsl:with-param name="pIdAsset" select="$vIdAsset"/>
          <xsl:with-param name="pTradeId" select="$vTradeId"/>
        </xsl:call-template>
      </xsl:variable>
      <!-- 20120821 MF Ticket 18073 -->
      <convertedLastPx>
        <xsl:copy-of select="$vConvertedLastPx"/>
      </convertedLastPx>
      <!--//-->
      <xsl:if test="$vIsStgy = true()">
        <stgy ordID="{string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/RptSide/@OrdID)}"
              type="{string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/TrdLeg/Leg/@SecSubTyp)}"
              legno="{number(./exchangeTradedDerivative/FIXML/TrdCaptRpt/TrdLeg/@LegNo)}"
              market="{string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@Exch)}"/>
      </xsl:if>
      <!--//-->
      <!--<book data="{$vBook}"/>-->
      <mmy data="{string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@MMY)}"/>
      <!--//-->
      <xsl:if test="$vCategory = 'O'">
        <strkPx data="{number(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@StrkPx)}"/>
        <xsl:variable name="vPutCall">
          <xsl:choose>
            <xsl:when test="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@PutCall) = '0'">
              <xsl:value-of select="'Put'"/>
            </xsl:when>
            <xsl:when test="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@PutCall) = '1'">
              <xsl:value-of select="'Call'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <putCall data="{$vPutCall}"/>
        <!--//-->
        <!-- 20120821 MF Ticket 18073 -->
        <xsl:variable name="vConvertedStrkPx">
          <xsl:call-template name="GetStrikeConvertedPriceValue">
            <xsl:with-param name="pIdAsset" select="$vIdAsset"/>
          </xsl:call-template>
        </xsl:variable>
        <!-- 20120821 MF Ticket 18073 -->
        <convertedStrkPx>
          <xsl:copy-of select="$vConvertedStrkPx"/>
        </convertedStrkPx>
        <!--//-->
      </xsl:if>
      <!--groupe by criteria-->
      <!--
      Dans un premier temps, on utilise en dure que deux critères de regroupement: Market et DerivativeContract
      Si le besoin d'utiliser d'autres critères se pose, décommenter les lignes suivantes et toutes les lignes qui s'y réfère( chercehr avec 'TRADE_INSTRUMENT')-->
      <!--<TRADE_INSTRUMENT id="{string($gSortingKeys[text()='TRADE_INSTRUMENT']/@id)}">
        <xsl:attribute name="data">
          <xsl:variable name="vTemp" select="string(./exchangeTradedDerivative/productType/text())"/>
          
          <xsl:choose>
            <xsl:when test="string-length($vTemp) > 0 ">
              <xsl:value-of select="$vTemp"/>
            </xsl:when>
            <xsl:when test="string-length($vTemp) = 0 ">
              <xsl:value-of select="$gcNA"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </TRADE_INSTRUMENT>
      <UNDERLYER_INSTRUMENT id="{string($gSortingKeys[text()='UNDERLYER_INSTRUMENT']/@id)}" data="{string($gcNA)}"/>
      <TRADE_AMOUNTCURRENCY id="{string($gSortingKeys[text()='TRADE_AMOUNTCURRENCY']/@id)}" data="{string($gcNA)}"/>
      <CLIENT_TRADER id="{string($gSortingKeys[text()='CLIENT_TRADER']/@id)}" data="{string($gcNA)}"/>
      <CLIENT_SALES id="{string($gSortingKeys[text()='CLIENT_SALES']/@id)}" data="{string($gcNA)}"/>
      <ISIN_CODE id="{string($gSortingKeys[text()='ISIN_CODE']/@id)}" data="{string($gcNA)}"/>-->
      <BOOK id="{string($gSortingKeys[text()='BOOK']/@id)}">
        <xsl:attribute name="data">
          <xsl:choose>
            <xsl:when test="string-length($vBook) > 0 ">
              <xsl:value-of select="$vBook"/>
            </xsl:when>
            <xsl:when test="string-length($vBook) = 0 ">
              <xsl:value-of select="$gcNA"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </BOOK>
      <MARKET id="{string($gSortingKeys[text()='MARKET']/@id)}">
        <xsl:attribute name="data">
          <xsl:variable name="vTemp" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@Exch)"/>
          <!-- // -->
          <xsl:choose>
            <xsl:when test="string-length($vTemp) > 0 ">
              <xsl:value-of select="$vTemp"/>
            </xsl:when>
            <xsl:when test="string-length($vTemp) = 0 ">
              <xsl:value-of select="$gcNA"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </MARKET>
      <DERIVATIVECONTRACT id="{string($gSortingKeys[text()='DERIVATIVECONTRACT']/@id)}">
        <xsl:attribute name="data">
          <xsl:variable name="vTemp" select="string(./exchangeTradedDerivative/FIXML/TrdCaptRpt/Instrmt/@Sym)"/>
          <!-- // -->
          <xsl:choose>
            <xsl:when test="string-length($vTemp) > 0 ">
              <xsl:value-of select="$vTemp"/>
            </xsl:when>
            <xsl:when test="string-length($vTemp) = 0 ">
              <xsl:value-of select="$gcNA"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </DERIVATIVECONTRACT>
    </trade>
  </xsl:template>
  <!--TradeCashBalance template-->
  <xsl:template name="TradeCashBalance">
    <xsl:param name="pBook"/>
    <xsl:param name="pBookTrades"/>
    <xsl:param name="pIsCreditNoteGroup" select="false()" />

    <xsl:variable name="vPrefixedReportingCurrency" select="concat($gPrefixCurrencyId, $gReportingCurrency)"/>
    <xsl:variable name="vRoundDirReportingCurrency" select="$gDataDocumentRepository/currency[@id=$vPrefixedReportingCurrency]/rounddir"/>
    <xsl:variable name="vRoundPrecReportingCurrency" select="$gDataDocumentRepository/currency[@id=$vPrefixedReportingCurrency]/roundprec"/>

    <xsl:variable name="vGroupSettingsNode">
      <xsl:choose>
        <xsl:when test="$pIsCreditNoteGroup">
          <xsl:copy-of select="msxsl:node-set($gGroupDisplaySettings)/group[@name='CBStreamCN']/row"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="msxsl:node-set($gGroupDisplaySettings)/group[@name='CBStream']/row"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vGroupSettings" select="msxsl:node-set($vGroupSettingsNode)/row"/>
    <xsl:variable name="vIsFees" select="$vGroupSettings[@name='feesVAT']"/>
    <xsl:variable name="vIsFeesDet" select="$vGroupSettings[@name='feeGrossAmount']"/>
    <xsl:variable name="vIsPRM" select="$vGroupSettings[@name='optionPremium']"/>
    <xsl:variable name="vIsSCU" select="$vGroupSettings[@name='cashSettlement']"/>
    <xsl:variable name="vIsRMG" select="$vGroupSettings[@name='futureRMG' or @name='optionRMG']"/>
    <xsl:variable name="vIsTotal" select="$vGroupSettings[@name='todayCashBalance']"/>

    <!--//-->
    <trade repCuRoundDir="{$vRoundDirReportingCurrency}" repCuRoundPrec="{$vRoundPrecReportingCurrency}">

      <!--Book-->
      <book data="{$pBook}"/>

      <xsl:variable name="vAllCurrencyAmount">
        <xsl:if test="$vIsFees or $vIsFeesDet or $pIsCreditNoteGroup">
          <xsl:for-each select="$pBookTrades//fees/payment">
            <amount currency="{@currency}"/>
          </xsl:for-each>
        </xsl:if>
        <xsl:if test="$vIsPRM">
          <xsl:for-each select="$pBookTrades/prm">
            <amount currency="{@currency}"/>
          </xsl:for-each>
        </xsl:if>
        <xsl:if test="$vIsRMG">
          <xsl:for-each select="$pBookTrades//rmg">
            <amount currency="{@currency}"/>
          </xsl:for-each>
        </xsl:if>
        <xsl:if test="$vIsSCU">
          <xsl:for-each select="$pBookTrades//scu">
            <amount currency="{@currency}"/>
          </xsl:for-each>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vAllPaymentTypeAmount">
        <xsl:if test="$vIsFees or $vIsFeesDet or $pIsCreditNoteGroup">
          <xsl:for-each select="$pBookTrades//fees/payment">
            <amount paymentType="{@paymentType}" currency="{@currency}" sort="{msxsl:node-set($gcFeeDisplaySettings)/paymentType[@name=current()/@paymentType]/@sort}"/>
          </xsl:for-each>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vAllTaxDetailAmount">
        <xsl:if test="$vIsFeesDet">
          <xsl:for-each select="$pBookTrades//fees/payment/tax">
            <amount paymentType="{../@paymentType}" detail="{@detail}" currency="{../@currency}"/>
          </xsl:for-each>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vAllTaxRateAmount">
        <xsl:if test="$vIsFeesDet">
          <xsl:for-each select="$pBookTrades//fees/payment/tax">
            <amount paymentType="{../@paymentType}" rate="{@rate}" currency="{../@currency}"/>
          </xsl:for-each>
        </xsl:if>
      </xsl:variable>

      <xsl:variable name="vAllCurrency" select="msxsl:node-set($vAllCurrencyAmount)/amount[generate-id()=generate-id(key('kCurrency',@currency))]"/>

      <xsl:for-each select="$vAllCurrency">
        <xsl:sort select="@currency" data-type="text"/>

        <xsl:variable name="vCurrency" select="string(@currency)"/>

        <xsl:variable name="vCurrencyPaymentTypeCopy">
          <xsl:copy-of select="msxsl:node-set($vAllPaymentTypeAmount)/amount[@currency=$vCurrency]"/>
        </xsl:variable>

        <xsl:variable name="vAllPaymentType" select="msxsl:node-set($vCurrencyPaymentTypeCopy)/amount[generate-id()=generate-id(key('kPaymentType',@paymentType))]"/>

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

        <cbStream currency="{$vCurrency}" cuDesc="{$vCurrencyDescription}" cuRoundDir="{$vRoundDir}" cuRoundPrec="{$vRoundPrec}" fxRate="{$vCurrencyFxRate}">

          <!-- Frais TTC    -->
          <xsl:variable name="vAmountFeesDetails">
            <xsl:for-each select="$vAllPaymentType">
              <xsl:sort select="@sort" data-type="number"/>

              <xsl:variable name="vPaymentType" select="string(@paymentType)"/>

              <xsl:call-template name="TradeCashBalanceTax">
                <xsl:with-param name="pBookTrades" select="$pBookTrades"/>
                <xsl:with-param name="pIsFeesDet" select="$vIsFeesDet"/>
                <xsl:with-param name="pPaymentType" select="$vPaymentType"/>
                <xsl:with-param name="pCurrency" select="$vCurrency"/>
                <xsl:with-param name="pCurrencyFxRate" select="$vCurrencyFxRate"/>
                <xsl:with-param name="pRoundDir" select="$vRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$vRoundPrec"/>
                <xsl:with-param name="pAllTaxDetailAmount" select="$vAllTaxDetailAmount"/>
                <xsl:with-param name="pAllTaxRateAmount" select="$vAllTaxRateAmount"/>
                <xsl:with-param name="pIsCreditNoteGroup" select="$pIsCreditNoteGroup"/>
              </xsl:call-template>

              <!--<xsl:variable name="vPaymentAllTaxDetailAmount">
                <xsl:copy-of select="msxsl:node-set($vAllTaxDetailAmount)/amount[@paymentType=$vPaymentType and @currency=$vCurrency]"/>
              </xsl:variable>
              <xsl:variable name="vPaymentAllTaxRateAmount">
                <xsl:copy-of select="msxsl:node-set($vAllTaxRateAmount)/amount[@paymentType=$vPaymentType and @currency=$vCurrency]"/>
              </xsl:variable>

              <xsl:variable name="vAllTaxDetail" select="msxsl:node-set($vPaymentAllTaxDetailAmount)/amount[generate-id()=generate-id(key('kTaxDetail',@detail))]"/>
              <xsl:variable name="vAllTaxRate" select="msxsl:node-set($vPaymentAllTaxRateAmount)/amount[generate-id()=generate-id(key('kTaxRate',@rate))]"/>

              <xsl:variable name="vFeeGrossAmount" select="
                            sum($pBookTrades//fees/payment[@paymentType=$vPaymentType and @currency=$vCurrency and @pr=$gcRec]/@amountET) 
                            - 
                            sum($pBookTrades//fees/payment[@paymentType=$vPaymentType and @currency=$vCurrency and @pr=$gcPay]/@amountET)"/>

              <xsl:variable name="vAbsFeeGrossAmount">
                <xsl:call-template name="abs">
                  <xsl:with-param name="n" select="$vFeeGrossAmount" />
                </xsl:call-template>
              </xsl:variable>

              <xsl:if test="$vAbsFeeGrossAmount > 0">

                <xsl:variable name="vExFeeGrossAmount">
                  <xsl:call-template name="ExchangeAmount">
                    <xsl:with-param name="pAmount" select="$vAbsFeeGrossAmount" />
                    <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
                  </xsl:call-template>
                </xsl:variable>

                <xsl:variable name="vCrDrFeeGrossAmount">
                  <xsl:call-template name="TotalAmountSuffix">
                    <xsl:with-param name="pAmount" select="$vFeeGrossAmount" />
                  </xsl:call-template>
                </xsl:variable>

                -->
              <!-- 20120626 MF adding row span evalaution on transaction report (after enhancements financial report) -->
              <!--
                <xsl:variable name="vHowManyTaxes">
                  <xsl:value-of select="count($vAllTaxDetail) + 1"/>
                </xsl:variable>

                -->
              <!-- 20120626 MF adding row span attribute (rowspan) on transaction report (after enhancements financial report) -->
              <!--
                <amount name="feeGrossAmount" crDr="{$vCrDrFeeGrossAmount}"
                        amount="{$vAbsFeeGrossAmount}" exAmount="{$vExFeeGrossAmount}"
                        keyForSum="{$vPaymentType}" rowspan="{$vHowManyTaxes}">
                </amount>

                -->
              <!--   Details                                         -->
              <!--

                <xsl:for-each select="$vAllTaxDetail">
                  <xsl:sort select="@detail" data-type="text"/>

                  <xsl:variable name="vTaxDetail" select="@detail"/>

                  <xsl:for-each select="$vAllTaxRate">
                    <xsl:sort select="@rate" data-type="number"/>

                    <xsl:variable name="vTaxRate" select="@rate"/>

                    <xsl:variable name="vFeeDetailAmount" select="
                      sum($pBookTrades//fees/payment[@paymentType=$vPaymentType and @currency=$vCurrency and @pr=$gcRec]/tax[@rate=$vTaxRate and @detail=$vTaxDetail]/@amount) 
                      - 
                      sum($pBookTrades//fees/payment[@paymentType=$vPaymentType and @currency=$vCurrency and @pr=$gcPay]/tax[@rate=$vTaxRate and @detail=$vTaxDetail]/@amount)"/>

                    <xsl:variable name="vAbsFeeDetailAmount">
                      <xsl:call-template name="abs">
                        <xsl:with-param name="n" select="$vFeeDetailAmount" />
                      </xsl:call-template>
                    </xsl:variable>

                    <xsl:if test="$vAbsFeeDetailAmount > 0">

                      <xsl:variable name="vFeeDetailAmountRounded">
                        <xsl:call-template name="RoundAmount">
                          <xsl:with-param name="pAmount" select="$vAbsFeeDetailAmount" />
                          <xsl:with-param name="pRoundDir" select="$vRoundDir"/>
                          <xsl:with-param name="pRoundPrec" select="$vRoundPrec"/>
                        </xsl:call-template>
                      </xsl:variable>

                      <xsl:variable name="vExFeeDetailAmount">
                        <xsl:call-template name="ExchangeAmount">
                          <xsl:with-param name="pAmount" select="$vFeeDetailAmountRounded" />
                          <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
                        </xsl:call-template>
                      </xsl:variable>

                      <xsl:variable name="vCrDrFeeDetailAmount">
                        <xsl:call-template name="TotalAmountSuffix">
                          <xsl:with-param name="pAmount" select="$vFeeDetailAmount" />
                        </xsl:call-template>
                      </xsl:variable>

                      <amount name="feeTax" crDr="{$vCrDrFeeDetailAmount}"
                              amount="{$vFeeDetailAmountRounded}" exAmount="{$vExFeeDetailAmount}"
                              taxDetailName="{$vTaxDetail}" taxDetailRate="{$vTaxRate}"
                              keyForSum="{concat($vTaxDetail,'-',$vPaymentType)}">
                      </amount>
                    </xsl:if>
                  </xsl:for-each>
                </xsl:for-each>

              </xsl:if>-->

            </xsl:for-each>
          </xsl:variable>

          <xsl:variable name="vAmountFeesVat" select="
                      sum(msxsl:node-set($vAmountFeesDetails)/amount[@crDr=$gcCredit]/@amount) 
                      - 
                      sum(msxsl:node-set($vAmountFeesDetails)/amount[@crDr=$gcDebit]/@amount)"/>

          <xsl:variable name="vAbsAmountFeesVat">
            <xsl:call-template name="abs">
              <xsl:with-param name="n" select="$vAmountFeesVat" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vExAmountFeesVat">
            <xsl:call-template name="ExchangeAmount">
              <xsl:with-param name="pAmount" select="$vAbsAmountFeesVat" />
              <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vCrDrAmountFeesVat">
            <xsl:call-template name="TotalAmountSuffix">
              <xsl:with-param name="pAmount" select="$vAmountFeesVat" />
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vAmountOptionPremium">
            <xsl:choose>
              <xsl:when test="$pBookTrades/prm">
                <xsl:value-of select="
                      sum($pBookTrades/prm[@currency=$vCurrency and @pr=$gcRec]/@amount) 
                      - 
                      sum($pBookTrades/prm[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="number('0')"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- Primes sur Options  -->
          <xsl:if test="$vIsPRM">
            <xsl:variable name="vAbsAmountOptionPremium">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vAmountOptionPremium" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountOptionPremium">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAbsAmountOptionPremium" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrAmountOptionPremium">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vAmountOptionPremium" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="optionPremium" crDr="{$vCrDrAmountOptionPremium}"
                    amount="{$vAbsAmountOptionPremium}" exAmount="{$vExAmountOptionPremium}"/>
          </xsl:if>

          <!-- Résultat réalisé sur Futures et Options      -->
          <xsl:if test="$vIsRMG">
            <!-- Résultat réalisé sur Futures       -->
            <xsl:variable name="vAmountFutureRMG" select="
                      sum($pBookTrades[@cat='F']//rmg[@currency=$vCurrency and @pr=$gcRec]/@amount) 
                      - 
                      sum($pBookTrades[@cat='F']//rmg[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>

            <xsl:variable name="vAbsAmountFutureRMG">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vAmountFutureRMG" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountFutureRMG">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAbsAmountFutureRMG" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrAmountFutureRMG">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vAmountFutureRMG" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="futureRMG" crDr="{$vCrDrAmountFutureRMG}"
                    amount="{$vAbsAmountFutureRMG}" exAmount="{$vExAmountFutureRMG}"/>

            <!-- Résultat réalisé sur Options       -->
            <xsl:variable name="vAmountOptionRMG" select="
                      sum($pBookTrades[@cat='O']//rmg[@currency=$vCurrency and @pr=$gcRec]/@amount) 
                      - 
                      sum($pBookTrades[@cat='O']//rmg[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>

            <xsl:variable name="vAbsAmountOptionRMG">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vAmountOptionRMG" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountOptionRMG">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAbsAmountOptionRMG" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrAmountOptionRMG">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vAmountOptionRMG" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="optionRMG" crDr="{$vCrDrAmountOptionRMG}"
                    amount="{$vAbsAmountOptionRMG}" exAmount="{$vExAmountOptionRMG}"/>
          </xsl:if>

          <!-- Cash Settlement  -->
          <xsl:if test="$vIsSCU">
            <xsl:variable name="vAmountCashSettlement" select="
                      sum($pBookTrades//scu[@currency=$vCurrency and @pr=$gcRec]/@amount) 
                      - 
                      sum($pBookTrades//scu[@currency=$vCurrency and @pr=$gcPay]/@amount)"/>

            <xsl:variable name="vAbsAmountCashSettlement">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vAmountCashSettlement" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountCashSettlement">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAbsAmountCashSettlement" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrAmountCashSettlement">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vAmountCashSettlement" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="cashSettlement" crDr="{$vCrDrAmountCashSettlement}"
                    amount="{$vAbsAmountCashSettlement}" exAmount="{$vExAmountCashSettlement}"/>
          </xsl:if>

          <!-- Frais TTC    -->
          <xsl:if test="$vIsFees">
            <amount name="feesVAT" crDr="{$vCrDrAmountFeesVat}"
                    amount="{$vAbsAmountFeesVat}" exAmount="{$vExAmountFeesVat}"/>
          </xsl:if>

          <!-- Solde Espèces Jour -->
          <xsl:if test="$vIsTotal">
            <xsl:variable name="vAmountTodayCashBalance" select="number($vAmountOptionPremium + $vAmountFeesVat)"/>

            <xsl:variable name="vAbsAmountTodayCashBalance">
              <xsl:call-template name="abs">
                <xsl:with-param name="n" select="$vAmountTodayCashBalance" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExAmountTodayCashBalance">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vAbsAmountTodayCashBalance" />
                <xsl:with-param name="pFxRate" select="$vCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrTodayCashBalance">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vAmountTodayCashBalance" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="todayCashBalance" crDr="{$vCrDrTodayCashBalance}"
                    amount="{$vAbsAmountTodayCashBalance}" exAmount="{$vExAmountTodayCashBalance}"/>
          </xsl:if>

          <!-- Détails frais et taxes -->
          <xsl:if test="$vIsFeesDet and count(msxsl:node-set($vAmountFeesDetails)/amount) > 0">
            <xsl:copy-of select="$vAmountFeesDetails"/>
            <amount name="totalFeesVAT" crDr="{$vCrDrAmountFeesVat}"
                    amount="{$vAbsAmountFeesVat}" exAmount="{$vExAmountFeesVat}"/>
          </xsl:if>
        </cbStream>
      </xsl:for-each>
    </trade>
  </xsl:template>

  <xsl:template name="TradeCashBalanceTaxCommon">
    <xsl:param name="pBookTrades"/>
    <xsl:param name="pPaymentType"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pCurrencyFxRate"/>
    <xsl:param name="pRoundDir"/>
    <xsl:param name="pRoundPrec"/>
    <xsl:param name="pAllTaxDetailAmount"/>
    <xsl:param name="pAllTaxRateAmount"/>

    <xsl:variable name="vPaymentAllTaxDetailAmount">
      <xsl:copy-of select="msxsl:node-set($pAllTaxDetailAmount)/amount[@paymentType=$pPaymentType and @currency=$pCurrency]"/>
    </xsl:variable>
    <xsl:variable name="vPaymentAllTaxRateAmount">
      <xsl:copy-of select="msxsl:node-set($pAllTaxRateAmount)/amount[@paymentType=$pPaymentType and @currency=$pCurrency]"/>
    </xsl:variable>

    <xsl:variable name="vAllTaxDetail" select="msxsl:node-set($vPaymentAllTaxDetailAmount)/amount[generate-id()=generate-id(key('kTaxDetail',@detail))]"/>
    <xsl:variable name="vAllTaxRate" select="msxsl:node-set($vPaymentAllTaxRateAmount)/amount[generate-id()=generate-id(key('kTaxRate',@rate))]"/>

    <xsl:variable name="vFeeGrossAmount" select="
                            sum($pBookTrades//fees/payment[@paymentType=$pPaymentType and @currency=$pCurrency and @pr=$gcRec]/@amountET) 
                            - 
                            sum($pBookTrades//fees/payment[@paymentType=$pPaymentType and @currency=$pCurrency and @pr=$gcPay]/@amountET)"/>

    <xsl:variable name="vAbsFeeGrossAmount">
      <xsl:call-template name="abs">
        <xsl:with-param name="n" select="$vFeeGrossAmount" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="$vAbsFeeGrossAmount > 0">

      <xsl:variable name="vExFeeGrossAmount">
        <xsl:call-template name="ExchangeAmount">
          <xsl:with-param name="pAmount" select="$vAbsFeeGrossAmount" />
          <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vCrDrFeeGrossAmount">
        <xsl:call-template name="TotalAmountSuffix">
          <xsl:with-param name="pAmount" select="$vFeeGrossAmount" />
        </xsl:call-template>
      </xsl:variable>

      <!-- 20120626 MF adding row span evalaution on transaction report (after enhancements financial report) -->
      <xsl:variable name="vHowManyTaxes">
        <xsl:value-of select="count($vAllTaxDetail) + 1"/>
      </xsl:variable>

      <!-- 20120626 MF adding row span attribute (rowspan) on transaction report (after enhancements financial report) -->
      <amount name="feeGrossAmount" crDr="{$vCrDrFeeGrossAmount}"
              amount="{$vAbsFeeGrossAmount}" exAmount="{$vExFeeGrossAmount}"
              keyForSum="{$pPaymentType}" rowspan="{$vHowManyTaxes}">
      </amount>

      <!--   Details                                         -->

      <xsl:for-each select="$vAllTaxDetail">
        <xsl:sort select="@detail" data-type="text"/>

        <xsl:variable name="vTaxDetail" select="@detail"/>

        <xsl:for-each select="$vAllTaxRate">
          <xsl:sort select="@rate" data-type="number"/>

          <xsl:variable name="vTaxRate" select="@rate"/>

          <xsl:variable name="vFeeDetailAmount" select="
                      sum($pBookTrades//fees/payment[@paymentType=$pPaymentType and @currency=$pCurrency and @pr=$gcRec]/tax[@rate=$vTaxRate and @detail=$vTaxDetail]/@amount) 
                      - 
                      sum($pBookTrades//fees/payment[@paymentType=$pPaymentType and @currency=$pCurrency and @pr=$gcPay]/tax[@rate=$vTaxRate and @detail=$vTaxDetail]/@amount)"/>

          <xsl:variable name="vAbsFeeDetailAmount">
            <xsl:call-template name="abs">
              <xsl:with-param name="n" select="$vFeeDetailAmount" />
            </xsl:call-template>
          </xsl:variable>

          <xsl:if test="$vAbsFeeDetailAmount > 0">

            <xsl:variable name="vFeeDetailAmountRounded">
              <xsl:call-template name="RoundAmount">
                <xsl:with-param name="pAmount" select="$vAbsFeeDetailAmount" />
                <xsl:with-param name="pRoundDir" select="$pRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$pRoundPrec"/>
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vExFeeDetailAmount">
              <xsl:call-template name="ExchangeAmount">
                <xsl:with-param name="pAmount" select="$vFeeDetailAmountRounded" />
                <xsl:with-param name="pFxRate" select="$pCurrencyFxRate" />
              </xsl:call-template>
            </xsl:variable>

            <xsl:variable name="vCrDrFeeDetailAmount">
              <xsl:call-template name="TotalAmountSuffix">
                <xsl:with-param name="pAmount" select="$vFeeDetailAmount" />
              </xsl:call-template>
            </xsl:variable>

            <amount name="feeTax" crDr="{$vCrDrFeeDetailAmount}"
                    amount="{$vFeeDetailAmountRounded}" exAmount="{$vExFeeDetailAmount}"
                    taxDetailName="{$vTaxDetail}" taxDetailRate="{$vTaxRate}"
                    keyForSum="{concat($vTaxDetail,'-',$pPaymentType)}">
            </amount>
          </xsl:if>
        </xsl:for-each>
      </xsl:for-each>

    </xsl:if>
  </xsl:template>

  <!--CreateGroups template-->
  <xsl:template name="CreateGroups">
    <xsl:param name="pBook" />
    <xsl:param name="pBookTrades" />
    <xsl:param name="pETDBlockGroup" />
    <xsl:param name="pIsCreditNoteGroup" select="false()" />

    <!--//-->
    <groups>
      <xsl:if test="$gIsMonoBook = 'true'">
        <xsl:attribute name="book">
          <xsl:value-of select="$pBook"/>
        </xsl:attribute>
      </xsl:if>

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
      <xsl:variable name="vGroupTrades" select="msxsl:node-set($vBookTrades)/trade"/>

      <!-- ETDBlockGroup -->
      <xsl:variable name="vETDBlockGroupNode">
        <xsl:choose>
          <xsl:when test="$pETDBlockGroup">
            <xsl:copy-of select="$pETDBlockGroup"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="msxsl:node-set($gGroupDisplaySettings)/group[@name='ETDBlock']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vETDBlockGroup" select="msxsl:node-set($vETDBlockGroupNode)/group"/>
      <xsl:if test="$vETDBlockGroup">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true'">
            <xsl:call-template name="CreateGroupsForBook">
              <xsl:with-param name="pBook" select="$pBook" />
              <xsl:with-param name="pBookTrades" select="$vGroupTrades"/>
              <xsl:with-param name="pETDBlockGroup" select="$vETDBlockGroup"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$gReportHeaderFooter/sort/text()='Book'">

            <xsl:for-each select="msxsl:node-set($gBusinessBooks)/BOOK">
              <xsl:sort select="@data" data-type="text"/>

              <xsl:variable name="vCurrentBook" select="@data"/>

              <xsl:call-template name="CreateGroupsForBook">
                <xsl:with-param name="pBook" select="$vCurrentBook" />
                <xsl:with-param name="pBookTrades" select="msxsl:node-set($gBusinessTrades)/trade[BOOK/@data=$vCurrentBook]"/>
                <xsl:with-param name="pETDBlockGroup" select="$vETDBlockGroup"/>
              </xsl:call-template>
            </xsl:for-each>

          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="CreateGroupsForBook">
              <xsl:with-param name="pBook" select="$pBook" />
              <xsl:with-param name="pBookTrades" select="$vGroupTrades"/>
              <xsl:with-param name="pETDBlockGroup" select="$vETDBlockGroup"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>

      <!-- ETDRecapStgy -->
      <xsl:variable name="vETDRecapStgy" select="msxsl:node-set($gGroupDisplaySettings)/group[@name='ETDRecapStgy']"/>
      <xsl:if test="$vETDRecapStgy">
        <xsl:call-template name="CreateStgyGroup">
          <xsl:with-param name="pBook" select="$pBook" />
          <xsl:with-param name="pGroupTrades" select="$vGroupTrades"/>
          <xsl:with-param name="pGroupSettings" select="$vETDRecapStgy"/>
        </xsl:call-template>
      </xsl:if>

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
        <!-- Cash-Balance Groups (CBStreamGroup and CBStreamExGroup) -->
        <xsl:variable name="vGroupTradesCB">
          <xsl:call-template name="TradeCashBalance">
            <xsl:with-param name="pBook" select="$pBook" />
            <xsl:with-param name="pBookTrades" select="$vGroupTrades" />
            <xsl:with-param name="pIsCreditNoteGroup" select="$pIsCreditNoteGroup"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$gcIsBusinessDebugMode">
          <xsl:copy-of select="msxsl:node-set($vGroupTradesCB)/trade"/>
        </xsl:if>
        <xsl:call-template name="CreateCashBalanceGroups">
          <xsl:with-param name="pBook" select="$pBook" />
          <xsl:with-param name="pBookTrade" select="msxsl:node-set($vGroupTradesCB)/trade" />
          <xsl:with-param name="pIsCreditNoteGroup" select="$pIsCreditNoteGroup"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:call-template name="CreateSpecificGroups">
        <xsl:with-param name="pBook" select="$pBook" />
        <xsl:with-param name="pBookTrades" select="$vGroupTrades" />
      </xsl:call-template>

    </groups>
  </xsl:template>
  <xsl:template name="CreateGroupsForBook">
    <xsl:param name="pBook" />
    <xsl:param name="pBookTrades" />
    <xsl:param name="pETDBlockGroup" />

    <!--//-->
    <xsl:choose>
      <xsl:when test="$gIsKeysFilled = false()">
        <xsl:call-template name="CreateGroup">
          <xsl:with-param name="pBook" select="$pBook" />
          <xsl:with-param name="pGroupTrades" select="$pBookTrades"/>
          <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$gIsKeysFilled">
        <xsl:choose>
          <xsl:when test="count($gAllKey1Values) = 0">
            <xsl:call-template name="CreateGroup">
              <xsl:with-param name="pBook" select="$pBook" />
              <xsl:with-param name="pGroupTrades" select="$pBookTrades"/>
              <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="count($gAllKey1Values) > 0">
            <xsl:for-each select="$gAllKey1Values">
              <xsl:sort select="@data" data-type="text"/>

              <xsl:variable name="vKey1Data" select="@data"/>
              <xsl:variable name="vBookTradesWithKey1" select="$pBookTrades[./*[name()=$gSortKey1 and @data=$vKey1Data]]"/>
              <!--//-->
              <xsl:if test="count($vBookTradesWithKey1) > 0">
                <xsl:choose>
                  <xsl:when test="count($gAllKey2Values) = 0">

                    <xsl:variable name="vBookTradesWithKey1Copy">
                      <xsl:copy-of select="$vBookTradesWithKey1"/>
                    </xsl:variable>

                    <xsl:call-template name="CreateGroup">
                      <xsl:with-param name="pBook" select="$pBook" />
                      <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey1Copy)/trade"/>
                      <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                      <xsl:with-param name="pKeys">
                        <keyValue href="sortKey1" data="{$vKey1Data}"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="count($gAllKey2Values) > 0">
                    <xsl:for-each select="$gAllKey2Values">
                      <xsl:sort select="@data" data-type="text"/>

                      <xsl:variable name="vKey2Data" select="@data"/>
                      <xsl:variable name="vBookTradesWithKey2" select="$vBookTradesWithKey1[./*[name()=$gSortKey2 and @data=$vKey2Data]]"/>
                      <!--//-->
                      <xsl:if test="count($vBookTradesWithKey2) > 0">
                        <xsl:choose>
                          <xsl:when test="count($gAllKey3Values) = 0">

                            <xsl:variable name="vBookTradesWithKey2Copy">
                              <xsl:copy-of select="$vBookTradesWithKey2"/>
                            </xsl:variable>

                            <xsl:call-template name="CreateGroup">
                              <xsl:with-param name="pBook" select="$pBook" />
                              <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey2Copy)/trade"/>
                              <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                              <xsl:with-param name="pKeys">
                                <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                <keyValue href="sortKey2" data="{$vKey2Data}"/>
                              </xsl:with-param>
                            </xsl:call-template>
                          </xsl:when>
                          <xsl:when test="count($gAllKey3Values) > 0">
                            <xsl:for-each select="$gAllKey3Values">
                              <xsl:sort select="@data" data-type="text"/>

                              <xsl:variable name="vKey3Data" select="@data"/>
                              <xsl:variable name="vBookTradesWithKey3" select="$vBookTradesWithKey2[./*[name()=$gSortKey3 and @data=$vKey3Data]]"/>
                              <!--//-->
                              <xsl:if test="count($vBookTradesWithKey3) > 0">
                                <xsl:choose>
                                  <xsl:when test="count($gAllKey4Values) = 0">

                                    <xsl:variable name="vBookTradesWithKey3Copy">
                                      <xsl:copy-of select="$vBookTradesWithKey3"/>
                                    </xsl:variable>

                                    <xsl:call-template name="CreateGroup">
                                      <xsl:with-param name="pBook" select="$pBook" />
                                      <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey3Copy)/trade"/>
                                      <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                      <xsl:with-param name="pKeys">
                                        <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                        <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                        <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                      </xsl:with-param>
                                    </xsl:call-template>
                                  </xsl:when>
                                  <xsl:when test="count($gAllKey4Values) > 0">
                                    <xsl:for-each select="$gAllKey4Values">
                                      <xsl:sort select="@data" data-type="text"/>

                                      <xsl:variable name="vKey4Data" select="@data"/>
                                      <xsl:variable name="vBookTradesWithKey4" select="$vBookTradesWithKey3[./*[name()=$gSortKey4 and @data=$vKey4Data]]"/>
                                      <!--//-->
                                      <xsl:if test="count($vBookTradesWithKey4) > 0">
                                        <xsl:choose>
                                          <xsl:when test="count($gAllKey5Values) = 0">

                                            <xsl:variable name="vBookTradesWithKey4Copy">
                                              <xsl:copy-of select="$vBookTradesWithKey4"/>
                                            </xsl:variable>

                                            <xsl:call-template name="CreateGroup">
                                              <xsl:with-param name="pBook" select="$pBook" />
                                              <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey4Copy)/trade"/>
                                              <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                              <xsl:with-param name="pKeys">
                                                <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                                <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                                <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                                <keyValue href="sortKey4" data="{$vKey4Data}"/>
                                              </xsl:with-param>
                                            </xsl:call-template>
                                          </xsl:when>
                                          <xsl:when test="count($gAllKey5Values) > 0">
                                            <xsl:for-each select="$gAllKey5Values">
                                              <xsl:sort select="@data" data-type="text"/>

                                              <xsl:variable name="vKey5Data" select="@data"/>
                                              <xsl:variable name="vBookTradesWithKey5" select="$vBookTradesWithKey4[./*[name()=$gSortKey5 and @data=$vKey5Data]]"/>
                                              <!--//-->
                                              <xsl:if test="count($vBookTradesWithKey5) > 0">
                                                <xsl:choose>
                                                  <xsl:when test="count($gAllKey6Values) = 0">

                                                    <xsl:variable name="vBookTradesWithKey5Copy">
                                                      <xsl:copy-of select="$vBookTradesWithKey5"/>
                                                    </xsl:variable>

                                                    <xsl:call-template name="CreateGroup">
                                                      <xsl:with-param name="pBook" select="$pBook" />
                                                      <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey5Copy)/trade"/>
                                                      <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                                      <xsl:with-param name="pKeys">
                                                        <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                                        <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                                        <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                                        <keyValue href="sortKey4" data="{$vKey4Data}"/>
                                                        <keyValue href="sortKey5" data="{$vKey5Data}"/>
                                                      </xsl:with-param>
                                                    </xsl:call-template>
                                                  </xsl:when>
                                                  <xsl:when test="count($gAllKey6Values) > 0">
                                                    <xsl:for-each select="$gAllKey6Values">
                                                      <xsl:sort select="@data" data-type="text"/>

                                                      <xsl:variable name="vKey6Data" select="@data"/>
                                                      <xsl:variable name="vBookTradesWithKey6" select="$vBookTradesWithKey5[./*[name()=$gSortKey6 and @data=$vKey6Data]]"/>
                                                      <!--//-->
                                                      <xsl:if test="count($vBookTradesWithKey6) > 0">
                                                        <xsl:choose>
                                                          <xsl:when test="count($gAllKey7Values) = 0">

                                                            <xsl:variable name="vBookTradesWithKey6Copy">
                                                              <xsl:copy-of select="$vBookTradesWithKey6"/>
                                                            </xsl:variable>

                                                            <xsl:call-template name="CreateGroup">
                                                              <xsl:with-param name="pBook" select="$pBook" />
                                                              <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey6Copy)/trade"/>
                                                              <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                                              <xsl:with-param name="pKeys">
                                                                <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                                                <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                                                <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                                                <keyValue href="sortKey4" data="{$vKey4Data}"/>
                                                                <keyValue href="sortKey5" data="{$vKey5Data}"/>
                                                                <keyValue href="sortKey6" data="{$vKey6Data}"/>
                                                              </xsl:with-param>
                                                            </xsl:call-template>
                                                          </xsl:when>
                                                          <xsl:when test="count($gAllKey7Values) > 0">
                                                            <xsl:for-each select="$gAllKey7Values">
                                                              <xsl:sort select="@data" data-type="text"/>

                                                              <xsl:variable name="vKey7Data" select="@data"/>
                                                              <xsl:variable name="vBookTradesWithKey7" select="$vBookTradesWithKey6[./*[name()=$gSortKey7 and @data=$vKey7Data]]"/>
                                                              <!--//-->
                                                              <xsl:if test="count($vBookTradesWithKey7) > 0">
                                                                <xsl:choose>
                                                                  <xsl:when test="count($gAllKey8Values) = 0">

                                                                    <xsl:variable name="vBookTradesWithKey7Copy">
                                                                      <xsl:copy-of select="$vBookTradesWithKey7"/>
                                                                    </xsl:variable>

                                                                    <xsl:call-template name="CreateGroup">
                                                                      <xsl:with-param name="pBook" select="$pBook" />
                                                                      <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey7Copy)/trade"/>
                                                                      <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                                                      <xsl:with-param name="pKeys">
                                                                        <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                                                        <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                                                        <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                                                        <keyValue href="sortKey4" data="{$vKey4Data}"/>
                                                                        <keyValue href="sortKey5" data="{$vKey5Data}"/>
                                                                        <keyValue href="sortKey6" data="{$vKey6Data}"/>
                                                                        <keyValue href="sortKey7" data="{$vKey7Data}"/>
                                                                      </xsl:with-param>
                                                                    </xsl:call-template>
                                                                  </xsl:when>
                                                                  <xsl:when test="count($gAllKey8Values) > 0">
                                                                    <xsl:for-each select="$gAllKey8Values">
                                                                      <xsl:sort select="@data" data-type="text"/>

                                                                      <xsl:variable name="vKey8Data" select="@data"/>
                                                                      <xsl:variable name="vBookTradesWithKey8" select="$vBookTradesWithKey7[./*[name()=$gSortKey8 and @data=$vKey8Data]]"/>
                                                                      <!--//-->
                                                                      <xsl:if test="count($vBookTradesWithKey8) > 0">
                                                                        <xsl:choose>
                                                                          <xsl:when test="count($gAllKey9Values) = 0">

                                                                            <xsl:variable name="vBookTradesWithKey8Copy">
                                                                              <xsl:copy-of select="$vBookTradesWithKey8"/>
                                                                            </xsl:variable>

                                                                            <xsl:call-template name="CreateGroup">
                                                                              <xsl:with-param name="pBook" select="$pBook" />
                                                                              <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey8Copy)/trade"/>
                                                                              <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                                                              <xsl:with-param name="pKeys">
                                                                                <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                                                                <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                                                                <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                                                                <keyValue href="sortKey4" data="{$vKey4Data}"/>
                                                                                <keyValue href="sortKey5" data="{$vKey5Data}"/>
                                                                                <keyValue href="sortKey6" data="{$vKey6Data}"/>
                                                                                <keyValue href="sortKey7" data="{$vKey7Data}"/>
                                                                                <keyValue href="sortKey8" data="{$vKey8Data}"/>
                                                                              </xsl:with-param>
                                                                            </xsl:call-template>
                                                                          </xsl:when>
                                                                          <xsl:when test="count($gAllKey9Values) > 0">
                                                                            <xsl:for-each select="$gAllKey9Values">
                                                                              <xsl:sort select="@data" data-type="text"/>

                                                                              <xsl:variable name="vKey9Data" select="@data"/>
                                                                              <xsl:variable name="vBookTradesWithKey9" select="$vBookTradesWithKey8[./*[name()=$gSortKey9 and @data=$vKey9Data]]"/>
                                                                              <!--//-->
                                                                              <xsl:if test="count($vBookTradesWithKey9) > 0">

                                                                                <xsl:variable name="vBookTradesWithKey9Copy">
                                                                                  <xsl:copy-of select="$vBookTradesWithKey9"/>
                                                                                </xsl:variable>

                                                                                <xsl:call-template name="CreateGroup">
                                                                                  <xsl:with-param name="pBook" select="$pBook" />
                                                                                  <xsl:with-param name="pGroupTrades" select="msxsl:node-set($vBookTradesWithKey9Copy)/trade"/>
                                                                                  <xsl:with-param name="pETDBlockGroup" select="$pETDBlockGroup"/>
                                                                                  <xsl:with-param name="pKeys">
                                                                                    <keyValue href="sortKey1" data="{$vKey1Data}"/>
                                                                                    <keyValue href="sortKey2" data="{$vKey2Data}"/>
                                                                                    <keyValue href="sortKey3" data="{$vKey3Data}"/>
                                                                                    <keyValue href="sortKey4" data="{$vKey4Data}"/>
                                                                                    <keyValue href="sortKey5" data="{$vKey5Data}"/>
                                                                                    <keyValue href="sortKey6" data="{$vKey6Data}"/>
                                                                                    <keyValue href="sortKey7" data="{$vKey7Data}"/>
                                                                                    <keyValue href="sortKey8" data="{$vKey8Data}"/>
                                                                                    <keyValue href="sortKey9" data="{$vKey9Data}"/>
                                                                                  </xsl:with-param>
                                                                                </xsl:call-template>
                                                                              </xsl:if>
                                                                            </xsl:for-each>
                                                                          </xsl:when>
                                                                        </xsl:choose>
                                                                      </xsl:if>
                                                                    </xsl:for-each>
                                                                  </xsl:when>
                                                                </xsl:choose>
                                                              </xsl:if>
                                                            </xsl:for-each>
                                                          </xsl:when>
                                                        </xsl:choose>
                                                      </xsl:if>
                                                    </xsl:for-each>
                                                  </xsl:when>
                                                </xsl:choose>
                                              </xsl:if>
                                            </xsl:for-each>
                                          </xsl:when>
                                        </xsl:choose>
                                      </xsl:if>
                                    </xsl:for-each>
                                  </xsl:when>
                                </xsl:choose>
                              </xsl:if>
                            </xsl:for-each>
                          </xsl:when>
                        </xsl:choose>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:when>
                </xsl:choose>
              </xsl:if>
            </xsl:for-each>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--CreateGroup template-->
  <xsl:template name="CreateGroup">
    <xsl:param name="pBook" />
    <xsl:param name="pGroupTrades" />
    <xsl:param name="pETDBlockGroup" />
    <xsl:param name="pKeys" />

    <xsl:if test="count($pGroupTrades) > 0">
      <xsl:variable name="vDCGlobal" select="string($pGroupTrades/DERIVATIVECONTRACT[string-length(@data) > 0][1]/@data)" />

      <xsl:variable name="vDCRepository" select="$gDataDocumentRepository/derivativeContract[identifier=$vDCGlobal]"/>
      <xsl:variable name="vCuPrice" select="string($vDCRepository/idC_Price/text())"/>
      <xsl:variable name="vCategory" select="string($vDCRepository/category/text())"/>
      <!--//-->
      <group sort="{$pETDBlockGroup/@sort}" name="{$pETDBlockGroup/@name}" cuPrice="{$vCuPrice}" cat="{$vCategory}">
        <xsl:variable name="vGroupePaymentCu">
          <xsl:apply-templates select="$gPaymentTypes" mode="groupe-currency">
            <xsl:with-param name="pPayments" select="$pGroupTrades//fees/payment"/>
          </xsl:apply-templates>
        </xsl:variable>

        <xsl:variable name="vGroupePaymentCurrency" select="msxsl:node-set($vGroupePaymentCu)/currency"/>

        <xsl:for-each select="$vGroupePaymentCurrency">
          <xsl:call-template name="AddAttributeOptionnal">
            <xsl:with-param name="pName" select="@name"/>
            <xsl:with-param name="pValue" select="@value"/>
          </xsl:call-template>
        </xsl:for-each>

        <xsl:if test="$pETDBlockGroup/@name = 'AmendedETDBlock'">
          <xsl:call-template name="AddAttributeOptionnal">
            <xsl:with-param name="pName" select="'cuDelta'"/>
            <xsl:with-param name="pValue" select="$vGroupePaymentCurrency/@value"/>
          </xsl:call-template>
        </xsl:if>

        <xsl:copy-of select="$pKeys"/>
        <!--//-->
        <xsl:variable name="vAllAssetMarket" select="$pGroupTrades/MARKET[(generate-id()=generate-id(key('kAssetMarket',@data)))]"/>

        <xsl:variable name="vSeries">
          <xsl:for-each select="$vAllAssetMarket">
            <xsl:sort select="@data" data-type="text"/>

            <xsl:variable name="vMarket" select="@data"/>
            <xsl:variable name="vMarketTradesCopy">
              <xsl:copy-of select="$pGroupTrades[MARKET/@data = $vMarket]"/>
            </xsl:variable>
            <xsl:variable name="vAllAssetDC" select="msxsl:node-set($vMarketTradesCopy)/trade/
                          DERIVATIVECONTRACT[(generate-id()=generate-id(key('kAssetDC',@data)))]"/>

            <xsl:for-each select="$vAllAssetDC">
              <xsl:sort select="@data" data-type="text"/>

              <xsl:variable name="vDC" select="@data"/>
              <xsl:variable name="vDCCategory" select="$gDataDocumentRepository/derivativeContract[identifier=$vDC]/category"/>

              <xsl:variable name="vDCTradesCopy">
                <xsl:copy-of select="msxsl:node-set($vMarketTradesCopy)/trade[(MARKET/@data = $vMarket) and(DERIVATIVECONTRACT/@data = $vDC)]"/>
              </xsl:variable>

              <xsl:choose>
                <xsl:when test="$gIsMonoBook = 'true'">
                  <xsl:call-template name="CreateSeriesForBook">
                    <xsl:with-param name="pBookTrades" select="msxsl:node-set($vDCTradesCopy)/trade"/>
                    <xsl:with-param name="pDCCategory" select="$vDCCategory"/>
                    <xsl:with-param name="pCuPrice" select="$vCuPrice"/>
                    <xsl:with-param name="pGroupePaymentCu" select="$vGroupePaymentCurrency" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:variable name="vAllAssetBook" select="msxsl:node-set($vDCTradesCopy)/trade/
                                BOOK[(generate-id()=generate-id(key('kBookKey',@data)))]"/>

                  <xsl:for-each select="$vAllAssetBook">
                    <xsl:sort select="@data" data-type="text"/>

                    <xsl:variable name="vBook" select="@data"/>
                    <xsl:variable name="vBookTradesCopy">
                      <xsl:copy-of select="msxsl:node-set($vDCTradesCopy)/trade[BOOK/@data = $vBook]"/>
                    </xsl:variable>

                    <xsl:call-template name="CreateSeriesForBook">
                      <xsl:with-param name="pBook" select="$vBook"/>
                      <xsl:with-param name="pBookTrades" select="msxsl:node-set($vBookTradesCopy)/trade"/>
                      <xsl:with-param name="pDCCategory" select="$vDCCategory"/>
                      <xsl:with-param name="pCuPrice" select="$vCuPrice"/>
                      <xsl:with-param name="pGroupePaymentCu" select="$vGroupePaymentCurrency" />
                    </xsl:call-template>
                  </xsl:for-each>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:variable>
        <!--//-->
        <xsl:copy-of select="$vSeries"/>
        <!--//-->
        <xsl:call-template name="SpecificGroupSubTotal">
          <xsl:with-param name="pSeries" select="$vSeries"/>
          <xsl:with-param name="pGroupePaymentCu" select="$vGroupePaymentCurrency" />
          <xsl:with-param name="pCuPrice" select="$vCuPrice" />
        </xsl:call-template>
      </group>
    </xsl:if>
  </xsl:template>
  <!--CreateSeriesForBook template-->
  <xsl:template name="CreateSeriesForBook">
    <xsl:param name="pBook"/>
    <xsl:param name="pBookTrades"/>
    <xsl:param name="pDCCategory"/>
    <xsl:param name="pCuPrice"/>
    <xsl:param name="pGroupePaymentCu" />

    <xsl:variable name="vAllAssetMaturity" select="$pBookTrades/mmy[(generate-id()=generate-id(key('kAssetMaturity',@data)))]"/>

    <xsl:for-each select="$vAllAssetMaturity">
      <xsl:sort select="string-length(@data)" data-type="number"/>
      <xsl:sort select="@data" data-type="text"/>

      <xsl:variable name="vMaturity" select="@data"/>
      <xsl:variable name="vMaturityTradesCopy">
        <xsl:copy-of select="$pBookTrades[mmy/@data = $vMaturity]"/>
      </xsl:variable>

      <xsl:choose>
        <xsl:when test="$pDCCategory = 'F'">
          <xsl:call-template name="CreateSerie">
            <xsl:with-param name="pSerieTrades" select="msxsl:node-set($vMaturityTradesCopy)/trade"/>
            <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
            <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu" />
            <xsl:with-param name="pBook" select="$pBook"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDCCategory = 'O'">
          <xsl:variable name="vAllAssetPC" select="msxsl:node-set($vMaturityTradesCopy)/trade/
                        putCall[(generate-id()=generate-id(key('kAssetPC',@data)))]"/>

          <xsl:for-each select="$vAllAssetPC">
            <xsl:sort select="@data" data-type="text"/>

            <xsl:variable name="vPC" select="@data"/>

            <xsl:variable name="vTradesPCCopy">
              <xsl:copy-of select="msxsl:node-set($vMaturityTradesCopy)/trade[(putCall/@data = $vPC)]"/>
            </xsl:variable>

            <xsl:variable name="vAllAssetStrike" select="msxsl:node-set($vTradesPCCopy)/trade/
                          strkPx[(generate-id()=generate-id(key('kAssetStrike',@data)))]"/>

            <xsl:for-each select="$vAllAssetStrike">
              <xsl:sort select="@data" data-type="number"/>

              <xsl:variable name="vStrike" select="@data"/>
              <xsl:variable name="vStrikeTradesCopy">
                <xsl:copy-of select="msxsl:node-set($vTradesPCCopy)/trade[(strkPx/@data = $vStrike)]"/>
              </xsl:variable>
              <xsl:call-template name="CreateSerie">
                <xsl:with-param name="pSerieTrades" select="msxsl:node-set($vStrikeTradesCopy)/trade"/>
                <xsl:with-param name="pCuPrice" select="$pCuPrice"/>
                <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu" />
                <xsl:with-param name="pBook" select="$pBook"/>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  <!--CreateSerie template-->
  <xsl:template name="CreateSerie">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pCuPrice"/>
    <xsl:param name="pGroupePaymentCu" />
    <xsl:param name="pBook"/>

    <xsl:if test="count($pSerieTrades) > 0">
      <serie>
        <xsl:if test="$gIsMonoBook = 'false'">
          <xsl:attribute name="book">
            <xsl:value-of select="$pBook"/>
          </xsl:attribute>
        </xsl:if>
        <!--//-->
        <xsl:variable name="vSerieTradesWithStatus">
          <xsl:for-each select="$pSerieTrades">
            <trade href="{@href}">
              <xsl:if test="prm[@currency!=$pCuPrice]">
                <prm status="{$gcNOkStatus}"/>
              </xsl:if>
              <!--//-->
              <!-- BusinessSpecificTradeWithStatusData -->
              <xsl:call-template name="BusinessSpecificTradeWithStatusData">
                <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu" />
                <xsl:with-param name="pCuPrice" select="$pCuPrice" />
              </xsl:call-template>
            </trade>
          </xsl:for-each>
        </xsl:variable>
        <!--//-->
        <xsl:copy-of select="$vSerieTradesWithStatus"/>
        <!--//-->
        <sum>
          <xsl:call-template name="SerieSubTotal">
            <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
            <xsl:with-param name="pSerieTradesWithStatus" select="msxsl:node-set($vSerieTradesWithStatus)/trade"/>
            <xsl:with-param name="pCuPrice" select="$pCuPrice" />
          </xsl:call-template>
          <!--//-->
          <xsl:call-template name="SpecificSerieSubTotal">
            <xsl:with-param name="pSerieTrades" select="$pSerieTrades"/>
            <xsl:with-param name="pSerieTradesWithStatus" select="msxsl:node-set($vSerieTradesWithStatus)/trade"/>
            <xsl:with-param name="pGroupePaymentCu" select="$pGroupePaymentCu" />
            <xsl:with-param name="pCuPrice" select="$pCuPrice" />
          </xsl:call-template>
        </sum>
      </serie>
    </xsl:if>

  </xsl:template>
  <!--SerieSubTotal template-->
  <xsl:template name="SerieSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pCuPrice"/>

    <xsl:if test="count($pSerieTradesWithStatus/prm[@status = $gcNOkStatus]) = 0">
      <xsl:variable name="vSumPay" select="sum($pSerieTrades/prm[@pr=$gcPay]/@amount)"/>
      <xsl:variable name="vSumRec" select="sum($pSerieTrades/prm[@pr=$gcRec]/@amount)"/>
      <xsl:variable name="vSum" select="number($vSumPay) - number($vSumRec)"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="number($vSum) > 0">
          <prm pr="{$gcPay}" currency="{$pCuPrice}" amount="{$vSum}"/>
        </xsl:when>
        <xsl:when test="0 > number($vSum) ">
          <prm pr="{$gcRec}" currency="{$pCuPrice}" amount="{$vSum * -1}"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <!-- SpecificSerieSubTotal template -->
  <xsl:template name="AllocAndInvSerieSubTotal">
    <xsl:param name="pSerieTrades"/>
    <xsl:param name="pSerieTradesWithStatus"/>
    <xsl:param name="pGroupePaymentCu" />
    <xsl:param name="pCuPrice"/>

    <!--//-->
    <!-- avg formula : sum( qty of trade * prix of trade) / sum ( qty of all trades) -->
    <xsl:variable name="vSumLongQty" select="sum($pSerieTrades[@side=1]/@lastQty)"/>
    <xsl:variable name="vSumLongTotalPx" select="sum($pSerieTrades[@side=1]/@totalPx)"/>
    <xsl:variable name="vAvgLong">
      <xsl:choose>
        <xsl:when test="$vSumLongQty > 0">
          <xsl:value-of select="number($vSumLongTotalPx) div number($vSumLongQty)"/>
        </xsl:when>
        <xsl:when test="$vSumLongQty = 0">
          <xsl:value-of select="'0'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vSumShortQty" select="sum($pSerieTrades[@side=2]/@lastQty)"/>
    <xsl:variable name="vSumShortTotalPx" select="sum($pSerieTrades[@side=2]/@totalPx)"/>
    <xsl:variable name="vAvgShort">
      <xsl:choose>
        <xsl:when test="$vSumShortQty > 0">
          <xsl:value-of select="number($vSumShortTotalPx) div number($vSumShortQty)"/>
        </xsl:when>
        <xsl:when test="$vSumShortQty = 0">
          <xsl:value-of select="'0'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <xsl:variable name="vPosQty">
      <xsl:call-template name="abs">
        <xsl:with-param name="n" select="number($vSumLongQty - $vSumShortQty)" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vPosSLF">
      <xsl:choose>
        <xsl:when test="$vSumShortQty > $vSumLongQty">
          <xsl:value-of select="'Short'"/>
        </xsl:when>
        <xsl:when test="$vSumLongQty > $vSumShortQty">
          <xsl:value-of select="'Long'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Flat'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <xsl:if test="$vSumLongQty >= 0">
      <long qty="{$vSumLongQty}" avgPx="{$vAvgLong}"/>
      <!-- 20120827 MF Long converted quantity -->
      <convertedLong>
        <xsl:call-template name="GetConvertedAveragePriceValue">
          <xsl:with-param name="pIdAsset" select="$pSerieTrades/@idAsset"/>
          <xsl:with-param name="pSide" select="'1'"/>
        </xsl:call-template>
      </convertedLong>
    </xsl:if>
    <xsl:if test="$vSumShortQty >= 0">
      <short qty="{$vSumShortQty}" avgPx="{$vAvgShort}"/>
      <!-- 20120827 MF Long converted quantity -->
      <convertedShort>
        <xsl:call-template name="GetConvertedAveragePriceValue">
          <xsl:with-param name="pIdAsset" select="$pSerieTrades/@idAsset"/>
          <xsl:with-param name="pSide" select="'2'"/>
        </xsl:call-template>
      </convertedShort>
    </xsl:if>
    <pos qty="{$vPosQty}" slf="{$vPosSLF}"/>
  </xsl:template>

  <!--CreateStgyGroup template-->
  <xsl:template name="CreateStgyGroup">
    <xsl:param name="pBook" />
    <xsl:param name="pGroupTrades" />
    <xsl:param name="pGroupSettings" />

    <xsl:variable name="vStgyTrades" select="$pGroupTrades[stgy]"/>
    <xsl:if test="count($vStgyTrades) > 0">
      <group sort="{$pGroupSettings/@sort}" name="{$pGroupSettings/@name}">
        <xsl:choose>
          <xsl:when test="$gIsMonoBook = 'true'">
            <xsl:call-template name="CreateStgyForBook">
              <xsl:with-param name="pBookTrades" select="$vStgyTrades"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>

            <xsl:variable name="vAllStgyBooksCopy">
              <xsl:copy-of select="$vStgyTrades"/>
            </xsl:variable>
            <xsl:variable name="vAllStgyBooks" select="msxsl:node-set($vAllStgyBooksCopy)/trade/
                                BOOK[(generate-id()=generate-id(key('kBookKey',@data)))]"/>

            <!--<xsl:variable name="vAllStgyBooks" select="$vStgyTrades/BOOK[(generate-id()=generate-id(key('kBookKey',@data)))]"/>-->

            <xsl:for-each select="$vAllStgyBooks">
              <xsl:sort select="@data" data-type="text"/>

              <xsl:variable name="vBook" select="@data"/>
              <xsl:variable name="vBookTrades" select="$vStgyTrades[(BOOK/@data = $vBook)]"/>

              <xsl:call-template name="CreateStgyForBook">
                <xsl:with-param name="pBook" select="$vBook"/>
                <xsl:with-param name="pBookTrades" select="$vBookTrades"/>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>
      </group>
    </xsl:if>
  </xsl:template>
  <!--CreateStgyForBook template-->
  <xsl:template name="CreateStgyForBook">
    <xsl:param name="pBook" />
    <xsl:param name="pBookTrades" />

    <xsl:variable name="vAllStgy" select="$pBookTrades/stgy[(generate-id()=generate-id(key('kOrderIdKey',@ordID)))]"/>

    <xsl:for-each select="$vAllStgy">
      <xsl:sort select="@market" data-type="text"/>
      <xsl:sort select="@ordID" data-type="text"/>

      <xsl:variable name="vOrdID" select="string(@ordID)"/>
      <xsl:variable name="vStgyTrades" select="$pBookTrades[stgy/@ordID = $vOrdID]"/>

      <xsl:if test="count($vStgyTrades) > 0">
        <stgy>

          <xsl:if test="$gIsMonoBook = 'false'">
            <xsl:attribute name="book">
              <xsl:value-of select="$pBook"/>
            </xsl:attribute>
          </xsl:if>

          <xsl:for-each select="$vStgyTrades">
            <xsl:sort select="stgy/@legno" data-type="number"/>

            <trade href="{@href}"/>

          </xsl:for-each>
        </stgy>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!--paymentType templates-->
  <xsl:template match="paymentType" mode="create">
    <xsl:param name="pPty" />
    <xsl:param name="pOtherPartyPayment" />

    <xsl:variable name="vPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@id=current()/@id]/@name"/>
    <!-- RD 20120302 / Ne considérer dans les avis d’opéré que des frais quotidiens , donc non destinés à être inclus dans une facture périodique-->
    <!-- RD 20120308 / Rollback de la modif ci-dessus-->
    <xsl:variable name="vAllPtyPayment">
      <!--<xsl:copy-of select="./otherPartyPayment[(string(paymentType/text()) = string($pPaymentType)) 
                   and ((string(payerPartyReference/@href) = string($pPty)) or (string(receiverPartyReference/@href) = string($pPty)))
                   and paymentSource[spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/feeInvoicing' and (text()='false')]]]"/>-->
      <xsl:copy-of select="$pOtherPartyPayment[(string(paymentType/text()) = string($vPaymentType)) 
                   and ((string(payerPartyReference/@href) = string($pPty)) or (string(receiverPartyReference/@href) = string($pPty)))]"/>
    </xsl:variable>
    <xsl:variable name="vAllPtyPaymentCount" select="count(msxsl:node-set($vAllPtyPayment)/otherPartyPayment)"/>
    <!--//-->
    <xsl:if test="number($vAllPtyPaymentCount) > 0">
      <xsl:variable name="vAllCuPayments" select="msxsl:node-set($vAllPtyPayment)/otherPartyPayment[string-length(paymentAmount/currency) > 0]/paymentAmount/currency"/>
      <xsl:variable name="vFirstCuPayments" select="$vAllCuPayments[position()=1]"/>
      <xsl:variable name="vIsAllPtyPaymentWithSameCu" select="$vAllPtyPaymentCount = count(msxsl:node-set($vAllPtyPayment)/otherPartyPayment[(paymentAmount/currency) = $vFirstCuPayments])"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="$vIsAllPtyPaymentWithSameCu">
          <xsl:variable name="vPayPayment" select="msxsl:node-set($vAllPtyPayment)/otherPartyPayment/payerPartyReference/@href"/>
          <xsl:variable name="vRecPayment" select="msxsl:node-set($vAllPtyPayment)/otherPartyPayment/receiverPartyReference/@href"/>
          <!--//-->
          <xsl:variable name="vIsAllPtyPaymentPayed" select="$vAllPtyPaymentCount = count(msxsl:node-set($vAllPtyPayment)/otherPartyPayment[(string(payerPartyReference/@href) = string($pPty)) and (string(receiverPartyReference/@href) = string($vRecPayment))])"/>
          <xsl:variable name="vIsAllPtyPaymentReceived" select="$vAllPtyPaymentCount = count(msxsl:node-set($vAllPtyPayment)/otherPartyPayment[(string(receiverPartyReference/@href) = string($pPty)) and (string(payerPartyReference/@href) = string($vPayPayment))])"/>
          <!--//-->
          <xsl:choose>
            <xsl:when test="$vIsAllPtyPaymentPayed or $vIsAllPtyPaymentReceived">
              <xsl:variable name="vPayRec">
                <xsl:choose>
                  <xsl:when test="$vIsAllPtyPaymentPayed">
                    <xsl:value-of select="$gcPay"/>
                  </xsl:when>
                  <xsl:when test="$vIsAllPtyPaymentReceived">
                    <xsl:value-of select="$gcRec"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vAmountET">
                <xsl:choose>
                  <xsl:when test="$vAllPtyPaymentCount = 1">
                    <xsl:value-of select="msxsl:node-set($vAllPtyPayment)/otherPartyPayment/paymentAmount/amount"/>
                  </xsl:when>
                  <xsl:when test="$vAllPtyPaymentCount > 1">
                    <xsl:value-of select="sum(msxsl:node-set($vAllPtyPayment)/otherPartyPayment/paymentAmount/amount)"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>

              <xsl:if test="string-length($vAmountET)>0">
                <payment paymentType="{$vPaymentType}" pr="{$vPayRec}" currency="{$vFirstCuPayments}" amountET="{$vAmountET}">

                  <xsl:variable name="vAllTaxDetail" select="msxsl:node-set($vAllPtyPayment)/otherPartyPayment/tax/taxDetail/taxSource/spheresId[generate-id()=generate-id(key('kSpheresIdTaxDetail',text()))]"/>

                  <xsl:for-each select="$vAllTaxDetail">
                    <xsl:sort select="text()" data-type="text"/>

                    <xsl:variable name="vTaxDetail" select="text()"/>

                    <xsl:variable name="vAllTaxRate" select="msxsl:node-set($vAllPtyPayment)/otherPartyPayment/tax/taxDetail/taxSource/spheresId[generate-id()=generate-id(key('kSpheresIdTaxRate',text()))]"/>

                    <xsl:for-each select="$vAllTaxRate">
                      <xsl:sort select="text()" data-type="text"/>

                      <xsl:variable name="vTaxRate">
                        <xsl:value-of select="text()"/>
                      </xsl:variable>
                      <xsl:variable name="vTaxAmount">
                        <xsl:if test="(msxsl:node-set($vAllPtyPayment)/otherPartyPayment/tax/taxDetail[taxSource[
                              spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetail' and (text()=$vTaxDetail)]
                              and spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetailRate' and (text()=$vTaxRate)]
                              ]]/taxAmount/amount/amount)">
                          <xsl:value-of select="sum(msxsl:node-set($vAllPtyPayment)/otherPartyPayment/tax/taxDetail[taxSource[
                              spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetail' and (text()=$vTaxDetail)]
                              and spheresId[@scheme='http://www.euro-finance-systems.fr/otcml/taxDetailRate' and (text()=$vTaxRate)]
                              ]]/taxAmount/amount/amount)"/>
                        </xsl:if>
                      </xsl:variable>

                      <xsl:if test="string-length($vTaxAmount) > 0">
                        <tax>
                          <xsl:attribute name="detail">
                            <xsl:value-of select="$vTaxDetail"/>
                          </xsl:attribute>
                          <xsl:attribute name="rate">
                            <xsl:value-of select="number($vTaxRate) * 100"/>
                          </xsl:attribute>
                          <xsl:attribute name="amount">
                            <xsl:value-of select="$vTaxAmount"/>
                          </xsl:attribute>
                        </tax>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:for-each>
                </payment>
              </xsl:if>
            </xsl:when>
            <xsl:when test="($vIsAllPtyPaymentPayed or $vIsAllPtyPaymentReceived)=false()">
              <payment paymentType="{$vPaymentType}" currency="{$vFirstCuPayments}" status="{$gcNOkStatus}">
                <xsl:if test="$gcIsBusinessDebugMode">
                  <xsl:attribute name="anomaly">
                    <xsl:value-of select="'Payed/Received'"/>
                  </xsl:attribute>
                </xsl:if>
              </payment>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$vIsAllPtyPaymentWithSameCu=false()">
          <payment paymentType="{$vPaymentType}" status="{$gcNOkStatus}">
            <xsl:if test="$gcIsBusinessDebugMode">
              <xsl:attribute name="anomaly">
                <xsl:value-of select="'Multi currencies'"/>
              </xsl:attribute>
            </xsl:if>
          </payment>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <xsl:template match="paymentType" mode="create-fromEvent">
    <xsl:param name="pIDB" />
    <xsl:param name="pOPPEvents" />
    <xsl:param name="pIsReversal" select="false()"/>

    <xsl:variable name="vPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@id=current()/@id]/@name"/>
    <xsl:variable name="vPaymentTypeOPPEvents" select="$pOPPEvents[(string(EventFee/PAYMENTTYPE/text()) = string($vPaymentType))
                  and ((number(IDB_PAY/text()) = number($pIDB)) or (number(IDB_REC/text()) = number($pIDB)))]"/>
    <xsl:variable name="vPaymentTypeOPPEventsCount" select="count($vPaymentTypeOPPEvents)"/>
    <!--//-->
    <xsl:if test="number($vPaymentTypeOPPEventsCount) > 0">
      <xsl:variable name="vPaymentTypeOPPEventsCu" select="$vPaymentTypeOPPEvents[string-length(UNIT/text()) > 0]/UNIT"/>
      <xsl:variable name="vPaymentTypeOPPEventsFirstCu" select="$vPaymentTypeOPPEventsCu[position()=1]"/>
      <xsl:variable name="vIsPaymentTypeOPPEventsWithSameCu" select="$vPaymentTypeOPPEventsCount = count($vPaymentTypeOPPEvents[(UNIT/text()) = $vPaymentTypeOPPEventsFirstCu])"/>
      <!--//-->
      <xsl:choose>
        <xsl:when test="$vIsPaymentTypeOPPEventsWithSameCu">
          <xsl:variable name="vPaymentTypePayer" select="$vPaymentTypeOPPEvents/IDB_PAY/text()"/>
          <xsl:variable name="vPaymentTypeReceiver" select="$vPaymentTypeOPPEvents/IDB_REC/text()"/>
          <!--//-->
          <xsl:variable name="vIsAllPaymentTypeOPPEventsPayed" select="$vPaymentTypeOPPEventsCount = count($vPaymentTypeOPPEvents[(number(IDB_PAY/text()) = number($pIDB)) and ((string-length($vPaymentTypeReceiver) = 0) or (number(IDB_REC/text()) = number($vPaymentTypeReceiver)))])"/>
          <xsl:variable name="vIsAllPaymentTypeOPPEventsReceived" select="$vPaymentTypeOPPEventsCount = count($vPaymentTypeOPPEvents[(number(IDB_REC/text()) = number($pIDB)) and ((string-length($vPaymentTypePayer) = 0) or (number(IDB_PAY/text()) = number($vPaymentTypePayer)))])"/>
          <!--//-->
          <xsl:choose>
            <xsl:when test="$vIsAllPaymentTypeOPPEventsPayed or $vIsAllPaymentTypeOPPEventsReceived">
              <xsl:variable name="vPayRecEvent">
                <xsl:choose>
                  <xsl:when test="$vIsAllPaymentTypeOPPEventsPayed">
                    <xsl:value-of select="$gcPay"/>
                  </xsl:when>
                  <xsl:when test="$vIsAllPaymentTypeOPPEventsReceived">
                    <xsl:value-of select="$gcRec"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vPayRec">
                <xsl:choose>
                  <xsl:when test="$pIsReversal = false()">
                    <xsl:value-of select="$vPayRecEvent"/>
                  </xsl:when>
                  <xsl:when test="$pIsReversal = true()">
                    <xsl:choose>
                      <xsl:when test="$vPayRecEvent = $gcPay">
                        <xsl:value-of select="$gcRec"/>
                      </xsl:when>
                      <xsl:when test="$vPayRecEvent = $gcRec">
                        <xsl:value-of select="$gcPay"/>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vAmountET">
                <xsl:choose>
                  <xsl:when test="$vPaymentTypeOPPEventsCount = 1">
                    <xsl:value-of select="$vPaymentTypeOPPEvents/VALORISATION"/>
                  </xsl:when>
                  <xsl:when test="$vPaymentTypeOPPEventsCount > 1">
                    <xsl:value-of select="sum($vPaymentTypeOPPEvents/VALORISATION)"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>

              <xsl:if test="string-length($vAmountET)>0">
                <payment paymentType="{$vPaymentType}" pr="{$vPayRec}" currency="{$vPaymentTypeOPPEventsFirstCu}" amountET="{$vAmountET}">

                  <xsl:variable name="vAllTaxEventCopy">
                    <xsl:copy-of select="$vPaymentTypeOPPEvents/Event[EVENTCODE='OPP' and EVENTTYPE='TAX']"/>
                  </xsl:variable>
                  <xsl:variable name="vAllTaxDetail" select="msxsl:node-set($vAllTaxEventCopy)/Event/EventFee/TAXDETAIL[generate-id()=generate-id(key('kEventFeeTaxDetail',text()))]"/>

                  <xsl:for-each select="$vAllTaxDetail">
                    <xsl:sort select="text()" data-type="text"/>

                    <xsl:variable name="vTaxDetail" select="text()"/>

                    <xsl:variable name="vTaxDetailEventFeeCopy">
                      <xsl:copy-of select="msxsl:node-set($vAllTaxEventCopy)/Event/EventFee[TAXDETAIL/text()=$vTaxDetail]"/>
                    </xsl:variable>

                    <xsl:variable name="vAllTaxRate" select="msxsl:node-set($vTaxDetailEventFeeCopy)/EventFee/TAXRATE[generate-id()=generate-id(key('kEventFeTaxRate',text()))]"/>

                    <xsl:for-each select="$vAllTaxRate">
                      <xsl:sort select="text()" data-type="text"/>

                      <xsl:variable name="vTaxRate" select="text()"/>

                      <xsl:variable name="vTaxRateEvent" select="msxsl:node-set($vAllTaxEventCopy)/Event[EventFee/TAXDETAIL/text()=$vTaxDetail and EventFee/TAXRATE/text()=$vTaxRate]"/>

                      <xsl:variable name="vTaxAmount">
                        <xsl:if test="$vTaxRateEvent/VALORISATION">
                          <xsl:value-of select="sum($vTaxRateEvent/VALORISATION)"/>
                        </xsl:if>
                      </xsl:variable>

                      <xsl:if test="string-length($vTaxAmount) > 0">
                        <tax>
                          <xsl:attribute name="detail">
                            <xsl:value-of select="$vTaxDetail"/>
                          </xsl:attribute>
                          <xsl:attribute name="rate">
                            <xsl:value-of select="number($vTaxRate) * 100"/>
                          </xsl:attribute>
                          <xsl:attribute name="amount">
                            <xsl:value-of select="$vTaxAmount"/>
                          </xsl:attribute>
                        </tax>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:for-each>
                </payment>
              </xsl:if>
            </xsl:when>
            <xsl:when test="($vIsAllPaymentTypeOPPEventsPayed or $vIsAllPaymentTypeOPPEventsReceived)=false()">
              <payment paymentType="{$vPaymentType}" currency="{$vPaymentTypeOPPEventsFirstCu}" status="{$gcNOkStatus}">
                <xsl:if test="$gcIsBusinessDebugMode">
                  <xsl:attribute name="anomaly">
                    <xsl:value-of select="'Payed/Received'"/>
                  </xsl:attribute>
                </xsl:if>
              </payment>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$vIsPaymentTypeOPPEventsWithSameCu=false()">
          <payment paymentType="{$vPaymentType}" status="{$gcNOkStatus}">
            <xsl:if test="$gcIsBusinessDebugMode">
              <xsl:attribute name="anomaly">
                <xsl:value-of select="'Multi currencies'"/>
              </xsl:attribute>
            </xsl:if>
          </payment>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <xsl:template match="paymentType" mode="groupe-currency">
    <xsl:param name="pPayments" />

    <xsl:variable name="vPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@id=current()/@id]"/>
    <xsl:variable name="vPaymentTypeName" select="$vPaymentType/@name"/>
    <xsl:variable name="vPaymentTypeCuLabel" select="$vPaymentType/@cuLabel"/>

    <xsl:variable name="vPaymentCu" select="$pPayments[(@paymentType=$vPaymentTypeName)]/@currency"/>
    <currency name="{$vPaymentTypeCuLabel}" value="{$vPaymentCu}"/>
  </xsl:template>
  <xsl:template match="paymentType" mode="serie-status">
    <xsl:param name="pPayments" />
    <xsl:param name="pGroupePaymentCu" />

    <xsl:variable name="vPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@id=current()/@id]"/>
    <xsl:variable name="vPaymentTypeName" select="$vPaymentType/@name"/>
    <xsl:variable name="vPaymentTypeCuLabel" select="$vPaymentType/@cuLabel"/>
    <xsl:variable name="vPaymentTypeCu" select="$pGroupePaymentCu[@name=$vPaymentTypeCuLabel]/@value"/>

    <xsl:if test="count($pPayments[@paymentType=$vPaymentTypeName]) > 0">
      <xsl:choose>
        <xsl:when test="$pPayments[@paymentType=$vPaymentTypeName and @status=$gcNOkStatus]">
          <xsl:copy-of select="$pPayments[@paymentType=$vPaymentTypeName]"/>
        </xsl:when>
        <xsl:when test="$pPayments[@paymentType=$vPaymentTypeName and @currency=$vPaymentTypeCu]">
          <!--<xsl:copy-of select="fees/payment[@paymentType=$gcBro]"/>-->
        </xsl:when>
        <xsl:otherwise>
          <payment paymentType="{$vPaymentTypeName}" status="{$gcNOkStatus}">
            <xsl:if test="$gcIsBusinessDebugMode">
              <xsl:attribute name="anomaly">
                <xsl:value-of select="concat('Currency (',$pPayments[@paymentType=$vPaymentTypeName]/@currency,'), Group currency (',$vPaymentTypeCu,')')"/>
              </xsl:attribute>
            </xsl:if>
          </payment>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

  </xsl:template>
  <xsl:template match="paymentType" mode="serie-subtotal">
    <xsl:param name="pPayments" />
    <xsl:param name="pGroupePaymentCu" />

    <xsl:variable name="vPaymentType" select="msxsl:node-set($gcFeeDisplaySettings)/paymentType[@id=current()/@id]"/>
    <xsl:variable name="vPaymentTypeName" select="$vPaymentType/@name"/>
    <xsl:variable name="vPaymentTypeCuLabel" select="$vPaymentType/@cuLabel"/>
    <xsl:variable name="vPaymentTypeCu" select="$pGroupePaymentCu[@name=$vPaymentTypeCuLabel]/@value"/>

    <xsl:variable name="vAllCurrentPayment" select="$pPayments[@paymentType=$vPaymentTypeName]"/>
    <xsl:variable name="vAllCurrentPaymentCount" select="count($vAllCurrentPayment)"/>
    <xsl:variable name="vCurrentPaymentPayRec" select="$vAllCurrentPayment/@pr"/>
    <!--//-->
    <xsl:if test="$vAllCurrentPaymentCount>0">
      <xsl:choose>
        <xsl:when test="$vAllCurrentPaymentCount = count($vAllCurrentPayment[@currency=$vPaymentTypeCu and @pr=$vCurrentPaymentPayRec])" >
          <payment paymentType="{$vPaymentTypeName}" pr="{$vCurrentPaymentPayRec}" currency="{$vPaymentTypeCu}" amountET="{sum($vAllCurrentPayment/@amountET)}"/>
        </xsl:when>
        <xsl:otherwise>
          <payment paymentType="{$vPaymentTypeName}" pr="{$gcNOkStatus}" currency="{$vPaymentTypeCu}"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <!-- Get and format to the xsl:fo standard  the trade price value searching the source XML of the confirmation message -->
  <xsl:template name="GetTradeConvertedPriceValue">
    <!-- the asset id value in the format used for the ETD repository -->
    <xsl:param name="pIdAsset"/>
    <!-- the trade Identifier value -->
    <xsl:param name="pTradeId"/>
    <xsl:variable name="convertedPrice">
      <xsl:copy-of select="$gDataDocumentRepository/etd[@OTCmlId=$pIdAsset]/convertedPrices/convertedTradePrices/convertedTradePrice[@href=$pTradeId]/convertedPrice"/>
    </xsl:variable>
    <xsl:call-template name="FormatConvertedPriceValue">
      <xsl:with-param name="pContent" select="$convertedPrice"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Get and format to the xsl:fo standard the strike price value searching the source XML of the confirmation message -->
  <xsl:template name="GetStrikeConvertedPriceValue">
    <!-- the asset id value in in the format used for the etd repository -->
    <xsl:param name="pIdAsset"/>
    <xsl:variable name="convertedPrice">
      <xsl:copy-of select="$gDataDocumentRepository/etd[@OTCmlId=$pIdAsset]/convertedPrices/convertedStrikePrice"/>
    </xsl:variable>
    <xsl:call-template name="FormatConvertedPriceValue">
      <xsl:with-param name="pContent" select="$convertedPrice"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Get and format to the xsl:fo standard the average transaction price value searching the source XML of the confirmation message -->
  <xsl:template name="GetConvertedAveragePriceValue">
    <!-- the asset id value in in the format used for the etd repository -->
    <xsl:param name="pIdAsset"/>
    <!-- the current side we want the average price, possible values: '1' to get the long average prices, '2' to get the short average prices -->
    <xsl:param name="pSide"/>
    <xsl:variable name="convertedPrice">
      <xsl:choose>
        <xsl:when test="$pSide = '1'">
          <xsl:copy-of select="$gDataDocumentRepository/etd[@OTCmlId=$pIdAsset]/convertedPrices/convertedLongAveragePrice"/>
        </xsl:when>
        <xsl:when test="$pSide = '2'">
          <xsl:copy-of select="$gDataDocumentRepository/etd[@OTCmlId=$pIdAsset]/convertedPrices/convertedShortAveragePrice"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="FormatConvertedPriceValue">
      <xsl:with-param name="pContent" select="$convertedPrice"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Get and format to the xsl:fo standard the event quotation price -->
  <!--
  <xsl:template name="GetEventConvertedPriceValue">
    -->
  <!-- the trade Identifier value. 
    this parameter is redondant because we pass the target event in the pActionEvent parameter, then the trade is already identified-->
  <!--
    <xsl:param name="pTradeId"/>
    -->
  <!-- the event node where we want to search the value -->
  <!--
    <xsl:param name="pActionEvent"/>
    <xsl:variable name="convertedPrice">
      <xsl:copy-of select="msxsl:node-set($pActionEvent)/Event/EventDetails_ETD/convertedPrices/convertedTradePrices/convertedTradePrice[@href=$pTradeId]/convertedPrice"/>
    </xsl:variable>
    <xsl:call-template name="FormatConvertedPriceValue">
      <xsl:with-param name="pContent" select="$convertedPrice"/>
    </xsl:call-template>
  </xsl:template>-->

  <!-- 20120822 MF ticket 18073 -->
  <!-- Passing a converted price value inside of pContent, and format this one if needed to the xsl:fo standard -->
  <xsl:template name="FormatConvertedPriceValue">
    <!-- converted price node retrieven in the source XML message -->
    <xsl:param name="pContent"/>
    <xsl:choose>
      <xsl:when test="msxsl:node-set($pContent)/convertedPrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedPrice/text()"/>
        </xsl:call-template>
        <!-- when the node sup exists then we have a fractional expression to display -->
        <xsl:if test="msxsl:node-set($pContent)/convertedPrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedPrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedPrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pContent)/convertedStrikePrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedStrikePrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedStrikePrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedStrikePrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedStrikePrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pContent)/convertedLongAveragePrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedLongAveragePrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedLongAveragePrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedLongAveragePrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedLongAveragePrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pContent)/convertedShortAveragePrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedShortAveragePrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedShortAveragePrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedShortAveragePrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedShortAveragePrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pContent)/convertedSynthPositionPrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedSynthPositionPrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedSynthPositionPrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedSynthPositionPrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedSynthPositionPrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <!-- RD 20140725 [20212] Use element convertedClearingPrice instead of convertedSynthPositionClearingPrice-->
      <xsl:when test="msxsl:node-set($pContent)/convertedClearingPrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedClearingPrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedClearingPrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedClearingPrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedClearingPrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <!-- RD 20140725 [20212] Add new element convertedSettltPrice-->
      <xsl:when test="msxsl:node-set($pContent)/convertedSettltPrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedSettltPrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedSettltPrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedSettltPrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedSettltPrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <!-- RD 20140725 [20212] Add new element convertedClosingPrice-->
      <xsl:when test="msxsl:node-set($pContent)/convertedClosingPrice">
        <xsl:call-template name="CheckDefaultFormatNumberV2">
          <xsl:with-param name="pValue" select="msxsl:node-set($pContent)/convertedClosingPrice/text()"/>
        </xsl:call-template>
        <xsl:if test="msxsl:node-set($pContent)/convertedClosingPrice/sup">
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedClosingPrice/sup/text()"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="msxsl:node-set($pContent)/convertedClosingPrice/sub/text()"/>
          </fo:inline>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="format-number">
          <xsl:with-param name="pAmount" select="msxsl:node-set($pContent)/text()" />
          <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="CheckDefaultFormatNumberV2">
    <xsl:param name="pValue"/>

    <xsl:choose>
      <!-- Donnée comportant un des 3 séparateurs suivants: "'", "-", ou "^"; on l'affiche en l'état car déjà formaté -->
      <xsl:when test="contains($pValue, '&quot;') or contains($pValue, '-') or contains($pValue, '^')">
        <xsl:value-of select="$pValue"/>
      </xsl:when>
      <!-- Donnée comportant le séparateur international ".", on le transforme en celui relatif à la culture -->
      <xsl:otherwise>
        <xsl:call-template name="format-number">
          <xsl:with-param name="pAmount" select="$pValue" />
          <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>