<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='MarginRequirement']">
    <xsl:param name="pPosition" select="0"/>
    <marginRequirement>
      <!-- Product -->
      <keyGroup name="MarginRequirement" key="PRD"/>
      <parameter id="timing">GetTiming()</parameter>
      <parameter id="EventCodeByTiming">EfsML.Enum.Tools.EventCodeFunc|LinkProductClosingIntraday(timing)</parameter>
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
        <startDateReference hRef="TradeDate"/>
        <endDateReference hRef="TradeDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <groupLevel>
        <keyGroup name="GroupLevel" key="GRP"/>
        <itemGroup>
          <key>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|MarginRequirement</eventCode>
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
        <xsl:call-template name="MarginRequirementSimplePayment"/>
      </groupLevel>

    </marginRequirement>
  </xsl:template>

  <!-- MarginRequirementSimplePaymentSimplePayment -->
  <!-- Ce n'est pas un véritable payment (sur ce template pas de  Settlement ni de prssettlement  -->
  <xsl:template name="MarginRequirementSimplePayment">
    <simplePayment>
      <keyGroup name="SimplePayment" key="PAY" />
      <itemGroup>
        <occurence to="All">efs_SimplePayment</occurence>
        <key>
          <eventCodeReference hRef="EventCodeByTiming"/>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|MarginRequirement</eventType>
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
        <!--<subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>AdjustedPaymentDate</eventDate>
          <isPayment>1</isPayment>
        </subItem>-->
      </itemGroup>
    </simplePayment>
  </xsl:template>

</xsl:stylesheet>