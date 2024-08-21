<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:template mode="product" match="item[@name='BondOption']">
		<xsl:param name="pPosition" select="0"/>
		<bondOption>
			<!-- Product -->
			<keyGroup name="BondOption" key="PRD"/>
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

      <!-- BondOptionPremium -->
      <bondOptionPremium>
        <keyGroup name="BondOptionPremium" key="PRM"/>
        <itemGroup>
          <occurence to="Unique">efs_BondOptionPremium</occurence>
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
          <!-- EG 20150630 [21124] ValueDate -->
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
            <eventDate>AdjustedSettlementDate</eventDate>
            <isPayment>1</isPayment>
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
      </bondOptionPremium>

      <!-- BondOptionType -->
      <bondOptionType>
        <keyGroup name="BondOptionType" key="BOT"/>
        <itemGroup>
          <occurence to="Unique">efs_BondOption</occurence>
          <parameter id="BO_Buyer">BuyerPartyReference</parameter>
          <parameter id="BO_Seller">SellerPartyReference</parameter>
          <parameter id="BO_EffectiveDate">EffectiveDate</parameter>
          <parameter id="BO_ExpiryDate">ExpiryDate</parameter>
          <parameter id="BO_AdjustedExpiryDate">AdjustedExpiryDate</parameter>
          <parameter id="BO_ExerciseEventClass">ExerciseEventClass</parameter>
          <key>
            <eventCode>EventCode</eventCode>
            <eventType>EventType</eventType>
          </key>
          <idStCalcul>[CALC]</idStCalcul>
          <startDateReference hRef="BO_EffectiveDate"/>
          <endDateReference hRef="BO_ExpiryDate"/>

          <item_Details>
            <asset>UnderlyingAsset</asset>
          </item_Details>

          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
            <eventDateReference hRef="BO_AdjustedExpiryDate"/>
          </subItem>
        </itemGroup>

        <xsl:call-template name="BondInitialValuation" />
        <xsl:call-template name="BondOptionFeatures" />
        <xsl:call-template name="BondExerciseDates" />
        <xsl:call-template name="BondAutomaticExercise" />

      </bondOptionType>
		</bondOption>
  </xsl:template>

  <!-- BondInitialValuation-->
  <xsl:template name="BondInitialValuation">
      <initialValuation>
        <keyGroup name="BondInitialValuation" key="INI"/>
        <itemGroup>
          <key>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|InitialValuation</eventCode>
            <eventType>UnderlyerType</eventType>
          </key>
          <idStCalcul>[CALC]</idStCalcul>
          <startDateReference hRef="BO_EffectiveDate"/>
          <endDateReference hRef="BO_ExpiryDate"/>

          <valorisation>NumberOptions</valorisation>
          <unitType>NumberOptionsUnitType</unitType>

          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
            <eventDateReference hRef="BO_AdjustedExpiryDate"/>
          </subItem>
        </itemGroup>
        <xsl:call-template name="BondInitialValuationNotional" />
        <xsl:call-template name="BondInitialValuationNbOption" />
        <xsl:call-template name="BondInitialValuationUnderlyer" />
      </initialValuation>
    </xsl:template>

  <!-- BondInitialValuationNotional -->
  <xsl:template name="BondInitialValuationNotional">
      <notional>
        <keyGroup name="InitialValuationNotional" key="NOM"/>
        <itemGroup>
          <occurence to="Unique" isOptional="true">notional</occurence>
          <key>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
            <eventType>EfsML.Enum.Tools.EventTypeFunc|Nominal</eventType>
          </key>
          <idStCalcul>[CALC]</idStCalcul>
          <startDateReference hRef="BO_EffectiveDate"/>
          <endDateReference   hRef="BO_ExpiryDate"/>
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

  <!-- BondInitialValuationNbOption -->
  <xsl:template name="BondInitialValuationNbOption">
    <nboption>
      <keyGroup name="InitialValuationNbOption" key="NBO"/>
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Quantity</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="BO_EffectiveDate"/>
        <endDateReference   hRef="BO_ExpiryDate"/>
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


  <!-- BondInitialValuationUnderlyer -->
  <xsl:template name="BondInitialValuationUnderlyer">
    <underlyer>
      <keyGroup name="InitialValuationUnderlyer" key="UNL"/>
      <itemGroup>
        <occurence to="Unique">underlyer</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Underlyer</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="BO_EffectiveDate"/>
        <endDateReference   hRef="BO_ExpiryDate"/>
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
    </underlyer>
  </xsl:template>

  <!-- BondOptionFeatures-->
  <xsl:template name="BondOptionFeatures">
    <xsl:call-template name="BondAsianFeatures">
      <xsl:with-param name="pAveragingType" select="'In'" />
    </xsl:call-template>
    <xsl:call-template name="BondAsianFeatures">
      <xsl:with-param name="pAveragingType" select="'Out'" />
    </xsl:call-template>
    <xsl:call-template name="BondBarrierFeatures">
      <xsl:with-param name="pBarrierType" select="'Cap'" />
    </xsl:call-template>
    <xsl:call-template name="BondBarrierFeatures">
      <xsl:with-param name="pBarrierType" select="'Floor'" />
    </xsl:call-template>
    <xsl:call-template name="BondKnockFeatures">
      <xsl:with-param name="pKnockType" select="'In'" />
    </xsl:call-template>
    <xsl:call-template name="BondKnockFeatures">
      <xsl:with-param name="pKnockType" select="'Out'" />
    </xsl:call-template>
  </xsl:template>

  <!-- BondAsianFeatures-->
  <xsl:template name="BondAsianFeatures">
    <xsl:param name="pAveragingType" />
    <asianFeatures>
      <keyGroup name="BondAsianFeatures" key="ASI"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true" field="averagingPeriod{$pAveragingType}">asian</occurence>
        <key>
          <eventCode>AveragingCode</eventCode>
          <eventType>AveragingType</eventType>
        </key>
        <startDateReference hRef="BO_EffectiveDate"/>
        <endDateReference hRef="BO_ExpiryDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="BO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="BondUnderlyerValuationDates"/>
    </asianFeatures>
  </xsl:template>

  <!-- BondBarrierFeatures-->
  <xsl:template name="BondBarrierFeatures">
    <xsl:param name="pBarrierType" />
    <barrierFeatures>
      <keyGroup name="bondBarrierFeatures" key="BAR"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true" field="barrier{$pBarrierType}">barrier</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
          <eventType>TriggerType</eventType>
        </key>
        <startDateReference hRef="BO_EffectiveDate"/>
        <endDateReference hRef="BO_ExpiryDate"/>

        <item_Details>
          <triggerEvent>TriggerEvent</triggerEvent>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="BO_AdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="BondUnderlyerValuationDates"/>
    </barrierFeatures>
  </xsl:template>

  <!-- BondKnockFeatures-->
  <xsl:template name="BondKnockFeatures">
    <xsl:param name="pKnockType" />
    <knockFeatures>
      <keyGroup name="BondKnockFeatures" key="KNK"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true" field="knock{$pKnockType}">knock</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Barrier</eventCode>
          <eventType>TriggerType</eventType>
        </key>
        <startDateReference hRef="BO_EffectiveDate"/>
        <endDateReference hRef="BO_ExpiryDate"/>

        <item_Details>
          <triggerEvent>TriggerEvent</triggerEvent>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="EQOAdjustedExpiryDate"/>
        </subItem>
      </itemGroup>
      <xsl:call-template name="BondUnderlyerValuationDates"/>
    </knockFeatures>
  </xsl:template>

  <!-- BondExercise-->
  <xsl:template name="BondExerciseDates">
    <exerciseDates>
      <keyGroup name="BondExerciseDates" key="EXD"/>
      <itemGroup>
        <occurence to="All" field="exerciseDatesEvents">exercise</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|ExerciseDates</eventCode>
          <eventType>EventType</eventType>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>
        <subItem>
          <eventClassReference hRef="BO_ExerciseEventClass"/>
          <eventDate>AdjustedEndPeriod</eventDate>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|RelevantUnderlyingDate</eventClass>
          <eventDate>AdjustedRelevantUnderlyingDate</eventDate>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedCashSettlementPaymentDate</eventDate>
        </subItem>
      </itemGroup>
    </exerciseDates>
  </xsl:template>

  <!-- BondUnderlyerValuationDates-->
  <xsl:template name="BondUnderlyerValuationDates">
    <underlyerValuationDates>
      <keyGroup name="BondUnderlyerValuationDates" key="UVD"/>
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

  <!-- BondAutomaticExercise-->
  <xsl:template name="BondAutomaticExercise">
    <automaticExercise>
      <keyGroup name="BondAutomaticExercise" key="EAD"/>
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