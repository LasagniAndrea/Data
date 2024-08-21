<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='DebtSecurityTransaction']">
    <xsl:param name="pPosition" select="0"/>
    <debtSecurityTransaction>

      <!-- Product -->
      <keyGroup name="DebtSecurityTransaction" key="PRD"/>
      <parameter id="MinEffectiveDate">MinEffectiveDate</parameter>
      <parameter id="MaxTerminationDate">MaxTerminationDate</parameter>
      <parameter id="ValueDate">ValueDate</parameter>
      <parameter id="BuyerPartyReference">BuyerPartyReference</parameter>
      <parameter id="SellerPartyReference">SellerPartyReference</parameter>
      <parameter id="IssuerPartyReference">IssuerPartyReference</parameter>

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
        <startDateReference hRef="ValueDate"/>
        <endDateReference hRef="MaxTerminationDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <!-- Amounts -->
      <debtSecurityTransactionAmounts>
        <xsl:call-template name="DebtSecurityTransactionAmounts"/>
      </debtSecurityTransactionAmounts>

      <!-- Stream -->
      <debtSecurityTransactionStream>
        <xsl:call-template name="DebtSecurityTransactionStream" />
      </debtSecurityTransactionStream>

    </debtSecurityTransaction>
  </xsl:template>

</xsl:stylesheet>