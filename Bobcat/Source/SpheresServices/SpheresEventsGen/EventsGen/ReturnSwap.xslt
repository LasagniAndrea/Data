<?xml version="1.0" encoding="UTF-8" ?>
<!--EG 20231127 [WI755] New-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='ReturnSwap']">
    <xsl:param name="pPosition" select="0"/>
    <returnSwap>

      <!-- Product = 
             EVENTCODE = Product (PRD)
             EVENTYPE  = Data (DAT)
      -->
      <keyGroup name="ReturnSwap" key="PRD"/>
      <parameter id="RTS_ClearingBusinessDate">ClearingBusinessDate</parameter>
      <parameter id="IMPayerPartyReference">InitialMarginPayerPartyReference</parameter>
      <parameter id="IMReceiverPartyReference">InitialMarginReceiverPartyReference</parameter>
      <parameter id="IsFungible">IsFungible</parameter>
      <parameter id="IsMargining">IsMargining</parameter>
      <parameter id="IsFunding">IsFunding</parameter>

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

  <!-- ================ -->
  <!--   RETURN  LEG    -->
  <!-- ================ -->

  <!-- ReturnLeg = Jambe du rendement de l'actif
       EVENTCODE = Price Return Leg (PRL)|Total Return Leg (TRL)|Dividend Return Leg (DRL) 
       EVENTYPE  = Term (TRM) | Open (OPN)
  -->
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
      <parameter id="RLegIsMarginRatio">IsMarginRatio</parameter>
      <parameter id="RLegIsNotionalReset">IsNotionalReset</parameter>
      <parameter id="RLegIsNotNotionalReset">IsNotNotionalReset</parameter>
      <parameter id="RLegPaymentCurrency">PaymentCurrency</parameter>
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

    <xsl:call-template name="ReturnLegPaymentDates"/>

    <xsl:call-template name="ReturnLegValuation" />

  </xsl:template>

  <!-- ====================== -->
  <!--  RETURN LEG UNDERLYER  -->
  <!-- ====================== -->

  <!-- ReturnLegUnderlyer : Actif du rendement
    EVENTCODE = Single Underlyer (SUL) | Basket (BSK)
    EVENTTYPE = EquityAsset (SHR) | Index (IND) | Basket (BSK)
  -->
  <xsl:template name="ReturnLegUnderlyer">
    <underlyer>
      <keyGroup name="ReturnLegUnderlyer" key="RLU"/>
      <itemGroup>
        <occurence to="Unique">efs_ReturnLeg</occurence>
        <parameter id="RLegIsNotionalReset">IsNotionalReset</parameter>
        <parameter id="RLegIsNotNotionalReset">IsNotNotionalReset</parameter>
        <parameter id="RLegInitialNotionalAmount">InitialNotionalAmount</parameter>
        <parameter id="RLegInitialNotionalCurrency">InitialNotionalCurrency</parameter>
        <parameter id="RLegIsMarginRatio">IsMarginRatio</parameter>
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

      <xsl:call-template name="ReturnLegInitialAmount">
        <xsl:with-param name="pAmountType" select="'Notional'"/>
      </xsl:call-template>

      <xsl:call-template name="ReturnLegInitialAmount">
        <xsl:with-param name="pAmountType" select="'Quantity'"/>
      </xsl:call-template>

      <xsl:call-template name="ReturnLegNotionalStep"/>
      <xsl:call-template name="ReturnLegResetNotionalStep"/>

      <xsl:call-template name="ReturnLegMarginRatioAmount" />
    </underlyer>
  </xsl:template>

  <!-- ReturnLegInitialAmount : Notionel initial
    EVENTCODE = STA
    EVENTTYPE = NOM|QTY
  -->
  <xsl:template name="ReturnLegInitialAmount">
    <xsl:param name="pAmountType" />

    <amount>
      <keyGroup name="InitialAmount" key="STAAMT" />
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EventTypeInitialAmount('<xsl:value-of select="$pAmountType" />')</eventType>
        </key>

        <xsl:choose>
          <xsl:when test = "$pAmountType = 'Quantity'">
            <startDateReference hRef="RLegEffectiveDate"/>
            <endDateReference   hRef="RLegTerminationDate"/>
            <valorisation>OpenUnits</valorisation>
            <unitType>UnitType</unitType>
            <unit>Unit</unit>
            <payerReference     hRef="RLegPayerPartyReference" />
            <receiverReference  hRef="RLegReceiverPartyReference" />
          </xsl:when>
          <xsl:when test = "$pAmountType = 'Notional'">
            <startDateReference hRef="RLegEffectiveDate"/>
            <endDateReference   hRef="RLegTerminationDate"/>
            <valorisationReference   hRef="RLegInitialNotionalAmount"/>
            <unitType>[Currency]</unitType>
            <unitReference hRef="RLegInitialNotionalCurrency"/>
            <payerReference hRef="RLegReceiverPartyReference" />
            <receiverReference hRef="RLegPayerPartyReference" />
          </xsl:when>
        </xsl:choose>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ReturnLegNotionalStep : Notionel Step (No reset)
    EVENTCODE = NOS
    EVENTTYPE = NOM
  -->
  <xsl:template name="ReturnLegNotionalStep">

    <amount>
      <keyGroup name="ReturnLegNotional" key="NOSNOM" />
      <itemGroup>
        <conditionalReference hRef="RLegIsNotNotionalReset"/>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>

        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference hRef="RLegTerminationDate"/>
        <valorisationReference hRef="RLegInitialNotionalAmount"/>
        <unitType>[Currency]</unitType>
        <unitReference hRef="RLegInitialNotionalCurrency"/>
        <payerReference     hRef="RLegReceiverPartyReference" />
        <receiverReference  hRef="RLegPayerPartyReference" />

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Date</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ReturnLegResetNotionalStep : Notionel Step  (si Reset)
    EVENTCODE = NOS
    EVENTTYPE = NOM
  -->
  <xsl:template name="ReturnLegResetNotionalStep">
    <amount>
      <keyGroup name="ReturnLegResetNotional" key="NOSNOMREST" />
      <itemGroup>
        <occurence to="All">notionalPeriods</occurence>
        <conditionalReference hRef="RLegIsNotionalReset"/>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>

        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <valorisationReference hRef="RLegInitialNotionalAmount"/>
        <unitType>[Currency]</unitType>
        <unitReference hRef="RLegInitialNotionalCurrency"/>
        <payerReference hRef="RLegReceiverPartyReference" />
        <receiverReference hRef="RLegPayerPartyReference" />

        <unitType>[Currency]</unitType>
        <unitReference hRef="RLegPaymentCurrency"/>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Date</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>


  <!-- ReturnLegMarginRatioAmount : Margin Ration Factor (sur CFD)
    EVENTCODE = LPP
    EVENTTYPE = MarGin ratio Factor (MGF)
  -->
  <xsl:template name="ReturnLegMarginRatioAmount">
    <amount>
      <keyGroup name="MarginRatioAmount" key="STAAMT" />
      <itemGroup>
        <conditionalReference hRef="RLegIsMarginRatio"/>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|MarginRequirementRatio</eventType>
        </key>

        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegTerminationDate"/>
        <valorisation>MarginRatioAmount</valorisation>
        <unitType>MarginRatioUnitType</unitType>
        <unit>MarginRatioUnit</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ====================== -->
  <!--  RETURN LEG VALUATION  -->
  <!-- ====================== -->

  <!-- ReturnLegValuation : Montant de résultats (si instrument fongible) 
    EVENTCODE = LPC
    EVENTTYPE = AMT
    +
    Ininital Margin (Si margin Ratio - CFD)
    Margin Requirement Amount (Si instrument IsMargining)
  -->
  <xsl:template name="ReturnLegValuation">
    <valuation>
      <keyGroup name="ReturnLegValuation" key="VAL"/>
      <itemGroup>
        <occurence to="Unique">efs_RateOfReturn</occurence>
        <conditionalReference hRef="IsFungible"/>
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

      <!-- InitialMarginAmount : (Si Margin Ratio de spécifié) -->
      <xsl:call-template name="ReturnLegInitialMarginAmount"/>

      <!-- ValuationAmount : MarginRequirementAmount (Si Margining) -->
      <xsl:call-template name="ReturnLegMarginRequirementAmount" />

    </valuation>
  </xsl:template>

  <!-- ReturnLegInitialMarginAmount (Si Margin Ratio de spécifié - CFD)
    EVENTCODE = LPP
    EVENTTYPE = Initial Margin Amount (IMG)
  -->
  <xsl:template name="ReturnLegInitialMarginAmount">
    <amount>
      <keyGroup name="InitialMarginAmount" key="VALIMG" />
      <itemGroup>
        <occurence to="unique" isOptional="true">initialMargin</occurence>
        <conditionalReference hRef="IsMargining"/>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|InitialMargin</eventType>
        </key>

        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegTerminationDate"/>

        <payerReference hRef="IMPayerPartyReference"/>
        <receiverReference hRef="IMReceiverPartyReference"/>

        <valorisation>amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>currency</unit>

        <!-- EG 20141029 GroupLevel -> Recognition -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ReturnLegMarginRequirementAmount (Si instrument IsMargining)
    EVENTCODE = LPC
    EVENTTYPE = Margin Requirement Amount (MRA)
  -->
  <xsl:template name="ReturnLegMarginRequirementAmount">

    <amount>
      <keyGroup name="ValuationAmount" key="MGRAMT" />
      <itemGroup>
        <occurence to="All" >valuationPeriods</occurence>
        <conditionalReference hRef="IsMargining"/>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|MarginRequirementAmount></eventType>
        </key>

        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- ReturnLegInitialBasketComponent (Composant si Rendement sur Basket : UNUSED)
    EVENTCODE = STA
    EVENTTYPE = Basket (BSK)
  -->
  <xsl:template name="ReturnLegInitialBasketComponent">
    <basketComponent>
      <keyGroup name="InitialSecBasketComponent" key="BSK"/>
      <itemGroup>
        <occurence to="All">underlyerComponent</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Basket</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference   hRef="RLegTerminationDate"/>

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


  <!-- =========================== -->
  <!--  RETURN LEG PAYMENT AMOUNT  -->
  <!-- =========================== -->


  <!-- ReturnLegPaymentDates : Cash flows de la jambe Return Leg (Rendement)
    EVENTCODE = Return Leg Amount (RLA)
    EVENTTYPE = Amounts (AMT)
  -->
  <xsl:template name="ReturnLegPaymentDates">
    <paymentDates>
      <keyGroup name="ReturnLegPaymentDates" key="RLPAY" />
      <itemGroup>
        <occurence to="Unique">efs_RateOfReturn</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>

        <startDateReference hRef="RLegEffectiveDate"/>
        <endDateReference hRef="RLegTerminationDate"/>
        <payerReference hRef="RLegPayerPartyReference" />
        <receiverReference hRef="RLegReceiverPartyReference" />

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDate>AdjustedEffectiveDate</eventDate>
        </subItem>
      </itemGroup>

      <xsl:call-template name="ReturnLegPaymentAmount">
        <xsl:with-param name="pEventCode" select="'Intermediary'"/>
      </xsl:call-template>
      <xsl:call-template name="ReturnLegPaymentAmount">
        <xsl:with-param name="pEventCode" select="'Termination'"/>
      </xsl:call-template>

    </paymentDates>
  </xsl:template>

  <!-- ReturnLegPaymentAmount : Cash flows de la jambe Return Leg (Rendement)
    EVENTCODE = INT|TER
    EVENTTYPE = Return Swap Amount (RSA)
  -->
  <xsl:template name="ReturnLegPaymentAmount">
    <xsl:param name="pEventCode"/>
    <amount>
      <xsl:variable name="occurence">
        <xsl:choose>
          <xsl:when test = "$pEventCode = 'Intermediary'">AllExceptLast</xsl:when>
          <xsl:when test = "$pEventCode = 'Termination'">Last</xsl:when>
          <xsl:otherwise>None</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <keyGroup name="ReturnLegPaymentAmount" key="RSAAMT" />
      <itemGroup>
        <occurence to="{$occurence}">valuationPeriods</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|ReturnLegAmount</eventType>
        </key>

        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <payerReference hRef="RLegPayerPartyReference" />
        <receiverReference hRef="RLegReceiverPartyReference" />

        <unitType>[Currency]</unitType>
        <unitReference hRef="RLegPaymentCurrency"/>

        <item_Details>
          <assetReference hRef="RLegAsset"/>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
          <isPayment>1</isPayment>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
        </subItem>

      </itemGroup>

    </amount>
  </xsl:template>

  <!-- ================ -->
  <!--  INTEREST LEG    -->
  <!-- ================ -->
  <!-- InterestLeg = Jambe des interêts
       EVENTCODE = INL
       EVENTYPE  = FIX|FLO|xxx
  -->
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
      <parameter id="ILegPaymentCurrency">PaymentCurrency</parameter>
      <key>
        <eventCode>LegEventCode</eventCode>
        <eventType>LegEventType</eventType>
      </key>
      <idStCalcul>[CALC]</idStCalcul>
      <startDateReference hRef="ILegEffectiveDate"/>
      <endDateReference hRef="ILegTerminationDate"/>
      <payerReference hRef="ILegPayerPartyReference" />
      <receiverReference hRef="ILegReceiverPartyReference" />

      <item_Details>
        <asset>Asset</asset>
      </item_Details>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDate>AdjustedEffectiveDate</eventDate>
      </subItem>
    </itemGroup>

    <!-- InterestLegPaymentDates Cash Flows (INT|TER/INT + RES/FLO) -->
    <xsl:call-template name="InterestLegPaymentDates">
      <xsl:with-param name="pEventCode" select="'Intermediary'"/>
    </xsl:call-template>
    <xsl:call-template name="InterestLegPaymentDates">
      <xsl:with-param name="pEventCode" select="'Termination'"/>
    </xsl:call-template>
    <!-- InterestLegPaymentDates Funding amount (LPC/FDA + RES/FLO) -->
    <xsl:call-template name="InterestLegFundingAmount"/>


  </xsl:template>

  <!-- InterestLegPaymentDates : Cash flows de la jambe Interest Leg 
    EVENTCODE = INT|TER
    EVENTTYPE = INT
  -->
  <xsl:template name="InterestLegPaymentDates">
    <xsl:param name="pEventCode"/>

    <xsl:variable name="occurence">
      <xsl:choose>
        <xsl:when test = "$pEventCode = 'Intermediary'">AllExceptLast</xsl:when>
        <xsl:when test = "$pEventCode = 'Termination'">Last</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <paymentDates>
      <keyGroup name="InterestLegPaymentAmount" key="ILPAY"/>
      <itemGroup>
        <occurence to="{$occurence}" field="paymentPeriods">efs_InterestLeg</occurence>
        <parameter id="StartPeriod">StartPeriod</parameter>
        <parameter id="EndPeriod">EndPeriod</parameter>
        <parameter id="AdjustedStartPeriod">AdjustedStartPeriod</parameter>
        <parameter id="Stub">Stub</parameter>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Interest</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <payerReference hRef="ILegPayerPartyReference" />
        <receiverReference hRef="ILegReceiverPartyReference" />

        <valorisation>PeriodAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unitReference hRef="ILegPaymentCurrency"/>

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

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedEndPeriod</eventDate>
          <isPayment>1</isPayment>
        </subItem>

      </itemGroup>

      <xsl:call-template name="InterestLegResetDates"/>

    </paymentDates>
  </xsl:template>

  <!-- InterestLegFundingAmount (Si instrument IsFunding - CFD)
    EVENTCODE = LPP
    EVENTTYPE = Funding amount (FDA)| Borrowing Amount (BWA)
  -->
  <xsl:template name="InterestLegFundingAmount">
    <paymentDates>
      <keyGroup name="PaymentDates" key="FDA"/>
      <itemGroup>
        <occurence to="All" field="paymentPeriods">efs_InterestLeg</occurence>
        <conditionalReference hRef="IsFunding"/>
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

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
          <isPayment>1</isPayment>
        </subItem>

      </itemGroup>

      <xsl:call-template name="InterestLegFundingResetDates"/>
    </paymentDates>
  </xsl:template>

  <!-- InterestLegResetDates : Reset date sur Periodes avec Taux flottant
    EVENTCODE = RES
    EVENTTYPE = FLO
  -->
  <xsl:template name="InterestLegResetDates">
    <xsl:param name="pCondition"/>
    <resetDates>
      <keyGroup name="InterestLegResetDates" key="RES"/>
      <itemGroup>
        <occurence to="All">resetDates</occurence>
        <conditional>
          <xsl:value-of select="$pCondition"/>
        </conditional>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <item_Details>
          <fixedRate>Rate(Stub,'FixedRate',Rate,RateInitialStub,RateFinalStub)</fixedRate>
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
  <xsl:template name="InterestLegFundingResetDates">
    <xsl:param name="pCondition"/>
    <resetDates>
      <keyGroup name="InterestLegResetDates" key="RES"/>
      <itemGroup>
        <occurence to="All">resetDates</occurence>
        <conditionalReference hRef="IsFunding"/>
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

</xsl:stylesheet>