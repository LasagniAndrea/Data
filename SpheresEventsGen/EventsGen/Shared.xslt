<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <!-- Stream -->
  <xsl:template name="Stream">
    <xsl:param name="pProduct"/>

    <xsl:variable name="stream">
      <xsl:choose>
        <xsl:when test = "$pProduct = 'CapFloor'">capFloorStream</xsl:when>
        <xsl:when test = "$pProduct = 'LoanDeposit'">loanDepositStream</xsl:when>
        <xsl:when test = "$pProduct = 'Swap'">swapStream</xsl:when>
        <xsl:when test = "$pProduct = 'Repo'">cashStream</xsl:when>
        <xsl:when test = "$pProduct = 'BuyAndSellBack'">cashStream</xsl:when>
        <xsl:when test = "$pProduct = 'CashBalanceInterest'">cashBalanceInterestStream</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="eventCode">
      <xsl:choose>
        <xsl:when test = "$pProduct = 'CapFloor'">CapFloor</xsl:when>
        <xsl:when test = "$pProduct = 'LoanDeposit'">LoanDeposit</xsl:when>
        <xsl:when test = "$pProduct = 'Swap'">InterestRateSwap</xsl:when>
        <xsl:when test = "$pProduct = 'Repo'">LoanDeposit</xsl:when>
        <xsl:when test = "$pProduct = 'BuyAndSellBack'">LoanDeposit</xsl:when>
        <xsl:when test = "$pProduct = 'CashBalanceInterest'">CashBalanceInterest</xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- [WI828] AssetDebtSecurity: La génération des évènements est en erreur -->
    <keyGroup name="Stream" key="STD"/>
    <itemGroup>
      <occurence to="All">
        <xsl:value-of select="$stream"/>
      </occurence>
      <parameter id="Payer">GetPayerPartyReference</parameter>
      <parameter id="Receiver">GetReceiverPartyReference</parameter>
      <parameter id="CalculationPeriodAmount">CalculationPeriodAmount</parameter>
      <parameter id="Rate">Rate('None')</parameter>
      <parameter id="RateInitialStub">Rate('Initial')</parameter>
      <parameter id="RateFinalStub">Rate('Final')</parameter>
      <parameter id="DCF">DayCountFraction</parameter>
      <parameter id="IsInitialExchange">IsInitialExchange</parameter>
      <parameter id="IsIntermediateExchange">IsIntermediateExchange</parameter>
      <parameter id="IsFinalExchange">IsFinalExchange</parameter>
      <parameter id="StreamEffectiveDate">EffectiveDate</parameter>
      <parameter id="StreamTerminationDate">TerminationDate</parameter>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$eventCode" /></eventCode>
        <eventType>EventType('<xsl:value-of select="$pProduct" />')</eventType>
      </key>
      <idStCalcul>[CALC]</idStCalcul>
      <startDateReference hRef="StreamEffectiveDate"/>
      <endDateReference hRef="StreamTerminationDate"/>
      <payerReference hRef="Payer"/>
      <receiverReference hRef="Receiver"/>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDate>AdjustedEffectiveDate</eventDate>
      </subItem>
    </itemGroup>

    <!-- NominalPeriodsVariation -->
    <xsl:call-template name="NominalPeriodsVariation">
      <xsl:with-param name="pEventCode" select="'Start'"/>
    </xsl:call-template>
    <xsl:call-template name="NominalPeriodsVariation">
      <xsl:with-param name="pEventCode" select="'Intermediary'"/>
    </xsl:call-template>
    <xsl:call-template name="NominalPeriodsVariation">
      <xsl:with-param name="pEventCode" select="'Termination'"/>
    </xsl:call-template>
    <xsl:call-template name="FxLinkedNominalPeriodsVariation">
      <xsl:with-param name="pEventCode" select="'Start'"/>
    </xsl:call-template>
    <xsl:call-template name="FxLinkedNominalPeriodsVariation">
      <xsl:with-param name="pEventCode" select="'Intermediary'"/>
    </xsl:call-template>
    <xsl:call-template name="FxLinkedNominalPeriodsVariation">
      <xsl:with-param name="pEventCode" select="'Termination'"/>
    </xsl:call-template>

    <!-- NominalPeriods -->
    <xsl:call-template name="NominalPeriods">
      <xsl:with-param name="pEventCode" select="'NominalStep'"/>
    </xsl:call-template>
    <xsl:call-template name="FxLinkedNominalPeriods">
      <xsl:with-param name="pEventCode" select="'NominalStep'"/>
    </xsl:call-template>

    <!-- PaymentDates -->
    <xsl:call-template name="PaymentDates">
      <xsl:with-param name="pEventCode" select="'Intermediary'"/>
    </xsl:call-template>
    <xsl:call-template name="PaymentDates">
      <xsl:with-param name="pEventCode" select="'Termination'"/>
    </xsl:call-template>
  </xsl:template>

  <!-- NominalPeriodsVariation -->
  <xsl:template name="NominalPeriodsVariation">
    <xsl:param name="pEventCode"/>

    <xsl:variable name="occurence">
      <xsl:choose>
        <xsl:when test = "$pEventCode = 'Start'">First</xsl:when>
        <xsl:when test = "$pEventCode = 'Intermediary'">AllExceptFirstAndLast</xsl:when>
        <xsl:when test = "$pEventCode = 'Termination'">Last</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <nominalPeriodsVariation>
      <keyGroup name="NominalPeriodsVariation" key="NOM"/>
      <itemGroup>
        <occurence to="{$occurence}" field="nominalPeriods">efs_CalculationPeriodDates</occurence>

        <!--<xsl:if test = "$pEventCode != 'Termination'"><conditionalReference hRef="IsNotDiscount"/></xsl:if>-->

        <parameter id="Variation">VariationAmount</parameter>
        <key>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Start'">
              <eventCode>GetEventCodeStart(StreamEffectiveDate,MinEffectiveDate)</eventCode>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Intermediary'">
              <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Termination'">
              <eventCode>GetEventCodeTermination(StreamTerminationDate,MaxTerminationDate)</eventCode>
            </xsl:when>
          </xsl:choose>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <idStCalcul>[CALCREV]</idStCalcul>
        <xsl:choose>
          <xsl:when test = "$pEventCode = 'Termination'">
            <startDate>EndPeriod</startDate>
            <endDate>EndPeriod</endDate>
          </xsl:when>
          <xsl:otherwise>
            <startDate>StartPeriod</startDate>
            <endDate>StartPeriod</endDate>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test = "$pEventCode = 'Start'">
          <payerReference hRef="Receiver"/>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Intermediary'">
          <payer>NominalPartyReference('Payer',Payer,Receiver,Variation)</payer>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Termination'">
          <payerReference hRef="Payer"/>
        </xsl:if>

        <xsl:if test = "$pEventCode = 'Start'">
          <receiverReference hRef="Payer"/>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Intermediary'">
          <receiver>NominalPartyReference('Receiver',Payer,Receiver,Variation)</receiver>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Termination'">
          <receiverReference hRef="Receiver"/>
        </xsl:if>

        <valorisation>AbsoluteVariationAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>PeriodCurrency</unit>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Start'">
              <eventDate>GetDateRecognition(AdjustedTradeDate,StreamEffectiveDate,MinEffectiveDate)</eventDate>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Termination'">
              <!--<eventDate>UnadjustedEndPeriod</eventDate>-->
              <eventDate>DateStepTerminationRecognition</eventDate>
            </xsl:when>
            <xsl:otherwise>
              <!--<eventDate>UnadjustedStartPeriod</eventDate>-->
              <eventDate>DateStepStartRecognition</eventDate>
            </xsl:otherwise>
          </xsl:choose>
        </subItem>
        <!-- 20070823 EG Ticket : 15643 -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Termination'">
              <eventDate>AdjustedPreSettlementEndDate</eventDate>
            </xsl:when>
            <xsl:otherwise>
              <eventDate>AdjustedPreSettlementStartDate</eventDate>
            </xsl:otherwise>
          </xsl:choose>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>
            <xsl:choose>
              <xsl:when test = "$pEventCode = 'Termination'">AdjustedEndPeriod</xsl:when>
              <xsl:otherwise>AdjustedStartPeriod</xsl:otherwise>
            </xsl:choose>
          </eventDate>
          <xsl:if test = "$pEventCode = 'Start'">
            <isPaymentReference hRef="IsInitialExchange"/>
          </xsl:if>
          <xsl:if test = "$pEventCode = 'Intermediary'">
            <isPaymentReference hRef="IsIntermediateExchange"/>
          </xsl:if>
          <xsl:if test = "$pEventCode = 'Termination'">
            <isPaymentReference hRef="IsFinalExchange"/>
          </xsl:if>
        </subItem>
      </itemGroup>
    </nominalPeriodsVariation>
  </xsl:template>


  <!-- NominalPeriods -->
  <xsl:template name="NominalPeriods">
    <nominalPeriods>
      <keyGroup name="NominalPeriods" key="NOS"/>
      <itemGroup>
        <occurence to="AllExceptLast" field="nominalPeriods">efs_CalculationPeriodDates</occurence>
        <!--<conditionalReference hRef="IsNotDiscount"/>-->
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <idStCalcul>[CALCREV]</idStCalcul>
        <startDate>FirstStartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <valorisation>PeriodAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>PeriodCurrency</unit>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Date</eventClass>
          <eventDate>AdjustedFirstStartPeriod</eventDate>
        </subItem>
      </itemGroup>
    </nominalPeriods>
  </xsl:template>


  <!-- FxLinkedNominalPeriodsVariation -->
  <xsl:template name="FxLinkedNominalPeriodsVariation">
    <xsl:param name="pEventCode"/>

    <xsl:variable name="occurence">
      <xsl:choose>
        <xsl:when test = "$pEventCode = 'Start'">First</xsl:when>
        <xsl:when test = "$pEventCode = 'Intermediary'">AllExceptFirst</xsl:when>
        <xsl:when test = "$pEventCode = 'Termination'">Last</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <nominalPeriodsVariation>
      <keyGroup name="NominalPeriodsVariation" key="NOM"/>
      <itemGroup>
        <occurence to="{$occurence}" field="fxLinkedNominalPeriods">efs_CalculationPeriodDates</occurence>
        <parameter id="FxLinkedVariation">VariationAmount</parameter>
        <key>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Start'">
              <eventCode>GetEventCodeStart(StreamEffectiveDate,MinEffectiveDate)</eventCode>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Intermediary'">
              <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Termination'">
              <eventCode>GetEventCodeTermination(StreamTerminationDate,MaxTerminationDate)</eventCode>
            </xsl:when>
          </xsl:choose>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <xsl:choose>
          <xsl:when test = "$pEventCode = 'Termination'">
            <startDate>EndPeriod</startDate>
            <endDate>EndPeriod</endDate>
          </xsl:when>
          <xsl:otherwise>
            <startDate>StartPeriod</startDate>
            <endDate>StartPeriod</endDate>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test = "$pEventCode = 'Start'">
          <payerReference hRef="Receiver"/>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Intermediary'">
          <payer>NominalPartyReference('Payer',Payer,Receiver,FxLinkedVariation)</payer>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Termination'">
          <payerReference hRef="Payer"/>
        </xsl:if>

        <xsl:if test = "$pEventCode = 'Start'">
          <receiverReference hRef="Payer"/>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Intermediary'">
          <receiver>NominalPartyReference('Receiver',Payer,Receiver,FxLinkedVariation)</receiver>
        </xsl:if>
        <xsl:if test = "$pEventCode = 'Termination'">
          <receiverReference hRef="Receiver"/>
        </xsl:if>

        <valorisation>AbsoluteVariationAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>PeriodCurrency</unit>

        <xsl:if test = "$pEventCode != 'Termination'">
          <item_Details>
            <asset>AssetFxRate</asset>
            <fixingRate>FixingRate</fixingRate>
          </item_Details>
        </xsl:if>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Start'">
              <eventDate>GetDateRecognition(AdjustedTradeDate,StreamEffectiveDate,MinEffectiveDate)</eventDate>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Termination'">
              <eventDate>UnadjustedEndPeriod</eventDate>
            </xsl:when>
            <xsl:otherwise>
              <eventDate>UnadjustedStartPeriod</eventDate>
            </xsl:otherwise>
          </xsl:choose>
        </subItem>
        <!-- 20070823 EG Ticket : 15643 -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Termination'">
              <eventDate>AdjustedPreSettlementEndDate</eventDate>
            </xsl:when>
            <xsl:otherwise>
              <eventDate>AdjustedPreSettlementPaymentDate</eventDate>
            </xsl:otherwise>
          </xsl:choose>

        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>
            <xsl:choose>
              <xsl:when test = "$pEventCode = 'Termination'">AdjustedEndPeriod</xsl:when>
              <xsl:otherwise>AdjustedPaymentDate</xsl:otherwise>
            </xsl:choose>
          </eventDate>

          <xsl:if test = "$pEventCode = 'Start'">
            <isPaymentReference hRef="IsInitialExchange"/>
          </xsl:if>
          <xsl:if test = "$pEventCode = 'Intermediary'">
            <isPaymentReference hRef="IsIntermediateExchange"/>
          </xsl:if>
          <xsl:if test = "$pEventCode = 'Termination'">
            <isPaymentReference hRef="IsFinalExchange"/>
          </xsl:if>
        </subItem>
        <xsl:if test = "$pEventCode != 'Termination'">
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
            <eventDate>AdjustedFixingDate</eventDate>
          </subItem>
        </xsl:if>
      </itemGroup>
    </nominalPeriodsVariation>
  </xsl:template>

  <!-- FxLinkedNominalPeriods -->
  <xsl:template name="FxLinkedNominalPeriods">
    <nominalPeriods>
      <keyGroup name="FxLinkedNominalPeriods" key="NOS"/>
      <itemGroup>
        <occurence to="All" field="fxLinkedNominalPeriods">efs_CalculationPeriodDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|NominalStep</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <valorisation>PeriodAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>PeriodCurrency</unit>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Date</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>
      </itemGroup>
    </nominalPeriods>
  </xsl:template>

  <!-- PaymentDates -->
  <!--EG 20190823 [FIXEDINCOME] Use Parameter IsPerpetual-->
  <xsl:template name="PaymentDates">
    <xsl:param name="pEventCode"/>

    <xsl:variable name="occurence">
      <xsl:choose>
        <xsl:when test = "$pEventCode = 'Intermediary'">AllExceptLast</xsl:when>
        <xsl:when test = "$pEventCode = 'Termination'">Last</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <paymentDates>
      <keyGroup name="PaymentDates" key="PAY"/>
      <itemGroup>
        <occurence to="{$occurence}" field="paymentDates">efs_PaymentDates</occurence>
        <parameter id="{$occurence}PaymentDatesCurrency">Currency</parameter>
        <conditionalReference hRef="IsNotDiscount"/>
        <key>
          <xsl:choose>
            <xsl:when test = "$pEventCode = 'Intermediary'">
              <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
            </xsl:when>
            <xsl:when test = "$pEventCode = 'Termination'">
              <eventCode>GetEventCodeTermination(StreamTerminationDate,MaxTerminationDate,IsPerpetual)</eventCode>
            </xsl:when>
          </xsl:choose>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Interest</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <payerReference hRef="Payer"/>
        <receiverReference hRef="Receiver"/>

        <unitType>[Currency]</unitType>
        <unitReference hRef="{$occurence}PaymentDatesCurrency"/>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>AdjustedRecordOrEndPeriodDate</eventDate>
        </subItem>

        <!-- EG 20150907 New RCD : Record date -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|RecordDate</eventClass>
          <eventDate>AdjustedRecordDate</eventDate>
        </subItem>

        <!-- EG 20150907 New EXD : Ex date -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ExDate</eventClass>
          <eventDate>AdjustedExDate</eventDate>
        </subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>

        <!-- 20070823 EG Ticket : 15643 -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDate>AdjustedPreSettlementDate</eventDate>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
          <isPayment>1</isPayment>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|RateCutOffDate</eventClass>
          <eventDate>AdjustedRateCutOff</eventDate>
        </subItem>

      </itemGroup>

      <xsl:call-template name="CalculationPeriodDates">
        <xsl:with-param name="pCurrency" select="$occurence" />
      </xsl:call-template>

    </paymentDates>
  </xsl:template>

  <!-- CalculationPeriodDates -->
  <xsl:template name="CalculationPeriodDates">
    <xsl:param name="pCurrency"/>
    <calculationPeriodDates>
      <keyGroup name="CalculationPeriodDates" key="PER"/>
      <itemGroup>
        <occurence to="All">calculationPeriods</occurence>
        <conditionalReference hRef="IsNotDiscount"/>
        <parameter id="StartPeriod">StartPeriod</parameter>
        <parameter id="EndPeriod">EndPeriod</parameter>
        <parameter id="AdjustedStartPeriod">AdjustedStartPeriod</parameter>
        <parameter id="Stub">Stub</parameter>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|CalculationPeriod</eventCode>
          <eventType>EventType(CalculationPeriodAmount,Rate,RateInitialStub,RateFinalStub)</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <payerReference hRef="Payer"/>
        <receiverReference hRef="Receiver"/>

        <valorisation>PeriodAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unitReference hRef="{$pCurrency}PaymentDatesCurrency"/>

        <item_Details>
          <fixedRate>Rate(CalculationPeriodAmount,'FixedRate',Rate,RateInitialStub,RateFinalStub)</fixedRate>
          <asset>AssetRateIndex(CalculationPeriodAmount,'FloatingRate',Rate,RateInitialStub,RateFinalStub)</asset>
          <asset2>AssetRateIndex(CalculationPeriodAmount,'FloatingRate2',Rate,RateInitialStub,RateFinalStub)</asset2>
          <dayCountFraction>DayCountFraction(DCF)</dayCountFraction>
          <spread>Spread</spread>
          <multiplier>Multiplier</multiplier>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>
      </itemGroup>
      <xsl:call-template name="CapFloored"/>
      <xsl:call-template name="ResetDates"/>
    </calculationPeriodDates>
  </xsl:template>

  <!-- CapFloored -->
  <xsl:template name="CapFloored">
    <capFlooreds>
      <keyGroup name="CapFloored" key="C_F"/>
      <itemGroup>
        <occurence to="All">capFlooreds</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|CalculationPeriod</eventCode>
          <eventType>EventType</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <payer>CapFlooredPayer</payer>
        <receiver>CapFlooredReceiver</receiver>

        <item_Details>
          <strike>Strike</strike>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Rate</eventClass>
          <eventDate>AdjustedStartPeriod</eventDate>
        </subItem>
      </itemGroup>
    </capFlooreds>
  </xsl:template>

  <!-- ResetDates -->
  <xsl:template name="ResetDates">
    <resetDates>
      <keyGroup name="ResetDates" key="RES"/>
      <itemGroup>
        <occurence to="All">resetDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <item_Details>
          <asset>AssetRateIndex(CalculationPeriodAmount,Stub,'FloatingRate',Rate,RateInitialStub,RateFinalStub)</asset>
          <asset2>AssetRateIndex(CalculationPeriodAmount,Stub,'FloatingRate2',Rate,RateInitialStub,RateFinalStub)</asset2>
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
      <xsl:call-template name="SelfAverageDates"/>
    </resetDates>
  </xsl:template>

  <!-- SelfAverageDates -->
  <xsl:template name="SelfAverageDates">
    <selfAverageDates>
      <keyGroup name="SelfAverageDates" key="SAV"/>
      <itemGroup>
        <occurence to="All">selfAverageDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|SelfAverage</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <item_Details>
          <asset>AssetBasisRateIndex</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Average</eventClass>
          <eventDate>AdjustedSelfDate</eventDate>
        </subItem>
      </itemGroup>
      <xsl:call-template name="SelfResetDates"/>
    </selfAverageDates>
  </xsl:template>

  <!-- SelfResetDates -->
  <xsl:template name="SelfResetDates">
    <selfResetDates>
      <keyGroup name="SelfResetDates" key="SRT"/>
      <itemGroup>
        <occurence to="All">selfResetDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|SelfReset</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|FloatingRate</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <item_Details>
          <asset>AssetBasisRateIndex</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
          <eventDate>AdjustedSelfDate</eventDate>
        </subItem>
      </itemGroup>
    </selfResetDates>
  </xsl:template>

  <!-- Exercise -->
  <xsl:template name="Exercise">
    <xsl:param name="pType"/>

    <xsl:variable name="field">
      <xsl:choose>
        <xsl:when test = "$pType = 'American'">american</xsl:when>
        <xsl:when test = "$pType = 'Bermuda'">bermuda</xsl:when>
        <xsl:when test = "$pType = 'European'">european</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <item>
      <occurence to="All" isOptional="true" field="{$field}ExerciseDatesEvents">efs_ExerciseDates</occurence>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|ExerciseDates</eventCode>
        <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pType"/></eventType>
      </key>
      <startDate>StartPeriod</startDate>
      <endDate>EndPeriod</endDate>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDate>AdjustedStartPeriod</eventDate>
      </subItem>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|RelevantUnderlyingDate</eventClass>
        <eventDate>AdjustedRelevantUnderlyingDate</eventDate>
      </subItem>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|CashSettlementValuationDate</eventClass>
        <eventDate>AdjustedCashSettlementValuationDate</eventDate>
      </subItem>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|CashSettlementPaymentDate</eventClass>
        <eventDate>AdjustedCashSettlementPaymentDate</eventDate>
      </subItem>
    </item>
  </xsl:template>

  <!-- Payment -->
  <!-- RD 20110526 Ticket : 17469 -->
  <!-- Add parameter pProduct -->
  <!-- PM 20150709 [21103] Ajout gestion CashBalanceSafekeeping -->
  <xsl:template name="Payment">
    <xsl:param name="pType"/>
    <xsl:param name="pProduct" select="''"/>

    <xsl:variable name="occurence">
      <xsl:choose>
        <xsl:when test = "$pType = 'BulletPayment'">payment</xsl:when>
        <xsl:when test = "$pType = 'TermDeposit'">payment</xsl:when>
        <xsl:when test = "$pType = 'Premium'">premium</xsl:when>
        <xsl:when test = "$pType = 'AdditionalPayment'">additionalPayment</xsl:when>
        <xsl:when test = "$pType = 'OtherPartyPayment'">otherPartyPayment</xsl:when>
        <xsl:when test = "$pType = 'CashBalanceFees'">constituent</xsl:when>
        <xsl:when test = "$pType = 'CashBalanceSafekeeping'">constituent</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="eventCode">
      <xsl:choose>
        <xsl:when test = "$pType = 'BulletPayment'">Start</xsl:when>
        <xsl:when test = "$pType = 'TermDeposit'">Start</xsl:when>
        <xsl:when test = "$pType = 'Premium'">LinkedProductPayment</xsl:when>
        <xsl:when test = "$pType = 'AdditionalPayment'">AdditionalPayment</xsl:when>
        <xsl:when test = "$pType = 'OtherPartyPayment'">OtherPartyPayment</xsl:when>
        <xsl:when test = "$pType = 'CashBalanceFees'">OtherPartyPayment</xsl:when>
        <xsl:when test = "$pType = 'CashBalanceSafekeeping'">SafeKeepingPayment</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <keyGroup name="Payment" key="PAY"/>
    <itemGroup>
      <!-- EG/PM 20150108 -->
      <!--<parameter id="{$pType}Payer">PayerPartyReference</parameter>-->
      <!--<parameter id="{$pType}Receiver">ReceiverPartyReference</parameter>-->
      <!--<parameter id="{$pType}PaymentType">PaymentType('<xsl:value-of select="$eventCode"/>')</parameter>-->

      <occurence>
        <xsl:attribute name ="to">All</xsl:attribute>
        <xsl:attribute name ="isOptional">true</xsl:attribute>
        <xsl:if test ="$pType = 'CashBalanceFees'">
          <xsl:attribute name ="field">cashFlows/constituent/fee</xsl:attribute>
        </xsl:if>
        <xsl:if test ="$pType = 'CashBalanceSafekeeping'">
          <xsl:attribute name ="field">cashFlows/constituent/safekeeping</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="$occurence"/>
      </occurence>


      <!--<occurence to="All" isOptional="true">
        <xsl:value-of select="$occurence"/>
      </occurence>-->
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$eventCode"/></eventCode>
        <!-- PM 20140926 [20066][20185] -->
        <!--<xsl:if test = "$pType = 'Premium'">
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Premium</eventType>
        </xsl:if>
        <xsl:if test = "$pType != 'Premium'">
          <eventTypeReference hRef="{$pType}PaymentType"/>
        </xsl:if>-->
        <xsl:choose>
          <xsl:when test = "$pType = 'Premium'">
            <eventType>EfsML.Enum.Tools.EventTypeFunc|Premium</eventType>
          </xsl:when>
          <xsl:when test = "$pType = 'CashBalanceFees'">
            <eventType>PaymentType('OtherPartyPayment')</eventType>
          </xsl:when>
          <xsl:when test = "$pType = 'CashBalanceSafekeeping'">
            <eventType>PaymentType('SafeKeepingPayment')</eventType>
          </xsl:when>
          <xsl:otherwise>
            <!-- EG/PM 20150108 -->
            <!--<eventTypeReference hRef="{$pType}PaymentType"/>-->
            <eventType>PaymentType('<xsl:value-of select="$eventCode"/>')</eventType>
          </xsl:otherwise>
        </xsl:choose>
      </key>
      <startDate>PaymentDate</startDate>
      <!-- EG 20120220 -->
      <!-- Add choose -->
      <xsl:choose>
        <xsl:when test="$pProduct = 'ExchangeTradedDerivative'">
          <endDate>PaymentDate</endDate>
        </xsl:when>
        <xsl:otherwise>
          <endDate>ExpirationDate</endDate>
        </xsl:otherwise>
      </xsl:choose>

      <!--<payerReference hRef="{$pType}Payer" />
      <receiverReference hRef="{$pType}Receiver"/>-->
      <!-- EG/PM 20150108 -->
      <payer>PayerPartyReference</payer>
      <receiver>ReceiverPartyReference</receiver>
      <valorisation>PaymentAmount</valorisation>
      <unitType>[Currency]</unitType>
      <unit>PaymentCurrency</unit>

      <item_Details>
        <paymentQuote>PaymentQuote</paymentQuote>
        <exchangeRate>ExchangeRate</exchangeRate>
        <paymentSource>PaymentSource</paymentSource>
      </item_Details>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
        <!-- RD 20110526 Ticket : 17469 -->
        <!-- Add choose -->
        <xsl:choose>
          <xsl:when test="$pProduct = 'ExchangeTradedDerivative'">
            <eventDateReference hRef="ETD_AdjustedClearingBusinessDate"/>
          </xsl:when>
          <xsl:otherwise>
            <eventDateReference hRef="AdjustedTradeDate"/>
          </xsl:otherwise>
        </xsl:choose>
      </subItem>

      <!-- EG 20150616 [21124] New VAL : ValueDate -->
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
        <xsl:choose>
          <xsl:when test="$pProduct = 'ExchangeTradedDerivative'">
            <eventDateReference hRef="ETD_AdjustedClearingBusinessDate"/>
          </xsl:when>
          <xsl:when test="$pProduct = 'CommoditySpot'">
            <eventDateReference hRef="COMD_AdjustedClearingBusinessDate"/>
          </xsl:when>
          <xsl:otherwise>
            <eventDate>AdjustedPaymentDate</eventDate>
          </xsl:otherwise>
        </xsl:choose>
      </subItem>

      <!-- 20070823 EG Ticket : 15643 -->
      <!-- EG 20110909 No PresSettlement on ETD/RISK -->
      <xsl:if test ="$pProduct != 'CashBalance' and  $pProduct != 'ExchangeTradedDerivative'">
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDate>AdjustedPreSettlementDate</eventDate>
        </subItem>
      </xsl:if>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
        <eventDate>AdjustedPaymentDate</eventDate>
        <isPayment>1</isPayment>
      </subItem>
      <!-- 20080703 PL -->
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Invoiced</eventClass>
        <eventDate>Invoicing_AdjustedPaymentDate</eventDate>
        <isPayment>1</isPayment>
      </subItem>
    </itemGroup>

    <originalPayment>
      <xsl:call-template name="OriginalPayment">
        <xsl:with-param name="pType" select="$pType" />
        <xsl:with-param name="pEventCode" select="$eventCode" />
      </xsl:call-template>
    </originalPayment>

    <tax>
      <xsl:call-template name="Tax">
        <xsl:with-param name="pType" select="$pType" />
        <xsl:with-param name="pProduct" select="$pProduct" />
        <xsl:with-param name="pEventCode" select="$eventCode" />
      </xsl:call-template>
    </tax>

  </xsl:template>

  <xsl:template name="OriginalPayment">
    <xsl:param name="pType"/>
    <xsl:param name="pEventCode"/>

    <keyGroup name="OriginalPayment" key="OPAY"/>
    <itemGroup>
      <occurence to="Unique" field="originalPayment" isFieldOptional="true">efs_Payment</occurence>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
        <xsl:if test = "$pType = 'Premium'">
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Premium</eventType>
        </xsl:if>
        <xsl:if test = "$pType != 'Premium'">
          <!--<eventTypeReference hRef="{$pType}PaymentType"/>-->
          <eventType>PaymentType('<xsl:value-of select="$pEventCode"/>')</eventType>
        </xsl:if>
      </key>
      <startDate>PaymentDate</startDate>
      <endDate>ExpirationDate</endDate>
      <!--<payerReference hRef="{$pType}Payer" />-->
      <!--<receiverReference hRef="{$pType}Receiver"/>-->
      <!-- EG/PM 20150108 -->
      <payer>PayerPartyReference</payer>
      <receiver>ReceiverPartyReference</receiver>
      <valorisation>PaymentAmount</valorisation>
      <unitType>[Currency]</unitType>
      <unit>PaymentCurrency</unit>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDateReference hRef="AdjustedTradeDate"/>
      </subItem>
    </itemGroup>
  </xsl:template>

  <xsl:template name="Tax">
    <xsl:param name="pType"/>
    <xsl:param name="pProduct" />
    <xsl:param name="pEventCode"/>

    <keyGroup name="Tax" key="TAX"/>
    <itemGroup>
      <occurence to="all" field="tax" isFieldOptional="true">efs_Payment</occurence>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
        <eventType>EventType</eventType>
      </key>
      <startDate>PaymentDate</startDate>
      <!-- FI 20120905 [18107] add Choose -->
      <!-- sur ExchangeTradedDerivative la date fin = la date de compensation (comme pour les OPP parents [voir EG 20120220]) -->
      <!--<endDate>ExpirationDate</endDate>-->
      <xsl:choose>
        <xsl:when test="$pProduct = 'ExchangeTradedDerivative'">
          <endDate>PaymentDate</endDate>
        </xsl:when>
        <xsl:otherwise>
          <endDate>ExpirationDate</endDate>
        </xsl:otherwise>
      </xsl:choose>
      <!--<payerReference hRef="{$pType}Payer" />-->
      <!--<receiverReference hRef="{$pType}Receiver"/>-->
      <!-- EG/PM 20150108 -->
      <payer>PayerPartyReference</payer>
      <receiver>ReceiverPartyReference</receiver>
      <valorisation>TaxAmount</valorisation>
      <unitType>[Currency]</unitType>
      <unit>TaxCurrency</unit>

      <item_Details>
        <taxSource>TaxSource</taxSource>
      </item_Details>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
        <!-- RD 20120717 Ticket : 17469 -->
        <!-- Add choose -->
        <xsl:choose>
          <xsl:when test="$pProduct = 'ExchangeTradedDerivative'">
            <eventDateReference hRef="ETD_AdjustedClearingBusinessDate"/>
          </xsl:when>
          <xsl:otherwise>
            <eventDateReference hRef="AdjustedTradeDate"/>
          </xsl:otherwise>
        </xsl:choose>
      </subItem>

      <!-- EG 20150616 [21124] New VAL : ValueDate -->
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
        <xsl:choose>
          <xsl:when test="$pProduct = 'ExchangeTradedDerivative'">
            <eventDateReference hRef="ETD_AdjustedClearingBusinessDate"/>
          </xsl:when>
          <xsl:otherwise>
            <eventDate>AdjustedPaymentDate</eventDate>
          </xsl:otherwise>
        </xsl:choose>
      </subItem>

      <!-- 20070823 EG Ticket : 15643 -->
      <!-- EG 20110909 No PresSettlement on ETD/RISK -->
      <xsl:if test ="$pProduct != 'CashBalance' and  $pProduct != 'ExchangeTradedDerivative'">
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDate>AdjustedPreSettlementDate</eventDate>
        </subItem>
      </xsl:if>      
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
        <eventDate>AdjustedPaymentDate</eventDate>
        <isPayment>1</isPayment>
      </subItem>
    </itemGroup>
  </xsl:template>

  <xsl:template name="SimplePayment">
    <xsl:param name="pOccurence"/>
    <xsl:param name="pEventCode"/>
    <xsl:param name="pEventType"/>
    <xsl:param name="pIsOptional" select ="'false'"/>
    <keyGroup name="SimplePayment" key="PAY" />
    <itemGroup>
      
      <!--<occurence to="All" field="efs_simplePayment"><xsl:value-of select="$pOccurence"/></occurence>-->
      <occurence>
        <xsl:attribute name ="to">All</xsl:attribute>
        <xsl:attribute name ="field">efs_simplePayment</xsl:attribute>
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
          <xsl:otherwise>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|<xsl:value-of select="$pEventCode"/></eventCode>
          </xsl:otherwise>
        </xsl:choose>
        <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pEventType"/></eventType>
      </key>
      <startDate>PaymentDate</startDate>
      <endDate>PaymentDate</endDate>
      <payer>PayerPartyReference</payer>
      <receiver>ReceiverPartyReference</receiver>
      <valorisation>Amount</valorisation>
      <unitType>[Currency]</unitType>
      <unit>Currency</unit>
      <idStCalcul>[CALC]</idStCalcul>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
        <eventDateReference hRef="AdjustedTradeDate" />
      </subItem>
      <!-- EG 20110909 No PresSettlement on ETD/RISK -->
      <!--<subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
        <eventDate>AdjustedPreSettlementDate</eventDate>
      </subItem>-->

      <!-- EG 20150616 [21124] New VAL : ValueDate -->
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
        <eventDate>AdjustedPaymentDate</eventDate>
      </subItem>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
        <eventDate>AdjustedPaymentDate</eventDate>
        <isPayment>1</isPayment>
      </subItem>
    </itemGroup>
  </xsl:template>

</xsl:stylesheet>
