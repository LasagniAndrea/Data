<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template mode="product" match="item[@name='DebtSecurityTransaction']">
    <xsl:param name="pPosition" select="0"/>
    <debtSecurityTransaction>

      <!-- Product -->
      <keyGroup name="DebtSecurityTransaction" key="PRD"/>
      <parameter id="DST_ClearingBusinessDate">ClearingBusinessDate</parameter>
      <parameter id="DST_MaxTerminationDate">MaxTerminationDate</parameter>
      <parameter id="DST_AdjustedClearingBusinessDate">AdjustedClearingBusinessDate</parameter>
      <parameter id="DST_ValueDate">ValueDate</parameter>
      <parameter id="DST_BuyerPartyReference">BuyerPartyReference</parameter>
      <parameter id="DST_SellerPartyReference">SellerPartyReference</parameter>
      <parameter id="DST_IssuerPartyReference">IssuerPartyReference</parameter>

      <itemGroup>
        <xsl:choose>
          <xsl:when test = "$pPosition = '0'"><occurence to="Unique">product</occurence></xsl:when>
          <xsl:otherwise><occurence to="Item" position="{$pPosition}">Item</occurence></xsl:otherwise>
        </xsl:choose>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Product</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Date</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="DST_ClearingBusinessDate"/>
        <endDateReference hRef="DST_MaxTerminationDate"/>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <!-- Amounts -->
      <xsl:call-template name="DST_Amounts" />
      
      <!-- Stream -->
      <xsl:call-template name="DST_Stream" />

    </debtSecurityTransaction>
  </xsl:template>

  <!-- DebtSecurityTransactionAmounts -->
  <!-- FI 20151228 [21660] Modify -->
  <!-- EG 20190730 (Gestion HGA|GAM) -->
  <xsl:template name="DST_Amounts">
    <debtSecurityTransactionAmounts>
      <keyGroup name="DebtSecurityTransactionAmounts" key="DSTA"/>
      <parameter id="DSTA_UnadjustedPaymentDate">UnadjustedPaymentDate</parameter>
      <parameter id="DSTA_PreSettlementPaymentDate">PreSettlementPaymentDate</parameter>
      <parameter id="DSTA_AdjustedPaymentDate">AdjustedPaymentDate</parameter>
      <parameter id="DST_Asset">DebtSecurity</parameter>
      <parameter id="DST_IsNotPositionOpeningAndIsPositionKeeping">IsNotPositionOpeningAndIsPositionKeeping</parameter>
      <parameter id="DST_EventTypeGAM">EventTypeGAM</parameter>

      <itemGroup>
        <occurence to="Unique">efs_DebtSecurityTransactionAmounts</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|DebtSecurityTransaction</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|DebtSecurityTransactionAmounts</eventType>
        </key>
        <idStCalcul>[CALC]</idStCalcul>

        <startDateReference hRef="DST_ClearingBusinessDate"/>
        <endDateReference hRef="DST_MaxTerminationDate"/>

        <valorisation>dirtyAmount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>DirtyAmountCurrency</unit>

        <item_Details>
          <assetReference hRef="DST_Asset"/>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>

      <xsl:call-template name="DST_NominalQuantity">
        <xsl:with-param name="pAmountType" select="'Nominal'"/>
      </xsl:call-template>
      <xsl:call-template name="DST_NominalQuantity">
        <xsl:with-param name="pAmountType" select="'Quantity'"/>
      </xsl:call-template>

      
      <xsl:call-template name="DST_GrossAmount" />
      
      <xsl:call-template name="DST_Amount">
        <xsl:with-param name="pAmountType" select="'AccruedInterestAmount'"/>
      </xsl:call-template>
      
      <!-- FI 20151228 [21660] Add PrincipalAmount  -->
      <xsl:call-template name="DST_Amount">
        <xsl:with-param name="pAmountType" select="'PrincipalAmount'"/>
      </xsl:call-template>

      <!-- LPC Amounts -->
      <xsl:call-template name="DST_LinkedProductClosing" />

    </debtSecurityTransactionAmounts>
  </xsl:template>

  <!-- DebtSecurityTransaction Nominal/Quantity -->
  <xsl:template name="DST_NominalQuantity">
    <xsl:param name="pAmountType" />
    <amount>
      <keyGroup name="InitialAmount" key="STAAMT" />
      <itemGroup>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|Start</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pAmountType"/></eventType>
        </key>

        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="DST_ClearingBusinessDate"/>
        <endDateReference   hRef="DST_ClearingBusinessDate"/>

        <xsl:choose>
          <xsl:when test = "$pAmountType = 'Nominal'">
            <valorisation>NotionalAmount</valorisation>
            <unitType>[Currency]</unitType>
            <unit>NotionalCurrency</unit>
            <payerReference     hRef="DST_BuyerPartyReference" />
            <receiverReference  hRef="DST_SellerPartyReference" />
          </xsl:when>
          <xsl:when test = "$pAmountType = 'Quantity'">
            <valorisation>Quantity</valorisation>
            <unitType>[Qty]</unitType>
            <payerReference hRef="DST_SellerPartyReference"/>
            <receiverReference hRef="DST_BuyerPartyReference"/>
          </xsl:when>
        </xsl:choose>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="DST_AdjustedClearingBusinessDate"/>
        </subItem>

      </itemGroup>
    </amount>
  </xsl:template>

  <!-- DebtSecurityTransactionAmount -->
  <!--  FI 20151228 [21660] Modify -->
  <xsl:template name="DST_Amount">
    <xsl:param name="pAmountType" />
    <amount>
      <keyGroup name="DebtSecurityTransactionAmount" key="AMT"/>
      <itemGroup>
        <xsl:choose>
          <xsl:when test = "$pAmountType = 'AccruedInterestAmount'">
            <occurence to="Unique" isOptional="true">accruedInterestAmount</occurence>
          </xsl:when>
          <xsl:when test = "$pAmountType = 'PrincipalAmount'">
            <occurence to="Unique" isOptional="true">principalAmount</occurence>
          </xsl:when>
        </xsl:choose>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pAmountType"/></eventType>
        </key>
        
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="DST_ValueDate"/>
        <endDateReference hRef="DST_ValueDate"/>

        <valorisation>Amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>Currency</unit>

        <item_Details>
          <asset>DebtSecurity</asset>
        </item_Details>


        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDateReference hRef="DSTA_AdjustedPaymentDate"/>
        </subItem>
        
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDateReference hRef="DSTA_PreSettlementPaymentDate"/>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDateReference hRef="DSTA_AdjustedPaymentDate"/>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>
  
  <xsl:template name="DST_GrossAmount">
    <amount>
      <keyGroup name="DebtSecurityTransactionAmount" key="GAM"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">grossAmount</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductPayment</eventCode>
          <eventTypeReference hRef="DST_EventTypeGAM"/>
        </key>
        
        <idStCalcul>[CALC]</idStCalcul>
        <startDateReference hRef="DST_ValueDate"/>
        <endDateReference hRef="DST_ValueDate"/>

        <valorisation>Amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>Currency</unit>

        <item_Details>
          <asset>DebtSecurity</asset>
        </item_Details>

        <payer>PayerPartyReference</payer>
        <receiver>ReceiverPartyReference</receiver>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <conditionalReference hRef="DST_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDateReference hRef="DSTA_AdjustedPaymentDate"/>
        </subItem>
        
        <subItem>
          <conditionalReference hRef="DST_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDateReference hRef="DSTA_PreSettlementPaymentDate"/>
        </subItem>
        <subItem>
          <conditionalReference hRef="DST_IsNotPositionOpeningAndIsPositionKeeping"/>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDateReference hRef="DSTA_AdjustedPaymentDate"/>
          <isPayment>1</isPayment>
        </subItem>
      </itemGroup>
    </amount>
  </xsl:template>

  <!-- DSTLinkedProductClosing -->
  <xsl:template name="DST_LinkedProductClosing">
    <linkedProductClosingAmounts>
      <keyGroup name="LinkedProductClosing" key="LPC"/>
      <itemGroup>
        <occurence to="Unique" isOptional="true">allocation</occurence>
        <key>
          <eventCode>EfsML.Enum.Tools.EventCodeFunc|LinkedProductClosing</eventCode>
          <eventType>EfsML.Enum.Tools.EventTypeFunc|Amounts</eventType>
        </key>

        <startDateReference hRef="DST_ClearingBusinessDate"/>
        <endDateReference hRef="DST_MaxTerminationDate"/>

        <item_Details>
          <assetReference hRef="DST_Asset"/>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDateReference hRef="AdjustedTradeDate"/>
        </subItem>
      </itemGroup>
    </linkedProductClosingAmounts>
  </xsl:template>

  <!-- DebtSecurityTransactionStream -->
  <xsl:template name="DST_Stream">
    <debtSecurityTransactionStream>
      <keyGroup name="Stream" key="STD"/>
      <itemGroup>
        <occurence to="All" field="debtSecurityStream">efs_DebtSecurityTransactionStream</occurence>
        <key>
          <eventCode>EventCode</eventCode>
          <eventType>EventType</eventType>
          <idE_Source>IdE_Source</idE_Source>
        </key>
        <idStCalcul>[CALC]</idStCalcul>
        <startDate>EffectiveDate</startDate>
        <endDate>TerminationDate</endDate>

        <item_Details>
          <asset>DebtSecurity</asset>
        </item_Details>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
          <eventDate>AdjustedEffectiveDate</eventDate>
        </subItem>
      </itemGroup>

      <xsl:call-template name="DST_StreamPaymentDates" />

    </debtSecurityTransactionStream>

  </xsl:template>


  <!-- DebtSecurityTransaction Stream PaymentDates -->
  <xsl:template name="DST_StreamPaymentDates">
    <paymentDates>
      <keyGroup name="PaymentDates" key="PAY"/>
      <itemGroup>
        <occurence to="All">interest</occurence>
        <key>
          <eventCode>EventCode</eventCode>
          <eventType>EventType</eventType>
          <idE_Source>IdE_Source</idE_Source>
        </key>
        <startDate>StartPeriod</startDate>
        <endDate>EndPeriod</endDate>

        <valorisation>Amount</valorisation>
        <unitType>[Currency]</unitType>
        <unit>Currency</unit>

        <!-- EG 20190730 Le custodian remplace l'emetteur -->
        <payerReference hRef="DST_SellerPartyReference"/>
        <receiverReference hRef="DST_BuyerPartyReference"/>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Recognition</eventClass>
          <eventDate>RecognitionDate</eventDate>
        </subItem>

        <!-- EG 20150907 New RCD : Record date -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|RecordDate</eventClass>
          <eventDate>RecordDate</eventDate>
        </subItem>

        <!-- EG 20150907 New EXD : Ex date -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ExDate</eventClass>
          <eventDate>ExDate</eventDate>
        </subItem>

        <!-- EG 20150616 [21124] New VAL : ValueDate -->
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|ValueDate</eventClass>
          <eventDate>ValueDate</eventDate>
        </subItem>

        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|PreSettlement</eventClass>
          <eventDate>AdjustedPreSettlementDate</eventDate>
        </subItem>
        <subItem>
          <eventClass>EfsML.Enum.Tools.EventClassFunc|Settlement</eventClass>
          <eventDate>SettlementDate</eventDate>
          <isPayment>1</isPayment>
        </subItem>
      </itemGroup>
    </paymentDates>
  </xsl:template>

</xsl:stylesheet>
