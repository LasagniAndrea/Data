<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:param name="pMasterProduct"/>
  <xsl:include href="AdditionalInvoice.xslt"/>
  <xsl:include href="BulletPayment.xslt"/>
  <xsl:include href="BuyAndSellBack.xslt"/>
  <xsl:include href="CapFloor.xslt"/>
  <xsl:include href="CommoditySpot.xslt"/>
  <xsl:include href="CreditNote.xslt"/>
  <xsl:include href="DebtSecurity.xslt"/>
  <xsl:include href="DebtSecurityTransaction.xslt"/>
  <xsl:include href="EquityOption.xslt"/>
  <xsl:include href="EquitySecurityTransaction.xslt"/>
  <xsl:include href="ExchangeTradedDerivative.xslt"/>
  <xsl:include href="Fra.xslt"/>
  <xsl:include href="FxAverageRateOption.xslt"/>
  <xsl:include href="FxBarrierOption.xslt"/>
  <xsl:include href="FxDigitalOption.xslt"/>
  <xsl:include href="FxLeg.xslt"/>
  <xsl:include href="FxOptionLeg.xslt"/>
  <xsl:include href="FxSwap.xslt"/>
  <xsl:include href="Invoice.xslt"/>
  <xsl:include href="InvoiceSettlement.xslt"/>
  <xsl:include href="LoanDeposit.xslt"/>
  <xsl:include href="Repo.xslt"/>
  <xsl:include href="ReturnSwap.xslt"/>
  <!--<xsl:include href="SecurityLending.xslt"/>-->
  <xsl:include href="Strategy.xslt"/>
  <xsl:include href="Swap.xslt"/>
  <xsl:include href="Swaption.xslt"/>
  <xsl:include href="TermDeposit.xslt"/>

  <xsl:include href="Shared.xslt"/>
  <xsl:include href="FxShared.xslt"/>
  <xsl:include href="InvoiceShared.xslt"/>
  <xsl:include href="Provisions.xslt"/>
  <xsl:include href="SecurityShared.xslt"/>
  <xsl:include href="MarginRequirement.xslt"/>
  <xsl:include href="CashBalance.xslt"/>
  <xsl:include href="CashBalanceInterest.xslt"/>
  <xsl:include href="BondOption.xslt"/>

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
          <!-- RD 20110526 [17469] -->
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
