<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:param name="pGUID"/>
  <xsl:param name="pIDE"/>
  <xsl:param name="pPayerReceiver"/>
  <xsl:param name="pPayer"/>
  <xsl:param name="pBookPayer"/>
  <xsl:param name="pReceiver"/>
  <xsl:param name="pBookReceiver"/>
  <xsl:param name="pIsBookEntry"/>
  <xsl:param name="pIsDeliveryMessage"/>	
  <xsl:param name="pCurrentCulture" select="'en-GB'"/>

  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" encoding="UTF-8"/>

  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:template match="/">

    <script type="text/javascript">
      function EventSiMsg(Payer_Receiver)
      {
      OpenEventSiMsg(<xsl:value-of select="$pIDE" />, Payer_Receiver);
      }
    </script>
    <xsl:variable name="Efssi" select ="EfsSettlementInstruction"/>
    <xsl:apply-templates select='$Efssi'/>
  </xsl:template>


  <xsl:template match="EfsSettlementInstruction">
    <xsl:variable name="resPayerReceiver">
      <xsl:value-of select="$varResource[@name=$pPayerReceiver]/value" />
    </xsl:variable>
    <table class="DataGrid" cellpadding="1" cellspacing="0" rules="none" style="vertical-align:top;width:100%;border-collapse:collapse;">
      <tr class="DataGrid_HeaderStyle">
        <td style="width:100px;">
          <xsl:choose>
            <xsl:when test = "$resPayerReceiver=''">
              <xsl:value-of select="$pPayerReceiver" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$resPayerReceiver" />
            </xsl:otherwise>
          </xsl:choose>
        </td>
      </tr>
      <tr class="DataGrid_ItemStyle">
        <td>
          <xsl:choose>
            <xsl:when test = "$pPayerReceiver='Payer'">
              <xsl:value-of select="$pPayer" />
              <xsl:if test = "$pBookPayer != ''">
                &#xa0;[<xsl:value-of select="$pBookPayer" />]
              </xsl:if>
            </xsl:when>
            <xsl:when test = "$pPayerReceiver='Receiver'">
              <xsl:value-of select="$pReceiver" />
              <xsl:if test = "$pBookReceiver != ''">
                &#xa0;[<xsl:value-of select="$pBookReceiver" />]
              </xsl:if>
            </xsl:when>
          </xsl:choose>
        </td>
      </tr>
      <xsl:if test = "$pIsBookEntry='0'">
        <tr class="DataGrid_ItemStyle">
          <td style="width:100px;">
            <xsl:choose>
              <xsl:when test = "$pPayerReceiver='Payer'">
                <input type="submit" name="DspSIXmlPayer" value="" id="btnXMLPay"  class="ui-main ui-main-xml" style="cursor:pointer;"       onclick="javascript:__doPostBack('DspSIXmlPayer','Payer');return false;" />
                <xsl:if test = "$pIsDeliveryMessage='0'">
                  <input type="submit" name="DspMSGPayer" value=""  id="btnMsgPAY" class="ui-main ui-main-msg" style="cursor:pointer;"
                          onclick="EventSiMsg('Payer');return false;" alt="Message" />
                </xsl:if>
              </xsl:when>
              <xsl:when test = "$pPayerReceiver='Receiver'">
                <input type="submit" name="DspSIXmlReceiver" value="" id="btnXMLRec"  class="ui-main ui-main-xml" style="cursor:pointer;"                     onclick="javascript:__doPostBack('DspSIXmlReceiver','Receiver');return false;" />
                <xsl:if test = "$pIsDeliveryMessage='0'">
                  <input type="submit" name="DspMSGReceiver"  value=""  id="btnMsgREC" class="ui-main ui-main-msg" style="cursor:pointer;"                       onclick="EventSiMsg('Receiver');return false;" alt="Message" />
                </xsl:if>
              </xsl:when>
            </xsl:choose>
          </td>
        </tr>
      </xsl:if>
    </table>
    <!--<div style="overflow:auto;height:410px;">-->
    <xsl:variable name="isExistSettlementMethodInformation" select="settlementMethodInformation" />
    <xsl:if test="$isExistSettlementMethodInformation = ''">
      <xsl:apply-templates select="settlementMethod"/>
    </xsl:if>
    <xsl:apply-templates select="settlementMethodInformation"/>
    <xsl:apply-templates select="correspondentInformation"/>
    <xsl:apply-templates select="intermediaryInformation"/>
    <xsl:apply-templates select="beneficiaryBank"/>
    <xsl:apply-templates select="beneficiary"/>

    <!--<xsl:apply-templates select="fpml:splitSettlement"/>-->

    <xsl:apply-templates select="originatorInformation"/>
    <xsl:apply-templates select="investorInformation"/>
    <!--</div>-->
  </xsl:template>


  <!-- SettlementMethod -->
  <xsl:template match="settlementMethod">
    <br/>
    <table class="DataGrid" cellpadding="1" cellspacing="0" rules="none" style="vertical-align:top;width:100%;border-collapse:collapse;">
		<thead>
			<tr class="DataGrid_HeaderStyle">
				<td colspan="2">
					<xsl:value-of select="'Settlement method'"/>
				</td>
			</tr>
		</thead>
		<tr class="DataGrid_ItemStyle">
			<td style="width:100px;">
				<xsl:value-of select="$varResource[@name='IDDATA']/value" />
			</td>
			<td>
				<xsl:value-of select="."/>
			</td>
		</tr>
    </table>
  </xsl:template>

  <!-- SettlementMethodInformation -->
  <xsl:template match="settlementMethodInformation">
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="'Settlement method'" />
    </xsl:call-template>
  </xsl:template>

  <!-- Correspondent -->
  <xsl:template match="correspondentInformation">
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="'Correspondent'" />
    </xsl:call-template>
  </xsl:template>

  <!-- Intermediary -->
  <xsl:template match="intermediaryInformation">
    <xsl:variable name="seqNumber" select="intermediarySequenceNumber"/>
    <xsl:variable name="type">
      Intermediary <xsl:value-of select="$seqNumber"/>
    </xsl:variable>
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="$type" />
    </xsl:call-template>
  </xsl:template>

  <!-- Beneficiary -->
  <xsl:template match="beneficiary">
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="'Account owner'" />
    </xsl:call-template>
  </xsl:template>

  <!-- BeneficiaryBank -->
  <xsl:template match="beneficiaryBank">
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="'Account servicer'" />
    </xsl:call-template>
  </xsl:template>

  <!-- InvestorInformation -->
  <xsl:template match="investorInformation">
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="'Investor'" />
    </xsl:call-template>
  </xsl:template>

  <!-- OriginatorInformation -->
  <xsl:template match="originatorInformation">
    <xsl:call-template name="Routing">
      <xsl:with-param name="pType" select="'Originator'" />
    </xsl:call-template>
  </xsl:template>


  <!-- Routing -->
  <xsl:template name="Routing">
    <xsl:param name="pType" />
    <table class="DataGrid" cellpadding="1" cellspacing="0" rules="none" style="vertical-align:top;width:100%;border-collapse:collapse;">
      <xsl:apply-templates select="routingIds">
        <xsl:with-param name="pType" select="$pType" />
      </xsl:apply-templates>
      <xsl:apply-templates select="routingExplicitDetails">
        <xsl:with-param name="pType" select="$pType" />
      </xsl:apply-templates>
      <xsl:apply-templates select="routingIdsAndExplicitDetails">
        <xsl:with-param name="pType" select="$pType" />
      </xsl:apply-templates>
    </table>
    <br/>
  </xsl:template>

  <!-- RoutingIds -->
  <xsl:template match="routingIds">
    <xsl:param name="pType" />
	<thead>
	  <tr class="DataGrid_HeaderStyle">
		  <td colspan="2">
			  <xsl:value-of select="$pType"/>
		  </td>
	  </tr>
	</thead>
	<tr class="DataGrid_ItemStyle">
		<td>Ids</td>
		<td>
			<xsl:apply-templates select="routingId"/>
		</td>
	</tr>
  </xsl:template>

  <!-- RoutingExplicitDetails -->
  <xsl:template match="routingExplicitDetails">
    <xsl:param name="pType" />
    <xsl:apply-templates select="routingName">
      <xsl:with-param name="pType" select="$pType" />
    </xsl:apply-templates>
    <xsl:apply-templates select="routingAccountNumber"/>
    <xsl:apply-templates select="routingAddress"/>
    <xsl:apply-templates select="routingReferenceText"/>
  </xsl:template>

  <!-- RoutingIdsAndExplicitDetails -->
  <xsl:template match="routingIdsAndExplicitDetails">
    <xsl:param name="pType" />
    <xsl:apply-templates select="routingName">
      <xsl:with-param name="pType" select="$pType" />
    </xsl:apply-templates>
    <xsl:apply-templates select="routingAccountNumber"/>
    <xsl:apply-templates select="routingReferenceText"/>
    <xsl:apply-templates select="routingAddress"/>
  </xsl:template>

  <!-- RoutingName -->
  <xsl:template match="routingName">
    <xsl:param name="pType" />
	<thead>
	  <tr class="DataGrid_HeaderStyle">
		  <td colspan="2">
			  <xsl:value-of select="$pType"/>
		  </td>
	  </tr>
	</thead>
	<tr class="DataGrid_ItemStyle">
		<td style="width:100px;">
			<xsl:value-of select="$varResource[@name='IDDATA']/value" />
		</td>
		<td>
	        <xsl:value-of select="."/>
		</td>
    </tr>
  </xsl:template>

  <!-- RoutingAddress -->
  <xsl:template match="routingAddress">
    <xsl:apply-templates select="streetAddress"/>
    <xsl:apply-templates select="country"/>
  </xsl:template>

  <!-- StreetAddress -->
  <xsl:template match="streetAddress">
    <tr class="DataGrid_ItemStyle">
		<td style="width:100px;">
			<xsl:value-of select="$varResource[@name='Address']/value" />
		</td>
		<td>
	        <xsl:apply-templates select="streetLine"/>
		</td>
    </tr>
  </xsl:template>

  <!-- Country -->
  <xsl:template match="country">
    <tr class="DataGrid_ItemStyle">
		<td style="width:100px;">
			<xsl:value-of select="$varResource[@name='Country_Title']/value" />
		</td>
		<td>
			<xsl:value-of select="."/>
		</td>
	</tr>
  </xsl:template>

  <!-- StreetLine -->
  <xsl:template match="streetLine">
    <xsl:value-of select="."/>
    <br/>
  </xsl:template>

  <!-- RoutingAccountNumber -->
  <xsl:template match="routingAccountNumber">
    <tr class="DataGrid_ItemStyle">
      <td width="100px">
        <xsl:value-of select="$varResource[@name='AccountNumber_']/value" />
      </td>
      <td>
        <xsl:value-of select="."/>
      </td>
    </tr>
  </xsl:template>

  <!-- RoutingId -->
  <xsl:template match="routingId">
    <xsl:value-of select="."/>
    <br/>
  </xsl:template>

  <!-- RoutingReferenceText -->
  <xsl:template match="routingReferenceText">
    <tr class="DataGrid_ItemStyle">
      <td>&#xa0;</td>
      <td>
        <xsl:value-of select="."/>
      </td>
    </tr>
  </xsl:template>

</xsl:stylesheet>

