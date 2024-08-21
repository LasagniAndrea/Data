<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:param name="pMasterProduct"/>
  <xsl:include href="CashBalance.xslt"/>  
  <xsl:include href="CashBalanceInterest.xslt"/>
  <xsl:include href="MarginRequirement.xslt"/>
  <xsl:include href="BulletPayment.xslt"/>
  <xsl:include href="Shared.xslt"/>

  <xsl:template match="EventMatrix">
    <EventMatrix>
      <trade>
        <!-- Trade -->
        <keyGroup name="Trade" key="TRD"/>
        <parameter id="TradeDate">TradeDate</parameter>
        <parameter id="AdjustedTradeDate">AdjustedTradeDate</parameter>
        <itemGroup>
          <occurence to="Unique">trade</occurence>
          <key>
            <eventCode>EfsML.Enum.Tools.EventCodeFunc|Trade</eventCode>
            <eventType>EfsML.Enum.Tools.EventTypeFunc|Date</eventType>
          </key>
          <idStCalcul>[CALC]</idStCalcul>
          <subItem>
            <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
            <eventDateReference hRef="AdjustedTradeDate"/>
          </subItem>
        </itemGroup>

        <!-- Product -->
        <xsl:apply-templates mode="product" select="item"/>

        <!-- OtherPartyPayment-->
        <otherPartyPayment>
          <xsl:call-template name="Payment">
            <xsl:with-param name="pType" select="'OtherPartyPayment'"/>
          </xsl:call-template>
        </otherPartyPayment>
      </trade>
    </EventMatrix>
  </xsl:template>
</xsl:stylesheet>
