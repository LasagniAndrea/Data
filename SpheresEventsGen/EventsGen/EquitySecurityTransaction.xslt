<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='EquitySecurityTransaction']">
    <xsl:param name="pPosition" select="0"/>
    <equitySecurityTransaction>

      <!-- Product -->
      <keyGroup name="EquitySecurityTransaction" key="PRD"/>
      <parameter id="ESE_ClearingBusinessDate">ClearingBusinessDate</parameter>
      <parameter id="ESE_EffectiveDate">ClearingBusinessDate</parameter>
      <parameter id="ESE_TerminationDate">OpenTerminationDate</parameter>
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
        <startDateReference hRef="ESE_EffectiveDate"/>
        <endDateReference hRef="ESE_TerminationDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <!-- Amounts -->
      <xsl:call-template name="EquitySecurityTransactionStream">
        <xsl:with-param name="pPosition" select="$pPosition"/>
      </xsl:call-template>
    </equitySecurityTransaction>
  </xsl:template>

  <!-- EquitySecurityTransactionStream -->
  <!-- EG 20190730 (Gestion HGA|GAM) -->
  <xsl:template name="EquitySecurityTransactionStream">
    <xsl:param name="pPosition" />
    <equitySecurityTransactionStream>
      <keyGroup name="EquitySecurityTransactionStream" key="ESE"/>
      <parameter id="ESE_IsNotPositionOpeningAndIsPositionKeeping">IsNotPositionOpeningAndIsPositionKeeping</parameter>      
      <parameter id="ESE_AdjustedClearingBusinessDate">AdjustedClearingBusinessDate</parameter>
      <parameter id="ESE_Buyer">BuyerPartyReference</parameter>
      <parameter id="ESE_Seller">SellerPartyReference</parameter>
      <parameter id="ESE_Asset">AssetEquity</parameter>
      <itemGroup>
        <occurence to="Unique">efs_EquitySecurityTransaction</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|EquitySecurityTransaction</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Share</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="ESE_EffectiveDate"/>
        <endDateReference hRef="ESE_TerminationDate"/>

        <item_Details>
          <assetReference hRef="ESE_Asset"/>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <xsl:call-template name="EquitySecurityTransactionQuantity" />
      <xsl:call-template name="EquitySecurityTransactionGrossAmount" />
      <xsl:call-template name="EquitySecurityTransactionMarginRatio" />

      <!-- LPC Amounts -->
      <xsl:if test = "$pPosition = '0'">
        <xsl:call-template name="ESELinkedProductClosing" />
      </xsl:if>
    </equitySecurityTransactionStream>
  </xsl:template>

  <!-- EquitySecurityTransactionQuantity -->
  <xsl:template name="EquitySecurityTransactionQuantity">
    <grossAmount>
      <keyGroup name="EquitySecurityTransactionQuantity" key="QTY"/>
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Quantity</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="ESE_ClearingBusinessDate"/>
        <endDateReference hRef="ESE_ClearingBusinessDate"/>

        <valorisation>Quantity</valorisation>
        <unitType>[Qty]</unitType>
        <payerReference hRef="ESE_Seller"/>
        <receiverReference hRef="ESE_Buyer"/>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>
      </itemGroup>
    </grossAmount>
  </xsl:template>

  <!-- EquitySecurityTransactionGrossAmount -->
  <xsl:template name="EquitySecurityTransactionGrossAmount">
    <grossAmount>
      <keyGroup name="EquitySecurityTransactionGrossAmount" key="GAM"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">grossAmount</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|GrossAmount</eventType>
          <eventType>EventTypeGAM</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="ESE_ClearingBusinessDate"/>
        <endDateReference hRef="ESE_ClearingBusinessDate"/>

        <valorisation>Amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>Currency</unit>

        <item_Details>
          <asset>AssetEquity</asset>
        </item_Details>

        <payerReference hRef="ESE_Buyer"/>
        <receiverReference hRef="ESE_Seller"/>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <conditionalReference hRef="ESE_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
        </subItem>

        <subItem>
          <conditionalReference hRef="ESE_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDate>PreSettlementPaymentDate</eventDate>
        </subItem>
        <subItem>
          <conditionalReference hRef="ESE_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
          <isPayment>1</isPayment>
        </subItem>
      </itemGroup>
    </grossAmount>
  </xsl:template>

  <!-- EquitySecurityTransactionMarginRatio -->
  <xsl:template name="EquitySecurityTransactionMarginRatio">
    <marginRatio>
      <keyGroup name="EquitySecurityTransactionMarginRatio" key="MGF"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">marginRatio</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|MarginRequirementRatio</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="ESE_ClearingBusinessDate"/>
        <endDateReference hRef="ESE_TerminationDate"/>

        <valorisation>Amount</valorisation>
        <unitType>UnitType</unitType>
        <unit>Unit</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>

      </itemGroup>
    </marginRatio>
  </xsl:template>

  <!-- ESELinkedProductClosing -->
  <xsl:template name="ESELinkedProductClosing">
    <linkedProductClosingAmounts>
      <keyGroup name="ESE_LinkedProductClosing" key="LPC"/>
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>

        <startDateReference hRef="ESE_EffectiveDate"/>
        <endDateReference hRef="ESE_TerminationDate"/>

        <item_Details>
          <assetReference hRef="ESE_Asset"/>
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
        <startDateReference hRef="ESE_EffectiveDate"/>
        <endDateReference   hRef="ESE_TerminationDate"/>

        <payerReference hRef="IMPayerPartyReference"/>
        <receiverReference hRef="IMReceiverPartyReference"/>

        <valorisation>InitialMarginAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>InitialMarginCurrency</unit>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </item>

    </linkedProductClosingAmounts>
  </xsl:template>


</xsl:stylesheet>