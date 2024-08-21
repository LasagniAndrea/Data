<?xml version="1.0" encoding="UTF-8"?>
<!-- bcsmessage-par.xsl version2.0 -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
	<xsl:template match="/">
		<xsl:element name="file">
			<!-- Copy the attributes of the node <file> -->
			<xsl:attribute name="name"/>
			<xsl:attribute name="folder"/>
			<xsl:attribute name="date"/>
			<xsl:attribute name="size"/>
			<xsl:attribute name="status">success</xsl:attribute>
			<xsl:apply-templates select="BCSMessageList/BCSMessage"/>
			<xsl:apply-templates select="BCSMessage"/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="BCSMessageList">
		<xsl:apply-templates select="BCSMessage"/>
	</xsl:template>
	<xsl:template match="BCSMessage">
		<xsl:variable name="idprefix" select="'r'"/>
		<xsl:variable name="idposition" select="position()"/>
		<xsl:element name="row">
			<!-- Copy the attributes of the node <row> -->
			<xsl:attribute name="id"><xsl:value-of select="$idprefix"/><xsl:value-of select="$idposition"/></xsl:attribute>
			<xsl:attribute name="src"><xsl:value-of select="$idposition"/></xsl:attribute>
			<xsl:attribute name="status">success</xsl:attribute>
			<!-- Create <data> node -->
			<xsl:choose>
				<xsl:when test="./messageclass='NotifyReportData' and ./datafields/data[@name='infotype']='ORDE'">
					<xsl:apply-templates select="messageclass">
						<xsl:with-param name="type" select="./datafields/data[@name='infotype']"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="message"/>
					<xsl:apply-templates select="datafields">
						<xsl:with-param name="type" select="./datafields/data[@name='infotype']"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="sequencenumber"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="messageclass"/>
					<xsl:apply-templates select="message"/>
					<xsl:apply-templates select="datafields"/>
					<xsl:apply-templates select="sequencenumber"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>
	<xsl:template match="messageclass">
		<xsl:param name="type">0</xsl:param>
		<!--
		<messageclass>xxxxxx</messageclass>
		-->
		<xsl:element name="data">
			<xsl:attribute name="name">MessageClass</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:choose>
				<xsl:when test="$type='ORDE'">OrderHistory</xsl:when>
				<xsl:when test="$type='TRAD'">TradeHistory</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>
	<xsl:template match="message">
		<!--
		<message>xxxxxxxxx</message>
		-->
	</xsl:template>
	<xsl:template match="datafields">
		<xsl:param name="type">0</xsl:param>
		<!--
		<datafields>
			<data>xxxxx</data>
			<data>yyyyy</data>
		</datafields>
		-->
		<xsl:copy-of select="data"/>
		<xsl:apply-templates select="data" mode="Child">
			<xsl:with-param name="type" select="$type"/>
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="sequencenumber">
		<!--
		<sequencenumber>xxxxxxxxx</sequencenumber>
		-->
		<xsl:element name="data">
			<xsl:attribute name="name">SequenceNumber</xsl:attribute>
			<xsl:attribute name="datatype">integer</xsl:attribute>
			<xsl:value-of select="."/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="data" mode="Child">
		<xsl:param name="type">0</xsl:param>
		<!--
		<data>xxxxx</data>
		<data>yyyyy</data>
		-->
		<xsl:choose>
			<xsl:when test="$type='ORDE' and @name='textbuffer'">
				<xsl:call-template name="OrderHistory">
					<xsl:with-param name="reportline" select="."/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$type='TRAD' and @name='textbuffer'">
				<xsl:call-template name="TradeHistory">
					<xsl:with-param name="reportline" select="."/>
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<xsl:template name="OrderHistory">
		<xsl:param name="reportline"/>
		<xsl:element name="data">
			<xsl:attribute name="name">seriesname</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,1,30)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">isincode</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,31,12)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">ordernumber</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,43,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">buyorsell</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,51,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">user</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,52,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">customer</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,60,20)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">account</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,80,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">execcustomer</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,81,12)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">ordertime</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,93,16)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">canceltime</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,109,16)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">pricecondition</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,125,3)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">price</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="concat(concat(substring($reportline,128,13),'.'),substring($reportline,141,4))"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">timecondition</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,145,3)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">datevalidity</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,148,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">timevalidity</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,156,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">quantitycondition</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,164,3)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">quantity</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,167,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">openorclose</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,175,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">orderfreetext</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,176,50)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">modificationnumber</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,226,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">internetmark</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,234,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">conditionid</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,235,1)"/>
		</xsl:element>
	</xsl:template>
	<xsl:template name="TradeHistory">
		<xsl:param name="reportline"/>
		<xsl:element name="data">
			<xsl:attribute name="name">tradenumber</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,1,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">seriesname</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,9,30)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">ordertime</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,39,16)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">isincode</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,55,12)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">modificationnumber</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,67,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">customer</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,75,20)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">account</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,95,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">execcustomer</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,96,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">ordernumber</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,104,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">openorclose</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,112,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">tradefreetext</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,113,50)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">matchordertime</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,168,16)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">boughtorsold</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,184,1)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">price</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="concat(concat(substring($reportline,185,13),'.'),substring($reportline,198,4))"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">quantity</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,202,8)"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">canceltradeflag</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="substring($reportline,210,1)"/>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>
