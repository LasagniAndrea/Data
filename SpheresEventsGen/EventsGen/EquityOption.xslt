<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='EquityOption']">
    <xsl:param name="pPosition" select="0"/>
    <equityOption>
      <!-- Product -->
      <keyGroup name="EquityOption" key="PRD"/>
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

      <!-- EquityOptionPremium -->
      <equityOptionPremium>
        <keyGroup name="EquityOptionPremium" key="PRM"/>
        <itemGroup>
          <occurence to="Unique">efs_EquityOptionPremium</occurence>
          <key>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
            <eventType>EfsML.Enum.Tools.EventTypeFunc|Premium</eventType>
          </key>
          <startDate>SettlementDate</startDate>
          <endDate>ExpiryDate</endDate>
          <payer>PayerPartyReference</payer>
          <receiver>ReceiverPartyReference</receiver>
          <valorisation>PremiumAmount</valorisation>
          <unitType>[Currency]</unitType>
          <unit>PremiumAmountCurrency</unit>
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
            <eventDateReference hRef="AdjustedTradeDate" />
          </subItem>
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
            <eventDate>AdjustedPreSettlementDate</eventDate>
          </subItem>
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
            <eventDate>AdjustedSettlementDate</eventDate>
            <isPayment>1</isPayment>
          </subItem>
        </itemGroup>
      </equityOptionPremium>

      <!-- EquityOptionType -->
      <equityOptionType>
        <keyGroup name="EquityOptionType" key="EOT"/>
        <itemGroup>
          <occurence to="Unique">efs_EquityOption</occurence>
          <parameter id="EQO_Buyer">BuyerPartyReference</parameter>
          <parameter id="EQO_Seller">SellerPartyReference</parameter>
          <parameter id="EQO_EffectiveDate">EffectiveDate</parameter>
          <parameter id="EQO_ExpiryDate">ExpiryDate</parameter>
          <parameter id="EQO_AdjustedExpiryDate">AdjustedExpiryDate</parameter>
          <key>
            <eventCode>EventCode</eventCode>
            <eventType>EventType</eventType>
          </key>
          <idStCalcul>[CALC]</idStCalcul>
          <startDateReference hRef="EQO_EffectiveDate"/>
          <endDateReference hRef="EQO_ExpiryDate"/>

          <item_Details>
            <asset>UnderlyingAsset</asset>
          </item_Details>

          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
            <eventDateReference hRef="EQO_AdjustedExpiryDate"/>
          </subItem>
        </itemGroup>

        <xsl:call-template name="EquityInitialValuation" />
        <xsl:call-template name="EquityOptionFeatures" />
        <xsl:call-template name="EquityExerciseDates" />
        <xsl:call-template name="EquityAutomaticExercise" />
       
      </equityOptionType>
      
    </equityOption>
  </xsl:template>


  <!-- EquityInitialValuation-->
  <xsl:template name="EquityInitialValuation">
    <initialValuation>
      <keyGroup name="EquityInitialValuation" key="INI"/>
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|InitialValuation</eventCode>
          <eventType>UnderlyerType</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference hRef="EQO_ExpiryDate"/>

        <valorisation>NumberOptions</valorisation>
        <unitType>NumberOptionsUnitType</unitType>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="EQO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityInitialValuationNotional" />
      <xsl:call-template name="EquityInitialValuationNbOption" />
      <xsl:call-template name="EquityInitialValuationSingleUnderlyer" />
      <xsl:call-template name="EquityInitialValuationBasket" />
    </initialValuation>
  </xsl:template>

  <!-- EquityInitialValuationNotional -->
  <xsl:template name="EquityInitialValuationNotional">
    <notional>
      <keyGroup name="InitialValuationNotional" key="NOM"/>
      <itemGroup>
        <occurence to="Unique">notional</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference   hRef="EQO_ExpiryDate"/>
        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <valorisation>NotionalAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>NotionalAmountCurrency</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>
    </notional>
  </xsl:template>

  <!-- EquityInitialValuationNbOption -->
  <xsl:template name="EquityInitialValuationNbOption">
    <nboption>
      <keyGroup name="InitialValuationNbOption" key="NBO"/>
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Quantity</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference   hRef="EQO_ExpiryDate"/>
        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <valorisation>NumberOptions</valorisation>
        <unitType>NumberOptionsUnitType</unitType>

        <item_Details>
          <asset>UnderlyingAsset</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>
    </nboption>
  </xsl:template>

  <!-- EquityInitialValuationSingleUnderlyer -->
  <xsl:template name="EquityInitialValuationSingleUnderlyer">
    <singleUnderlyer>
      <keyGroup name="InitialValuationUnderlyer" key="UNL"/>
      <itemGroup>
        <occurence to="Unique">underlyer</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Underlyer</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference   hRef="EQO_ExpiryDate"/>
        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <valorisation>OpenUnits</valorisation>
        <unitType>UnitType</unitType>
        <unit>Unit</unit>

        <item_Details>
          <asset>UnderlyingAsset</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>
    </singleUnderlyer>
  </xsl:template>

  <!-- EquityInitialValuationBasket -->
  <xsl:template name="EquityInitialValuationBasket">
    <basket>
      <keyGroup name="InitialValuationBasketConstituent" key="ELT"/>
      <itemGroup>
        <occurence to="All" isFieldOptional="true" field="basketConstituent">basket</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>UnderlyerEventType</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference   hRef="EQO_ExpiryDate"/>
        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <valorisation>UnitValueUnderlyerConstituent(EQO_UnitValueTotalUnderlyer)</valorisation>
        <unitType>UnitTypeUnderlyerConstituent</unitType>
        <unit>UnitUnderlyerConstituent</unit>

        <item_Details>
          <asset>UnderlyingAsset</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate" />
        </subItem>
      </itemGroup>
    </basket>
  </xsl:template>


  <!-- EquityOptionFeatures-->
  <xsl:template name="EquityOptionFeatures">
    <xsl:call-template name="EquityAsianFeatures">
      <xsl:with-param name="pAveragingType" select="'In'" />
    </xsl:call-template>
    <xsl:call-template name="EquityAsianFeatures">
      <xsl:with-param name="pAveragingType" select="'Out'" />
    </xsl:call-template>
    <xsl:call-template name="EquityBarrierFeatures">
      <xsl:with-param name="pBarrierType" select="'Cap'" />
    </xsl:call-template>
    <xsl:call-template name="EquityExtendedBarrierFeatures">
      <xsl:with-param name="pBarrierType" select="'Cap'" />
    </xsl:call-template>
    <xsl:call-template name="EquityBarrierFeatures">
      <xsl:with-param name="pBarrierType" select="'Floor'" />
    </xsl:call-template>
    <xsl:call-template name="EquityExtendedBarrierFeatures">
      <xsl:with-param name="pBarrierType" select="'Floor'" />
    </xsl:call-template>
    <xsl:call-template name="EquityKnockFeatures">
      <xsl:with-param name="pKnockType" select="'In'" />
    </xsl:call-template>
    <xsl:call-template name="EquityKnockFeatures">
      <xsl:with-param name="pKnockType" select="'Out'" />
    </xsl:call-template>
  </xsl:template>

  <!-- EquityAsianFeatures-->
  <xsl:template name="EquityAsianFeatures">
    <xsl:param name="pAveragingType" />
    <asianFeatures>
      <keyGroup name="EquityAsianFeatures" key="ASI"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true" field="averagingPeriod{$pAveragingType}">asian</occurence>
        <key>
          <eventCode>AveragingCode</eventCode>
          <eventType>AveragingType</eventType>
        </key>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference hRef="EQO_ExpiryDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="EQO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityUnderlyerValuationDates">
        <xsl:with-param name="pTypeUnderlyer" select="'singleUnderlyerValuationDates'" />
      </xsl:call-template>
      <xsl:call-template name="EquityBasketValuationDates" />
    </asianFeatures>
  </xsl:template>

  <!-- EquityBarrierFeatures-->
  <xsl:template name="EquityBarrierFeatures">
    <xsl:param name="pBarrierType" />
    <barrierFeatures>
      <keyGroup name="equityBarrierFeatures" key="BAR"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true" field="barrier{$pBarrierType}">barrier</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
          <eventType>TriggerType</eventType>
        </key>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference hRef="EQO_ExpiryDate"/>

        <item_Details>
          <triggerEvent>TriggerEvent</triggerEvent>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="EQO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityUnderlyerValuationDates">
        <xsl:with-param name="pTypeUnderlyer" select="'singleUnderlyerValuationDates'" />
      </xsl:call-template>
      <xsl:call-template name="EquityBasketValuationDates" />
    </barrierFeatures>
  </xsl:template>

  <!-- EquityExtendedBarrierFeatures-->
  <xsl:template name="EquityExtendedBarrierFeatures">
    <xsl:param name="pBarrierType" />
    <barrierFeatures>
      <keyGroup name="equityExtendedBarrierFeatures" key="EXTBAR"/>
      <itemGroup>
        <occurence to="All" isOptional="true" isFieldOptional="true" field="barrier{$pBarrierType}">multipleBarrier</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
          <eventType>TriggerType</eventType>
        </key>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference hRef="EQO_ExpiryDate"/>

        <item_Details>
          <triggerEvent>TriggerEvent</triggerEvent>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="EQO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityUnderlyerValuationDates">
        <xsl:with-param name="pTypeUnderlyer" select="'singleUnderlyerValuationDates'" />
      </xsl:call-template>
      <xsl:call-template name="EquityBasketValuationDates" />
    </barrierFeatures>
  </xsl:template>

  <!-- EquityKnockFeatures-->
  <xsl:template name="EquityKnockFeatures">
    <xsl:param name="pKnockType" />
    <knockFeatures>
      <keyGroup name="equityKnockFeatures" key="KNK"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true" field="knock{$pKnockType}">knock</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
          <eventType>TriggerType</eventType>
        </key>
        <startDateReference hRef="EQO_EffectiveDate"/>
        <endDateReference hRef="EQO_ExpiryDate"/>

        <item_Details>
          <triggerEvent>TriggerEvent</triggerEvent>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="EQO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityUnderlyerValuationDates">
        <xsl:with-param name="pTypeUnderlyer" select="'singleUnderlyerValuationDates'" />
      </xsl:call-template>
      <xsl:call-template name="EquityBasketValuationDates" />
    </knockFeatures>
  </xsl:template>

  <!-- EquityExercise-->
  <xsl:template name="EquityExerciseDates">
    <exerciseDates>
      <keyGroup name="EquityExerciseDates" key="EXD"/>
      <itemGroup>
        <occurence to="All" field="exerciseDates">exercise</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|ExerciseDates</eventCode>
          <eventType>EventType</eventType>
        </key>
        <startDate>CommencementDate</startDate>
        <endDate>ExpiryDate</endDate>
        <subItem>
          <eventClass>SettlementType</eventClass>
          <eventDate>AdjustedExerciseDate</eventDate>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityUnderlyerValuationDates"/>
      <xsl:call-template name="EquityBasketValuationDates" />
    </exerciseDates>
  </xsl:template>

  <!-- EquityUnderlyerValuationDates-->
  <xsl:template name="EquityUnderlyerValuationDates">
    <underlyerValuationDates>
      <keyGroup name="EquityUnderlyerValuationDates" key="UVD"/>
      <itemGroup>
        <occurence to="All">valuationDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|UnderlyerValuationDate</eventCode>
          <eventType>UnderlyerEventType</eventType>
        </key>
        <startDate>ValuationDate</startDate>
        <endDate>ValuationDate</endDate>

        <item_Details>
          <asset>UnderlyingAsset</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
          <eventDate>AdjustedValuationDate</eventDate> 
        </subItem>
      </itemGroup>
    </underlyerValuationDates>
  </xsl:template>

  <!-- EquityBasketValuationDates-->
  <xsl:template name="EquityBasketValuationDates">
    <basketValuationDates>
      <keyGroup name="EquityBasketValuationDates" key="BVD"/>
      <itemGroup>
        <occurence to="All">basketValuationDates</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|UnderlyerValuationDate</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Basket</eventType>
        </key>
        <startDate>ValuationDate</startDate>
        <endDate>ValuationDate</endDate>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDate>AdjustedValuationDate</eventDate>
        </subItem>
      </itemGroup>
      <xsl:call-template name="EquityUnderlyerValuationDates">
        <xsl:with-param name="pTypeUnderlyer" select="'basketConstituentValuationDates'" />
      </xsl:call-template>
    </basketValuationDates>
  </xsl:template>

  <!-- EquityAutomaticExercise-->
<xsl:template name="EquityAutomaticExercise">
  <automaticExercise>
    <keyGroup name="EquityAutomaticExercise" key="EAD"/>
    <itemGroup>
      <occurence to="Unique" isOptional="true">automaticExercise</occurence>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|AutomaticExerciseDates</eventCode>
        <eventType>EventType</eventType>
      </key>
      <startDate>StartPeriod</startDate>
      <endDate>EndPeriod</endDate>

      <subItem>
        <eventClass>SettlementType</eventClass>
        <eventDate>AdjustedAutomaticExerciseDate</eventDate>
      </subItem>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
        <eventDate>AdjustedSettlementDate</eventDate>
      </subItem>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|ElectionSettlementDate</eventClass>
        <eventDate>AdjustedElectionDate</eventDate>
      </subItem>

    </itemGroup>
  </automaticExercise>
</xsl:template>


</xsl:stylesheet>