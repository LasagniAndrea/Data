<?xml version="1.0" encoding="utf-8"?>
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys-->
<!-- bcsmessage-map.xsl version2.0 -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
	<xsl:template match="/iotask">
		<iotask>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="parameters"/>
			<xsl:apply-templates select="iotaskdet"/>
		</iotask>
	</xsl:template>
	<xsl:template match="parameters">
		<parameters>
			<xsl:for-each select="parameter">
				<parameter>
					<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
					<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
					<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
					<xsl:attribute name="direction"><xsl:value-of select="@direction"/></xsl:attribute>
					<xsl:attribute name="datatype"><xsl:value-of select="@datatype"/></xsl:attribute>
					<xsl:value-of select="."/>
				</parameter>
			</xsl:for-each>
		</parameters>
	</xsl:template>
	<xsl:template match="iotaskdet">
		<iotaskdet>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="ioinput"/>
		</iotaskdet>
	</xsl:template>
	<xsl:template match="ioinput">
		<ioinput>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="file"/>
		</ioinput>
	</xsl:template>
	<xsl:template match="file">
		<file>
			<!-- Copy the attributes of the node <file> -->
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="folder"><xsl:value-of select="@folder"/></xsl:attribute>
			<xsl:attribute name="date"><xsl:value-of select="@date"/></xsl:attribute>
			<xsl:attribute name="size"><xsl:value-of select="@size"/></xsl:attribute>
			<xsl:variable name="date_of_parameter_p1" select="/iotask/parameters/parameter[@id='p1']"/>
			<!-- we take into account only the rows with the same date of the parameter p1 -->
			<!-- <xsl:apply-templates select="row[data[@name='time'] = $date_of_parameter_p1]"/> -->
			<xsl:apply-templates select="row"/>
		</file>
	</xsl:template>
	<xsl:template match="row">
		<row>
			<!-- Copy the attributes of the node <row> -->
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="src"><xsl:value-of select="@src"/></xsl:attribute>
			<xsl:attribute name="status"><xsl:value-of select="@status"/></xsl:attribute>
			<xsl:choose>
				<xsl:when test="./data='NotifyMarkets'">
					<xsl:call-template name="NotifyMarkets"/>
				</xsl:when>
				<xsl:when test="./data='NotifyClasses'">
					<xsl:call-template name="NotifyClasses"/>
				</xsl:when>
				<xsl:when test="./data='NotifyClearingMemberCodes'">
					<xsl:call-template name="MembersCodes"/>
				</xsl:when>
				<xsl:when test="./data='NotifyNonClearingMemberCodes'">
					<xsl:call-template name="MembersCodes"/>
				</xsl:when>
				<xsl:when test="./data='NotifySeries'">
					<xsl:call-template name="NotifySeries"/>
				</xsl:when>
				<xsl:when test="./data='NotifyContracts'">
					<xsl:call-template name="NotifyContracts"/>
				</xsl:when>
        <xsl:when test="./data='NotifyContractsByTime'">
          <xsl:call-template name="NotifyContracts"/>
        </xsl:when>
        <xsl:when test="./data='DeleteContracts'">
					<xsl:call-template name="DeleteContracts"/>
				</xsl:when>
				<xsl:when test="./data='NotifyContractTransfers'">
					<xsl:call-template name="NotifyContractTransfers"/>
				</xsl:when>
				<xsl:when test="./data='NotifyCollateralGuarantees'">
					<xsl:call-template name="NotifyCollateralGuarantees"/>
				</xsl:when>
				<xsl:when test="./data='NotifyDepositedGuarantees'">
					<xsl:call-template name="NotifyDepositedGuarantees"/>
				</xsl:when>
				<xsl:when test="./data='NotifyEarlyExercises'">
					<xsl:call-template name="NotifyEarlyExercises"/>
				</xsl:when>
				<xsl:when test="./data='NotifyExByEx'">
					<xsl:call-template name="NotifyExByEx"/>
				</xsl:when>
				<xsl:when test="./data='DeleteExByEx'">
					<xsl:call-template name="DeleteExByEx"/>
				</xsl:when>
				<xsl:when test="./data='NotifyExerciseAtExpiry'">
					<xsl:call-template name="NotifyExerciseAtExpiry"/>
				</xsl:when>
        <xsl:when test="./data='NotifySubAssignments'">
          <xsl:call-template name="MarketMessages"/>
        </xsl:when>
        <xsl:when test="./data='NotifyAssignments'">
					<xsl:call-template name="NotifyAssignments"/>
				</xsl:when>
        <xsl:when test="./data='NotifyClearingMessages'">
          <xsl:call-template name="MarketMessages"/>
        </xsl:when>
        <xsl:when test="./data='NotifyClearingMessagesSent'">
          <xsl:call-template name="MarketMessages"/>
        </xsl:when>
        <xsl:when test="./data='NotifyMarketMessages'">
					<xsl:call-template name="MarketMessages"/>
				</xsl:when>
				<xsl:when test="./data='NotifyMarketMessagesSent'">
					<xsl:call-template name="MarketMessages"/>
				</xsl:when>
				<xsl:when test="./data='NotifyReport'">
					<xsl:call-template name="NotifyReport"/>
				</xsl:when>
				<xsl:when test="./data='NotifyReportData'">
					<xsl:call-template name="ReportData"/>
				</xsl:when>
        <xsl:when test="./data='NotifyZipReportData'">
          <xsl:call-template name="ReportData"/>
        </xsl:when>
        <xsl:when test="./data='NotifyReportSent'">
					<xsl:call-template name="NotifyReport"/>
				</xsl:when>
				<xsl:when test="./data='NotifyClosingPrice'">
					<xsl:call-template name="NotifyClosingPrice"/>
				</xsl:when>
				<xsl:when test="./data='NotifyTheoreticalValue'">
					<xsl:call-template name="NotifyTheoreticalValue"/>
				</xsl:when>
				<xsl:when test="./data='NotifyClassFile'">
					<xsl:call-template name="NotifyClassFile"/>
				</xsl:when>
				<xsl:when test="./data='ConnectionResponse'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='DisconnectionResponse'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='MarketStatus'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='SubmitResponse'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='SubscribeResponse'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='UnSubscribeResponse'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='InquireResponse'">
					<xsl:call-template name="ResponseStatus"/>
				</xsl:when>
				<xsl:when test="./data='OrderHistory'">
					<xsl:call-template name="ReportData"/>
					<xsl:call-template name="OrderHistory"/>
				</xsl:when>
				<xsl:when test="./data='TradeHistory'">
					<xsl:call-template name="ReportData"/>
				</xsl:when>
				<xsl:when test="./data='NotifyNewDay'">
					<xsl:call-template name="NotifyNewDay"/>
				</xsl:when>
			</xsl:choose>
		</row>
	</xsl:template>
	<xsl:template name="DateSysInsUpd">
		<column name="DTSYS" datakey="true" datakeyupd="false" datatype="datetime" dataformat="yyyyMMdd">
			<xsl:value-of select="data[@name='dtsys']"/>
		</column>
		<column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
			<SpheresLib function="GetUTCDateTimeSys()"/>
			<controls>
				<control action="RejectColumn" return="true" logtype="None">
					<SpheresLib function="IsUpdate()"/>
				</control>
			</controls>
		</column>
		<column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
			<SpheresLib function="GetUTCDateTimeSys()"/>
		</column>
	</xsl:template>
	<xsl:template name="DateSysInsUpdNoKey">
		<column name="DTSYS" datakey="false" datakeyupd="false" datatype="datetime" dataformat="yyyyMMdd">
			<xsl:value-of select="data[@name='dtsys']"/>
		</column>
		<column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
			<SpheresLib function="GetUTCDateTimeSys()"/>
			<controls>
				<control action="RejectColumn" return="true" logtype="None">
					<SpheresLib function="IsUpdate()"/>
				</control>
			</controls>
		</column>
		<column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
			<SpheresLib function="GetUTCDateTimeSys()"/>
		</column>
	</xsl:template>
	<xsl:template name="NotifyMarkets">
		<table name="BCS_MARKETS" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="MARKETID" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="MARKETACRONYM" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketacronym']"/>
			</column>
			<column name="MARKETCODEALFA" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketcodealfa']"/>
			</column>
			<column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='description']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyClasses">
		<table name="BCS_CLASSES" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="SYMBOL" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
			<column name="PRODUCTTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='producttype']"/>
			</column>
			<column name="PRODUCTGROUP" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='productgroup']"/>
			</column>
			<column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='description']"/>
			</column>
			<column name="MARKETID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="UNDERLYINGID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='underlyingid']"/>
			</column>
			<column name="MINMARGIN" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='minmargin']"/>
			</column>
			<column name="MARGININTERVAL" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='margininterval']"/>
			</column>
			<column name="SETTLEMENTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='settlementtype']"/>
			</column>
			<column name="CONTRACTSIZE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='contractsize']"/>
			</column>
			<column name="OPTIONTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='optiontype']"/>
			</column>
			<column name="OPTIONSTYLE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='optionstyle']"/>
			</column>
			<column name="SETTLEMENTDAYS" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='settlementdays']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="MembersCodes">
		<table name="BCS_MEMBERCODES" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="ABICODE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='abicode']"/>
			</column>
			<column name="MNEMONIC" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='mnemonic']"/>
			</column>
			<column name="MARKETID" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="PARTECIPANTCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='partecipantcode'],1,40)"/>
			</column>
			<column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='description']"/>
			</column>
			<column name="MEMBERTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='membertype']"/>
			</column>
			<column name="CEDCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='cedcode']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifySeries">
		<table name="BCS_SERIES" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="SYMBOL" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
			<column name="PUTCALL" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="EXPIRATIONMONTH" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='expiryperiod']"/>
			</column>
			<column name="STRIKEPRICE" datakey="true" datakeyupd="false" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
			<column name="PRODUCTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='producttype']"/>
			</column>
			<column name="MARKETID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="LASTTRADINGDAY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='lasttradingday']"/>
			</column>
			<column name="CLOSINGPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='closingprice']"/>
			</column>
			<column name="LASTDAYPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='lastdayprice']"/>
			</column>
			<column name="UNDERLYINGPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='underlyingprice']"/>
			</column>
			<column name="OPENINTEREST" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='openinterest']"/>
			</column>
			<column name="VOLATILITY" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='volatility']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
			<column name="EXPIRATIONDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationdate']"/>
			</column>
			<column name="CLOSINGPRICEDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='closingpricedate']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyContracts">
		<table name="BCS_CONTRACTS" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="ABICODE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='abicode']"/>
			</column>
			<column name="ACCOUNTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='accounttype']"/>
			</column>
			<column name="SYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="EXPIRATIONMONTH" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationmonth']"/>
			</column>
			<column name="STRIKEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="PUTCALL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="CONTRACTDATE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='contractdate']"/>
			</column>
			<column name="CONTRACTTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='contracttime']"/>
			</column>
			<column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="QUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='quantity']"/>
			</column>
			<column name="PRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='price']"/>
			</column>
			<column name="OPENCLOSE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='openclose']"/>
			</column>
			<column name="MARKETID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="CONTRACTNUMBER" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='contractnumber']"/>
			</column>
			<column name="ORIGCONTRACTNUMBER" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='origcontractnumber']"/>
			</column>
			<column name="GIVEUPABICODE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='giveupabicode']"/>
			</column>
			<column name="SIDE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='side']"/>
			</column>
			<column name="CLIENTINFO" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='clientinfo']"/>
			</column>
			<column name="TRADEDESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='tradedescription']"/>
			</column>
			<column name="VALUE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='value']"/>
			</column>
			<column name="ACCRUAL" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='accrual']"/>
			</column>
			<column name="SETTLEMENTDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='settlementdate']"/>
			</column>
			<column name="REPOINDEX" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='repoindex']"/>
			</column>
			<column name="REPORATE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='reporate']"/>
			</column>
			<column name="TRANSFERREDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='transferredquantity']"/>
			</column>
			<column name="TRANSFERREDREQUEST" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='transferredrequest']"/>
			</column>
			<column name="CLIENTCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='clientcode']"/>
			</column>
			<column name="SUBACCOUNT" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
			<column name="ORDERNUMBER" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='ordernumber']"/>
			</column>
			<column name="TRADERID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='traderid']"/>
			</column>
			<column name="CONTRACTSTATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='contractstate']"/>
			</column>
			<column name="MARKETCONTRACTNUMBER" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='marketcontractnumber']"/>
			</column>
			<column name="MARKETSOURCE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketsource']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="DeleteContracts">
		<table name="BCS_CONTRACTS" action="D">
			<column name="DTSYS" datakey="true" datakeyupd="false" datatype="datetime" dataformat="yyyyMMdd">
				<xsl:value-of select="data[@name='dtsys']"/>
			</column>
			<column name="MARKETID" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyContractTransfers">
		<table name="BCS_CONTRACTTRANSFERS" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="TRANSFERTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='transfertype']"/>
			</column>
			<column name="DELIVERABICODE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='deliverabicode']"/>
			</column>
			<column name="DELIVERACCOUNTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='deliveraccounttype']"/>
			</column>
			<column name="RECEIVERABICODE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='receiverabicode']"/>
			</column>
			<column name="RECEIVERACCOUNTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='receiveraccounttype']"/>
			</column>
			<column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="SYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="CONTRACTNUMBER" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='contractnumber']"/>
			</column>
			<column name="CONTRACTDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='contractdate']"/>
			</column>
			<column name="PRODUCTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='producttype']"/>
			</column>
			<column name="EXPIRATIONMONTH" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationmonth']"/>
			</column>
			<column name="STRIKEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="PUTCALL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="MARKETID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="QUANTITY" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='quantity']"/>
			</column>
			<column name="SIDE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='side']"/>
			</column>
			<column name="TRANSFERSTATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='transferstate']"/>
			</column>
			<column name="RETURNCODE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='returncode']"/>
			</column>
			<column name="ENTRYTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='entrytime']"/>
			</column>
			<column name="EXECUTIONTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='executiontime']"/>
			</column>
			<column name="REQUESTKEY" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='requestkey']"/>
			</column>
			<column name="DELIVERNAME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='delivername']"/>
			</column>
			<column name="RECEIVERNAME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='receivername']"/>
			</column>
			<column name="ADDITIONALINFO" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='additionalinfo']"/>
			</column>
			<column name="DELIVERCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='delivercode']"/>
			</column>
			<column name="DELIVERINFO" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='deliverinfo']"/>
			</column>
			<column name="TRANSFERDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='transferdate']"/>
			</column>
			<column name="PRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='price']"/>
			</column>
			<column name="SUBACCOUNT" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
			<column name="RECEIVERCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='receivercode']"/>
			</column>
			<column name="RECEIVERINFO" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='receiverinfo']"/>
			</column>
			<column name="OPENCLOSE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='openclose']"/>
			</column>
			<column name="TRANSFERMODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='transfermode']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
			<column name="MARKETCONTRACTNUMBER" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='marketcontractnumber']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyCollateralGuarantees">
		<table name="BCS_COLLATERALGUARANTEES" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="ISINCODE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="DESCRIPTION" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='description']"/>
			</column>
			<column name="CURRENCY" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='currency']"/>
			</column>
			<column name="PRICE" datakey="true" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='price']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyDepositedGuarantees">
		<table name="BCS_DEPOSITEDGUARANTEES" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="ACCOUNTTYPE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='accounttype']"/>
			</column>
			<column name="DEPOSITTYPE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='deposittype']"/>
			</column>
			<column name="DEPOSITDATE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='depositdate']"/>
			</column>
			<column name="EXPIRATIONDATE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationdate']"/>
			</column>
			<column name="DEPOSITQUANTITY" datakey="true" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='depositquantity']"/>
			</column>
			<column name="VALUE" datakey="true" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='value']"/>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="DESCRIPTION" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='description']"/>
			</column>
			<column name="CURRENCYID" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='currencyid']"/>
			</column>
			<column name="FACEVALUE" datakey="true" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='facevalue']"/>
			</column>
			<column name="COVEREDPOSITION" datakey="true" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='coveredposition']"/>
			</column>
			<column name="MEMBERDESCRIPTION" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='memberdescription']"/>
			</column>
			<column name="BDDEPOSITTYPE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='bddeposittype']"/>
			</column>
			<column name="INELIGIBILITY" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='ineligibility']"/>
			</column>
			<column name="RETURNDATE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='returndate']"/>
			</column>
			<column name="SYMBOL" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="BDMULTIPLIER" datakey="true" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='bdmultiplier']"/>
			</column>
			<column name="MODIFYCURRENTDAY" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='modifycurrentday']"/>
			</column>
			<column name="DEPOSITSERIALNUMBER" datakey="true" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='depositserialnumber']"/>
			</column>
			<column name="SUBACCOUNT" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyEarlyExercises">
		<table name="BCS_EARLYEXERCISE" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="MARKETID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="ABICODE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='abicode']"/>
			</column>
			<column name="ACCOUNTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='accounttype']"/>
			</column>
			<column name="SYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="EXPIRATIONMONTH" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationmonth']"/>
			</column>
			<column name="STRIKEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="PUTCALL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="REQUESTKEY" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='requestkey']"/>
			</column>
			<column name="EXERCISETIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='exercisetime']"/>
			</column>
			<column name="QUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='quantity']"/>
			</column>
			<column name="INOUTTHEMONEYAMOUNT" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='inoutthemoneyamount']"/>
			</column>
			<column name="SUBACCOUNT" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
			<column name="REQUESTSTATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='requeststate']"/>
			</column>
			<column name="EXERCISEDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='exercisedate']"/>
			</column>
			<column name="TOTALEXERCISEQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='totalexercisequantity']"/>
			</column>
			<column name="INOUTTHEMONEY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='inoutthemoney']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="DeleteExByEx">
		<table name="BCS_EXBYEX" action="D">
			<column name="DTSYS" datakey="true" datakeyupd="false" datatype="datetime" dataformat="yyyyMMdd">
				<xsl:value-of select="data[@name='dtsys']"/>
			</column>
			<column name="MARKETID" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyExByEx">
		<table name="BCS_EXBYEX" action="IU">
			<xsl:call-template name="DateSysInsUpdNoKey"/>
			<column name="MARKETID" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="ABICODE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='abicode']"/>
			</column>
			<column name="ACCOUNTTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='accounttype']"/>
			</column>
			<column name="SUBACCOUNT" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
			<column name="SYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="EXPIRATIONMONTH" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationdate']"/>
			</column>
			<column name="STRIKEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="PUTCALL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="PRODUCTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='producttype']"/>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="REQUESTTIME" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='requesttime']"/>
			</column>
			<column name="PROPOSEDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='proposedquantity']"/>
			</column>
			<column name="REQUESTEDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='requestedquantity']"/>
			</column>
			<column name="CLIENTINFO" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='clientinfo']"/>
			</column>
			<column name="REQUESTSTATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='requeststate']"/>
			</column>
			<column name="INOUTTHEMONEY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='inoutthemoney']"/>
			</column>
			<column name="INOUTTHEMONEYAMOUNT" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='inoutthemoneyamount']"/>
			</column>
			<column name="UNDERLYINGPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='underlyingprice']"/>
			</column>
			<column name="RETURNCODE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='returncode']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyExerciseAtExpiry">
		<table name="BCS_EXERCISEATEXPIRY" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="MARKETID" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="ABICODE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='abicode']"/>
			</column>
			<column name="ACCOUNTTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='accounttype']"/>
			</column>
			<column name="SUBACCOUNT" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
			<column name="SYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="EXPIRATIONDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationdate']"/>
			</column>
			<column name="STRIKEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="PUTCALL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="PRODUCTTYPE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='producttype']"/>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="PROPOSEDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='proposedquantity']"/>
			</column>
			<column name="REQUESTEDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='requestedquantity']"/>
			</column>
			<column name="EXPIRYEXERCISETYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='expiryexercisetype']"/>
			</column>
			<column name="INOUTTHEMONEY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='inoutthemoney']"/>
			</column>
			<column name="INOUTTHEMONEYAMOUNT" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='inoutthemoneyamount']"/>
			</column>
			<column name="UNDERLYINGPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='underlyingprice']"/>
			</column>
			<column name="ABANDONEDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='abandonedquantity']"/>
			</column>
			<column name="AVAILABLEQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='availablequantity']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyAssignments">
		<table name="BCS_ASSIGNMENTS" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="ASSIGNMENTDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='assignmentdate']"/>
			</column>
			<column name="ABICODE" datakey="true" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='abicode']"/>
			</column>
			<column name="ACCOUNTTYPE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='accounttype']"/>
			</column>
			<column name="SUBACCOUNT" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='subaccount']"/>
			</column>
			<column name="SYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='symbol']"/>
			</column>
			<column name="EXPIRATIONDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='expirationdate']"/>
			</column>
			<column name="STRIKEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='strikeprice']"/>
			</column>
			<column name="PUTCALL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='putcall']"/>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="ASSIGNEDQUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='assignedquantity']"/>
			</column>
			<column name="MARKETID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='marketid']"/>
			</column>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="MarketMessages">
		<table name="BCS_CLEARINGMESSAGES" action="I">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='description']"/>
			</column>
			<column name="BUSINESSDATE" datakey="true" datakeyupd="false" datatype="datetime" dataformat="yyyyMMdd">
				<xsl:value-of select="data[@name='dtsys']"/>
			</column>
			<column name="SEQUENCENUMBER" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='sequencenumber']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyReport">
		<table name="BCS_REPORT" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="INFOTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='infotype']"/>
			</column>
			<column name="BUSINESSDATE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='businessdate']"/>
			</column>
			<column name="SENTDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='sentdate']"/>
			</column>
			<column name="SENTTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='senttime']"/>
			</column>
			<column name="FILETYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='filetype']"/>
			</column>
			<column name="PARTECIPANTCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='partecipantcode']"/>
			</column>
			<column name="FILESIZE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='filesize']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="ReportData">
		<table name="BCS_REPORTDATA" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="INFOTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='infotype']"/>
			</column>
			<column name="LINE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='SequenceNumber']"/>
			</column>
			<column name="LISTNAME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='listname']"/>
			</column>
      <column name="FILETYPE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="data[@name='filetype']"/>
      </column>
      <column name="PARTECIPANTCODE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="data[@name='partecipantcode']"/>
      </column>
      <column name="TEXTBUFFER" datakey="false" datakeyupd="false" datatype="text">
				<xsl:value-of select="data[@name='textbuffer']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyClosingPrice">
		<table name="BCS_CLOSINGPRICE" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
			</column>
			<column name="BIDPREMIUM" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='bidpremium']"/>
			</column>
			<column name="ASKPREMIUM" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='askpremium']"/>
			</column>
			<column name="OPENINGPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='openingprice']"/>
			</column>
			<column name="SETTLEPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='settleprice']"/>
			</column>
			<column name="LASTPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='lastprice']"/>
			</column>
			<column name="HIGHPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='highprice']"/>
			</column>
			<column name="LOWPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='lowprice']"/>
			</column>
			<column name="TODAYVOLUME" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='todayvolume']"/>
			</column>
			<column name="YESTERDAYVOLUME" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='yesterdayvolume']"/>
			</column>
			<column name="OPENINTEREST" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='openinterest']"/>
			</column>
			<column name="VOLATILITY" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='volatility']"/>
			</column>
			<column name="YESTERDAYTURNOVER" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='yesterdayturnover']"/>
			</column>
			<column name="TODAYTURNOVER" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='todayturnover']"/>
			</column>
			<column name="UNDERLYINGPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='underlyingprice']"/>
			</column>
			<column name="BIDTHEORETICAL" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='bidtheoretical']"/>
			</column>
			<column name="ASKTHEORETICAL" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='asktheoretical']"/>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyTheoreticalValue">
		<table name="BCS_THEORETICALVALUE" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="SERIESID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesid']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
			<column name="MARKETPRICE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='marketprice']"/>
			</column>
			<column name="DOWNSIDE5" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='downside5']"/>
			</column>
			<column name="DOWNSIDE4" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='downside4']"/>
			</column>
			<column name="DOWNSIDE3" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='downside3']"/>
			</column>
			<column name="DOWNSIDE2" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='downside2']"/>
			</column>
			<column name="DOWNSIDE1" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='downside1']"/>
			</column>
			<column name="UPSIDE5" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='upside5']"/>
			</column>
			<column name="UPSIDE4" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='upside4']"/>
			</column>
			<column name="UPSIDE3" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='upside3']"/>
			</column>
			<column name="UPSIDE2" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='upside2']"/>
			</column>
			<column name="UPSIDE1" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='upside1']"/>
			</column>
			<column name="DOWNSIDESIGN5" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='downsidesign5']"/>
			</column>
			<column name="DOWNSIDESIGN4" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='downsidesign4']"/>
			</column>
			<column name="DOWNSIDESIGN3" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='downsidesign3']"/>
			</column>
			<column name="DOWNSIDESIGN2" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='downsidesign2']"/>
			</column>
			<column name="DOWNSIDESIGN1" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='downsidesign1']"/>
			</column>
			<column name="UPSIDESIGN5" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='upsidesign5']"/>
			</column>
			<column name="UPSIDESIGN4" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='upsidesign4']"/>
			</column>
			<column name="UPSIDESIGN3" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='upsidesign3']"/>
			</column>
			<column name="UPSIDESIGN2" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='upsidesign2']"/>
			</column>
			<column name="UPSIDESIGN1" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='upsidesign1']"/>
			</column>
			<column name="OPTADJUST" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='optadjust']"/>
			</column>
			<column name="OPTADJUSTSIGN" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='optadjustsign']"/>
			</column>
			<column name="CEDSYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='cedsymbol']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
			<column name="ISINCODE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
				<controls>
					<control action="RejectRow" return="true">
						<SpheresLib function="IsEmpty()"/>
					</control>
				</controls>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyClassFile">
		<table name="BCS_CLASSFILE" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="COMMODITY" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='commodity']"/>
			</column>
			<column name="PRODUCTTYPE" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='producttype']"/>
			</column>
			<column name="OFFSET" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='offset']"/>
			</column>
			<column name="DECINSPOTSPREADRATE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decinspotspreadrate']"/>
			</column>
			<column name="SPOTSPREADRATE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='spotspreadrate']"/>
			</column>
			<column name="DECINNONSPOTSPREADRATE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decinnonspotspreadrate']"/>
			</column>
			<column name="NONSPOTSPREADRATE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='nonspotspreadrate']"/>
			</column>
			<column name="DECINDELIVERYRATE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decindeliveryrate']"/>
			</column>
			<column name="DELIVERYRATE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='deliveryrate']"/>
			</column>
			<column name="DECINCONTRACTSIZE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decincontractsize']"/>
			</column>
			<column name="CONTRACTSIZE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='contractsize']"/>
			</column>
			<column name="DECINCURRENTMARKETVALUE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decincurrentmarketvalue']"/>
			</column>
			<column name="CURRENTMARKETVALUE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='currentmarketvalue']"/>
			</column>
			<column name="DECINMARGININTERVAL" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decinmargininterval']"/>
			</column>
			<column name="MARGININTERVAL" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='margininterval']"/>
			</column>
			<column name="DECINEXCHANGERATE" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decinexchangerate']"/>
			</column>
			<column name="EXCHANGERATE" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='exchangerate']"/>
			</column>
			<column name="CURRENCYHAIRCUT" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='currencyhaircut']"/>
			</column>
			<column name="DECOPTIONMINMARGIN" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decoptionminmargin']"/>
			</column>
			<column name="OPTIONMINMARGIN" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='optionminmargin']"/>
			</column>
			<column name="DECINFUTUREMARGIN" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decinfuturemargin']"/>
			</column>
			<column name="FUTUREMARGIN" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='futuremargin']"/>
			</column>
			<column name="DECINRISKFREEINTEREST" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decinriskfreeinterest']"/>
			</column>
			<column name="RISKFREEINTEREST" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='riskfreeinterest']"/>
			</column>
			<column name="DECINDIVIDEND" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='decindividend']"/>
			</column>
			<column name="DIVIDENDAMOUNT" datakey="false" datakeyupd="true" datatype="decimal">
				<xsl:value-of select="data[@name='dividendamount']"/>
			</column>
			<column name="CEDDIVIDENDDATE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='ceddividenddate']"/>
			</column>
			<column name="CEDCURRENCY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='cedcurrency']"/>
			</column>
			<column name="CEDSYMBOL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='cedsymbol']"/>
			</column>
			<column name="CEDCLASSGROUP" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='cedclassgroup']"/>
			</column>
			<column name="CEDPRODUCTGROUP" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='cedproductgroup']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="ResponseStatus">
		<table name="BCS_RESPONSESTATUS" action="I">
			<xsl:call-template name="DateSysInsUpdNoKey"/>
			<column name="MESSAGECLASS" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='MessageClass']"/>
			</column>
			<column name="CONNECTIONSTATUS" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='connectionstatus']"/>
			</column>
			<column name="MARKETREPLY" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='marketreply']"/>
			</column>
			<column name="NOTIFICATION" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='notification']"/>
			</column>
			<column name="SPECIFICATION" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='specification']"/>
			</column>
			<column name="REQUESTCLASSNAME" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='requestclassname']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="OrderHistory">
		<table name="BCS_ORDERHISTORY" action="IU">
			<xsl:call-template name="DateSysInsUpd"/>
			<column name="LINE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='SequenceNumber']"/>
			</column>
			<column name="INFOTYPE" datakey="true" datakeyupd="false" datatype="integer">
				<xsl:value-of select="data[@name='infotype']"/>
			</column>
			<column name="LISTNAME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='listname']"/>
			</column>
			<column name="SERIESNAME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='seriesname']"/>
			</column>
			<column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='isincode']"/>
			</column>
			<column name="ORDERNUMBER" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='ordernumber']"/>
			</column>
			<column name="BUYORSELL" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='buyorsell']"/>
			</column>
			<column name="USERNAME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='user']"/>
			</column>
			<column name="CUSTOMER" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='customer']"/>
			</column>
			<column name="ACCOUNT" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='account']"/>
			</column>
			<column name="EXECCUSTOMER" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='execcustomer']"/>
			</column>
			<column name="ORDERTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='ordertime']"/>
			</column>
			<column name="CANCELTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='canceltime']"/>
			</column>
			<column name="PRICECONDITION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='pricecondition']"/>
			</column>
			<column name="PRICE" datakey="false" datakeyupd="true" datatype="decimal" dataformat="##########0.0000">
				<xsl:value-of select="data[@name='price']"/>
			</column>
			<column name="TIMECONDITION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='timecondition']"/>
			</column>
			<column name="DATEVALIDITY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='datevalidity']"/>
			</column>
			<column name="TIMEVALIDITY" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='timevalidity']"/>
			</column>
			<column name="QUANTITYCONDITION" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='quantitycondition']"/>
			</column>
			<column name="QUANTITY" datakey="false" datakeyupd="true" datatype="integer">
				<xsl:value-of select="data[@name='quantity']"/>
			</column>
			<column name="OPENORCLOSE" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='openorclose']"/>
			</column>
			<column name="ORDERFREETEXT" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='orderfreetext']"/>
			</column>
			<column name="MODIFICATIONNUMBER" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='modificationnumber']"/>
			</column>
			<column name="MO6MARK" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='mo6mark']"/>
			</column>
			<column name="INTERNETMARK" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='internetmark']"/>
			</column>
			<column name="CONDITIONPRICE" datakey="false"  datakeyupd="true" datatype="decimal" dataformat="##########0.0000">
				<xsl:value-of select="data[@name='conditionprice']"/>
			</column>
			<column name="CONDITIONID" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='conditionid']"/>
			</column>
			<column name="STOPSERIES" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='stopseries']"/>
			</column>
			<column name="TRIGGERTIME" datakey="false" datakeyupd="true" datatype="string">
				<xsl:value-of select="data[@name='triggertime']"/>
			</column>
		</table>
	</xsl:template>
	<xsl:template name="NotifyNewDay">
		<table name="BCS_DAY" action="IU">
			<xsl:call-template name="DateSysInsUpdNoKey"/>
		</table>
	</xsl:template>
</xsl:stylesheet>
