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
  <!-- BizFxOptionLeg_Trade                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Create a "Business" XML for fxSimpleOption Trades Confirmations and Open Positions sections
       ................................................ -->
  <xsl:template match="trades|posTrades" mode="BizFxOptionLeg_Trade">

    <xsl:variable name="vFamily" select="'FxNDO'"/>
    <xsl:variable name="vAllEfsmlTradeList" select="current()/trade"/>
    <xsl:variable name="vAllFxOptionLegTradeList" select="$gTrade[fxSimpleOption/cashSettlementTerms]"/>

    <xsl:variable name="vFxOptionLegTradeList" select="$vAllFxOptionLegTradeList[@tradeId=$vAllEfsmlTradeList/@tradeId]"/>
    <xsl:variable name="vEfsmlTradeList" select="$vAllEfsmlTradeList[@tradeId=$vFxOptionLegTradeList/@tradeId]"/>

    <xsl:variable name="vRepository-market" select="$gRepository/market[identifier='FOREX']"/>

    <xsl:if test="$vFxOptionLegTradeList">

      <xsl:variable name="vBizDt">
        <xsl:choose>
          <xsl:when test="current()/@bizDt">
            <xsl:value-of select="current()/@bizDt"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gValueDate"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <date bizDt="{$vBizDt}">
        
      <xsl:variable name="vCurrency_ListNode">
        <xsl:for-each select="$vFxOptionLegTradeList/fxSimpleOption[optionType/text()='Call']/putCurrencyAmount/currency | 
                      $vFxOptionLegTradeList/fxSimpleOption[optionType/text()='Put']/callCurrencyAmount/currency">
          <currency ccy="{current()/text()}"/>
        </xsl:for-each>
      </xsl:variable>
      <xsl:variable name="vCurrency_List"
                    select="msxsl:node-set($vCurrency_ListNode)/currency[generate-id()=generate-id(key('kCcy',@ccy))]"/>

      <xsl:for-each select="$vCurrency_List">

        <xsl:variable name="vCurrency" select="current()/@ccy"/>
        <xsl:variable name="vCurrencyFxOptionLegTradeList"
                      select="$vFxOptionLegTradeList[fxSimpleOption[optionType/text()='Call']/putCurrencyAmount/currency/text()=$vCurrency] |
                      $vFxOptionLegTradeList[fxSimpleOption[optionType/text()='Put']/callCurrencyAmount/currency/text()=$vCurrency]"/>

        <xsl:if test="$vCurrencyFxOptionLegTradeList">

          <xsl:variable name="vCurrencyEfsmlTradeList" select="$vEfsmlTradeList[@tradeId=$vCurrencyFxOptionLegTradeList/@tradeId]"/>
          <xsl:variable name="vCcyPattern">
            <xsl:call-template name="GetCcyPattern">
              <xsl:with-param name="pCcy" select="$vCurrency"/>
            </xsl:call-template>
          </xsl:variable>

          <market OTCmlId="{$vRepository-market/@OTCmlId}">
            <currency ccy="{$vCurrency}">
              <pattern ccy="{$vCcyPattern}"/>
            </currency>

            <xsl:variable name="vCurrencyFxOptionLegTradeListCopyNode">
              <xsl:copy-of select="$vCurrencyFxOptionLegTradeList"/>
            </xsl:variable>
            <xsl:variable name="vCurrencyFxOptionLegTradeListCopy"
                          select="msxsl:node-set($vCurrencyFxOptionLegTradeListCopyNode)/trade"/>
            <xsl:variable name="vRptSide-Acct_List"
                          select="$vCurrencyFxOptionLegTradeListCopy/fxSimpleOption/RptSide[generate-id()=generate-id(key('kRptSide-Acct',@Acct))]"/>

            <xsl:for-each select="$vRptSide-Acct_List">

              <xsl:variable name="vRptSide-Acct" select="current()"/>
              <xsl:variable name="vBookFxOptionLegTradeList" select="$vCurrencyFxOptionLegTradeList[fxSimpleOption/RptSide/@Acct=$vRptSide-Acct/@Acct]"/>

              <xsl:if test="$vBookFxOptionLegTradeList">

                <book OTCmlId="{$gRepository/book[identifier=$vRptSide-Acct/@Acct]/@OTCmlId}">

                  <xsl:variable name="vBookEfsmlTradeList" select="$vCurrencyEfsmlTradeList[@tradeId=$vBookFxOptionLegTradeList/@tradeId]"/>

                  <xsl:variable name="vBookFxOptionLegTradeListCopyNode">
                    <xsl:copy-of select="$vBookFxOptionLegTradeList"/>
                  </xsl:variable>
                  <xsl:variable name="vBookFxOptionLegTradeListCopy" select="msxsl:node-set($vBookFxOptionLegTradeListCopyNode)/trade"/>
                  <xsl:variable name="vInstr-OTCmlId_List"
                                select="$vBookFxOptionLegTradeListCopy/fxSimpleOption/productType[generate-id()=generate-id(key('kOTCmlId',@OTCmlId))]"/>

                  <xsl:for-each select="$vInstr-OTCmlId_List">

                    <xsl:variable name="vInstr-OTCmlId" select="current()"/>
                    <xsl:variable name="vInstrFxOptionLegTradeList" select="$vBookFxOptionLegTradeList[fxSimpleOption/productType/@OTCmlId=$vInstr-OTCmlId/@OTCmlId]"/>

                    <xsl:if test="$vInstrFxOptionLegTradeList">

                      <xsl:variable name="vInstrEfsmlTradeList" select="$vBookEfsmlTradeList[@tradeId=$vInstrFxOptionLegTradeList/@tradeId]"/>
                      <xsl:variable name="vDealtCurrency_ListNode">
                        <xsl:for-each select="$vInstrFxOptionLegTradeList/fxSimpleOption[optionType/text()='Call']/callCurrencyAmount/currency |
                                      $vInstrFxOptionLegTradeList/fxSimpleOption[optionType/text()='Put']/putCurrencyAmount/currency">
                          <currency ccy="{current()/text()}"/>
                        </xsl:for-each>
                      </xsl:variable>
                      <xsl:variable name="vDealtCurrency_List"
                                    select="msxsl:node-set($vDealtCurrency_ListNode)/currency[generate-id()=generate-id(key('kCcy',@ccy))]"/>

                      <xsl:for-each select="$vDealtCurrency_List">

                        <xsl:variable name="vDealtCurrency" select="current()/@ccy"/>
                        <xsl:variable name="vDealtCurrencyFxOptionLegTradeList"
                                      select="$vInstrFxOptionLegTradeList[fxSimpleOption[optionType/text()='Call']/callCurrencyAmount/currency/text()=$vDealtCurrency] |
                                      $vInstrFxOptionLegTradeList[fxSimpleOption[optionType/text()='Put']/putCurrencyAmount/currency/text()=$vDealtCurrency]"/>

                        <xsl:if test="$vDealtCurrencyFxOptionLegTradeList">

                          <xsl:variable name="vDealtCurrencyEfsmlTradeList"
                                        select="$vInstrEfsmlTradeList[@tradeId=$vDealtCurrencyFxOptionLegTradeList/@tradeId]"/>
                          <xsl:variable name="vDealtCurrencyCommonTradeList"
                                        select="$gCommonData/trade[@tradeId=$vDealtCurrencyFxOptionLegTradeList/@tradeId]"/>

                          <dealt ccy="{$vDealtCurrency}"
                                 idI="{$vInstr-OTCmlId/@OTCmlId}"
                                 family="{$vFamily}">

                            <xsl:call-template name="Business_GetPattern">
                              <xsl:with-param name="pCcy" select="$vDealtCurrency"/>
                              <xsl:with-param name="pFmtLastPx" select="$vDealtCurrencyCommonTradeList/@fmtLastPx"/>
                              <xsl:with-param name="pFmtClrPx" select="$vDealtCurrencyEfsmlTradeList/@fmtClrPx"/>
                            </xsl:call-template>

                            <xsl:for-each select="$vDealtCurrencyEfsmlTradeList">

                              <xsl:variable name="vEfsmlTrade" select="current()"/>
                              <!--<xsl:variable name="vFxOptionLegTrade" select="$vDealtCurrencyFxOptionLegTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>-->
                              <xsl:variable name="vCommonTrade" select="$vDealtCurrencyCommonTradeList[@tradeId=$vEfsmlTrade/@tradeId]"/>

                              <!--expDt="{$vFxOptionLegTrade/fxSimpleOption/expiryDateTime/expiryDate/text()}"-->

                              <trade trdNum="{$vEfsmlTrade/@tradeId}" tradeId="{$vEfsmlTrade/@tradeId}"
                                     expDt="{$vCommonTrade/@expDt}"
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
