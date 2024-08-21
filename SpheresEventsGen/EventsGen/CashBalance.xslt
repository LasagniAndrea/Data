<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:template mode="product" match="item[@name='CashBalance']">
    <xsl:param name="pPosition" select="0"/>
    <cashBalance>
      <!-- Product -->
      <keyGroup name="CashBalance" key="PRD"/>
      <parameter id="timing">GetTiming()</parameter>
      <parameter id="EventCodeByTiming">EfsML.Enum.Tools.EventCodeFunc|LinkProductClosingIntraday(timing)</parameter>
      <parameter id="EventCodeFutureByTiming">EfsML.Enum.Tools.EventCodeFunc|LinkFutureClosingIntraday(timing)</parameter>
      <parameter id="EventCodeOptionByTiming">EfsML.Enum.Tools.EventCodeFunc|LinkOptionClosingIntraday(timing)</parameter>
      <itemGroup>
        <xsl:choose>
          <xsl:when test = "$pPosition = '0'">
            <occurence to="Unique">product</occurence>
          </xsl:when>
          <xsl:otherwise>
            <occurence to="Item" position="{$pPosition}">Item</occurence>
          </xsl:otherwise>
        </xsl:choose>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Product</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Date</eventType>
        </key>
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <idStCalcul>[CALC]</idStCalcul>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="ExchangeCashBalanceStream"/>
      <xsl:call-template name="CashBalanceStream"/>
    </cashBalance>
  </xsl:template>

  <xsl:template name="CashBalanceStream">
    <cashBalanceStream>
      <keyGroup name="Stream" key="STD"/>
      <itemGroup>
        <occurence to="All">cashBalanceStream</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|CashBalanceStream</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <unitType>[Currency]</unitType>
        <unit>GetCurrencyValue()</unit>
        <idStCalcul>[CALC]</idStCalcul>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>

      <!-- marginRequirement -->
      <marginRequirement>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'marginRequirement'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'MarginRequirement'"/>
          <xsl:with-param name="pIsOptional" select="'false'"/>
        </xsl:call-template>
      </marginRequirement>

      <!-- cashAvailable -->
      <xsl:call-template name="CashAvailable"/>

      <!-- cashUsed -->
      <cashUsed>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'cashUsed'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CashUsed'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </cashUsed>

      <!-- collateralAvailable -->
      <collateralAvailable>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'collateralAvailable'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CollateralAvailable'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </collateralAvailable>

      <!-- collateralUsed -->
      <collateralUsed>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'collateralUsed'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CollateralUsed'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </collateralUsed>

      <!-- uncoveredMarginRequirement -->
      <uncoveredMarginRequirement>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'uncoveredMarginRequirement'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'UncoveredMarginRequirement'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </uncoveredMarginRequirement>

      <!-- marginCall -->
      <marginCall>
        <xsl:call-template name="SimplePayment">
          <xsl:with-param name="pOccurence" select="'marginCall'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'MarginCall'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </marginCall>

      <!-- CashBalance -->
      <cashBalance>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'cashBalance'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CashBalance'"/>
          <xsl:with-param name="pIsOptional" select="'false'"/>
        </xsl:call-template>
      </cashBalance>

      <!-- CashBalanceCommonStream -->
      <xsl:call-template name="CashBalanceCommonStream"/>
      
    </cashBalanceStream>
  </xsl:template>

  <xsl:template name="ExchangeCashBalanceStream">
    <exchangeCashBalanceStream>
      <keyGroup name="Stream" key="STD"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">exchangeCashBalanceStream</occurence>
        <key>
          <!--<eventCode>EfsML.Enum.Tools.EventCodeFunc|CashBalanceStream</eventCode>-->
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|ExchangeCashBalanceStream</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <unitType>[Currency]</unitType>
        <unit>GetCurrencyValue()</unit>
        <idStCalcul>[CALC]</idStCalcul>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>

      <!-- marginRequirement -->
      <marginRequirement>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'marginRequirement'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'MarginRequirement'"/>
          <xsl:with-param name="pIsOptional" select="'false'"/>
        </xsl:call-template>
      </marginRequirement>

      <!-- cashAvailable -->
      <!--PM 20140919 [20066][20185] Gestion méthode UK-->
      <!--<cashAvailable>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'cashAvailable'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CashAvailable'"/>
          <xsl:with-param name ="pIsOptional" select="'false'"/>
        </xsl:call-template>
      </cashAvailable>-->
      <xsl:call-template name="CashAvailable"/>

      <!-- cashUsed -->
      <cashUsed>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'cashUsed'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CashUsed'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </cashUsed >

      <!-- collateralAvailable -->
      <collateralAvailable>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'collateralAvailable'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CollateralAvailable'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </collateralAvailable>

      <!-- collateralUsed -->
      <collateralUsed>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'collateralUsed'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CollateralUsed'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </collateralUsed >

      <!-- uncoveredMarginRequirement -->
      <uncoveredMarginRequirement>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'uncoveredMarginRequirement'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'UncoveredMarginRequirement'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </uncoveredMarginRequirement >

      <!-- marginCall -->
      <marginCall>
        <xsl:call-template name="SimplePayment">
          <xsl:with-param name="pOccurence" select="'marginCall'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'MarginCall'"/>
        </xsl:call-template>
      </marginCall>

      <!-- Cash Balance -->
      <cashBalance>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'cashBalance'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CashBalance'"/>
          <xsl:with-param name="pIsOptional" select="'true'"/>
        </xsl:call-template>
      </cashBalance>

      <!-- CashBalanceCommonStream -->
      <xsl:call-template name="CashBalanceCommonStream"/>
      
    </exchangeCashBalanceStream>
  </xsl:template>

  <xsl:template name="CashAvailable">
    <cashAvailable>
      <keyGroup name="CashAvailable" key="CSA"/>
      <itemGroup>
        <occurence to="Unique">cashAvailable</occurence>
        <key>
          <eventCodeReference hRef="EventCodeByTiming"/>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|CashAvailable</eventType>
        </key>
        <startDate>GetEventDate()</startDate>
        <endDate>GetEventDate()</endDate>
        <payer>GetPayerReceiver('Payer')</payer>
        <receiver>GetPayerReceiver('Receiver')</receiver>
        <valorisation>GetAmount()</valorisation>
        <unitType>[Currency]</unitType>
        <unit>GetCurrency()</unit>
        <idStCalcul>[CALC]</idStCalcul>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>

      <!-- previousCashBalance -->
      <previousCashBalance>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'previousCashBalance'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'PreviousCashBalance'"/>
          <xsl:with-param name="pIsOptional" select="'false'"/>
        </xsl:call-template>
      </previousCashBalance >

      <!-- cashBalancePayment -->
      <cashBalancePayment>
        <xsl:call-template name="CashPosition">
          <xsl:with-param name="pOccurence" select="'cashBalancePayment'"/>
          <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
          <xsl:with-param name="pEventType" select="'CashBalancePayment'"/>
          <xsl:with-param name="pIsOptional" select="'false'"/>
        </xsl:call-template>
      </cashBalancePayment >

      <!-- cashFlowConstituent -->
      <cashFlowConstituent>
        <keyGroup name="GroupLevel" key="GRP"/>
        <itemGroup>
          <key>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|CashFlowConstituent</eventCode>
            <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
          </key>
          <idStCalcul>[CALC]</idStCalcul>
          <startDateReference hRef="TradeDate"/>
          <endDateReference hRef="TradeDate"/>
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
            <eventDateReference hRef="AdjustedTradeDate"/>
          </subItem>
        </itemGroup>
        <xsl:call-template name="CashFlow"/>
      </cashFlowConstituent>

    </cashAvailable>
  </xsl:template>

  <xsl:template name="CashPosition">
    <xsl:param name="pOccurence"/>
    <xsl:param name="pEventCode"/>
    <xsl:param name="pEventType"/>
    <xsl:param name="pIsOptional" select="false"/>
    <keyGroup name="CashPosition" key="CSP"/>
    <itemGroup>
      <occurence>
        <xsl:attribute name ="to">Unique</xsl:attribute>
        <xsl:attribute name ="isOptional">
          <xsl:value-of select="$pIsOptional"/>
        </xsl:attribute>
        <xsl:value-of select="$pOccurence"/>
      </occurence>
      <key>
        <xsl:choose>
          <xsl:when test ="$pEventCode='EventCodeByTiming'">
            <eventCodeReference hRef="EventCodeByTiming"/>
          </xsl:when>
          <xsl:when test ="$pEventCode='EventCodeFutureByTiming'">
            <eventCodeReference hRef="EventCodeFutureByTiming"/>
          </xsl:when>
          <xsl:when test ="$pEventCode='EventCodeOptionByTiming'">
            <eventCodeReference hRef="EventCodeOptionByTiming"/>
          </xsl:when>
          <xsl:otherwise>
            <eventCode>
              EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/>
            </eventCode>
          </xsl:otherwise>
        </xsl:choose>
        <eventType>
          EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pEventType"/>
        </eventType>
      </key>
      <startDate>GetEventDate()</startDate>
      <endDate>GetEventDate()</endDate>
      <payer>GetPayerReceiver('Payer')</payer>
      <receiver>GetPayerReceiver('Receiver')</receiver>
      <valorisation>GetAmount()</valorisation>
      <unitType>[Currency]</unitType>
      <unit>GetCurrency()</unit>
      <idStCalcul>[CALC]</idStCalcul>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
        <eventDateReference hRef="AdjustedTradeDate" />
      </subItem>
    </itemGroup>
  </xsl:template>

  <xsl:template name="CashFlow">
    <!-- variationMargin -->
    <variationMargin>
      <xsl:call-template name="SimplePayment">
        <xsl:with-param name="pOccurence" select="'variationMargin'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'VariationMargin'"/>
      </xsl:call-template>
    </variationMargin>

    <!-- premium -->
    <premium>
      <xsl:call-template name="SimplePayment">
        <xsl:with-param name="pOccurence" select="'premium'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'Premium'"/>
      </xsl:call-template>
    </premium>

    <!-- cashSettlement -->
    <cashSettlement>
      <xsl:call-template name="SimplePayment">
        <xsl:with-param name="pOccurence" select="'cashSettlement'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'SettlementCurrency'"/>
      </xsl:call-template>
    </cashSettlement>

    <!-- fee-->
    <fee>
      <xsl:call-template name="Payment">
        <xsl:with-param name="pType" select="'CashBalanceFees'"/>
        <xsl:with-param name="pProduct" select="item/@name"/>
      </xsl:call-template>
    </fee>

    <!-- safekeeping-->
    <safekeeping>
      <xsl:call-template name="Payment">
        <xsl:with-param name="pType" select="'CashBalanceSafekeeping'"/>
        <xsl:with-param name="pProduct" select="item/@name"/>
      </xsl:call-template>
    </safekeeping>

    <!-- equalisationPayment -->
    <!--PM 20170911 [23408] Add Equalisation Payment-->
    <equalisationPayment>
      <xsl:call-template name="SimplePayment">
        <xsl:with-param name="pOccurence" select="'equalisationPayment'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'EqualisationPayment'"/>
        <xsl:with-param name="pIsOptional" select ="'true'"/>
      </xsl:call-template>
    </equalisationPayment>
    
  </xsl:template>

  <xsl:template name="CashBalanceCommonStream">
    <!--Cash Deposit-->
    <cashDeposit>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'cashDeposit'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'CashDeposit'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </cashDeposit >

    <!--Cash Withdrawal-->
    <cashWithdrawal>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'cashWithdrawal'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'CashWithdrawal'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </cashWithdrawal >

    <!-- Realized Margin -->
    <realizedMargin>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'globalRealizedMargin'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'RealizedMargin'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </realizedMargin >

    <!-- Unrealized Margin -->
    <unrealizedMargin>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'globalUnrealizedMargin'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'UnrealizedMargin'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </unrealizedMargin >

    <!-- Funding Amount -->
    <funding>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'funding'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'FundingAmount'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </funding >

    <!-- Borrowing Amount -->
    <!-- PM 20150323 [POC] Add borrowing -->
    <borrowing>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'borrowing'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'BorrowingAmount'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </borrowing>

    <!-- Unsettled Transaction -->
    <!-- PM 20150330 Add unsettledCash -->
    <unsettledTransaction>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'unsettledCash'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'UnsettledTransaction'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </unsettledTransaction>

    <!--Long Option Value-->
    <longOptionValue>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'longOptionValue'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'LongOptionValue'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </longOptionValue >

    <!--Short Option Value-->
    <shortOptionValue>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'shortOptionValue'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'ShortOptionValue'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </shortOptionValue >

    <!-- Market Value  -->
    <!--PM 20150616 [21124] add marketValue -->
    <marketValue>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'marketValue'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'MarketValue'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </marketValue >
    
    <!-- Forward Cash Payment -->
    <forwardCashPayment>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'forwardCashPayment'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'ForwardCashPayment'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </forwardCashPayment >

    <!-- Equity Balance -->
    <equityBalance>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'equityBalance'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'EquityBalance'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </equityBalance >

    <!-- Equity Balance With Forward Cash -->
    <equityBalanceWithForwardCash>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'equityBalanceWithForwardCash'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'EquityBalanceWithForwardCash'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </equityBalanceWithForwardCash >

    <!-- Total Account Value -->
    <!--PM 20150616 [21124] add totalAccountValue -->
    <totalAccountValue>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'totalAccountValue'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'TotalAccountValue'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </totalAccountValue >
    
    <!-- Excess/Deficit -->
    <excessDeficit>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'excessDeficit'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'ExcessDeficit'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </excessDeficit >

    <!-- Excess/Deficit With Forward Cash -->
    <excessDeficitWithForwardCash>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="'excessDeficitWithForwardCash'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeByTiming'"/>
        <xsl:with-param name="pEventType" select="'ExcessDeficitWithForwardCash'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </excessDeficitWithForwardCash >

    <!-- Margin Constituent -->
    <xsl:call-template name="MarginConstituent"/>

  </xsl:template>

  <xsl:template name="MarginConstituent">
    <!-- Future Exchange Traded Derivative -->
    <futureExchangeTradedDerivative>
      <keyGroup name="FutureExchangeTradedDerivative" key="GRP"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">futureMarginConstituent</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|FutureExchangeTradedDerivative</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="RealizedUnrealizedMargin">
        <xsl:with-param name="pOccurenceRMG" select="'realizedMargin'"/>
        <xsl:with-param name="pOccurenceUMG" select="'unrealizedMargin'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeFutureByTiming'"/>
      </xsl:call-template>
    </futureExchangeTradedDerivative>

    <!-- Futures Style Option -->
    <futuresStyleOption>
      <keyGroup name="FuturesStyleOption" key="GRP"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">futuresStyleOptionMarginConstituent</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|FuturesStyleOption</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="RealizedUnrealizedMargin">
        <xsl:with-param name="pOccurenceRMG" select="'realizedMargin'"/>
        <xsl:with-param name="pOccurenceUMG" select="'unrealizedMargin'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeOptionByTiming'"/>
      </xsl:call-template>
    </futuresStyleOption>

    <!-- Premium Style Option -->
    <premiumStyleOption>
      <keyGroup name="PremiumStyleOption" key="GRP"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">premiumStyleOptionMarginConstituent</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|PremiumStyleOption</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="RealizedUnrealizedMargin">
        <xsl:with-param name="pOccurenceRMG" select="'realizedMargin'"/>
        <xsl:with-param name="pOccurenceUMG" select="'unrealizedMargin'"/>
        <xsl:with-param name="pEventCode" select="'EventCodeOptionByTiming'"/>
      </xsl:call-template>
    </premiumStyleOption>
  </xsl:template>

  <xsl:template name="RealizedUnrealizedMargin">
    <xsl:param name="pOccurenceRMG"/>
    <xsl:param name="pOccurenceUMG"/>
    <xsl:param name="pEventCode"/>
    
    <!-- Realized Margin -->
    <realizedMargin>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="$pOccurenceRMG"/>
        <xsl:with-param name="pEventCode" select="$pEventCode"/>
        <xsl:with-param name="pEventType" select="'RealizedMargin'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </realizedMargin>
    <!-- Unrealized Margin -->
    <unrealizedMargin>
      <xsl:call-template name="CashPosition">
        <xsl:with-param name="pOccurence" select="$pOccurenceUMG"/>
        <xsl:with-param name="pEventCode" select="$pEventCode"/>
        <xsl:with-param name="pEventType" select="'UnrealizedMargin'"/>
        <xsl:with-param name="pIsOptional" select="'true'"/>
      </xsl:call-template>
    </unrealizedMargin>
  </xsl:template>

</xsl:stylesheet>
