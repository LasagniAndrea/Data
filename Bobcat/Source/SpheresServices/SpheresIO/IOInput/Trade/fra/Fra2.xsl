<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:fpml="http://www.fpml.org/2007/FpML-4-4"
>
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
            <parameter name="http://www.efs.org/otcml/tradeImport/instrumentIdentifier" datatype="string">fra</parameter>
            <!-- template from instrument default -->
            <parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string"/>
            <parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string">FraOTCml</parameter>
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
          <efs:EfsML xmlns:efs="http://www.efs.org/2007/EFSmL-3-0" xmlns="http://www.fpml.org/2007/FpML-4-4"
                     xsi:type="efs:EfsDocument">
            <xsl:copy-of  select ="data[@name='FpML']/ValueXML/fpml:FpML/*" />
          </efs:EfsML>
          <customCaptureInfos>
            <customCaptureInfo clientId="tradeHeader_party1_actor" dataType="string">
              <value>CLIENT1</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party1_book" dataType="string">
              <value>BOOK-CLIENT1</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_party2_actor" dataType="string">
              <value>BNP-PARIBAS</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="tradeHeader_broker1_actor" dataType="string">
              <value>OTCEX</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_payer" dataType="string" >
              <value>BNP-PARIBAS</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_receiver" dataType="string" >
              <value>OTCEX</value>
            </customCaptureInfo>

            <customCaptureInfo clientId="otherPartyPayment1_paymentDate_unadjustedDate" dataType="date">
              <value >
                <xsl:value-of  select ="data[@name='FpML']/ValueXML/fpml:FpML/fpml:trade/fpml:tradeHeader/fpml:tradeDate" />
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentDate_dateAdjustments_bDC" dataType="string">
              <value>NONE</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentType" dataType="string">
              <value>Brokerage</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentAmount_amount" dataType="decimal" >
              <value >
                <xsl:value-of  select ="data[@name='SWML']/ValueXML/fpml:SWML/fpml:swPrivateData/fpml:swBrokerageAmount2/fpml:amount" />
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentAmount_currency" dataType="string">
              <value >
                <xsl:value-of  select ="data[@name='SWML']/ValueXML/fpml:SWML/fpml:swPrivateData/fpml:swBrokerageAmount2/fpml:currency" />
              </value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_settlementInformation" dataType="string">
              <value>None</value>
            </customCaptureInfo>
            <customCaptureInfo clientId="otherPartyPayment1_paymentSource_feeInvoicing" dataType="bool">
              <value>None</value>
            </customCaptureInfo>
          </customCaptureInfos>
        </tradeInput>
      </tradeImport>
    </row>
  </xsl:template >


  <!-- 
          <xsl:variable name ="dataDocument">
            <xsl:value-of disable-output-escaping="yes"  select ="msxsl:node-set(data[@name='FpML']/ValueXML)" />
          </xsl:variable>
          
          <efs:EfsML xmlns:efs="http://www.efs.org/2007/EFSmL-3-0" xmlns="http://www.fpml.org/2007/FpML-4-4" xsi:type="efs:EfsDocument">
            <xsl:value-of disable-output-escaping="yes"  select ="msxsl:node-set($dataDocument)" />
          </efs:EfsML>
          -->

</xsl:stylesheet>
