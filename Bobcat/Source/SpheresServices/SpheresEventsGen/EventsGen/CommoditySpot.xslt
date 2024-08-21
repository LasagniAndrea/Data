<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='CommoditySpot']">
    <xsl:param name="pPosition" select="0"/>
    <commoditySpot>

      <!-- Product -->
      <keyGroup name="CommoditySpot" key="PRD"/>
			<parameter id="COMD_AdjustedClearingBusinessDate">AdjustedClearingBusinessDate</parameter>
      <parameter id="COMD_ClearingBusinessDate">ClearingBusinessDate</parameter>
      <parameter id="COMD_EffectiveDate">EffectiveDate</parameter>
      <parameter id="COMD_AdjustedEffectiveDate">AdjustedEffectiveDate</parameter>
      <parameter id="COMD_TerminationDate">TerminationDate</parameter>
      <parameter id="COMD_Asset">AssetCommodity</parameter>
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

        <startDateReference hRef="COMD_EffectiveDate"/>
        <endDateReference hRef="COMD_TerminationDate"/>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <fixedLeg>
        <xsl:call-template name="FixedSpotLeg"/>
      </fixedLeg>
      <physicalLeg>
        <xsl:call-template name="PhysicalLeg"/>
      </physicalLeg>
    </commoditySpot>
  </xsl:template>


  <!-- FixedSpotLeg -->
  <xsl:template name="FixedSpotLeg">

    <keyGroup name="Leg" key="FLEG"/>
    <itemGroup>
      <occurence to="Unique" field="fixedLeg">efs_CommoditySpot</occurence>
      <parameter id="FLegPayerPartyReference">PayerPartyReference</parameter>
      <parameter id="FLegReceiverPartyReference">ReceiverPartyReference</parameter>

      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|FinancialLeg</eventCode>
        <eventType>EventType</eventType>
      </key>

      <idStCalcul>[CALC]</idStCalcul>
      <startDateReference hRef="COMD_EffectiveDate"/>
      <endDateReference hRef="COMD_TerminationDate"/>
      <payerReference hRef="FLegPayerPartyReference" />
      <receiverReference hRef="FLegReceiverPartyReference" />

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDateReference hRef="AdjustedTradeDate"/>
      </subItem>

    </itemGroup>

    <!-- Commodity payment  -->
    <xsl:call-template name="CommoditySpotPayment"/>
  
  </xsl:template>

  <!-- CommoditySpotPayment -->
  <xsl:template name="CommoditySpotPayment">
    <commodityPayment>
      <keyGroup name="CommoditySpotPayment" key="COMPAY"/>
      <itemGroup>
        <occurence to="unique">commodityPayment</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|GrossAmount</eventType>
        </key>

        <valorisation>Amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>Currency</unit>

        <startDateReference hRef="COMD_EffectiveDate"/>
        <endDateReference hRef="COMD_TerminationDate"/>
        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <item_Details>
          <fixedPrice>FixedPrice</fixedPrice>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="COMD_AdjustedClearingBusinessDate"/>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>ValuedPaymentDate</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
          <isPayment>1</isPayment>
        </subItem>

      </itemGroup>

      <xsl:call-template name="FixingCommodityPayment" />

    </commodityPayment>
  </xsl:template>

  <!-- FixingCommodityPayment -->
  <xsl:template name="FixingCommodityPayment">
    <fixing>
      <keyGroup name="SettlementFixing" key="STLCOMPAY" />
      <itemGroup>
        <occurence to="unique" isOptional="true" field="efs_Fixing">fixedPrice</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Reset</eventCode>
          <eventType>EventType</eventType>
        </key>
        <idStCalcul>[CALCREV]</idStCalcul>
        <startDateReference hRef="COMD_EffectiveDate"/>
        <endDateReference hRef="COMD_TerminationDate"/>
        <item_Details>
          <asset>AssetFxRate</asset>
          <fixingRate>FixingRate</fixingRate>
        </item_Details>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Fixing</eventClass>
          <eventDate>FixingDate</eventDate>
        </subItem>
      </itemGroup>
    </fixing>
  </xsl:template>

  <!-- PhysicalLeg -->
  <xsl:template name="PhysicalLeg">

    <keyGroup name="Leg" key="PLEG"/>
    <itemGroup>
      <occurence to="Unique" field="physicalLeg">efs_CommoditySpot</occurence>
      <parameter id="PLegPayerPartyReference">PayerPartyReference</parameter>
      <parameter id="PLegReceiverPartyReference">ReceiverPartyReference</parameter>

      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|PhysicalLeg</eventCode>
        <eventType>EventType</eventType>
      </key>

      <idStCalcul>[CALC]</idStCalcul>
      <startDateReference hRef="COMD_EffectiveDate"/>
      <endDateReference hRef="COMD_TerminationDate"/>
      <payerReference hRef="PLegPayerPartyReference" />
      <receiverReference hRef="PLegReceiverPartyReference" />

      <item_Details>
        <assetReference hRef="COMD_Asset"/>
      </item_Details>

      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDateReference hRef="AdjustedTradeDate"/>
      </subItem>

    </itemGroup>

    <!-- Physical Delivery  -->
    <xsl:call-template name="CommodityQuantity"/>

  </xsl:template>

  <!-- CommodityQuantity-->

  <xsl:template name="CommodityQuantity">
    <quantity>
      <keyGroup name="Quantity" key="STAQTY" />
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Quantity</eventType>
        </key>

        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="COMD_EffectiveDate"/>
        <endDateReference hRef="COMD_TerminationDate"/>
        <valorisation>Quantity</valorisation>
        <unitType>[Qty]</unitType>
        <unit>UnitQuantity</unit>
        <payerReference hRef="PLegPayerPartyReference" />
        <receiverReference hRef="PLegReceiverPartyReference" />

        <item_Details>
          <assetReference hRef="COMD_Asset"/>
          <deliveryDate>DeliveryDate</deliveryDate>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="COMD_AdjustedClearingBusinessDate"/>
        </subItem>

      </itemGroup>
    </quantity>
  </xsl:template>

</xsl:stylesheet>