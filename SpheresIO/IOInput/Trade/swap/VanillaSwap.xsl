<?xml version="1.0" encoding="UTF-8"?>
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
      <tradeImport>
        <settings>
          <importMode>New</importMode>
          <parameters>
            <parameter name="http://www.efs.org/otcml/tradeImport/instrumentIdentifier">VanillaSwap</parameter>
            <!-- screen and template from instrument default -->
            <parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string"/>
            <parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string"/>
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
            
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party1_actor</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR1']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party1_book</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR1_BOOK']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party1_broker1_actor</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR1_BROKER1']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party1_trader1_identifier</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR1_TRADER1']"/>
              </xsl:element>
            </customCaptureInfo>
            
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party2_actor</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR2']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party2_book</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR2_BOOK']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party2_broker1_actor</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR2_BROKER1']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_party2_trader1_identifier</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='ACTOR2_TRADER1']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_tradeDate</xsl:attribute>
              <xsl:attribute name="dataType">date</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='TRADE_DATE']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">tradeHeader_timeStamping</xsl:attribute>
              <xsl:attribute name="dataType">time</xsl:attribute>
              <xsl:attribute name="regex">RegexLongTime</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='TRADE_HHMMSS']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream_calculationPeriodDates_effectiveDate</xsl:attribute>
              <xsl:attribute name="dataType">date</xsl:attribute>
              <xsl:attribute name="regex">RegexLongTime</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_EFFECTIVE_DATE']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream_calculationPeriodDates_terminationDate</xsl:attribute>
              <xsl:attribute name="dataType">date</xsl:attribute>
              <xsl:attribute name="regex">RegexLongTime</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_TERMINATION_DATE']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream_calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue</xsl:attribute>
              <xsl:attribute name="dataType">decimal</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_INITIALVALUE']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream_calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency</xsl:attribute>
              <xsl:attribute name="dataType">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_CURRENCY']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream1_payer</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <!-- ACTOR.IDENTIFIER-->
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM1_PAYER']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream1_calculationPeriodAmount_calculation_rate</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <xsl:attribute name="Regex">RegexRate</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM1_RATE']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream1_calculationPeriodDates_calculationPeriodFrequency_periodMultiplier</xsl:attribute>
              <xsl:attribute name="dataType">integer</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_PERIODMULTIPLIER']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo >
              <xsl:attribute name="clientId">swapStream1_calculationPeriodDates_calculationPeriodFrequency_period</xsl:attribute>
              <xsl:attribute name="integer">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_PERIOD']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream1_calculationPeriodDates_calculationPeriodFrequency_rollConvention</xsl:attribute>
              <xsl:attribute name="integer">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_ROLLCONV']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream1_calculationPeriodDates_calculationPeriodDatesAdjustments_bDC</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_BDC']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo >
              <xsl:attribute name="clientId">swapStream1_calculationPeriodAmount_calculation_dayCountFraction</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_DCF']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream2_payer</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <!-- ACTOR.IDENTIFIER-->
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM2_PAYER']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream2_calculationPeriodAmount_calculation_rate</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <xsl:attribute name="Regex">RegexRate</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM2_RATE']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream2_calculationPeriodDates_calculationPeriodFrequency_periodMultiplier</xsl:attribute>
              <xsl:attribute name="dataType">integer</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_PERIODMULTIPLIER']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo >
              <xsl:attribute name="clientId">swapStream2_calculationPeriodDates_calculationPeriodFrequency_period</xsl:attribute>
              <xsl:attribute name="integer">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_PERIOD']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream2_calculationPeriodDates_calculationPeriodFrequency_rollConvention</xsl:attribute>
              <xsl:attribute name="integer">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_ROLLCONV']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo>
              <xsl:attribute name="clientId">swapStream2_calculationPeriodDates_calculationPeriodDatesAdjustments_bDC</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_BDC']"/>
              </xsl:element>
            </customCaptureInfo>
            <customCaptureInfo >
              <xsl:attribute name="clientId">swapStream2_calculationPeriodAmount_calculation_dayCountFraction</xsl:attribute>
              <xsl:attribute name="string">string</xsl:attribute>
              <xsl:element name ="value">
                <xsl:value-of select="data[@name='STREAM_DCF']"/>
              </xsl:element>
            </customCaptureInfo>
          </customCaptureInfos>
        </tradeInput>
      </tradeImport>
    </row>
  </xsl:template >

</xsl:stylesheet>
