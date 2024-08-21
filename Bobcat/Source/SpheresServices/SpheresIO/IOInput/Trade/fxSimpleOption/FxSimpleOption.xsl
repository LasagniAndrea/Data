<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <xsl:decimal-format name="decimalFormat" decimal-separator="." />

  <xsl:template match="/iotask">
    <iotask>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter" >
        <parameter>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="@name"/>
          </xsl:attribute>
          <xsl:attribute name="displayname">
            <xsl:value-of select="@displayname"/>
          </xsl:attribute>
          <xsl:attribute name="direction">
            <xsl:value-of select="@direction"/>
          </xsl:attribute>
          <xsl:attribute name="datatype">
            <xsl:value-of select="@datatype"/>
          </xsl:attribute>
          <xsl:value-of select="."/>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>

  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="ioinput"/>
    </iotaskdet>
  </xsl:template>

  <xsl:template match="ioinput">
    <ioinput>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="file"/>
    </ioinput>
  </xsl:template>

  <xsl:template match="file">
    <file>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="folder">
        <xsl:value-of select="@folder"/>
      </xsl:attribute>
      <xsl:attribute name="date">
        <xsl:value-of select="@date"/>
      </xsl:attribute>
      <xsl:attribute name="size">
        <xsl:value-of select="@size"/>
      </xsl:attribute>
      <xsl:apply-templates select="row[@status='success']"/>
    </file>
  </xsl:template>

  <xsl:template match="row">
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>
      <tradeImport xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <settings>
          <importMode>New</importMode>
          <user>SYSADM</user>
          <parameters>
            <parameter name="http://www.efs.org/otcml/tradeImport/instrumentIdentifier" datatype="string">fxSimpleOption</parameter>
            <!-- template from instrument default -->
            <parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string">FxSimpleOption</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string">FxSimpleOption-Broker</parameter>
            <!--/////-->
            <parameter name="http://www.efs.org/otcml/tradeImport/displayname" datatype="string"></parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/description" datatype="string">Import Sample</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/extllink" datatype="string" />
            <!--/////-->
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyFeeCalculation" datatype="bool">false</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyPartyTemplate" datatype="bool">false</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isPostToEventsGen" datatype="bool">true</parameter>
          </parameters>
        </settings>
        <tradeInput>
          <customCaptureInfos>
            <!-- actor1-->
            <customCaptureInfo clientId="tradeHeader_party1_actor" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_book" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1_BOOK']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_localClassDerv" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1_LOCALCLASS']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_iasClassDerv" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1_IASCLASS']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_hedgeClassDerv" dataType="string">
              <value/>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_trader1_identifier" dataType="string">
              <value/>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_frontId" dataType="string">
              <value/>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_folder" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_sales1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party1_sales1_factor" dataType="decimal" />
            <customCaptureInfo clientId="tradeHeader_party1_broker1_actor" dataType="string"/>
            <customCaptureInfo clientId="tradeHeader_party1_broker1_frontId" dataType="string"/>
            <customCaptureInfo clientId="tradeHeader_party1_broker1_trader1_identifier" dataType="string" />
            <!-- actor2-->
            <customCaptureInfo clientId="tradeHeader_party2_actor" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR2']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_book" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR2_BOOK']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_localClassDerv" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR2_LOCALCLASS']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_iasClassDerv" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR2_IASCLASS']"/>
              </value >
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_hedgeClassDerv" dataType="string">
              <value/>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_trader1_identifier" dataType="string">
              <value/>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_frontId" dataType="string">
              <value/>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_folder" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_sales1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party2_sales1_factor" dataType="decimal" />
            <customCaptureInfo clientId="tradeHeader_party2_broker1_actor" dataType="string"/>
            <customCaptureInfo clientId="tradeHeader_party2_broker1_frontId" dataType="string"/>
            <customCaptureInfo clientId="tradeHeader_party2_broker1_trader1_identifier" dataType="string" />
            <!-- tradeHeader -->
            <customCaptureInfo clientId="tradeHeader_tradeDate" dataType="date">
              <value>
                <xsl:value-of select="data[@name='TRADE_DATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_timeStamping" dataType="time" regex="RegexLongTime">
              <value>
                <xsl:value-of select="data[@name='TRADE_HHMMSS']"/>
              </value>
            </customCaptureInfo>
            <!-- product -->
            <customCaptureInfo clientId="fxOpt_optionType" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_OPTIONTYPE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_exerciseStyle" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_EXERCISESTYLE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_expiryDateTime_expiryDate" dataType="date">
              <value>
                <xsl:value-of select="data[@name='FXOPT_EXPIRY_DATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_expiryDateTime_expiryTime_hourMinuteTime" dataType="time" regex="RegexLongTime">
              <value>
                <xsl:value-of select="data[@name='FXOPT_EXPIRY_HHMMSS']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_expiryDateTime_expiryTime_businessCenter" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_EXPIRY_BUSINESSCENTER']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_valueDate" dataType="date">
              <value>
                <xsl:value-of select="data[@name='FXOPT_VALUEDATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_callCurrencyAmount_amount" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FXOPT_CALL_AMOUNT']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_callCurrencyAmount_currency" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_CALL_CURRENCY']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_putCurrencyAmount_amount" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PUT_AMOUNT']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_putCurrencyAmount_currency" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PUT_CURRENCY']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxStrikePrice_strikeQuoteBasis" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_STRIKEPRICE_QUOTEBASIS']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxStrikePrice_rate" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FXOPT_STRIKEPRICE_RATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_procedureSpecified" dataType="bool">
              <value>false</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_payer" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_PAYER']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_receiver" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_RECEIVER']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_premiumSettlementDate" dataType="date">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_STLDATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_premiumQuote_premiumValue" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_QUOTE_VALUE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_premiumQuote_premiumQuoteBasis" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_QUOTE_QUOTEBASIS']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_premiumAmount_amount" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_AMOUNT']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_premiumAmount_currency" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FXOPT_PREMIUM_CURRENCY']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_fxOptionPremium1_settlementInformation" dataType="string">
              <value>None</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_cashSettlementTerms_settlementCurrency" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_cashSettlementTerms_fixing1_fixingDate" dataType="date">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_cashSettlementTerms_fixing1_assetFxRate" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_cashSettlementTerms_fixing1_fixingTime_hourMinuteTime" dataType="time" regex="RegexLongTime">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="fxOpt_cashSettlementTerms_fixing1_fixingTime_businessCenter" dataType="string">
              <value />
            </customCaptureInfo>
            <!-- otherPartyPayment-->
            <customCaptureInfo clientId="otherPartyPayment1_paymentSource_feeSchedule" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_payer" dataType="string" />
            <customCaptureInfo clientId="otherPartyPayment1_receiver" dataType="string" />
            <customCaptureInfo clientId="otherPartyPayment1_paymentDate_unadjustedDate" dataType="date">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentDate_dateAdjustments_bDC" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentType" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentAmount_amount" dataType="decimal" >
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentAmount_currency" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_settlementInformation" dataType="string">
              <value>None</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentSource_feeInvoicing" dataType="bool">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="settlementInput1_settlementInputInfo_cssCriteria_cssCriteriaCss" dataType="string">
              <value />
            </customCaptureInfo>
            <!--netting-->
            <customCaptureInfo clientId="nettingInformation_nettingMethod" dataType="string" />
            <customCaptureInfo clientId="nettingInformation_nettingDesignation" dataType="string" />
            <!--settlementInput-->
            <customCaptureInfo clientId="settlementInput1_settlementContext_currency" dataType="string">
              <value />
            </customCaptureInfo>

          </customCaptureInfos>
        </tradeInput>
      </tradeImport>

    </row>
  </xsl:template >

</xsl:stylesheet>
