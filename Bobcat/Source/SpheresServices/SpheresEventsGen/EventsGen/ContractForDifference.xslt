<?xml version="1.0" encoding="UTF-8" ?>
<!--EG 20231127 [WI755] New -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='ContractForDifference']">
    <xsl:param name="pPosition" select="0"/>
    <returnSwap>

      <!-- Product -->
      <keyGroup name="ReturnSwap" key="PRD"/>
      <parameter id="RTS_ClearingBusinessDate">ClearingBusinessDate</parameter>
      <parameter id="IMPayerPartyReference">InitialMarginPayerPartyReference</parameter>
      <parameter id="IMReceiverPartyReference">InitialMarginReceiverPartyReference</parameter>
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
        <idStCalcul>[CALC]</idStCalcul>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <returnLeg>
        <xsl:call-template name="ReturnLeg"/>
      </returnLeg>
      <interestLeg>
        <xsl:call-template name="InterestLeg"/>
      </interestLeg>
    </returnSwap>
  </xsl:template>


  <!-- ReturnLeg -->
  <xsl:template name="ReturnLeg">

    <keyGroup name="Leg" key="RLEG"/>
    <itemGroup>
      <occurence to="All">returnLeg</occurence>
      <parameter id="RLegEffectiveDate">EffectiveDate</parameter>
      <parameter id="RLegAdjustedEffectiveDate">AdjustedEffectiveDate</parameter>
      <parameter id="RLegTerminationDate">TerminationDate</parameter>
      <parameter id="RLegPayerPartyReference">PayerPartyReference</parameter>
      <parameter id="RLegReceiverPartyReference">ReceiverPartyReference</parameter>
      <parameter id="RLegValuationEventCode">ValuationEventCode</parameter>
      <parameter id="RLegValuationEventType">ValuationEventType</parameter>
      <parameter id="RLegAsset">Asset</parameter>

      <item_Details>
        <assetReference hRef="RLegAsset"/>
      </item_Details>

      <key>
        <eventCode>LegEventCode</eventCode>
        <eventType>LegEventType</eventType>
      </key>
      <idStCalcul>[CALC]</idStCalcul>
      <startDateReference hRef="RLegEffectiveDate"/>
      <endDateReference hRef="RLegTerminationDate"/>
      <payerReference hRef="RLegPayerPartyReference" />
      <receiverReference hRef="RLegReceiverPartyReference" />
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDate>AdjustedTerminationDate</eventDate>
      </subItem>
    </itemGroup>

    <xsl:call-template name="ReturnLegUnderlyer" />
    <xsl:call-template name="ReturnLegValuation" />

  </xsl:template>

  <!-- ReturnLegUnderlyer -->
  <xsl:template name="ReturnLegUnderlyer">
    <underlyer>
      <keyGroup name="ReturnLegUnderlyer" key="RLU"/>
      <itemGroup>
        <occurence to="Unique">efs_ReturnLeg</occurence>
        <key>
          <eventCode>UnderlyerEventCode</eventCode>
          <eventType>UnderlyerEventType</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegTerminationDate"/>

        <item_Details>
          <assetReference hRef="RLegAsset"/>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <!-- ReturnLegInitialAmount : NotionalAmount|NotionalReference|OpenUnits|MarginRatio -->
      <xsl:call-template name="ReturnLegInitialAmount">
        <xsl:with-param name="pAmountType" select="'Notional'"/>
      </xsl:call-template>

      <xsl:call-template name="ReturnLegInitialNotionalBase" />

      <xsl:call-template name="ReturnLegInitialAmount">
        <xsl:with-param name="pAmountType" select="'Quantity'"/>
      </xsl:call-template>
      
      <xsl:call-template name="ReturnLegInitialAmount">
        <xsl:with-param name="pAmountType" select="'MarginRequirementRatio'"/>
      </xsl:call-template>

    </underlyer>
  </xsl:template>

  <xsl:template name="ReturnLegInitialAmount">
    <xsl:param name="pAmountType" />

    <amount>
      <keyGroup name="InitialAmount" key="STAAMT" />
      <itemGroup>
        <key>
          <xsl:choose>
            <xsl:when test = "$pAmountType = 'MarginRequirementRatio'">
              <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
            </xsl:when>
            <xsl:otherwise>
              <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
            </xsl:otherwise>
          </xsl:choose>
          <eventType>EventTypeInitialAmount('<xsl:value-of select="$pAmountType" />')</eventType>
        </key>

        <xsl:choose>
          <xsl:when test = "$pAmountType = 'MarginRequirementRatio'">
            <startDateReference hRef="RLegEffectiveDate"/>
            <endDateReference   hRef="RLegTerminationDate"/>
            <valorisation>MarginRatioAmount</valorisation>
            <unitType>MarginRatioUnitType</unitType>
            <unit>MarginRatioUnit</unit>
          </xsl:when>
          <xsl:when test = "$pAmountType = 'Quantity'">
            <startDateReference hRef="RLegEffectiveDate"/>
            <endDateReference   hRef="RLegEffectiveDate"/>
            <valorisation>OpenUnits</valorisation>
            <unitType>UnitType</unitType>
            <unit>Unit</unit>
            <payerReference     hRef="RLegPayerPartyReference" />
            <receiverReference  hRef="RLegReceiverPartyReference" />
          </xsl:when>
          <xsl:when test = "$pAmountType = 'Notional'">
            <startDateReference hRef="RLegEffectiveDate"/>
            <endDateReference   hRef="RLegEffectiveDate"/>
            <valorisation>NotionalAmount</valorisation>
            <unitType>[Currency]</unitType>
            <unit>NotionalCurrency</unit>
            <payerReference     hRef="RLegReceiverPartyReference" />
            <receiverReference  hRef="RLegPayerPartyReference" />
          </xsl:when>
        </xsl:choose>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <xsl:template name="ReturnLegInitialNotionalBase">
    <amount>
      <keyGroup name="InitialAmountBase" key="STABCU" />
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|BaseCurrency</eventType>
        </key>

        <startDate>EventDateNotionalBase(RLegEffectiveDate)</startDate>
        <endDate>EventDateNotionalBase(RLegEffectiveDate)</endDate>
        <valorisation>NotionalBaseAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>NotionalBaseCurrency</unit>
        <payerReference     hRef="RLegPayerPartyReference" />
        <receiverReference  hRef="RLegReceiverPartyReference" />

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ReturnLegValuation -->
  <xsl:template name="ReturnLegValuation">
    <valuation>
      <keyGroup name="ReturnLegValuation" key="VAL"/>
      <itemGroup>
        <occurence to="Unique">efs_RateOfReturn</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegTerminationDate"/>

        <item_Details>
          <assetReference hRef="RLegAsset"/>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <!-- Initial Margin -->
      <item>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|InitialMargin</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegTerminationDate"/>

        <!-- EG 20141029 IMPayerPartyReference/IMReceiverPartyReference replace RLPayerPartyReference/RLReceiverReference -->
        <payerReference hRef="IMPayerPartyReference"/>
        <receiverReference hRef="IMReceiverPartyReference"/>

        <valorisation>InitialMarginAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>InitialMarginCurrency</unit>

        <!-- EG 20141029 GroupLevel -> Recognition -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </item>

      <!-- ValuationAmount : ReturnSwapAmount|MarginRequirementAmount -->
      <xsl:call-template name="ReturnLegValuationAmount">
        <xsl:with-param name="pAmountType" select="'ReturnSwapAmount'"/>
      </xsl:call-template>

      <xsl:call-template name="ReturnLegValuationAmount">
        <xsl:with-param name="pAmountType" select="'MarginRequirement'"/>
      </xsl:call-template>


    </valuation>
  </xsl:template>

  <!-- ReturnLegValuationAmount -->
  <xsl:template name="ReturnLegValuationAmount">
    <xsl:param name="pAmountType" />

    <amount>
      <keyGroup name="ValuationAmount" key="VALAMT" />
      <itemGroup>
        <!--<occurence to="All" field="valuationPeriods">efs_RateOfReturn</occurence>-->
        <occurence to="All" >valuationPeriods</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pAmountType"/></eventType>
        </key>

        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
        </subItem>
        <!--<subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>-->
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ReturnLegInitialBasketComponent -->
  <xsl:template name="ReturnLegInitialBasketComponent">
    <basketComponent>
      <keyGroup name="InitialSecBasketComponent" key="BAS"/>
      <itemGroup>
        <occurence to="All">underlyerComponent</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Basket</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegEffectiveDate"/>

        <payerReference     hRef="RLegPayerPartyReference" />
        <receiverReference  hRef="RLegReceiverPartyReference" />

        <valorisation>UnitValue</valorisation>
        <unitType>UnitType</unitType>
        <unit>Unit</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>
    </basketComponent>
  </xsl:template>


  <!-- InterestLeg -->
  <xsl:template name="InterestLeg">
    <xsl:param name="pLeg"/>

    <keyGroup name="Leg" key="ILEG"/>
    <itemGroup>
      <occurence to="All">interestLeg</occurence>
      <parameter id="ILegEffectiveDate">EffectiveDate</parameter>
      <parameter id="ILegAdjustedEffectiveDate">AdjustedEffectiveDate</parameter>
      <parameter id="ILegAdjustedTerminationDate">AdjustedTerminationDate</parameter>
      <parameter id="ILegTerminationDate">TerminationDate</parameter>
      <parameter id="ILegPayerPartyReference">PayerPartyReference</parameter>
      <parameter id="ILegReceiverPartyReference">ReceiverPartyReference</parameter>
      <parameter id="Rate">Rate('None')</parameter>
      <parameter id="RateInitialStub">Rate('Initial')</parameter>
      <parameter id="RateFinalStub">Rate('Final')</parameter>
      <parameter id="DCF">DayCountFraction</parameter>

      <key>
        <eventCode>LegEventCode</eventCode>
        <eventType>LegEventType</eventType>
      </key>
      <idStCalcul>[CALC]</idStCalcul>
      <startDateReference hRef="ILegEffectiveDate"/>
      <endDateReference hRef="ILegTerminationDate"/>
      <!--<payerReference hRef="ILegPayerPartyReference" />
      <receiverReference hRef="ILegReceiverPartyReference" />-->

      <item_Details>
        <asset>Asset</asset>
      </item_Details>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDate>AdjustedEffectiveDate</eventDate>
      </subItem>
    </itemGroup>

    <!-- InterestLegPaymentDates FDA (NOS/NOM|RES/FLO) -->
    <xsl:call-template name="InterestLegPaymentDates"/>

  </xsl:template>


  <!-- InterestLegPaymentDates -->
  <xsl:template name="InterestLegPaymentDates">
    <paymentDates>
      <keyGroup name="PaymentDates" key="PAY"/>
      <itemGroup>
        <occurence to="All" field="paymentPeriods">efs_InterestLeg</occurence>
        <parameter id="StartPeriod">StartPeriod</parameter>
        <parameter id="EndPeriod">EndPeriod</parameter>
        <parameter id="AdjustedStartPeriod">AdjustedStartPeriod</parameter>
        <parameter id="Stub">Stub</parameter>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|FundingAmount</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <payerReference     hRef="ILegPayerPartyReference" />
        <receiverReference  hRef="ILegReceiverPartyReference" />

        <item_Details>
          <fixedRate>Rate('FixedRate',Rate,RateInitialStub,RateFinalStub)</fixedRate>
          <asset>AssetRateIndex('FloatingRate',Rate,RateInitialStub,RateFinalStub)</asset>
          <dayCountFraction>DayCountFraction(DCF)</dayCountFraction>
          <spread>Spread</spread>
          <multiplier>Multiplier</multiplier>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>

        <!-- EG 20150616 [XXXXX] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>

        <!-- FI 20141215 [20570] add STL--> 
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
          <isPayment>1</isPayment>
        </subItem>

      </itemGroup>

      <!-- InterestLegReturnSwapAmountStep NOS/NOM -->
      <!--<xsl:call-template name="InterestLegNominal"/>-->
      <!-- InterestLegResetDates RES/FLO -->
      <xsl:call-template name="InterestLegResetDates"/>

    </paymentDates>
  </xsl:template>

  <!-- InterestLegNominal -->
  <xsl:template name="InterestLegNominal">
    <returnSwapAmountStep>
      <keyGroup name="InterestLegNominal" key="NOS"/>
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <idStCalcul>[CALCREV]</idStCalcul>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <unitType>[Currency]</unitType>
        <unit>NotionalCurrency</unit>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Date</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>
      </itemGroup>
    </returnSwapAmountStep>
  </xsl:template>

  <!-- InterestLegResetDates -->
  <xsl:template name="InterestLegResetDates">
    <resetDates>
      <keyGroup name="InterestLegResetDates" key="RES"/>
      <itemGroup>
        <occurence to="All">resetDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <item_Details>
          <asset>AssetRateIndex(Stub,'FloatingRate',Rate,RateInitialStub,RateFinalStub)</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDate>AdjustedResetDate</eventDate>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
          <eventDate>AdjustedFixingDate</eventDate>
        </subItem>
      </itemGroup>
    </resetDates>
  </xsl:template>

  <!-- PrincipalExchangeFeatures -->
  <xsl:template name="PrincipalExchangeFeatures">
    <principalExchange>
      <keyGroup name="PrincipalExchangeFeatures" key="PEF"/>
      <itemGroup>
        <occurence to="Unique">underlyerComponent</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDate>XXX</startDate>
        <endDate>XXX</endDate>
        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <valorisation>ExchangeAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>ExchangeCurrency</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>UnadjustedPaymentDate</eventDate>
        </subItem>

        <!-- EG 20150616 [XXXXX] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDate>AdjustedPreSettlementDate</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>

      </itemGroup>
    </principalExchange>
  </xsl:template>
</xsl:stylesheet>