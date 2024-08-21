<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

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
  <!-- DisplayData_FxLeg                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in fxSingleLeg Trade          
       ................................................ -->
  <xsl:template name="DisplayData_FxLeg">
    <xsl:param name="pDataName"/>
    <xsl:param name="pFxLegTrade"/>
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='BuyQty'" >
          <xsl:if test="$pFxLegTrade/fxSingleLeg/RptSide/@Side = '1'">
            <xsl:call-template name="DisplayData_FxLeg">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pFxLegTrade" select="$pFxLegTrade"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='SellQty'" >
          <xsl:if test="$pFxLegTrade/fxSingleLeg/RptSide/@Side = '2'">
            <xsl:call-template name="DisplayData_FxLeg">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pFxLegTrade" select="$pFxLegTrade"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='Qty'" >
          <xsl:call-template name="DisplayData_Amounts">
            <xsl:with-param name="pDataName" select="'amount'"/>
            <xsl:with-param name="pAmount" select="$pFxLegTrade/fxSingleLeg/exchangedCurrency1/paymentAmount/amount"/>
            <xsl:with-param name="pCcy" select="$pFxLegTrade/fxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='CurrencyPair'" >
          <xsl:call-template name="DisplayCurrencyPair">
            <xsl:with-param name="pQuotedCurrencyPair" select="$pFxLegTrade/fxSingleLeg/exchangeRate/quotedCurrencyPair"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementCcy'" >
          <xsl:value-of select="concat($pColumnSettings/data[@name='Settlt']/@resource,':',$gcSpace,$pFxLegTrade/fxSingleLeg/nonDeliverableForward/settlementCurrency/text())"/>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementRate'" >
          <xsl:value-of select="concat($pColumnSettings/data[@name='Rate']/@resource,':',$gcSpace,$gRepository/fxRate[@OTCmlId=$pFxLegTrade/fxSingleLeg/nonDeliverableForward/fixing/primaryRateSource/@OTCmlId]/identifier/text())"/>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementFixing'" >
          <xsl:variable name="vDate">
            <xsl:call-template name="DisplayData_Format">
              <xsl:with-param name="pData" select="$pFxLegTrade/fxSingleLeg/nonDeliverableForward/fixing/fixingDate/text()"/>
              <xsl:with-param name="pDataType" select="'Date'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vFxRate" select="$gRepository/fxRate[@OTCmlId=$pFxLegTrade/fxSingleLeg/nonDeliverableForward/fixing/primaryRateSource/@OTCmlId]"/>
          <xsl:variable name="vBC" select="$gRepository/businessCenter[@id=concat('BUSINESSCENTER.IDBC.',$vFxRate/fixingTime/businessCenter/text())]/identifier/text()"/>
          <xsl:variable name="vTime">
            <xsl:call-template name="format-shorttime3">
              <xsl:with-param name="xsd-date-time" select="$vFxRate/fixingTime/hourMinuteTime/text()"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:value-of select="concat($pColumnSettings/data[@name='Fixing']/@resource,':',$gcSpace,$vDate,$gcSpace,$vBC,$gcSpace,$vTime)"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vDataLength">
      <xsl:choose>
        <xsl:when test="$pDataName='CashSettlementRate'">
          <xsl:value-of select="$pColumnSettings/data[@name='Rate']/@length"/>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementFixing'">
          <xsl:value-of select="$pColumnSettings/data[@name='Fixing']/@length"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number('0')"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$vDataLength"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplaySubtotal_FxLeg                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Subtotal for fxSingleLeg Trade          
       ................................................ -->
  <xsl:template name="DisplaySubtotal_FxLeg">
    <xsl:param name="pDataName"/>
    <xsl:param name="pFxLegTrades"/>
    <xsl:param name="pEfsmlTrades"/>
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='LongQty'" >
          <xsl:call-template name="DisplaySubtotal_FxLeg">
            <xsl:with-param name="pDataName" select="'Qty'"/>
            <xsl:with-param name="pFxLegTrades" select="$pFxLegTrades[fxSingleLeg/RptSide/@Side=1]"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='ShortQty'" >
          <xsl:call-template name="DisplaySubtotal_FxLeg">
            <xsl:with-param name="pDataName" select="'Qty'"/>
            <xsl:with-param name="pFxLegTrades" select="$pFxLegTrades[fxSingleLeg/RptSide/@Side=2]"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='Qty'" >
          <xsl:variable name="vCcy" select="$pFxLegTrades[1]/fxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
          <xsl:variable name="vTotalAmount" select="sum($pFxLegTrades/fxSingleLeg/exchangedCurrency1/paymentAmount[currency=$vCcy]/amount)"/>
          <xsl:call-template name="DisplayData_Amounts">
            <xsl:with-param name="pDataName" select="'amount'"/>
            <xsl:with-param name="pAmount" select="$vTotalAmount"/>
            <xsl:with-param name="pCcy" select="$vCcy"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayTradeDetRow_FxLeg                         -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Timestamp, Fee and Cash-Settlement details for fxSingleLeg Trade          
       ................................................ -->
  <!--RD 20200818 [25456] Add pFee -->
  <xsl:template name="DisplayTradeDetRow_FxLeg">
    <xsl:param name="pFamily"/>
    <xsl:param name="pFxLegTrade"/>
    <xsl:param name="pEfsmlTrade"/>
    <xsl:param name="pFee"/>

    <xsl:variable name="vDisplayTimestamp">
      <xsl:call-template name="DisplayData_Efsml">
        <xsl:with-param name="pDataName" select="'TimeStamp'"/>
        <xsl:with-param name="pDataEfsml" select="$pEfsmlTrade"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$pFamily='FxNDF'">
        <xsl:variable name="vCashSettltColor" select="$gBlockSettings_Data/column[@name='CashSettlt']/data/@color"/>
        <!-- 2/ Row with Cash-Settlt data, and second Fee-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gData_font-weight}"
                      font-style="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@font-style}"
                      text-align="{$gData_text-align}"
                      display-align="{$gData_display-align}"
                      keep-with-previous="always">

          <xsl:choose>
            <xsl:when test="$gIsDisplayTimestamp=true()">
              <xsl:call-template name="UKDisplay_Timestamp">
                <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
              </xsl:call-template>
              <fo:table-cell number-columns-spanned="7">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell number-columns-spanned="8">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@text-align}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vCashSettltColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxLeg">
                <xsl:with-param name="pDataName" select="'CashSettlementCcy'"/>
                <xsl:with-param name="pFxLegTrade" select="$pFxLegTrade"/>
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='CashSettlt']"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@text-align}"
                         number-columns-spanned="2">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vCashSettltColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxLeg">
                <xsl:with-param name="pDataName" select="'CashSettlementRate'"/>
                <xsl:with-param name="pFxLegTrade" select="$pFxLegTrade"/>
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='CashSettlt']"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell>
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:for-each select="msxsl:node-set($pFee)/fee">
            <xsl:sort select="@paymentType"/>
            <xsl:sort select="@ccy"/>
            <xsl:sort select="@side=$gcDebit"/>
            <xsl:sort select="@side=$gcCredit"/>

            <!--Le premier a été déjà affiché sur la première ligne-->
            <xsl:if test="position()=2">
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell number-columns-spanned="5">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:if>
          </xsl:for-each>
        </fo:table-row>
        <!-- 3/ Row with Fixing data, and third Fee-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gData_font-weight}"
                      font-style="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@font-style}"
                      text-align="{$gData_text-align}"
                      display-align="{$gData_display-align}"
                      keep-with-previous="always">

          <fo:table-cell number-columns-spanned="8">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@text-align}"
                         number-columns-spanned="3">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vCashSettltColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxLeg">
                <xsl:with-param name="pDataName" select="'CashSettlementFixing'"/>
                <xsl:with-param name="pFxLegTrade" select="$pFxLegTrade"/>
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='CashSettlt']"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell>
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:for-each select="msxsl:node-set($pFee)/fee">
            <xsl:sort select="@paymentType"/>
            <xsl:sort select="@ccy"/>
            <xsl:sort select="@side=$gcDebit"/>
            <xsl:sort select="@side=$gcCredit"/>

            <!--Le premier a été déjà affiché sur la première ligne-->
            <xsl:if test="position()=3">
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell number-columns-spanned="5">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:if>
          </xsl:for-each>
        </fo:table-row>
        <!-- 4/ Other Fee rows-->
        <xsl:for-each select="msxsl:node-set($pFee)/fee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>

          <!--Le premier a été déjà affiché sur la première ligne-->
          <xsl:if test="position()>3">
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}"
                          keep-with-previous="always">

              <fo:table-cell number-columns-spanned="12">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell number-columns-spanned="5">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="count(msxsl:node-set($pFee)/fee) > 1">
        <!--Other Fee rows-->
        <xsl:for-each select="msxsl:node-set($pFee)/fee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>

          <!--Le premier a été déjà affiché sur la première ligne-->
          <xsl:if test="position()>1">
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}"
                          keep-with-previous="always">

              <xsl:choose>
                <xsl:when test="position()=2 and $gIsDisplayTimestamp=true()">
                  <xsl:call-template name="UKDisplay_Timestamp">
                    <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
                  </xsl:call-template>
                  <fo:table-cell number-columns-spanned="11">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="12">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell number-columns-spanned="5">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="$gIsDisplayTimestamp=true()">
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gData_font-weight}"
                      text-align="{$gData_text-align}"
                      display-align="{$gData_display-align}"
                      keep-with-previous="always">

          <xsl:call-template name="UKDisplay_Timestamp">
            <xsl:with-param name="pTimestamp" select="$vDisplayTimestamp"/>
          </xsl:call-template>
          <fo:table-cell number-columns-spanned="20">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
        </fo:table-row>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Display_NetAmount_FxLeg                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display NetAmount for fxSingleLeg Trades
       ................................................ -->
  <xsl:template name="Display_NetAmount_FxLeg">
    <xsl:param name="pFxLegTrade"/>

    <xsl:variable name="vCurrency_NetAmount" select="$pFxLegTrade/fxSingleLeg/exchangedCurrency2/paymentAmount/currency"/>
    <xsl:variable name="vTotal_NetAmount" select="
                  sum($pFxLegTrade/fxSingleLeg[RptSide/@Side=2]/exchangedCurrency2/paymentAmount[currency=$vCurrency_NetAmount]/amount/text()) 
                  - 
                  sum($pFxLegTrade/fxSingleLeg[RptSide/@Side=1]/exchangedCurrency2/paymentAmount[currency=$vCurrency_NetAmount]/amount/text())"/>

    <xsl:call-template name="UKDisplay_Amount">
      <xsl:with-param name="pAmount" select="$vTotal_NetAmount" />
      <xsl:with-param name="pCcy" select="$vCurrency_NetAmount"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplaySubtotal_NetAmount_FxLeg                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display NetAmount subtotal details for fxSingleLeg Trades
       ................................................ -->
  <xsl:template name="DisplaySubtotal_NetAmount_FxLeg">
    <xsl:param name="pFxLegTrades"/>

    <xsl:variable name="vCurrency_NetAmount" select="$pFxLegTrades[1]/fxSingleLeg/exchangedCurrency2/paymentAmount/currency"/>
    <xsl:variable name="vTotal_NetAmount" select="
                  sum($pFxLegTrades/fxSingleLeg[RptSide/@Side=2]/exchangedCurrency2/paymentAmount[currency=$vCurrency_NetAmount]/amount/text()) 
                  - 
                  sum($pFxLegTrades/fxSingleLeg[RptSide/@Side=1]/exchangedCurrency2/paymentAmount[currency=$vCurrency_NetAmount]/amount/text())"/>

    <xsl:call-template name="UKDisplay_SubTotal_Amount">
      <xsl:with-param name="pAmount" select="$vTotal_NetAmount" />
      <xsl:with-param name="pCcy" select="$vCurrency_NetAmount"/>
    </xsl:call-template>
  </xsl:template>
</xsl:stylesheet>