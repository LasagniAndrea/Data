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
  <xsl:variable name="vFxBullionCcyList" select="',XAU,XAG,XPT,XPD,'"/>

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->

  <!-- ................................................ -->
  <!-- BizFxNDF_Trade                                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for FxNDF Trades Confirmations and Open Positions sections
       ................................................ -->
  <xsl:template match="trades|posTrades" mode="BizFxNDF_Trade">
    <xsl:call-template name="BizFxLeg_Trade">
      <xsl:with-param name="pFamily" select="'FxNDF'"/>
      <xsl:with-param name="pEfsmlTradeList" select="current()/trade"/>
      <xsl:with-param name="pFxLegTradeList" select="$gTrade[fxSingleLeg/nonDeliverableForward]"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizFxForward_Trade                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for FxForward Trades Confirmations and Open Positions sections
       ................................................ -->
  <xsl:template match="trades|posTrades" mode="BizFxForward_Trade">
    <xsl:call-template name="BizFxLeg_Trade">
      <xsl:with-param name="pFamily" select="'FxForward'"/>
      <xsl:with-param name="pEfsmlTradeList" select="current()/trade"/>
      <xsl:with-param name="pFxLegTradeList" select="$gTrade[
                      fxSingleLeg and 
                      fxSingleLeg/nonDeliverableForward=false() and 
                      contains($vFxBullionCcyList, concat(',',fxSingleLeg/exchangedCurrency1/paymentAmount/currency/text(),','))=false()]"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizFxBullion_Trade                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for FxBullion Trades Confirmations and Open Positions sections
       ................................................ -->
  <xsl:template match="trades|posTrades" mode="BizFxBullion_Trade">
    <xsl:call-template name="BizFxLeg_Trade">
      <xsl:with-param name="pFamily" select="'FxBullion'"/>
      <xsl:with-param name="pEfsmlTradeList" select="current()/trade"/>
      <xsl:with-param name="pFxLegTradeList" select="$gTrade[
                      contains($vFxBullionCcyList, concat(',',fxSingleLeg/exchangedCurrency1/paymentAmount/currency/text(),','))]"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- BizFxLeg_MarketCurrency-market                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for fxSingleLeg Trades Confirmations and Open Positions sections
       ................................................ -->
  <xsl:template name="BizFxLeg_Trade">
    <xsl:param name="pFamily"/>
    <xsl:param name="pEfsmlTradeList"/>
    <xsl:param name="pFxLegTradeList"/>

    <xsl:variable name="vFxLegTradeList" select="$pFxLegTradeList[@tradeId=$pEfsmlTradeList/@tradeId]"/>
    <xsl:variable name="vEfsmlTradeList" select="$pEfsmlTradeList[@tradeId=$vFxLegTradeList/@tradeId]"/>

    <xsl:variable name="vRepository-market" select="$gRepository/market[identifier='FOREX']"/>

    <xsl:if test="$vFxLegTradeList">

      <date bizDt="{current()/@bizDt}">

      <xsl:variable name="vCurrency_ListNode">
        <xsl:for-each select="$vFxLegTradeList/fxSingleLeg/exchangedCurrency2/paymentAmount/currency">
          <currency ccy="{current()/text()}"/>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="vCurrency_List"
                    select="msxsl:node-set($vCurrency_ListNode)/currency[generate-id()=generate-id(key('kCcy',@ccy))]"/>

      <xsl:for-each select="$vCurrency_List">

        <xsl:variable name="vCurrency" select="current()/@ccy"/>
        <xsl:variable name="vCurrencyFxLegTradeList"
                      select="$vFxLegTradeList[fxSingleLeg/exchangedCurrency2/paymentAmount/currency/text()=$vCurrency]"/>

        <xsl:if test="$vCurrencyFxLegTradeList">

          <xsl:variable name="vCurrencyEfsmlTradeList" select="$vEfsmlTradeList[@tradeId=$vCurrencyFxLegTradeList/@tradeId]"/>
          <xsl:variable name="vCcyPattern">
            <xsl:call-template name="GetCcyPattern">
              <xsl:with-param name="pCcy" select="$vCurrency"/>
            </xsl:call-template>
          </xsl:variable>

          <market OTCmlId="{$vRepository-market/@OTCmlId}">
            <currency ccy="{$vCurrency}">
              <pattern ccy="{$vCcyPattern}"/>
            </currency>

            <xsl:variable name="vCurrencyFxLegTradeListCopyNode">
              <xsl:copy-of select="$vCurrencyFxLegTradeList"/>
            </xsl:variable>
            <xsl:variable name="vCurrencyFxLegTradeListCopy"
                          select="msxsl:node-set($vCurrencyFxLegTradeListCopyNode)/trade"/>
            <xsl:variable name="vRptSide-Acct_List"
                          select="$vCurrencyFxLegTradeListCopy/fxSingleLeg/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

            <xsl:for-each select="$vRptSide-Acct_List">

              <xsl:variable name="vRptSide-Acct" select="current()"/>
              <xsl:variable name="vBookFxLegTradeList" select="$vCurrencyFxLegTradeList[fxSingleLeg/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

              <xsl:if test="$vBookFxLegTradeList">

                <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}">

                  <xsl:variable name="vBookEfsmlTradeList" select="$vCurrencyEfsmlTradeList[@tradeId=$vBookFxLegTradeList/@tradeId]"/>

                  <xsl:variable name="vBookFxLegTradeListCopyNode">
                    <xsl:copy-of select="$vBookFxLegTradeList"/>
                  </xsl:variable>
                  <xsl:variable name="vBookFxLegTradeListCopy" select="msxsl:node-set($vBookFxLegTradeListCopyNode)/trade"/>
                  <xsl:variable name="vInstr-OTCmlId_List"
                                select="$vBookFxLegTradeListCopy/fxSingleLeg/productType[generate-id()=generate-id(key('kOTCmlId',@OTCmlId))]"/>

                  <xsl:for-each select="$vInstr-OTCmlId_List">

                    <xsl:variable name="vInstr-OTCmlId" select="current()"/>
                    <xsl:variable name="vInstrFxLegTradeList" select="$vBookFxLegTradeList[fxSingleLeg/productType/@OTCmlId=$vInstr-OTCmlId/@OTCmlId]"/>

                    <xsl:if test="$vInstrFxLegTradeList">

                      <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrFxLegTradeList/@tradeId]"/>
                      <xsl:variable name="vDealtCurrency_ListNode">
                        <xsl:for-each select="$vInstrFxLegTradeList/fxSingleLeg/exchangedCurrency1/paymentAmount/currency">
                          <currency ccy="{current()/text()}"/>
                        </xsl:for-each>
                      </xsl:variable>
                      <xsl:variable name="vDealtCurrency_List"
                                    select="msxsl:node-set($vDealtCurrency_ListNode)/currency[generate-id()=generate-id(key('kCcy',@ccy))]"/>

                      <xsl:for-each select="$vDealtCurrency_List">

                        <xsl:variable name="vDealtCurrency" select="current()/@ccy"/>
                        <xsl:variable name="vDealtCurrencyFxLegTradeList"
                                      select="$vFxLegTradeList[fxSingleLeg/exchangedCurrency1/paymentAmount/currency/text()=$vDealtCurrency]"/>

                        <xsl:if test="$vDealtCurrencyFxLegTradeList">

                          <xsl:variable name="vDealtCurrencyEfsmlTradeList"
                                        select="$vInstrEfsmlTradeList[@tradeId=$vDealtCurrencyFxLegTradeList/@tradeId]"/>
                          <xsl:variable name="vDealtCurrencyCommonTradeList"
                                        select="$gCommonData/trade[@tradeId=$vDealtCurrencyFxLegTradeList/@tradeId]"/>

                          <dealt ccy="{$vDealtCurrency}"
                                 idI="{$vInstr-OTCmlId/@OTCmlId}"
                                 family="{$pFamily}">

                            <xsl:call-template name="Business_GetPattern">
                              <xsl:with-param name="pCcy" select="$vDealtCurrency"/>
                              <xsl:with-param name="pFmtLastPx" select="$vDealtCurrencyCommonTradeList/@fmtLastPx"/>
                              <xsl:with-param name="pFmtClrPx" select="$vDealtCurrencyEfsmlTradeList/@fmtClrPx"/>
                            </xsl:call-template>

                            <xsl:for-each select="$vDealtCurrencyEfsmlTradeList">

                              <xsl:variable name="vEfsmlTrade" select="current()"/>
                              <xsl:variable name="vFxLegTrade" select="$vDealtCurrencyFxLegTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>
                              <xsl:variable name="vCommonTrade" select="$vDealtCurrencyCommonTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>

                              <trade trdNum="{$vEfsmlTrade/@tradeId}" tradeId="{$vEfsmlTrade/@tradeId}"
                                     valDt="{$vCommonTrade/@valDt}"
                                     lastPx="{$vCommonTrade/@lastPx}">
                                
                                <!--RD 20200818 [25456] Faire le cumul des frais par trade -->
                                <xsl:call-template name="Business_GetFee">
                                  <xsl:with-param name="pFee" select="$vEfsmlTrade/fee"/>
                                </xsl:call-template>
                              
                              </trade>
                            </xsl:for-each>

                            <xsl:call-template name="Business_GetFeeSubtotal">
                              <xsl:with-param name="pFee" select="$vDealtCurrencyEfsmlTradeList/fee"/>
                            </xsl:call-template>
                          </dealt>
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

</xsl:stylesheet>
