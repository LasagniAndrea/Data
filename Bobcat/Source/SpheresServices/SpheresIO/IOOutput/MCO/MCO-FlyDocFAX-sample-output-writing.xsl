<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="yes" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1" />

  <xsl:variable name="vMyRow" select="iotask/iotaskdet/iooutput/file/row"/>

  <!--
  Catching the message type, possible values:
  - MONO-TRADE
  - MULTI-PARTIES
  - MULTI-TRADES
  -->
  <xsl:variable name="vMsgType" select="$vMyRow/data[@name='MSGTYPE']"/>

  <xsl:template match="/">
    <WebService>
      <!-- One element Name -->
      <Name>FlyDoc</Name>
      <!-- One element Login -->
      <Login>
        <UserName>{UserName}</UserName>
        <Password crypted="no">{Password}</Password>
      </Login>
      <TransportName>Fax</TransportName>
      <!-- One element Message -->
      <Message>
        <!-- One element From -->
        <From>
          <xsl:call-template name="Contact">
            <xsl:with-param name="pRoutingIdsAndExplicitDetails" select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/sendBy/routingIdsAndExplicitDetails"/>
          </xsl:call-template>
        </From>
        <!-- One element To -->
        <To>
          <xsl:call-template name="Contact">
            <xsl:with-param name="pRoutingIdsAndExplicitDetails" select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/sendTo/routingIdsAndExplicitDetails"/>
          </xsl:call-template>
        </To>
        <!-- One element Subject -->
        <Subject encoding="UTF-8">
          <!-- 
          MONO-TRADE -> Subject: "Trade confirmation"
          MULTI-TRADES/MULTI-PARTIES -> Subject: "Transactions report"
          -->
          <xsl:choose>
            <xsl:when test="$vMsgType = 'MULTI-TRADES' or $vMsgType = 'MULTI-PARTIES'">
              <xsl:text>Transactions report (</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>Trade confirmation (</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
          <!-- add OTCmlId of the notification -->
          <xsl:value-of select ="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/@OTCmlId"/>
          <!-- add "Business date" if exists, else "value date" -->
          <xsl:choose>
            <xsl:when test="($vMsgType = 'MULTI-TRADES' or $vMsgType = 'MULTI-PARTIES') and ($vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/dataDocument/trade[1]/exchangeTradedDerivative)">
              <xsl:text> / </xsl:text>
              <xsl:value-of select ="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/dataDocument/trade[1]/exchangeTradedDerivative/FIXML/TrdCaptRpt/@BizDt"/>
            </xsl:when>
            <xsl:when test="($vMsgType = 'MULTI-TRADES' or $vMsgType = 'MULTI-PARTIES') and ($vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/dataDocument/trade[1]/exchangeTradedDerivative != true())">
              <xsl:text> / </xsl:text>
              <xsl:value-of select ="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/valueDate"/>
            </xsl:when>
            <xsl:otherwise/>
          </xsl:choose>
          <!-- add sendTo identifier -->
          <xsl:choose>
            <xsl:when test="($vMsgType = 'MULTI-TRADES' or $vMsgType = 'MULTI-PARTIES')">
              <xsl:text> / </xsl:text>
              <xsl:variable name="vPartyReference" select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/sendTo/@href"/>
              <xsl:value-of select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/dataDocument/party[@id=concat($vPartyReference,'')]/partyName"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text></xsl:text>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text>)</xsl:text>
        </Subject>
        <!-- One element Body -->
        <Body format="Text" encoding="UTF-8">
          <xsl:text>
        Dear Sir or Madam,

Please find as attachment</xsl:text>
          <xsl:choose>
            <xsl:when test="$vMsgType = 'MULTI-TRADES' or $vMsgType = 'MULTI-PARTIES'">
              <xsl:text> your transactions report.</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text> the confirmation of your trade.</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text>

Yours sincerely,
</xsl:text>
          <xsl:value-of select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/sendBy/routingIdsAndExplicitDetails/routingName" />
        </Body>
        <!-- Zero or one element CoverTemplate -->
        <!--CoverTemplate contenttype="application/msword">.\SpheresIO\FAXCOVER\coverpage.rtf</CoverTemplate-->

        <!-- Zero or more element Attachment -->
        <xsl:call-template name="Attachment">
          <xsl:with-param name="pAttachementData" select="$vMyRow/data[@name='Attachment']"/>
        </xsl:call-template>
      </Message>
    </WebService>
  </xsl:template>

  <xsl:template name="Contact">
    <xsl:param name="pRoutingIdsAndExplicitDetails"/>

    <Name>
      <xsl:value-of select="$pRoutingIdsAndExplicitDetails/routingName" />
    </Name>
    <Company>
      <xsl:value-of select="$pRoutingIdsAndExplicitDetails/routingName" />
    </Company>
    <Address>
      <xsl:value-of select="$pRoutingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber']" />
    </Address>
  </xsl:template>
  <xsl:template name="Attachment">
    <xsl:param name="pAttachementData"/>

    <xsl:for-each select="$pAttachementData">
      <xsl:copy-of select="ValueXML/Attachment/." />
    </xsl:for-each>

  </xsl:template>

</xsl:stylesheet>
