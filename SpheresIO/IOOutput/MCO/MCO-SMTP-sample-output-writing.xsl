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
    <SendSmtp>
      <!-- One element SmtpClient -->
      <SmtpClient>
        <Host>smtp.provider.com</Host>
        <Port>25</Port>
      </SmtpClient>
      <!-- One element MailMessage -->
      <MailMessage>
        <!--Add element "From" -->
        <xsl:call-template name="Contact">
          <xsl:with-param name="pRoutingIdsAndExplicitDetails" select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/sendBy/routingIdsAndExplicitDetails"/>
          <xsl:with-param name="pNodeDest" select="'From'" />
        </xsl:call-template>
        <!-- One or more element To -->
        <xsl:apply-templates select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/sendTo"/>
        <!-- Zero or more element Cc -->
        <xsl:apply-templates select="$vMyRow/data[@name='CNFMSGXML']/ValueXML/confirmationMessage/header/copyTo"/>

        <Priority>Normal</Priority>
        <DeliveryNotificationOptions>None</DeliveryNotificationOptions>
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
        <!-- Zero or more element Attachment -->
        <!--We juste recuperate Attachment added in Mapping XSL-->
        <xsl:call-template name="Attachment">
          <xsl:with-param name="pAttachementData" select="$vMyRow/data[@name='Attachment']"/>
        </xsl:call-template>
      </MailMessage>
    </SendSmtp>
  </xsl:template>
  <xsl:template match="sendTo">
    <!--Add element "To" -->
    <xsl:call-template name="Contact">
      <xsl:with-param name="pRoutingIdsAndExplicitDetails" select="routingIdsAndExplicitDetails"/>
      <xsl:with-param name="pNodeDest" select="'To'" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template match="copyTo">
    <xsl:choose>
      <xsl:when test="@bcc='true'">
        <!--Add element "Bcc" -->
        <xsl:call-template name="Contact">
          <xsl:with-param name="pRoutingIdsAndExplicitDetails" select="routingIdsAndExplicitDetails"/>
          <xsl:with-param name="pNodeDest" select="'Bcc'" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!--Add element "Cc" -->
        <xsl:call-template name="Contact">
          <xsl:with-param name="pRoutingIdsAndExplicitDetails" select="routingIdsAndExplicitDetails"/>
          <xsl:with-param name="pNodeDest" select="'Cc'" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="Contact">
    <xsl:param name="pRoutingIdsAndExplicitDetails"/>
    <xsl:param name="pNodeDest"/>

    <xsl:call-template name="tokenizeAddresses">
      <xsl:with-param name="pAddresses" select="$pRoutingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email']"/>
      <xsl:with-param name="pDisplayName" select="$pRoutingIdsAndExplicitDetails/routingName" />
      <xsl:with-param name="pDelimiter" select ="';'"/>
      <xsl:with-param name="pNodeDest" select="$pNodeDest" />
    </xsl:call-template>

  </xsl:template>
  <xsl:template name="Attachment">
    <xsl:param name="pAttachementData"/>

    <xsl:for-each select="$pAttachementData">
      <xsl:copy-of select="ValueXML/Attachment/." />
    </xsl:for-each>

  </xsl:template>

  <!--
  official tokenize implementation disponible at http://www.exslt.org/str/functions/tokenize/str.tokenize.template.xsl
  ToDo: to remplace or to delete when xslt extenstion (exslt) will be disponible
  -->
  <xsl:template name="tokenizeAddresses">
    <xsl:param name="pAddresses" />
    <xsl:param name="pDisplayName" />
    <xsl:param name="pDelimiter" select="' '" />
    <xsl:param name="pNodeDest" select="' '" />

    <xsl:variable name="actualaddress">
      <xsl:choose>
        <xsl:when test="$pDelimiter and contains($pAddresses, $pDelimiter)">
          <xsl:value-of select="normalize-space(substring-before($pAddresses, $pDelimiter))" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space($pAddresses)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- no regexp available in xslt 1, checking not empty and @ existence to validate an electronic address -->
    <xsl:if test="$actualaddress != '' and contains($actualaddress, '@')">
      <xsl:choose>
        <xsl:when test="$pNodeDest = 'Cc'">
          <Cc>
            <xsl:call-template name="AddAddress">
              <xsl:with-param name="pDisplayName" select="$pDisplayName"/>
              <xsl:with-param name="pAddress" select="$actualaddress"/>
            </xsl:call-template>
          </Cc>
        </xsl:when>
        <xsl:when test="$pNodeDest = 'Bcc'">
          <Bcc>
            <xsl:call-template name="AddAddress">
              <xsl:with-param name="pDisplayName" select="$pDisplayName"/>
              <xsl:with-param name="pAddress" select="$actualaddress"/>
            </xsl:call-template>
          </Bcc>
        </xsl:when>
        <xsl:when test="$pNodeDest = 'From'">
          <From>
            <xsl:call-template name="AddAddress">
              <xsl:with-param name="pDisplayName" select="$pDisplayName"/>
              <xsl:with-param name="pAddress" select="$actualaddress"/>
            </xsl:call-template>
          </From>
        </xsl:when>
        <xsl:otherwise>
          <To>
            <xsl:call-template name="AddAddress">
              <xsl:with-param name="pDisplayName" select="$pDisplayName"/>
              <xsl:with-param name="pAddress" select="$actualaddress"/>
            </xsl:call-template>
          </To>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$pDelimiter and contains($pAddresses, $pDelimiter)">
      <xsl:call-template name="tokenizeAddresses">
        <xsl:with-param name="pAddresses"
                        select="substring-after($pAddresses, $pDelimiter)" />
        <xsl:with-param name="pDisplayName" select="$pDisplayName" />
        <xsl:with-param name="pDelimiter" select="$pDelimiter" />
        <xsl:with-param name="pNodeDest" select="$pNodeDest" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <xsl:template name="AddAddress">
    <xsl:param name="pDisplayName"/>
    <xsl:param name="pAddress"/>
    <DisplayName>
      <xsl:value-of select="$pDisplayName" />
    </DisplayName>
    <MailAddress>
      <xsl:value-of select="$pAddress" />
    </MailAddress>
  </xsl:template>

</xsl:stylesheet>
