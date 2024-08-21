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
  <!-- DisplayData_FxOptionLeg                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in fxSingleLeg Trade          
       ................................................ -->
  <xsl:template name="DisplayData_FxOptionLeg">
    <xsl:param name="pDataName"/>
    <xsl:param name="pFxOptionLegTrade"/>
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='BuyQty'" >
          <xsl:if test="$pFxOptionLegTrade/fxSimpleOption/RptSide/@Side = '1'">
            <xsl:call-template name="DisplayData_FxOptionLeg">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='SellQty'" >
          <xsl:if test="$pFxOptionLegTrade/fxSimpleOption/RptSide/@Side = '2'">
            <xsl:call-template name="DisplayData_FxOptionLeg">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='Qty'" >
          <xsl:variable name="vCurrencyAmount"
                        select="$pFxOptionLegTrade/fxSimpleOption[optionType/text()='Call']/callCurrencyAmount |
                        $pFxOptionLegTrade/fxSimpleOption[optionType/text()='Put']/putCurrencyAmount"/>

          <xsl:call-template name="DisplayData_Amounts">
            <xsl:with-param name="pDataName" select="'amount'"/>
            <xsl:with-param name="pAmount" select="$vCurrencyAmount/amount"/>
            <xsl:with-param name="pCcy" select="$vCurrencyAmount/currency"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='Strike'" >
          <xsl:variable name="vCurrencyAmount"
                        select="$pFxOptionLegTrade/fxSimpleOption[optionType/text()='Call']/callCurrencyAmount |
                        $pFxOptionLegTrade/fxSimpleOption[optionType/text()='Put']/putCurrencyAmount"/>

          <xsl:call-template name="DisplayData_Amounts">
            <xsl:with-param name="pDataName" select="'amount'"/>
            <xsl:with-param name="pAmount" select="number($pFxOptionLegTrade/fxSimpleOption/fxStrikePrice/rate/text())"/>
            <xsl:with-param name="pCcy" select="$vCurrencyAmount/currency"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementCcy'" >
          <xsl:value-of select="concat($pColumnSettings/data[@name='Settlt']/@resource,':',$gcSpace,$pFxOptionLegTrade/fxSimpleOption/cashSettlementTerms/settlementCurrency/text())"/>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementRate'" >
          <xsl:value-of select="concat($pColumnSettings/data[@name='Rate']/@resource,':',$gcSpace,$gRepository/fxRate[@OTCmlId=$pFxOptionLegTrade/fxSimpleOption/cashSettlementTerms/fixing/primaryRateSource/@OTCmlId]/identifier/text())"/>
        </xsl:when>
        <xsl:when test="$pDataName='CashSettlementFixing'" >
          <xsl:variable name="vDate">
            <xsl:call-template name="DisplayData_Format">
              <xsl:with-param name="pData" select="$pFxOptionLegTrade/fxSimpleOption/cashSettlementTerms/fixing/fixingDate/text()"/>
              <xsl:with-param name="pDataType" select="'Date'"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vFxRate" select="$gRepository/fxRate[@OTCmlId=$pFxOptionLegTrade/fxSimpleOption/cashSettlementTerms/fixing/primaryRateSource/@OTCmlId]"/>
          <xsl:variable name="vBC" select="$gRepository/businessCenter[@id=concat('BUSINESSCENTER.IDBC.',$vFxRate/fixingTime/businessCenter/text())]/identifier/text()"/>
          <xsl:variable name="vTime">
            <xsl:call-template name="format-shorttime3">
              <xsl:with-param name="xsd-date-time" select="$vFxRate/fixingTime/hourMinuteTime/text()"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:value-of select="concat($pColumnSettings/data[@name='Fixing']/@resource,':',$gcSpace,$vDate,$gcSpace,$vBC,$gcSpace,$vTime)"/>
        </xsl:when>
        <xsl:when test="$pDataName='OptionType'" >
          <xsl:value-of select="$pFxOptionLegTrade/fxSimpleOption/optionType/text()"/>
        </xsl:when>
        <xsl:when test="$pDataName='nonOptionType'" >
          <xsl:choose>
            <xsl:when test="$pFxOptionLegTrade/fxSimpleOption/optionType/text()='Call'">
              <xsl:value-of select="'Put'"/>
            </xsl:when>
            <xsl:when test="$pFxOptionLegTrade/fxSimpleOption/optionType/text()='Put'">
              <xsl:value-of select="'Call'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='DealtCcy'" >
          <xsl:value-of select="($pFxOptionLegTrade/fxSimpleOption[optionType/text()='Call']/callCurrencyAmount/currency | 
                        $pFxOptionLegTrade/fxSimpleOption[optionType/text()='Put']/putCurrencyAmount/currency)/text()"/>
        </xsl:when>
        <xsl:when test="$pDataName='nonDealtCcy'" >
          <xsl:value-of select="($pFxOptionLegTrade/fxSimpleOption[optionType/text()='Call']/putCurrencyAmount/currency | 
                        $pFxOptionLegTrade/fxSimpleOption[optionType/text()='Put']/callCurrencyAmount/currency)/text()"/>
        </xsl:when>
        <xsl:when test="$pDataName='FxOptionExpiry'" >
          <xsl:variable name="vBC" select="$gRepository/businessCenter[@id=concat('BUSINESSCENTER.IDBC.',$pFxOptionLegTrade/fxSimpleOption/expiryDateTime/expiryTime/businessCenter/text())]/identifier/text()"/>
          <xsl:variable name="vTime">
            <xsl:call-template name="format-shorttime3">
              <xsl:with-param name="xsd-date-time" select="$pFxOptionLegTrade/fxSimpleOption/expiryDateTime/expiryTime/hourMinuteTime/text()"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="vStyle" select="concat(substring($pFxOptionLegTrade/fxSimpleOption/exerciseStyle/text(),1,4),'.')"/>
          <xsl:value-of select="concat($pColumnSettings/data[@name='Expiry']/@resource,':',$gcSpace,$vBC,$gcSpace,$vTime,$gcSpace,$pColumnSettings/data[@name='Style']/@resource,':',$gcSpace,$vStyle)"/>
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
  <!-- DisplaySubtotal_FxOptionLeg                      -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Subtotal for fxSimpleOption Trade          
       ................................................ -->
  <xsl:template name="DisplaySubtotal_FxOptionLeg">
    <xsl:param name="pDataName"/>
    <xsl:param name="pFxOptionLegTrades"/>
    <xsl:param name="pEfsmlTrades"/>
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='LongQty'" >
          <xsl:call-template name="DisplaySubtotal_FxOptionLeg">
            <xsl:with-param name="pDataName" select="'Qty'"/>
            <xsl:with-param name="pFxOptionLegTrades" select="$pFxOptionLegTrades[fxSimpleOption/RptSide/@Side=1]"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='ShortQty'" >
          <xsl:call-template name="DisplaySubtotal_FxOptionLeg">
            <xsl:with-param name="pDataName" select="'Qty'"/>
            <xsl:with-param name="pFxOptionLegTrades" select="$pFxOptionLegTrades[fxSimpleOption/RptSide/@Side=2]"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='Qty'" >
          <xsl:variable name="vCurrencyAmount"
                        select="$pFxOptionLegTrades/fxSimpleOption[optionType/text()='Call']/callCurrencyAmount |
                        $pFxOptionLegTrades/fxSimpleOption[optionType/text()='Put']/putCurrencyAmount"/>

          <xsl:variable name="vCcy" select="$vCurrencyAmount[1]/currency/text()"/>
          <xsl:variable name="vTotalAmount" select="sum($vCurrencyAmount[currency/text()=$vCcy]/amount)"/>
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
  <!-- DisplayTradeDetRow_FxOptionLeg                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Timestamp, Fee and Cash-Settlement details for fxSingleLeg Trade          
       ................................................ -->
  <!--RD 20200818 [25456] Add pFee -->
  <xsl:template name="DisplayTradeDetRow_FxOptionLeg">
    <xsl:param name="pFamily"/>
    <xsl:param name="pFxOptionLegTrade"/>
    <xsl:param name="pEfsmlTrade"/>
    <xsl:param name="pFee"/>

    <xsl:variable name="vDisplayTimestamp">
      <xsl:call-template name="DisplayData_Efsml">
        <xsl:with-param name="pDataName" select="'TimeStamp'"/>
        <xsl:with-param name="pDataEfsml" select="$pEfsmlTrade"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$pFamily='FxNDO'">
        <xsl:variable name="vFxOptionColor">
          <xsl:choose>
            <xsl:when test="$pFxOptionLegTrade/fxSimpleOption/RptSide/@Side = '1'">
              <xsl:value-of select="$gBlockSettings_Data/column[@name='FxOption']/data/buy/@color"/>
            </xsl:when>
            <xsl:when test="$pFxOptionLegTrade/fxSimpleOption/RptSide/@Side = '2'">
              <xsl:value-of select="$gBlockSettings_Data/column[@name='FxOption']/data/sell/@color"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vCashSettltColor" select="$gBlockSettings_Data/column[@name='CashSettlt']/data/@color"/>
        <!-- 2/ Row with FxOption data, and second Fee-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gData_font-weight}"
                      font-style="{$gBlockSettings_Data/column[@name='FxOption']/data/@font-style}"
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
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='FxOption']/data/@text-align}"
                         font-weight="{$gBlockSettings_Data/column[@name='FxOption']/data[@name='dealt']/putCall/@font-weight}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vFxOptionColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxOptionLeg">
                <xsl:with-param name="pDataName" select="'OptionType'"/>
                <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='FxOption']"/>
              </xsl:call-template>
              <xsl:value-of select="$gcSpace"/>
              <fo:inline font-weight="{$gBlockSettings_Data/column[@name='FxOption']/data[@name='dealt']/ccy/@font-weight}">
                <xsl:call-template name="DisplayData_FxOptionLeg">
                  <xsl:with-param name="pDataName" select="'DealtCcy'"/>
                  <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
                  <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='FxOption']"/>
                </xsl:call-template>
              </fo:inline>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='FxOption']/data/@text-align}"
                         font-weight="{$gBlockSettings_Data/column[@name='FxOption']/data[@name='nonDealt']/putCall/@font-weight}"
                         number-columns-spanned="2">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vFxOptionColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxOptionLeg">
                <xsl:with-param name="pDataName" select="'nonOptionType'"/>
                <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='FxOption']"/>
              </xsl:call-template>
              <xsl:value-of select="$gcSpace"/>
              <fo:inline font-weight="{$gBlockSettings_Data/column[@name='FxOption']/data[@name='nonDealt']/ccy/@font-weight}">
                <xsl:call-template name="DisplayData_FxOptionLeg">
                  <xsl:with-param name="pDataName" select="'nonDealtCcy'"/>
                  <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
                  <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='FxOption']"/>
                </xsl:call-template>
              </fo:inline>
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
                      font-style="{$gBlockSettings_Data/column[@name='FxOption']/data/@font-style}"
                      text-align="{$gData_text-align}"
                      display-align="{$gData_display-align}"
                      keep-with-previous="always">

          <fo:table-cell number-columns-spanned="8">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='FxOption']/data/@text-align}"
                         number-columns-spanned="3">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vFxOptionColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxOptionLeg">
                <xsl:with-param name="pDataName" select="'FxOptionExpiry'"/>
                <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='FxOption']"/>
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
        <!-- 4/ Row with Cash-Settlt data, and second Fee-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gData_font-weight}"
                      font-style="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@font-style}"
                      text-align="{$gData_text-align}"
                      display-align="{$gData_display-align}"
                      keep-with-previous="always">

          <fo:table-cell number-columns-spanned="8">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell text-align="{$gBlockSettings_Data/column[@name='CashSettlt']/data/@text-align}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vCashSettltColor"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="DisplayData_FxOptionLeg">
                <xsl:with-param name="pDataName" select="'CashSettlementCcy'"/>
                <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
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
              <xsl:call-template name="DisplayData_FxOptionLeg">
                <xsl:with-param name="pDataName" select="'CashSettlementRate'"/>
                <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
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
            <xsl:if test="position()=4">
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell number-columns-spanned="5">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:if>
          </xsl:for-each>
        </fo:table-row>
        <!-- 5/ Row with Fixing data, and third Fee-->
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
              <xsl:call-template name="DisplayData_FxOptionLeg">
                <xsl:with-param name="pDataName" select="'CashSettlementFixing'"/>
                <xsl:with-param name="pFxOptionLegTrade" select="$pFxOptionLegTrade"/>
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
            <xsl:if test="position()=5">
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell number-columns-spanned="5">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:if>
          </xsl:for-each>
        </fo:table-row>
        <!-- 6/ Other Fee rows-->
        <xsl:for-each select="msxsl:node-set($pFee)/fee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>

          <!--Le premier a été déjà affiché sur la première ligne-->
          <xsl:if test="position()>5">
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

</xsl:stylesheet>