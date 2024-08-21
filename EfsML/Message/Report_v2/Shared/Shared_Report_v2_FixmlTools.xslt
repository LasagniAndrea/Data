<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - Shared - Common variables for all reports                           -->
  <!-- File    : Shared_Report_v2_PDF.xslt                                                            -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140905                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Settings                                          -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Templates                                         -->
  <!-- ============================================================================================== -->

  <!-- .......................................................................... -->
  <!--              Display data                                                  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- DisplayData_Fixml                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in Fixml Trade          
       ................................................ -->
  <!-- FI 20150825 [21287] Modify (Utilisation de pDataEfsml pour 'BuyQty','SellQty','Qty' à l'image de ce qui existait déjà pour 'PosBuyQty','PosSellQty','PosQty') -->
  <!-- FI 20150825 [21287] Modify (performance : Utilisation *[1] pour pointer l'élément FIXML) -->
  <!-- FI 20151019 [21317] Modify -->
  <!-- RD 20190619 [23912] Modify -->
  <xsl:template name="DisplayData_Fixml">
    <!-- Represente la donnée demandée-->
    <xsl:param name="pDataName"/>
    <!-- Represente un trade qui contient un noeud FIXML-->
    <xsl:param name="pFixmlTrade"/>
    <!-- Represente un noeud trades/trade ou posActions/posAction ou posTrades/trade-->
    <xsl:param name="pDataEfsml"/>
    <!-- Represente le fommat numérique demandé -->
    <xsl:param name="pNumberPattern" select="$gDefaultPricePattern"/>
    <!-- Si renseigné tronque(*) le résultat.  -->
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name ="vRptSide" select ="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/RptSide"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <!-- FI 20151019 [21317] Call RptSideGetValue -->
        <xsl:when test="$pDataName='Side' or $pDataName='PosSide' or $pDataName='Acct' " >
          <xsl:call-template name="RptSideGetValue">
            <xsl:with-param name ="pDataName" select ="$pDataName"/>
            <xsl:with-param name ="pRptSide" select ="$vRptSide"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="contains(',TrdTyp,TrdTypPos,',concat(',',$pDataName,','))" >
          <xsl:choose>
            <!--4: LateTrade-->
            <xsl:when test="$pDataName='TrdTyp' and $pFixmlTrade/*/FIXML/TrdCaptRpt/@TrdTyp = '4'">
              <xsl:value-of select="'Late'" />
            </xsl:when>
            <!--42: PortFolioTransfer and 1: InternalTransferOrAdjustment-->
            <xsl:when test="$pDataName='TrdTyp' and 
                      $pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdTyp = '42' and  
                      $pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdSubTyp = '1'">
              <xsl:value-of select="'Trf.'" />
            </xsl:when>
            <!--45: OptionExercice-->
            <xsl:when test="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdTyp = '45'">
              <xsl:choose>
                <!--9: TransactionFromExercise-->
                <xsl:when test="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdSubTyp = '9'">
                  <xsl:value-of select="'Exe'" />
                </xsl:when>
                <!--10: TransactionFromAssignment-->
                <xsl:when test="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdSubTyp = '10'">
                  <xsl:value-of select="'Ass'" />
                </xsl:when>
              </xsl:choose>
            </xsl:when>
            <!--1001: Cascading-->
            <xsl:when test="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdTyp = '1001'">
              <xsl:value-of select="'Casc'" />
            </xsl:when>
            <!--1002: Shifting-->
            <xsl:when test="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdTyp = '1002'">
              <xsl:value-of select="'Shft'" />
            </xsl:when>
            <!--1003: CorporateAction-->
            <xsl:when test="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@TrdTyp = '1003'">
              <xsl:value-of select="'CA'" />
            </xsl:when>
          </xsl:choose>
        </xsl:when>

        <xsl:when test="$pDataName='BuyQty'" >
          <xsl:if test="$vRptSide/@Side = '1'">
            <xsl:call-template name="DisplayData_Fixml">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pFixmlTrade" select="$pFixmlTrade"/>
              <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='SellQty'" >
          <xsl:if test="$vRptSide/@Side = '2'">
            <xsl:call-template name="DisplayData_Fixml">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pFixmlTrade" select="$pFixmlTrade"/>
              <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='Qty'" >
          <xsl:choose>
            <xsl:when test="$pDataEfsml">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'FmtQty'"/>
                <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
                <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="format-integer">
                <xsl:with-param name="integer" select="$pFixmlTrade/*[1]/FIXML/TrdCaptRpt/@LastQty"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <xsl:when test="$pDataName='PosBuyQty'" >
          <xsl:if test="$vRptSide/@Side = '1'">
            <xsl:call-template name="DisplayData_Fixml">
              <xsl:with-param name="pDataName" select="'PosQty'"/>
              <xsl:with-param name="pFixmlTrade" select="$pFixmlTrade"/>
              <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='PosSellQty'" >
          <xsl:if test="$vRptSide/@Side = '2'">
            <xsl:call-template name="DisplayData_Fixml">
              <xsl:with-param name="pDataName" select="'PosQty'"/>
              <xsl:with-param name="pFixmlTrade" select="$pFixmlTrade"/>
              <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='PosQty'" >
          <xsl:choose>
            <xsl:when test="$pDataEfsml">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'FmtQty'"/>
                <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
                <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="DisplayData_Fixml">
                <xsl:with-param name="pDataName" select="'Qty'"/>
                <xsl:with-param name="pFixmlTrade" select="$pFixmlTrade"/>
                <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <!-- RD 20190619 [23912] Add Order data -->
        <xsl:when test="$pDataName='OrdCat'" >
          <xsl:choose>
            <!--1: Order-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '1'">
              <xsl:value-of select="'Order'" />
            </xsl:when>
            <!--2": Quote-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '2'">
              <xsl:value-of select="'Quote'" />
            </xsl:when>
            <!--3: Privately Negotiated Trade-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '3'">
              <xsl:value-of select="'PrivatelyNegotiatedTrade'" />
            </xsl:when>
            <!--4: Multileg order-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '4'">
              <xsl:value-of select="'MultilegOrder'" />
            </xsl:when>
            <!--5: Linked order-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '5'">
              <xsl:value-of select="'LinkedOrder'" />
            </xsl:when>
            <!--6: Quote Request-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '6'">
              <xsl:value-of select="'QuoteRequest'" />
            </xsl:when>
            <!--7: Implied Order-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '7'">
              <xsl:value-of select="'ImpliedOrder'" />
            </xsl:when>
            <!--8: Cross Order-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '8'">
              <xsl:value-of select="'CrossOrder'" />
            </xsl:when>
            <!--9: Streaming price (quote)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat = '9'">
              <xsl:value-of select="'StreamingPrice'" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdCat" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='OrdStat'" >
          <xsl:choose>
            <!--0: New-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '0'">
              <xsl:value-of select="'New'" />
            </xsl:when>
            <!--1: Partially filled-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '1'">
              <xsl:value-of select="'Partial'" />
            </xsl:when>
            <!--2: Filled-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '2'">
              <xsl:value-of select="'Filled'" />
            </xsl:when>
            <!--3: Done for day-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '3'">
              <xsl:value-of select="'Done'" />
            </xsl:when>
            <!--4: Canceled-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '4'">
              <xsl:value-of select="'Canceled'" />
            </xsl:when>
            <!--6: Pending Cancel (i.e. result of Order Cancel Request)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '6'">
              <xsl:value-of select="'PendingCR'" />
            </xsl:when>
            <!--7: Stopped-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '7'">
              <xsl:value-of select="'Stopped'" />
            </xsl:when>
            <!--8: Rejected-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '8'">
              <xsl:value-of select="'Rejected'" />
            </xsl:when>
            <!--9: Suspended-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '9'">
              <xsl:value-of select="'Suspended'" />
            </xsl:when>
            <!--A: Pending New-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = 'A'">
              <xsl:value-of select="'PendingNew'" />
            </xsl:when>
            <!--B: Calculated-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = 'B'">
              <xsl:value-of select="'Calculated'" />
            </xsl:when>
            <!--C: Expired-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = 'C'">
              <xsl:value-of select="'Expired'" />
            </xsl:when>
            <!--D: Accepted for Bidding-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = 'D'">
              <xsl:value-of select="'AcceptBidding'" />
            </xsl:when>
            <!--E: Pending Replace (i.e. result of Order Cancel/Replace Request)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = 'E'">
              <xsl:value-of select="'PendingRep'" />
            </xsl:when>
            <!--5: Replaced (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat = '5'">
              <xsl:value-of select="'Replaced'" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pFixmlTrade/*/FIXML/TrdCaptRpt/@OrdStat" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='OrdTyp'" >
          <xsl:choose>
            <!--1: Market-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '1'">
              <xsl:value-of select="'Market'" />
            </xsl:when>
            <!--2: Limit-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '2'">
              <xsl:value-of select="'Limit'" />
            </xsl:when>
            <!--3: Stop / Stop Loss-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '3'">
              <xsl:value-of select="'Stop'" />
            </xsl:when>
            <!--4: Stop Limit-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '4'">
              <xsl:value-of select="'StopLimit'" />
            </xsl:when>
            <!--5: Market On Close (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '5'">
              <xsl:value-of select="'MarketOnClose'" />
            </xsl:when>
            <!--6: With Or Without-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '6'">
              <xsl:value-of select="'WithOrWithout'" />
            </xsl:when>
            <!--7: Limit Or Better-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '7'">
              <xsl:value-of select="'LimitOrBetter'" />
            </xsl:when>
            <!--8: Limit With Or Without-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '8'">
              <xsl:value-of select="'LimitWithOrWithout'" />
            </xsl:when>
            <!--9: On Basis-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = '9'">
              <xsl:value-of select="'OnBasis'" />
            </xsl:when>
            <!--A: On Close (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'A'">
              <xsl:value-of select="'OnClose'" />
            </xsl:when>
            <!--B: Limit On Close (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'B'">
              <xsl:value-of select="'LimitOnClose'" />
            </xsl:when>
            <!--C: Forex Market (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'C'">
              <xsl:value-of select="'ForexMarket'" />
            </xsl:when>
            <!--D: Previously Quoted-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'D'">
              <xsl:value-of select="'PreviouslyQuoted'" />
            </xsl:when>
            <!--E: Previously Indicated-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'E'">
              <xsl:value-of select="'PreviouslyIndicated'" />
            </xsl:when>
            <!--F: Forex Limit (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'F'">
              <xsl:value-of select="'ForexLimit'" />
            </xsl:when>
            <!--G: Forex Swap-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'G'">
              <xsl:value-of select="'ForexSwap'" />
            </xsl:when>
            <!--H: Forex Previously Quoted (No longer used)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'H'">
              <xsl:value-of select="'ForexPreviouslyQuoted'" />
            </xsl:when>
            <!--I: Funari (Limit day order with unexecuted portion handles as Market On Close. E.g. Japan)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'I'">
              <xsl:value-of select="'Funari'" />
            </xsl:when>
            <!--J: Market If Touched (MIT)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'J'">
              <xsl:value-of select="'MIT'" />
            </xsl:when>
            <!--K: Market With Left Over as Limit (market order with unexecuted quantity becoming limit order at last price)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'K'">
              <xsl:value-of select="'MarketWithLeftOverLimit'" />
            </xsl:when>
            <!--L: Previous Fund Valuation Point (Historic pricing;  for CIV)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'L'">
              <xsl:value-of select="'PreviousFundValuationPoint'" />
            </xsl:when>
            <!--M: Next Fund Valuation Point (Forward pricing;  for CIV)-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'M'">
              <xsl:value-of select="'NextFundValuationPoint'" />
            </xsl:when>
            <!--P: Pegged-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'P'">
              <xsl:value-of select="'Pegged'" />
            </xsl:when>
            <!--Q: Counter-order selection-->
            <xsl:when test="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp = 'Q'">
              <xsl:value-of select="'CounterOrderSelection'" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pFixmlTrade/*/FIXML/TrdCaptRpt/RptSide/@OrdTyp" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
  </xsl:template>


  <!-- ................................................ -->
  <!-- GetTrdNum_Fixml                                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        
       Get Fixml trade number (ExexID or Spheres® trade Identifier)
       ................................................ -->
  <!-- FI 20150827 [21287] modify (deprecated) -->
  <!-- RD 20170426 [21113] Add Electronic Trade Id for COM -->
  <xsl:template name="GetTrdNum_Fixml">
    <xsl:param name="pTrade"/>

    <xsl:choose>
      <xsl:when test="($gSettings-headerFooter=false()) or $gSettings-headerFooter/tradeNumberIdent/text()='TradeId'">
        <!--Display "Spheres Trade Identifier"-->
        <xsl:value-of select="$pTrade/@tradeId"/>
      </xsl:when>
      <!--Display "FIX Execution Id" if exist, otherwise "Spheres Trade Identifier"-->
      <xsl:when test="string-length($pTrade/*[1]/FIXML/TrdCaptRpt/@ExecID) > 0">
        <xsl:value-of select="$pTrade/*[1]/FIXML/TrdCaptRpt/@ExecID"/>
      </xsl:when>
      <!--Display "ExecRefID" if exist, otherwise "Spheres Trade Identifier"-->
      <xsl:when test="string-length($pTrade/*[1]/RptSide/@ExecRefID) > 0">
        <xsl:value-of select="$pTrade/*[1]/RptSide/@ExecRefID"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pTrade/@tradeId"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>