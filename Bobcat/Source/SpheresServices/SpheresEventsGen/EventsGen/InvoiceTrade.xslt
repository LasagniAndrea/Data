<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:param name="pMasterProduct"/>
  <xsl:include href="AdditionalInvoice.xslt"/>
  <xsl:include href="CreditNote.xslt"/>
  <xsl:include href="Invoice.xslt"/>
  <xsl:include href="InvoiceSettlement.xslt"/>
  <xsl:include href="Shared.xslt"/>
  <xsl:include href="InvoiceShared.xslt"/>

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
          <!-- EG 20120215 [17469] -->
          <!-- Add parameter pProduct -->
          <xsl:call-template name="Payment">
            <xsl:with-param name="pType" select="'OtherPartyPayment'"/>
            <xsl:with-param name="pProduct" select="item/@name"/>
          </xsl:call-template>
        </otherPartyPayment>
      </trade>
    </EventMatrix>
  </xsl:template>
</xsl:stylesheet>