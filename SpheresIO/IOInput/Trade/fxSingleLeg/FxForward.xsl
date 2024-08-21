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
            <parameter name="http://www.efs.org/otcml/tradeImport/instrumentIdentifier" datatype="string">FxForward</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string">Fx Forward</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string">FxForward-Broker</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/displayname" datatype="string">Fx Forward</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/description" datatype="string">Fx Forward</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/extllink" datatype="string" />
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyFeeCalculation" datatype="bool">false</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyPartyTemplate" datatype="bool">false</parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isPostToEventsGen" datatype="bool">true</parameter>
          </parameters>
        </settings>
        <tradeInput>
          <customCaptureInfos>
            <customCaptureInfo clientId="tradeHeader_party1_actor" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_book" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1_BOOK']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_frontId" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_folder" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_trader1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party1_fxClass" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR1_FXCLASS']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_sales1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party1_sales1_factor" dataType="decimal" />
            <customCaptureInfo clientId="tradeHeader_party1_broker1_actor" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_broker1_frontId" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_broker1_trader1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party2_actor" dataType="string">
              <value>
                <xsl:value-of select="data[@name='ACTOR2']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_book" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_frontId" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_folder" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_trader1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party2_fxClass" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_sales1_identifier" dataType="string" />
            <customCaptureInfo clientId="tradeHeader_party2_sales1_factor" dataType="decimal" />
            <customCaptureInfo clientId="tradeHeader_party2_broker1_actor" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_broker1_frontId" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_broker1_trader1_identifier" dataType="string" />
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
            <customCaptureInfo clientId="fx_valueDate" dataType="date">
              <value>
                <xsl:value-of select="data[@name='FX_VALUEDATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency1_payer" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGEDCURRENCY1_PAYER']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency1_paymentAmount_amount" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGEDCURRENCY1_AMOUNT']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency1_paymentAmount_currency" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGEDCURRENCY1_CURRENCY']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency1_settlementInformation" dataType="string">
              <value>None</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency2_payer" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGEDCURRENCY2_PAYER']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency2_paymentAmount_amount" dataType="decimal">
              <!--
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGEDCURRENCY2_AMOUNT']"/>
              </value>
              -->
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency2_paymentAmount_currency" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGEDCURRENCY2_CURRENCY']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency2_settlementInformation" dataType="string">
              <value>None</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_quotedCurrencyPair_quoteBasis" dataType="string">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGERATE_QUOTEBASIS']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_spotRate" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGERATE_SPOTRATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_forwardPoints" dataType="decimal">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_rate" dataType="decimal">
              <value>
                <xsl:value-of select="data[@name='FX_EXCHANGERATE_RATE']"/>
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_sideRatesSpecified" dataType="bool">
              <value>false</value>
            </customCaptureInfo>
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
            <customCaptureInfo clientId="otherPartyPayment1_paymentAmount_amount" dataType="decimal" />
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
            <customCaptureInfo clientId="settlementInput1_settlementContext_currency" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="settlementInput2_settlementInputInfo_cssCriteria_cssCriteriaCss" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="settlementInput2_settlementContext_currency" dataType="string">
              <value />
            </customCaptureInfo>
            <customCaptureInfo clientId="nettingInformation_nettingMethod" dataType="string" />
            <customCaptureInfo clientId="nettingInformation_nettingDesignation" dataType="string" />
            <customCaptureInfo clientId="fx_exchangedCurrency1_receiver" dataType="string">
              <value>OTCEX</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangedCurrency2_receiver" dataType="string">
              <value>SG_FR</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_quotedCurrencyPair_currency1" dataType="string">
              <value>USD</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="fx_exchangeRate_quotedCurrencyPair_currency2" dataType="string">
              <value>EUR</value>
            </customCaptureInfo>
          </customCaptureInfos>
        </tradeInput>
      </tradeImport>

    </row>
  </xsl:template >

</xsl:stylesheet>
